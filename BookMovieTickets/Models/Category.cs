namespace BookMovieTickets.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Status { get; set; }

        public List<Movie> Movies { get; set; } = new();
    }
}
