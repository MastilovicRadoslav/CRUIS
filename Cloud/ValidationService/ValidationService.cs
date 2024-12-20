﻿using Common.Interfaces;
using Common.Models;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace ValidationService
{
    internal sealed class ValidationService : StatelessService, IValidation
    {
        private readonly IBank _bankService;
        private readonly IBookstore _bookstoreService;
        private readonly ITransactionCoordinator _transactionCoordinator;

        public ValidationService(StatelessServiceContext context) : base(context)
        {
            _bankService = ServiceProxy.Create<IBank>(new Uri("fabric:/Cloud/BankService"), new ServicePartitionKey(0), TargetReplicaSelector.Default);
            _bookstoreService = ServiceProxy.Create<IBookstore>(new Uri("fabric:/Cloud/BookstoreService"), new ServicePartitionKey(0), TargetReplicaSelector.Default);
            _transactionCoordinator = ServiceProxy.Create<ITransactionCoordinator>(new Uri("fabric:/Cloud/TransactionCoordinatorService"));
        }

        // Vraća listu dostupnih korisnika
        public async Task<IEnumerable<Customer>> ListClients()
        {
            var clients = await _bankService.ListClients();

            // Validacija: proverava da li lista nije prazna
            if (clients == null)
            {
                throw new Exception("Neuspelo preuzimanje klijenata: prazan odgovor od BankService.");
            }

            // Dodatna validacija za svaki klijent (opciono)
            foreach (var client in clients)
            {
                if (string.IsNullOrWhiteSpace(client.ClientId) || string.IsNullOrWhiteSpace(client.FullName) || client.AccountBalance < 0)
                {
                    throw new Exception($"Otkriveni su nevažeći podaci klijenta: {client.ClientId}, {client.FullName}");
                }
            }

            return clients;
        }

        // Vraća listu dostupnih proizvoda (knjiga)
        public async Task<IEnumerable<Book>> ListBooks()
        {
            var products = await _bookstoreService.ListAvailableItems();

            // Validacija: proverava da li lista nije prazna
            if (products == null)
            {
                throw new Exception("Nije moguće preuzeti proizvode: prazan odgovor iz biblioteke");
            }

            // Dodatna validacija za svaki proizvod (opciono)
            foreach (var product in products)
            {
                if (string.IsNullOrWhiteSpace(product.BookId) || string.IsNullOrWhiteSpace(product.NameBook) || product.UnitPrice < 0 || product.Quantity < 0)
                {
                    throw new Exception($"Otkriveni neispravni podaci o knjizi: {product.BookId}, {product.NameBook}");
                }
            }

            return products;
        }

        // Validira podatke o transakciji i prosleđuje zahtev TransactionCoordinator-u ako validacija uspe
        public async Task<bool> Validate(string clientId, string productId, uint quantity, double unitPrice)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(productId) || quantity < 1 || unitPrice < 0)
                {
                    return false;
                }
                return await _transactionCoordinator.BuyBook(clientId, productId, quantity, unitPrice);
            }
            catch
            {
                return false;
            }
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }
    }
}
