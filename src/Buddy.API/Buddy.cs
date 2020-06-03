using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Buddy
{
    public static class Buddy
    {
        /// <summary>
        /// Adds the swagger API document middlware to the pipeline if the application is
        /// in development mode.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env">The current envirionment</param>
        public static void UseBuddyApi(this IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseOpenApi();
                app.UseSwaggerUi3();
            }
        }

        /// <summary>
        /// Adds the necessary services for the Swagger API to work properly
        /// </summary>
        /// <param name="services"></param>
        public static void AddBuddyApi(this IServiceCollection services)
        {
            services.AddSwaggerDocument(doc =>
            {
                doc.Title = "Development API document";
            });
        }

        /// <summary>
        /// Assist in the integration of JWT authentication into an asp.net core app
        /// Scheme name: JwtAuth
        /// </summary>
        /// <param name="services"></param>
        /// <param name="secretKey">Key to be used to encrypt the JWT (should be shared across server instances)</param>
        public static void AddJwtAuth(this IServiceCollection services, string secretKey)
        {
            //add the key signing class to the dependencies
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            services.AddSingleton<SigningCredentials>(credentials);

            //setup asp.net jwt dependencies
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "JwtAuth";
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    IssuerSigningKey = securityKey
                };
            });
        }

        /// <summary>
        /// Assist in the integration of cookie authentication into an asp.net core app
        /// Scheme name: CookieAuth
        /// </summary>
        /// <param name="services"></param>
        /// <param name="loginPath">URL path to the login page to be used in case of access denied</param>
        /// <param name="name">Cookie name to be used in client storage</param>
        public static void AddCookieAuth(this IServiceCollection services, string loginPath = "/user/login", string name = "buddy.cookie", int expireDays = 30)
        {
            services.AddAuthentication("CookieAuth")
            .AddCookie("CookieAuth", options =>
            {
                options.LoginPath = loginPath;
                options.Cookie.Name = name;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.MaxAge = TimeSpan.FromDays(expireDays);
                options.ExpireTimeSpan = TimeSpan.FromDays(expireDays);
            });
        }

        /// <summary>
        /// Assist in the integration of Azure AD SSO authentication into and asp.net core app
        /// The callbackURL will be: /signin-oidc (for logout should be /signout-oidc)
        /// Scheme name: AzureADAuth
        /// </summary>
        /// <param name="services"></param>
        /// <param name="tenantId">The tenant GUID string</param>
        /// <param name="clientId">The client (or private) GUID string</param>
        /// <param name="websiteDomain">Websites domain, exmaple: "my.gretsite.net"</param>
        /// <param name="azureInstance">Host server for the Azure AD, the default should fit 99% of use cases</param>
        public static void AddAzureADAuth(this IServiceCollection services, string tenantId, string clientId, string websiteDomain, string azureInstance = "https://login.microsoftonline.com/")
        {
            services.AddAuthentication("AzureADAuth")
            .AddAzureAD(options => {
                options.Instance = azureInstance;

                //test azure AD
                options.ClientId = clientId;
                options.TenantId = tenantId;
                options.Domain = websiteDomain;
                options.CallbackPath = "/signin-oidc";
            });

            services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
            {
                options.Authority = options.Authority + "/v2.0/";         // Microsoft identity platform
                options.TokenValidationParameters.ValidateIssuer = false; // accept several tenants (here simplified)
            });
        }
    }
}
