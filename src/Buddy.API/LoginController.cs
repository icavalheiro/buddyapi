﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using static Buddy.API.Helpers.LoginHelper;
using System.IdentityModel.Tokens.Jwt;
using Buddy.API.Models;

namespace Buddy.API
{
    /// <summary>
    /// Controller that hanldes user login with JWT.
    /// </summary>
    /// <typeparam name="T">The user model type</typeparam>
    [Route("api/[controller]")]
    [ApiController]
    public abstract class LoginController<T> : Controller where T : User
    {
        private readonly SigningCredentials _credentials;

        public LoginController(SigningCredentials credentials)
        {
            _credentials = credentials;
        }

        /// <summary>
        /// Login endpoint.
        /// </summary>
        /// <param name="login">The login model</param>
        /// <returns>A response containing the token or "unauthorized" in case it fails to validate</returns>
        [AllowAnonymous]
        [HttpPost]
        public virtual IActionResult Login([FromForm] string username, [FromForm]string password)
        {
            var user = AuthenticateUser(username, password);

            if (user != null)
            {
                return BuildOkResultForLogin(user);
            }

            return Unauthorized();
        }

        /// <summary>
        /// Used to build the OK result returned when a user successfully
        /// logins into the API.
        /// 
        /// If you are building a alternative way for the user to login
        /// you could call this function to retrieve the OK result
        /// after you validate the user youself.
        /// 
        /// Also you can override it if you want to add specials things
        /// to the OK message.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected virtual OkObjectResult BuildOkResultForLogin(T user)
        {
            var token = GenerateJSONWebToken(user);
            return Ok(new
            {
                authenticated = true,
                created = DateTime.Now,
                expiration = token.ValidTo,
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                message = "Ok"
            });
        }

        /// <summary>
        /// Creates a JWT for the given user.
        /// </summary>
        /// <param name="user">User to be used to create the JWT token</param>
        /// <returns>The JWT token</returns>
        protected JwtSecurityToken GenerateJSONWebToken(T user)
        {
            var token = new JwtSecurityToken(
                null,
                "ApiUser",
                ToClaims(user.PermissionGroup),
                expires: DateTime.Now.AddDays(1),
                signingCredentials: _credentials
            );


            return token;
        }

        /// <summary>
        /// Method that try to convert a login model into a user.
        /// If it fails null should be returned.
        /// </summary>
        /// <param name="login">The login model to be used</param>
        /// <returns>The user that matches the login or null</returns>
        protected abstract T AuthenticateUser(string username, string password);
    }
}
