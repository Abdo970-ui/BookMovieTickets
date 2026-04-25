namespace BookMovieTickets.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string MainImg { get; set; }
        public decimal Price { get; set; }
        public bool Status { get; set; }
        public DateTime DateTime { get; set; }
        public List<Actor> Actors { get; set; } = new();

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int CinemaId { get; set; }
        public Cinema Cinema { get; set; }
        public List<MovieImage> SubImages { get; set; } = new();
    }
}
