using System.ComponentModel.DataAnnotations;

namespace BookMovieTickets.ViewModel
{
    public class ValidateOtpVM
    {
        [Required]
        public string UserId { get; set; }

        [Required(ErrorMessage = "OTP is required")]
        [StringLength(6, MinimumLength = 4, ErrorMessage = "OTP must be 4-6 digits")]
        public string OTP { get; set; }
    }
}