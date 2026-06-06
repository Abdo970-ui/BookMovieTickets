namespace BookMovieTickets.ViewModel
{
    public class AdminDashboardVM
    {
        public int TotalUsers { get; set; }

        public int TotalMovies { get; set; }

        public int TotalBookings { get; set; }

        public decimal TotalRevenue { get; set; }

        public string MostBookedMovie { get; set; } = string.Empty;

        public List<string> MovieNames { get; set; } = new();

        public List<int> BookingCounts { get; set; } = new();
    }
}