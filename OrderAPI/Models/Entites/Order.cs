using OrderAPI.Models.Enums;

namespace OrderAPI.Models.Entites
{
    public class Order
    {
        public int OrderId { get; set; }
        public int BuyerId { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public OrderStatusType OrderStatu { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal TotalPrice { get; set; }

    }
}
