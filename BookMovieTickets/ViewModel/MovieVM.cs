using BookMovieTickets.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BookMovieTickets.ViewModels
{
    public class MovieVM
    {
      
        public string Name { get; set; }
        public int Id{ get; set; }

        public string? Description { get; set; }

      
        public IFormFile? MainImg { get; set; }

        public List<IFormFile>? SubImages { get; set; }

        public decimal Price { get; set; }
        public bool Status { get; set; }
        public DateTime DateTime { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int CinemaId { get; set; }

        public List<int> ActorIds { get; set; } = [];
        // Dropdown Data
        public List<Category> Categories { get; set; } = new();
        public List<Cinema> Cinemas { get; set; } = new();
        public List<Actor> Actors { get; set; } = new();
       

    }
}