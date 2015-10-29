using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HubWeb.ViewModels
{
    public class ForgotPasswordVM
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Your email")]
        public string Email { get; set; }
    }
}