using Microsoft.AspNetCore.Builder;
using System;

namespace Buddy
{
    public static class SPA
    {
        /// <summary>
        /// Spa router middleware.
        /// It's similar to the Microsoft's SPA middleware but it redirects
        /// to the SPA controller ("/") instead of the index.html file. That
        /// way you could add back-end metrics os something like that.
        /// 
        /// The usage of this middleware is not needed for the API to work properly.
        /// But in case you want to host a SPA alongiside the API you could use it to
        /// avoid incompatibilities with routes, since this helper will not affect
        /// the "/api/*" routes of your API, those not affecting 404 responses wrongly.
        /// 
        /// You should add this middleware as the first one, or as one of the first
        /// ones when setting up the middlewares for your application, since it realies
        /// on the routing and mvc middlewares to be registered after it.
        /// 
        /// It will automatically add the MVC and Static Files middlewares since it depends
        /// on them.
        /// </summary>
        /// <param name="app">"this" parameter that defines this as an extension method</param>
        public static void UseSpaHelperRoute(this IApplicationBuilder app)
        {
            //Register the middleware in the app
            app.Use(async (route, next) => {

                //let all the other middlewares run before it
                await next();

                //now we check if any of the middlewares had run
                //if they did something other than 404 will be 
                //set as the status code.
                if (route.Response.StatusCode == 404)
                {
                    //before continuing, lets first check if its not an API route
                    //because we won't be showing custom pages for API routes
                    if (route.Request.Path.StartsWithSegments("/api/"))
                    {
                        return;
                    }

                    //since no middlware/controller/route was found for this request
                    //we will pass it through the home route
                    //and set the status code to 200
                    route.Request.Path = "/";
                    route.Response.StatusCode = 200;

                    //now we run all the middlewares again
                    //to allow the request to hit the home
                    //controller
                    await next();
                }
            });

            //Now register the static files middleware since it's needed
            //to serve the static files from the SPA page, like scripts
            //styles and images
            app.UseStaticFiles();

            //Also register the MVC routes since it's a must have in order
            //for the spa routing to work properly, at least a home controller
            //must exists
            app.UseMvcWithDefaultRoute();
        }
    }
}
