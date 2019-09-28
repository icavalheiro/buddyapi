using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Buddy.API.Models;

namespace Buddy.API.Helpers
{
    public static class LoginHelper
    {
        private static string UserPermissionKey = "User_Permission";

        public static IEnumerable<Claim> ToClaims(PermissionGroup permissionGroup)
        {
            yield return new Claim(UserPermissionKey, JsonConvert.SerializeObject(permissionGroup));
        }


        public static PermissionGroup ToPermissionGroup(IEnumerable<Claim> claims)
        {
            var claim = claims.Where(x => x.Type == UserPermissionKey).FirstOrDefault();
            if(claim != null)
            {
               return JsonConvert.DeserializeObject<PermissionGroup>(claim.Value);
            }

            return null;
        }
    }
}
