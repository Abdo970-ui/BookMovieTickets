namespace BookMovieTickets.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public int ShowTimeId { get; set; }
        public ShowTime ShowTime { get; set; }

        // 👇 User Relation
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public int Tickets { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // العلاقة مع المقاعد
        public ICollection<Seat> Seats { get; set; }
            = new List<Seat>();
    }
}