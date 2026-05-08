using System.ComponentModel.DataAnnotations.Schema;

namespace BookMovieTickets.Models
{
    public class ApplicationUserOtp
    {
        public Guid Id { get; set; }

        public string ApplicationUserId { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser ApplicationUser { get; set; }

        public string Code { get; set; }

        public bool IsValid { get; set; }

        public DateTime ValidTo { get; set; }

        public DateTime CreatedAt { get; set; }

        public ApplicationUserOtp()
        {
        }

        public ApplicationUserOtp(string applicationUserId, string code)
        {
            Id = Guid.NewGuid();
            ApplicationUserId = applicationUserId;
            Code = code;
            IsValid = true;
            ValidTo = DateTime.UtcNow.AddMinutes(5);
            CreatedAt = DateTime.UtcNow;
        }
    }
}

