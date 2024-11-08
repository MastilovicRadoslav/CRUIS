namespace Common.Models
{
    public class Product
    {
        public string ProductId { get; set; }
        public string Name { get; set; }
        public uint StockQuantity { get; set; }
        public double UnitPrice { get; set; }

        public Product()
        {
            ProductId = Guid.NewGuid().ToString();
            Name = string.Empty;
            StockQuantity = 0;
            UnitPrice = 0.0;
        }

        public Product(string name, uint quantity, double price)
        {
            ProductId = Guid.NewGuid().ToString();
            Name = name;
            StockQuantity = quantity;
            UnitPrice = price;
        }
    }

}
