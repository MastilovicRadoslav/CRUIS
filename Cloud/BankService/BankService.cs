using Common.Interfaces;
using Common.Models;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace BankService
{
    internal sealed class BankService : StatefulService, IBank
    {
        private IReliableDictionary<string, Customer>? customerAccounts;
        private Dictionary<string, Customer> transactionDictionary = new(); // Skladište za transakcije u obradi
        private Dictionary<string, Customer> pendingTransactions = new(); // Skladište za transakcije spremne za potvrdu

        public BankService(StatefulServiceContext context) : base(context) { }

        // Vraća sve klijente banke
        public async Task<IEnumerable<Customer>> ListClients()
        {
            using var trx = StateManager.CreateTransaction();
            var allCustomers = await customerAccounts.CreateEnumerableAsync(trx);
            var enumerator = allCustomers.GetAsyncEnumerator();
            List<Customer> clientList = new();

            while (await enumerator.MoveNextAsync(CancellationToken.None))
            {
                clientList.Add(enumerator.Current.Value);
            }
            return clientList;
        }

        // Dodaje zahtev za transakciju (kupovinu) sa određenim iznosom za klijenta
        public async Task EnlistMoneyTransfer(string clientId, double amount)
        {
            using var trx = StateManager.CreateTransaction();
            var customerResult = await customerAccounts.TryGetValueAsync(trx, clientId);

            if (customerResult.HasValue && customerResult.Value.AccountBalance >= amount)
            {
                var transaction = new Customer(customerResult.Value.FullName, amount)
                {
                    ClientId = clientId
                };
                transactionDictionary[clientId] = transaction; // Dodajemo transakciju u `transactionDictionary`
            }
        }

        // Priprema transakciju (kupovinu) tako što premesti stavke iz transactionDictionary u pendingTransactions
        public async Task<bool> Prepare()
        {
            using var trx = StateManager.CreateTransaction();
            foreach (var transaction in transactionDictionary)
            {
                pendingTransactions[transaction.Key] = transaction.Value; // Premesti sve u pendingTransactions
            }
            transactionDictionary.Clear(); // Očisti transactionDictionary nakon premene
            await Task.Delay(0); // Simulacija pripreme transakcije
            return true;
        }

        // Izvršava transakciju (kupovinu) i ažurira stanje korisnika
        public async Task<bool> Commit()
        {
            if (customerAccounts is not null)
            {
                using var trx = StateManager.CreateTransaction();
                foreach (var transaction in pendingTransactions)
                {
                    var customerResult = await customerAccounts.TryGetValueAsync(trx, transaction.Key);
                    if (customerResult.HasValue)
                    {
                        customerResult.Value.AccountBalance -= transaction.Value.AccountBalance;
                        await customerAccounts.SetAsync(trx, transaction.Key, customerResult.Value);
                    }
                }
                await trx.CommitAsync();
                pendingTransactions.Clear(); // Očisti pendingTransactions nakon commit-a
                return true;
            }
            return false;
        }

        // Vraća transakciju (kupovinu) uklanjanjem iz pendingTransactions ako dođe do greške
        public async Task Rollback()
        {
            pendingTransactions.Clear(); // Očisti sve ako dođe do greške
            await Task.Delay(0);
        }

        // Glavna metoda servisa koja se izvršava kada servis postane primarni i ima status za pisanje
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            customerAccounts = await StateManager.GetOrAddAsync<IReliableDictionary<string, Customer>>("customerAccounts");
            using var trx = StateManager.CreateTransaction();

            var client1 = new Customer("Petar Petrovic", new Random().NextDouble() * 1000);
            var client2 = new Customer("Ivana Ivanovic", new Random().NextDouble() * 1000);

            await customerAccounts.TryAddAsync(trx, client1.ClientId, client1);
            await customerAccounts.TryAddAsync(trx, client2.ClientId, client2);

            await trx.CommitAsync();
        }

        // Podesava slušaoce za obradu zahteva
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }
    }
}
