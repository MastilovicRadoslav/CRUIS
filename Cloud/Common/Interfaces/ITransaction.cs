using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface ITransaction : IService
    {
        [OperationContract]
        Task<bool> Prepare();

        [OperationContract]
        Task<bool> Commit();

        [OperationContract]
        Task Rollback();
    }
}
