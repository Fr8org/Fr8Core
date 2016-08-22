using System.ComponentModel.DataAnnotations;

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