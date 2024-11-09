using Common.Interfaces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace TransactionCoordinatorService
{
    internal sealed class TransactionCoordinatorService : StatelessService, ITransactionCoordinator
    {
        private readonly IBank bank;
        private readonly IBookstore store;

        public TransactionCoordinatorService(StatelessServiceContext context) : base(context)
        {
            bank = ServiceProxy.Create<IBank>(new Uri("fabric:/Cloud/BankService"), new ServicePartitionKey(0), TargetReplicaSelector.Default);
            store = ServiceProxy.Create<IBookstore>(new Uri("fabric:/Cloud/BookstoreService"), new ServicePartitionKey(0), TargetReplicaSelector.Default);
        }

        // Koordinira kupovinu proizvoda
        public async Task<bool> BuyBook(string clientId, string productId, uint quantity, double unitPrice)
        {
            try
            {
                // Rezerviše proizvod i sredstva
                await store.EnlistPurchase(productId, quantity);
                await bank.EnlistMoneyTransfer(clientId, quantity * unitPrice);

                // Priprema obe strane transakcije
                if (await store.Prepare() && await bank.Prepare())
                {
                    if (await store.Commit())
                    {
                        if (await bank.Commit())
                            return true;

                        await store.Rollback();
                        await bank.Rollback();
                        return false;
                    }
                }

                await store.Rollback();
                await bank.Rollback();

                return false;
            }
            catch
            {
                return false;
            }
        }

        // Podešavanje slušalaca za obradu zahteva
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }
    }
}
