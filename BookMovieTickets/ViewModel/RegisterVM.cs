using System.ComponentModel.DataAnnotations;

namespace BookMovieTickets.ViewModel
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }

        //[Required(ErrorMessage = "Password is required")]
        ////[DataType(DataType.Password)]
        //[MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        //[Required(ErrorMessage = "Confirm Password is required")]
        ////[DataType(DataType.Password)]
        //[Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}