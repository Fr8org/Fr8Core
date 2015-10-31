using System.ComponentModel.DataAnnotations;

namespace HubWeb.ViewModels
{
    public class MyAccountVM
    {
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
