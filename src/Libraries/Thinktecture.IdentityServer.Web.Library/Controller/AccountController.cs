﻿/*
 * Copyright (c) Dominick Baier.  All rights reserved.
 * see license.txt
 */

using System.ComponentModel.Composition;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using Thinktecture.IdentityServer.Repositories;
using Thinktecture.IdentityServer.Web.ViewModels;

namespace Thinktecture.IdentityServer.Web.Controllers
{
    public class AccountController : AccountControllerBase
    {
        [Import]
        public IUserRepository UserRepository { get; set; }

        [Import]
        public IConfigurationRepository ConfigurationRepository { get; set; }

        public AccountController() : base()
        { }

        public AccountController(IUserRepository userRepository, IConfigurationRepository configurationRepository) : base(userRepository, configurationRepository)
        { }
        
        // shows the signin screen
        public ActionResult SignIn(string returnUrl)
        {
            // you can call AuthenticationHelper.GetRelyingPartyDetailsFromReturnUrl to get more information about the requested relying party

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.ShowClientCertificateLink = ConfigurationRepository.Global.EnableClientCertificateAuthentication;

            return View();
        }

        // handles the signin
        [HttpPost]
        public ActionResult SignIn(SignInModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (UserRepository.ValidateUser(model.UserName, model.Password))
                {
                    // establishes a principal, set the session cookie and redirects
                    // you can also pass additional claims to signin, which will be embedded in the session token

                    return SignIn(
                        model.UserName, 
                        AuthenticationMethods.Password, 
                        returnUrl, 
                        model.EnableSSO, 
                        ConfigurationRepository.Global.SsoCookieLifetime);
                }
            }

            ModelState.AddModelError("", "Incorrect credentials or no authorization.");

            ViewBag.ShowClientCertificateLink = ConfigurationRepository.Global.EnableClientCertificateAuthentication;
            return View(model);
        }

        // handles client certificate based signin
        public ActionResult CertificateSignIn(string returnUrl)
        {
            if (!ConfigurationRepository.Global.EnableClientCertificateAuthentication)
            {
                return new HttpNotFoundResult();
            }

            var clientCert = HttpContext.Request.ClientCertificate;

            if (clientCert != null && clientCert.IsPresent && clientCert.IsValid)
            {
                string userName;
                if (UserRepository.ValidateUser(new X509Certificate2(clientCert.Certificate), out userName))
                {
                    // establishes a principal, set the session cookie and redirects
                    // you can also pass additional claims to signin, which will be embedded in the session token

                    return SignIn(
                        userName, 
                        AuthenticationMethods.X509, 
                        returnUrl, 
                        false, 
                        ConfigurationRepository.Global.SsoCookieLifetime);
                }
            }

            return View("Error");
        }
    }
}