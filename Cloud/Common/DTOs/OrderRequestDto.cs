namespace Common.DTOs
{
    public class OrderRequestDto
    {
        public string BookId { get; set; }
        public uint Quantity { get; set; }

        public OrderRequestDto() { }

        public OrderRequestDto(string productId, uint quantity)
        {
            BookId = productId;
            Quantity = quantity;
        }
    }

}
