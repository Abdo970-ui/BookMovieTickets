using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BookMovieTickets.Models
{
    [Index(nameof(Code), IsUnique = true)]
    public class Promotion
    {
        public int Id { get; set; }

        [Required]
        public string Code { get; set; } 

        public int MaxUsage { get; set; } 

        public decimal Discount { get; set; } 

        public bool IsValid { get; set; } 

        public DateTime ValidTo { get; set; } 

        
        public int? MovieId { get; set; }  
        public Movie Movie { get; set; }
    }
}