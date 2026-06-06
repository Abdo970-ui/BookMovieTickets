namespace BookMovieTickets.Models
{
    public class PromotionUsage
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int PromotionId { get; set; }
        public Promotion Promotion { get; set; }

    }
}
