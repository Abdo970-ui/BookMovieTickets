using System.ComponentModel.DataAnnotations;

namespace BookMovieTickets.ViewModel
{
    public class ApplicationUserVM
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }
        public IFormFile ProfileImage { get; set; }

        public string ImageUrl { get; set; }


    }
}
