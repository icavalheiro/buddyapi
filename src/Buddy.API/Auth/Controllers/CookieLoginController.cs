using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Buddy.API.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;

namespace Buddy.API.Auth.Controllers
{
    /// <summary>
    /// Controller that handles user login with JWT.
    /// Default base route is "/user/" so the login action would be routed to "/user/login"
    /// </summary>
    /// <typeparam name="T">The user model type</typeparam>
    [Route("/user/")]
    public abstract class CookieLoginController : Controller
    {
        /// <summary>
        /// Path the user will be redirected to once logged in successfully.
        /// Defaults to "/"
        /// </summary>
        /// <returns>A root based string, example: "/user/profile"</returns>
        protected virtual string GetRedirectPathOnceLogin()
        {
            return "/";
        }

        /// <summary>
        /// Login endpoint.
        /// </summary>
        /// <param name="login">The login model</param>
        /// <returns>A response containing the cookie or "unauthorized" in case it fails to validate</returns>
        [AllowAnonymous]
        [HttpGet]
        public virtual IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Login endpoint.
        /// Will return the login view with 'ViewData["LoginFailed"] = true;' in case the AuthenticateUser method return null.
        /// </summary>
        /// <param name="login">The login model</param>
        /// <returns>A response containing the cookie or "unauthorized" in case it fails to validate</returns>
        [AllowAnonymous]
        [HttpPost]
        public virtual async Task<IActionResult> Login(string username, string password)
        {
            var user = AuthenticateUser(username, password);
            if(user == null)
            {
                ViewData["LoginFailed"] = true;
                return View();
            }

            await HttpContext.SignInAsync("CookieAuth", user);
            return Redirect(GetRedirectPathOnceLogin());
        }

        /// <summary>
        /// Logouts the user and redirects him to the login page
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public virtual IActionResult Logout()
        {
            InvalidateLogin(User);
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        /// <summary>
        /// Method that try to convert a login model into a user.
        /// If it fails null should be returned.
        /// </summary>
        /// <param name="login">The login model to be used</param>
        /// <returns>The user that matches the login or null</returns>
        protected abstract ClaimsPrincipal AuthenticateUser(string username, string password);

        /// <summary>
        /// Methot that invalidates a user login (called on logout).
        /// Make sure to invalidate any login tokens that imght be stored in a database.
        /// </summary>
        /// <param name="currentUser">User to be logged off</param>
        protected abstract void InvalidateLogin(ClaimsPrincipal currentUser);
    }
}
