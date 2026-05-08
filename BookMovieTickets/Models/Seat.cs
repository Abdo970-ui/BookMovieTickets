namespace BookMovieTickets.Models
{
    public class Seat
    {
        public int Id { get; set; }

        public int ShowTimeId { get; set; }
        public ShowTime ShowTime { get; set; }

        public string SeatNumber { get; set; } // A1, A2, B1...

        public bool IsBooked { get; set; } = false;

        // Booking Relation
        public int? BookingId { get; set; }
        public Booking Booking { get; set; }
    }
}