using Common.DTOs;
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
        private IReliableDictionary<string, Customer>? customerAccounts; // IReliable za smeštanje kupaca (ID, Ime kupca)
        private Dictionary<string, AccountAmountDto> transactionDictionary = new(); // Skladište za kupovine u obradi
        private Dictionary<string, AccountAmountDto> pendingTransactions = new(); // Skladište za kupovine spremne za potvrdu

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

        // Dodaje zahtev za kupovinu sa određenim iznosom za kupca
        public async Task EnlistMoneyTransfer(string clientId, double amount)
        {
            using var trx = StateManager.CreateTransaction();
            var customerResult = await customerAccounts.TryGetValueAsync(trx, clientId);
            //HasValue = true ako kupac postoji u bazi, takodje ne mogu da kupim nesto za sta nemam pare
            if (customerResult.HasValue && customerResult.Value.AccountBalance >= amount)
            {
                var transaction = new AccountAmountDto(customerResult.Value.ClientId, amount);
                transactionDictionary[clientId] = transaction; // transactionDictionary sada ima kupca iz baze sa vrijednosti AccountBalance od amount 
            }
        }

        // Priprema kupovinu tako što premesti stavke iz transactionDictionary u pendingTransactions
        public async Task<bool> Prepare()
        {
            using var trx = StateManager.CreateTransaction();
            foreach (var transaction in transactionDictionary)
            {
                pendingTransactions[transaction.Key] = transaction.Value; // Premesti sve u pendingTransactions --> spremna za potvrdu
            }
            transactionDictionary.Clear(); // Očisti transactionDictionary nakon promene
            await Task.Delay(0); // Simulacija pripreme transakcije
            return true;
        }

        // Izvršava kupovinu i ažurira stanje korisnika
        public async Task<bool> Commit()
        {
            if (customerAccounts is not null)
            {
                using var trx = StateManager.CreateTransaction();
                foreach (var transaction in pendingTransactions)
                {
                    var customerResult = await customerAccounts.TryGetValueAsync(trx, transaction.Key); //preuizmanje podataka o korisniku na osnovu ID
                    if (customerResult.HasValue) //true
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

        // Vraća kupovinu uklanjanjem iz pendingTransactions ako dođe do greške
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

            // Proveri da li već postoje korisnici kako bi se izbeglo ponovno dodavanje
            var customersExist = await customerAccounts.GetCountAsync(trx) > 0;
            if (!customersExist)
            {
                var client1 = new Customer("Vladimir Mandic", 1000.0);
                var client2 = new Customer("Radoslav Mastilovic", 1500.0);

                await customerAccounts.TryAddAsync(trx, client1.ClientId, client1);
                await customerAccounts.TryAddAsync(trx, client2.ClientId, client2);
            }

            await trx.CommitAsync();
        }

        // Podesava slušaoce za obradu zahteva
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }
    }
}
