using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface ITransaction : IService
    {
        [OperationContract]
        Task<bool> Prepare(); // Priprema za izvrsavanje kupovine

        [OperationContract]
        Task<bool> Commit(); // Izvrsavanje kupovine

        [OperationContract]
        Task Rollback(); //Ponistavanje kupovine
    }
}
