namespace Common.DTO
{
    public class PurchaseRequestDto
    {
        public string UserId { get; set; }
        public string BookId { get; set; }
        public int Quantity { get; set; }
        public double PricePerPC { get; set; }
    }
}
