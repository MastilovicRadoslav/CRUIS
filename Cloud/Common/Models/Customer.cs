namespace Common.Models
{
    public class Customer
    {
        public string ClientId { get; set; } //Kupcov ID za identifikaciju
        public string FullName { get; set; } //Ime kupca
        public double AccountBalance { get; set; } //Stanje na računu kupca

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
