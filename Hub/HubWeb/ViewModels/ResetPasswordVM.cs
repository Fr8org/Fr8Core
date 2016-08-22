using System.ComponentModel.DataAnnotations;

namespace HubWeb.ViewModels
{
    public class ResetPasswordVM
    {
        public string UserId { get; set; }
        public string Code { get; set; }
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Compare("Password")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }
}