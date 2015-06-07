using System.ComponentModel.DataAnnotations;

namespace KwasantWeb.ViewModels
{
    public class ExternalLoginConfirmationVM
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }
}