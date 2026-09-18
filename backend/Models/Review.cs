namespace Resturant_Backend.Models
{
    public class Review
    {

        public int Id { get; set; }

        public string AppuserId { get; set; }
        public Appuser Appuser { get; set; }


        public int ProductId { get; set; }

        public Product Product { get; set; }


        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;



    }


}
