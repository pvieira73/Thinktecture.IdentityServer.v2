﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Thinktecture.IdentityServer.Web.App_LocalResources.OAuth2Authorize {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ShowConsent_cshtml {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ShowConsent_cshtml() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Thinktecture.IdentityServer.Web.App_LocalResources.OAuth2Authorize.ShowConsent.cs" +
                            "html", typeof(ShowConsent_cshtml).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Allow Access.
        /// </summary>
        public static string AllowAccess {
            get {
                return ResourceManager.GetString("AllowAccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No Thanks.
        /// </summary>
        public static string DenyAccess {
            get {
                return ResourceManager.GetString("DenyAccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The client will be able to access the resource, even when you are offline..
        /// </summary>
        public static string RefreshTokenEnabledWarning {
            get {
                return ResourceManager.GetString("RefreshTokenEnabledWarning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to is requesting permission to access a resource on your behalf:.
        /// </summary>
        public static string RequestingAccessOnBehalf {
            get {
                return ResourceManager.GetString("RequestingAccessOnBehalf", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resource Consent.
        /// </summary>
        public static string ResourceConsent {
            get {
                return ResourceManager.GetString("ResourceConsent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resource ID:.
        /// </summary>
        public static string ResourceId {
            get {
                return ResourceManager.GetString("ResourceId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resource Name:.
        /// </summary>
        public static string ResourceName {
            get {
                return ResourceManager.GetString("ResourceName", resourceCulture);
            }
        }
    }
}
