﻿using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface ITransactionCoordinator : IService
    {
        [OperationContract]
        Task<bool> BuyBook(string clientId, string productId, uint quantity, double unitPrice);
    }
}