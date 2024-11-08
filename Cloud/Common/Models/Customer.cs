namespace Common.Models
{
    public class Customer
    {
        public string ClientId { get; set; }
        public string FullName { get; set; }
        public double AccountBalance { get; set; }

        public Customer()
        {
            ClientId = Guid.NewGuid().ToString();
            FullName = string.Empty;
            AccountBalance = 0.0;
        }

        public Customer(string name, double balance)
        {
            ClientId = Guid.NewGuid().ToString();
            FullName = name;
            AccountBalance = balance;
        }
    }

}
