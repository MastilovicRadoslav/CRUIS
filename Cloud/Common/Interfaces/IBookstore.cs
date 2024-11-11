using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IBookstore : IService, ITransaction
    {
        [OperationContract]
        Task<IEnumerable<Book>> ListAvailableItems();

        [OperationContract]
        Task EnlistPurchase(string productId, uint quantity);

        [OperationContract]
        Task<double> GetItemPrice(string productId);
    }
}
