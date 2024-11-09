﻿using Common.DTOs;
using Common.Interfaces;
using Common.Models;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace BookstoreService
{
    internal sealed class BookstoreService : StatefulService, IBookstore
    {
        private IReliableDictionary<string, Product>? productInventory;
        private Dictionary<string, OrderRequestDto> reservationDictionary = new(); // Skladište za rezervacije
        private Dictionary<string, OrderRequestDto> pendingReservations = new(); // Skladište za rezervacije koje čekaju potvrdu

        public BookstoreService(StatefulServiceContext context) : base(context) { }

        // Vraća listu dostupnih proizvoda (knjiga) u inventaru
        public async Task<IEnumerable<Product>> ListAvailableItems()
        {
            using var trx = StateManager.CreateTransaction();
            var allProducts = await productInventory.CreateEnumerableAsync(trx);
            var enumerator = allProducts.GetAsyncEnumerator();
            List<Product> availableProducts = new();

            while (await enumerator.MoveNextAsync(CancellationToken.None))
            {
                if (enumerator.Current.Value.StockQuantity > 0)
                {
                    availableProducts.Add(enumerator.Current.Value); // Dodaj proizvode koji su na stanju
                }
            }
            return availableProducts;
        }

        // Dodaje rezervaciju za određeni proizvod
        public async Task EnlistPurchase(string productId, uint quantity)
        {
            using var trx = StateManager.CreateTransaction();
            var productResult = await productInventory.TryGetValueAsync(trx, productId);

            if (productResult.HasValue && productResult.Value.StockQuantity >= quantity)
            {
                var reservation = new OrderRequestDto(productResult.Value.ProductId, quantity);
                reservationDictionary[productId] = reservation; // Dodajemo rezervaciju u reservationDictionary
            }
        }

        // Vraća cenu određenog proizvoda na osnovu ID-ja
        public async Task<double> GetItemPrice(string productId)
        {
            using var trx = StateManager.CreateTransaction();
            var productResult = await productInventory.TryGetValueAsync(trx, productId);
            return productResult.HasValue ? productResult.Value.UnitPrice : 0;
        }

        // Priprema rezervaciju (kupovinu) tako što premesti stavke iz reservationDictionary u pendingReservations
        public async Task<bool> Prepare()
        {
            using var trx = StateManager.CreateTransaction();
            foreach (var reservation in reservationDictionary)
            {
                pendingReservations[reservation.Key] = reservation.Value; // Premesti sve u pendingReservations
            }
            reservationDictionary.Clear(); // Očisti reservationDictionary nakon premene
            await Task.Delay(0); // Simulacija pripreme transakcije
            return true;
        }

        // Izvršava rezervaciju(kupovina) i ažurira količinu proizvoda na stanju
        public async Task<bool> Commit()
        {
            if (productInventory is not null)
            {
                using var trx = StateManager.CreateTransaction();
                foreach (var reservation in pendingReservations)
                {
                    var productResult = await productInventory.TryGetValueAsync(trx, reservation.Key);
                    if (productResult.HasValue)
                    {
                        productResult.Value.StockQuantity -= reservation.Value.Quantity; // Ažuriraj količinu proizvoda
                        await productInventory.SetAsync(trx, reservation.Key, productResult.Value);
                    }
                }
                await trx.CommitAsync();
                pendingReservations.Clear(); // Očisti pendingReservations nakon commit-a
                return true;
            }
            return false;
        }

        // Vraća rezervaciju (kupovinu) uklanjanjem iz pendingReservations ako dođe do greške
        public async Task Rollback()
        {
            pendingReservations.Clear(); // Očisti sve ako dođe do greške
            await Task.Delay(0);
        }

        // Inicijalizuje kolekciju proizvoda sa početnim podacima
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            productInventory = await StateManager.GetOrAddAsync<IReliableDictionary<string, Product>>("productInventory");
            using var trx = StateManager.CreateTransaction();

            for (int i = 1; i <= 5; i++)
            {
                var product = new Product($"Proizvod {i}", (uint)new Random().Next(1, 20), new Random().NextDouble() * 50);
                await productInventory.TryAddAsync(trx, product.ProductId, product);
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