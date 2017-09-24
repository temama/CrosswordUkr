using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XWordsUrkAdminConsole.Models
{
    public partial class User
    {
        public bool HasRole(string role)
        {
            if (string.IsNullOrEmpty(Roles))
                return false;

            var roles = Roles.Split(';');
            return roles.Any(r => r == role);
        }
    }
}