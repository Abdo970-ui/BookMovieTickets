namespace BookMovieTickets.Models
{
    public class MovieImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }
    }
}
