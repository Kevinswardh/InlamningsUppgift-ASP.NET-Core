using System.ComponentModel.DataAnnotations;

namespace _1.PresentationLayer.ViewModels
{
    public class RegisterViewModel
    {
        // Vanligt för både login och register
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // Endast för register
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
    }
}
