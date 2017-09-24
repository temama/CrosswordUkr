using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace XWordsUrkAdminConsole.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Login can't be empty")]
        [Display(Name = "Login")]
        public string LoginName { get; set; }

        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}