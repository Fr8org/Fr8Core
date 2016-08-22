using System.ComponentModel.DataAnnotations;

namespace HubWeb.ViewModels
{
    public class RegistrationVM
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        // This property is required when the Guest user Register after trying the system in Guest mode
        // In Process registration this property is available 
        // Based on the value of this property it is identifyed whether the user is registration after trying 
        // the system or the user is directly registering into the system.
        public string GuestUserTempEmail { get; set; }

        [Display(Name = "Set up an Organization")]
        public bool HasOrganization { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 3)]
        [Display(Name = "Organization Name")]
        public string OrganizationName { get; set; }
        public string AnonimousId { get; set; }
    }
}