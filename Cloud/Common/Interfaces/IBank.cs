using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IBank : IService, ITransaction
    {
        [OperationContract]
        Task<IEnumerable<Customer>> ListClients();        // Vraća sve klijente banke

        [OperationContract]
        Task EnlistMoneyTransfer(string clientId, double amount); // Dodaje zahtev za kupovinu sa određenim iznosom za kupca
    }
}
