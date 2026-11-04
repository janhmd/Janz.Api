namespace Service.Helpers.DTOs.Community
{
    public class CommunityStatsDto
    {
        public DateTime MemberSince { get; set; }
        public int Orders { get; set; }
        public int TriedCoffees { get; set; }
        public double AverageRating { get; set; }
        public int Posts { get; set; }
        public int Comments { get; set; }
        public int LikesReceived { get; set; }
    }
}
