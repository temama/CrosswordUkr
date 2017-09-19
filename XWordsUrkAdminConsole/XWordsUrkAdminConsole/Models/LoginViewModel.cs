using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace XWordsUrkAdminConsole.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "*")]
        [Display(Name = "Login")]
        public string LoginName { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}