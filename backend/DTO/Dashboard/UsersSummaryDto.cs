namespace Resturant_Backend.DTO.Dashboard
{
    public class UsersSummaryDto
    {
        public int TotalUsers { get; set; }
        public int Admins { get; set; }
        public int Managers { get; set; }
        public int RegularUsers { get; set; }
    }
}
