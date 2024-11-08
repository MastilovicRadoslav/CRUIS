using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IBank : IService, ITransaction
    {
        [OperationContract]
        Task<IEnumerable<Customer>> ListClients();

        [OperationContract]
        Task EnlistMoneyTransfer(string clientId, double amount);
    }
}
