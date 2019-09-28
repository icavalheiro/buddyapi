using Buddy.API.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Buddy.API.Models
{
    /// <summary>
    /// Represents the minimun that an User model should contains
    /// in order to work with the Login Controller that ships with
    /// the API
    /// </summary>
    [Serializable]
    public class User : Entity
    {
        /// <summary>
        /// Password of the user.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Salt to be used in the password.
        /// </summary>
        public string PasswordSalt { get; set; }

        /// <summary>
        /// The permission group that this user belongs to.
        /// </summary>
        public PermissionGroup PermissionGroup { get; set; }
        public Guid PermissionGroupId { get; set; }
    }
}
