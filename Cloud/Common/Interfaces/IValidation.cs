using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IValidation : IService
    {
        [OperationContract]
        Task<bool> Validate(string clientId, string productId, uint quantity, double unitPrice);

        [OperationContract]
        Task<IEnumerable<Customer>> ListClients();

        [OperationContract]
        Task<IEnumerable<Book>> ListBooks();
    }
}
