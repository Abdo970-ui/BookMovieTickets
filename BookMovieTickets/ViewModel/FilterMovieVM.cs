using BookMovieTickets.Models;

namespace BookMovieTickets.ViewModel
{
    public class FilterMovieVM
    {
        public string? MovieName { get; set; }

        public int? CategoryId { get; set; }

        public int? CinemaId { get; set; }

        public bool? IsExciting { get; set; }

        public DateTime? Date { get; set; }   // 🔥 مهم للـ ShowTime

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 8;
        public int TotalPages { get; set; }

        // 🔥 النتائج
        public List<Movie> Movies { get; set; } = new();
    }
}
