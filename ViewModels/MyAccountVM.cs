using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels
{
    public class MyAccountVM
    {
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
