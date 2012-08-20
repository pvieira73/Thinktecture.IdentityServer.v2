﻿/*
 * Copyright (c) Dominick Baier.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Xml;
using Thinktecture.IdentityModel.Constants;
using Thinktecture.IdentityServer.Models;
using Thinktecture.IdentityServer.Repositories;
using Thinktecture.IdentityServer.TokenService;
using Thinktecture.IdentityModel.Tokens;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Protocols.OAuth2;

namespace Thinktecture.IdentityServer.Protocols
{
    public class AuthenticationHelper
    {
        [Import]
        public IUserRepository UserRepository { get; set; }

        [Import]
        public IRelyingPartyRepository RelyingPartyRepository { get; set; }

        public AuthenticationHelper()
        {
            Container.Current.SatisfyImportsOnce(this);
        }

        public AuthenticationHelper(IUserRepository userRepository, IRelyingPartyRepository relyingPartyRepository)
        {
            UserRepository = userRepository;
            RelyingPartyRepository = relyingPartyRepository;
        }

        public bool TryGetPrincipalFromHttpRequest(HttpRequestBase request, out ClaimsPrincipal principal)
        {
            principal = null;

            // first check for client certificate
            if (TryGetClientCertificatePrinciaplFromRequest(request, out principal))
            {
                return true;
            }

            // then basic authentication
            if (TryGetBasicAuthenticationPrincipalFromRequest(request, out principal))
            {
                return true;
            }

            return false;
        }

        public bool TryGetPrincipalFromOAuth2Request(HttpRequestBase request, ResourceOwnerCredentialRequest tokenRequest, out ClaimsPrincipal principal)
        {
            principal = null;

            // first check for client certificate
            if (TryGetClientCertificatePrinciaplFromRequest(request, out principal))
            {
                return true;
            }

            // then OAuth2 userName credential
            if (UserRepository.ValidateUser(tokenRequest.UserName ?? "", tokenRequest.Password ?? ""))
            {
                principal = CreatePrincipal(tokenRequest.UserName, AuthenticationMethods.Password);
                return true;
            }

            return false;
        }

        public bool TryGetPrincipalFromWrapRequest(HttpRequestBase request, out ClaimsPrincipal principal)
        {
            HttpListenerBasicIdentity userNameId;
            principal = null;

            // first check for client certificate
            if (TryGetClientCertificatePrinciaplFromRequest(request, out principal))
            {
                return true;
            }

            // then WRAP userName credential
            if (TryGetUserNameCredentialsFromWrapRequest(request, out userNameId))
            {
                if (UserRepository.ValidateUser(userNameId.Name, userNameId.Password))
                {
                    principal = CreatePrincipal(userNameId.Name, AuthenticationMethods.Password);
                    return true;
                }
            }

            return false;
        }

        public bool TryGetClientCertificatePrinciaplFromRequest(HttpRequestBase request, out ClaimsPrincipal principal)
        {
            X509Certificate2 clientCertificate = null;
            principal = null;

            if (TryGetClientCertificateFromRequest(request, out clientCertificate))
            {
                string userName;
                if (UserRepository.ValidateUser(clientCertificate, out userName))
                {
                    principal = CreatePrincipal(userName, AuthenticationMethods.X509);
                    return true;
                }
            }

            return false;
        }

        public bool TryGetClientCertificateFromRequest(HttpRequestBase request, out X509Certificate2 clientCertificate)
        {
            clientCertificate = null;

            if (request.ClientCertificate.IsPresent && request.ClientCertificate.IsValid)
            {
                clientCertificate = new X509Certificate2(request.ClientCertificate.Certificate);
                return true;
            }

            return false;
        }

        public bool TryGetBasicAuthenticationPrincipalFromRequest(HttpRequestBase request, out ClaimsPrincipal principal)
        {
            principal = null;
            HttpListenerBasicIdentity identity = null;

            if (TryGetBasicAuthenticationCredentialsFromRequest(request, out identity))
            {
                if (UserRepository.ValidateUser(identity.Name, identity.Password))
                {
                    principal = CreatePrincipal(identity.Name, AuthenticationMethods.Password);
                    return true;
                }
            }

            return false;
        }

        public bool TryGetBasicAuthenticationCredentialsFromRequest(HttpRequestBase request, out HttpListenerBasicIdentity identity)
        {
            identity = null;

            string header = request.Headers["Authorization"] ?? request.Headers["X-Authorization"];
            if (header != null && header.StartsWith("Basic"))
            {
                string encodedUserPass = header.Substring(6).Trim();

                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                string userPass = encoding.GetString(Convert.FromBase64String(encodedUserPass));
                int separator = userPass.IndexOf(':');

                string[] credentials = new string[2];
                credentials[0] = userPass.Substring(0, separator);
                credentials[1] = userPass.Substring(separator + 1);

                identity = new HttpListenerBasicIdentity(credentials[0], credentials[1]);
                return true;
            }

            return false;
        }
        
        public bool TryGetUserNameCredentialsFromWrapRequest(HttpRequestBase request, out HttpListenerBasicIdentity identity)
        {
            identity = null;
            var userName = request.Form["wrap_name"];
            var password = request.Form["wrap_password"];

            if (string.IsNullOrWhiteSpace(userName) ||
               string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            identity = new HttpListenerBasicIdentity(userName, password);
            return true;
        }

        public ClaimsPrincipal CreatePrincipal(string username, string authenticationMethod)
        {
            var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.AuthenticationMethod, authenticationMethod),
                        AuthenticationInstantClaim.Now,
                    };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Constants.AuthenticationType));
            return FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthenticationManager.Authenticate(string.Empty, principal);
        }

        public bool TryIssueToken(EndpointAddress appliesTo, ClaimsPrincipal principal, string tokenType, out SecurityToken token)
        {
            token = null;

            var rst = new RequestSecurityToken
            {
                RequestType = RequestTypes.Issue,
                AppliesTo = new EndpointReference(appliesTo.Uri.AbsoluteUri),
                KeyType = KeyTypes.Bearer,
                TokenType = tokenType
            };

            var sts = TokenServiceConfiguration.Current.CreateSecurityTokenService();

            try
            {
                var rstr = sts.Issue(principal, rst);
                token = rstr.RequestedSecurityToken.SecurityToken;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryIssueToken(EndpointAddress appliesTo, ClaimsPrincipal principal, string tokenType, out TokenResponse response)
        {
            SecurityToken token = null;
            response = new TokenResponse { TokenType = tokenType };

            var result = TryIssueToken(appliesTo, principal, tokenType, out token);
            if (result == false)
            {
                return false;
            }

            if (tokenType == TokenTypes.JsonWebToken || tokenType == TokenTypes.SimpleWebToken)
            {
                var handler = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers[tokenType];
                response.TokenString = handler.WriteToken(token);
                response.ContentType = "text";
            }
            else
            {
                var handler = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers;
                var sb = new StringBuilder(128);
                handler.WriteToken(new XmlTextWriter(new StringWriter(sb)), token);

                response.ContentType = "text/xml";
                response.TokenString = sb.ToString();
            }

            return result;
        }

        public void SetSessionToken(string userName, string authenticationMethod, bool isPersistent, int ttl, string resourceName, IEnumerable<Claim> additionalClaims = null)
        {
            var principal = CreatePrincipal(userName, authenticationMethod);
            var transformedPrincipal = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthenticationManager.Authenticate(resourceName, principal);
            
            // add additional claims if present
            if (additionalClaims != null)
            {
                additionalClaims.ToList().ForEach(c => transformedPrincipal.Identities.First().AddClaim(c));
            }

            var sessionToken = new SessionSecurityToken(transformedPrincipal, TimeSpan.FromHours(ttl))
            {
                IsPersistent = isPersistent
            };

            FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(sessionToken);
        }

        public RelyingParty GetRelyingPartyDetails(string realm)
        {
            RelyingParty rp = null;

            if (RelyingPartyRepository.TryGet(realm, out rp))
            {
                return rp;
            }

            return null;
        }

        public RelyingParty GetRelyingPartyDetailsFromReturnUrl(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return null;
            }

            var url = HttpUtility.UrlDecode(returnUrl);
            var message = WSFederationMessage.CreateFromUri(new Uri("http://foo.com" + url, UriKind.Absolute)) as SignInRequestMessage;

            if (message != null)
            {
                return GetRelyingPartyDetails(message.Realm);
            }

            return null;
        }
    }
}