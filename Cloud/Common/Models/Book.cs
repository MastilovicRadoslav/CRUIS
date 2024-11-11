namespace Common.Models
{
    public class Book
    {
        public string BookId { get; set; }
        public string NameBook { get; set; }
        public uint Quantity { get; set; }
        public double UnitPrice { get; set; }

        public Book()
        {
            BookId = Guid.NewGuid().ToString();
            NameBook = string.Empty;
            Quantity = 0;
            UnitPrice = 0.0;
        }

        public Book(string name, uint quantity, double price)
        {
            BookId = Guid.NewGuid().ToString();
            NameBook = name;
            Quantity = quantity;
            UnitPrice = price;
        }
    }

}
