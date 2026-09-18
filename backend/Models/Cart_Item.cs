namespace Resturant_Backend.Models
{
    public class Cart_Item
    {

        public int Id { get; set; }

        public string AppuserId { get; set; }

        public Appuser Appuser { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; }

        public int Quantity { get; set; }



    }
}
