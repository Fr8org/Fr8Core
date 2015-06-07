using System.ComponentModel.DataAnnotations;

namespace KwasantWeb.ViewModels
{
    public class MyAccountVM
    {
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
