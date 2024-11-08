namespace Common.DTOs
{
    public class OrderRequestDto
    {
        public string ProductId { get; set; }
        public uint Quantity { get; set; }

        public OrderRequestDto() { }

        public OrderRequestDto(string productId, uint quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }
    }

}
