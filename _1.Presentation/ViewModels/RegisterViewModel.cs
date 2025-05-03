using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using __Cross_cutting_Concerns.SiteAttributes;

namespace _1.PresentationLayer.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [RegularExpression(@"^[A-Za-z]{2,}$", ErrorMessage = "Username must contain at least 2 letters and no special characters or numbers")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{6,}$",
            ErrorMessage = "Password must be at least 6 characters long, contain one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }

        [TermsMustBeTrue(ErrorMessage = "You must accept the terms and conditions")]
        public bool AcceptTerms { get; set; }



    }
}
