namespace Common.DTOs
{
    public class AccountAmountDto
    {
        public string ClientId { get; set; } //Kupcov ID za identifikaciju
        public double AccountBalance { get; set; } //Stanje na računu kupca

        public AccountAmountDto() { }
        public AccountAmountDto(string clientId, double balance)
        {
            ClientId = clientId;
            AccountBalance = balance;
        }
    }
}
