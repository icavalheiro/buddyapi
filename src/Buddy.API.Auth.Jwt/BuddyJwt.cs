using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Buddy.API.Auth.Jwt
{
    public static class BuddyJwt
    {
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
