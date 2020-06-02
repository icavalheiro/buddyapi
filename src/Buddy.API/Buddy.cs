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
    }
}
