﻿using System.ComponentModel.DataAnnotations;

namespace _1.PresentationLayer.ViewModels
{
    public class LoginViewModel
    {
        // Vanligt för både login och register
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
