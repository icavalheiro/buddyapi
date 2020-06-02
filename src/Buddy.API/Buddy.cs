using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NSwag.Generation.Processors.Security;
using System;
using System.Collections.Generic;
using System.Text;

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
        /// Assist in the integration of JWT authentication into a asp.net core app
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
    }
}
