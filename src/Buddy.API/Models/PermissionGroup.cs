using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buddy.API.Models
{
    /// <summary>
    /// Represents the permission groups of the application.
    /// </summary>
    [Serializable]
    public class PermissionGroup : Entity
    {
        private static char SEPARATOR = '\n';

        public string SerializedPermissions { get; set; } = "";

        public string[] GetPermissions()
        {
            return SerializedPermissions.Split(new char[] { SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Allows this permission group to the specifice permission.
        /// </summary>
        /// <param name="permissionToBeAllowed">Permission to be allowed</param>
        public void AllowTo(params string[] permissionsToBeAllowed)
        {
            var permissions = GetPermissions();

            //Add back the current permissions
            List<string> permissionList = new List<string>(permissions);
            
            //Add the new permissions
            foreach(var permission in permissionsToBeAllowed)
            {
                //lets not add the same permission twice
                if (!permissionList.Contains(permission))
                    permissionList.Add(permission);
            }

            //let's start building the string
            var sb = new StringBuilder();

            for(int i = 0; i < permissionList.Count; i++)
            {
                var entry = permissionList[i];
                var isLast = (i + 1) >= permissionList.Count;

                //add to the builder
                sb.Append(entry);

                //if its not last we must add the separator
                if (!isLast)
                    sb.Append(SEPARATOR);
            }

            //serialize builder and save in class
            SerializedPermissions = sb.ToString();
        }

        /// <summary>
        /// Method that allows us to check if a member of this group can do an action described
        /// by the permission.
        /// </summary>
        /// <param name="permissionRequired">Name of the permission that is required</param>
        /// <returns>True if members of this group can do it</returns>
        public bool Can(params string[] permissionsRequired)
        {
            var permissions = GetPermissions();

            //If this group contains the all key we just skip to true
            if (permissions.Contains("*"))
                return true;

            foreach (var permission in permissionsRequired)
                if (!permissions.Contains(permission))
                    return false;

            return true;
        }
    }
}
