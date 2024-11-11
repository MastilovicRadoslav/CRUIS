using Common.DTOs;
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
        private IReliableDictionary<string, Book>? library; //IReliable baza za knjige
        private Dictionary<string, OrderRequestDto> reservationDictionary = new(); // Skladište za rezervacije knjiga za kupovinu
        private Dictionary<string, OrderRequestDto> pendingReservations = new(); // Skladište za rezervacijeknjiga koje čekaju potvrdu kupovine

        public BookstoreService(StatefulServiceContext context) : base(context) { }

        // Vraća listu dostupnih proizvoda (knjiga) u inventaru
        public async Task<IEnumerable<Book>> ListAvailableItems()
        {
            using var trx = StateManager.CreateTransaction();
            var allProducts = await library.CreateEnumerableAsync(trx);
            var enumerator = allProducts.GetAsyncEnumerator();
            List<Book> availableProducts = new();

            while (await enumerator.MoveNextAsync(CancellationToken.None))
            {
                if (enumerator.Current.Value.Quantity > 0) // Ako postoje knjige u biblioteci
                {
                    availableProducts.Add(enumerator.Current.Value); // Dodaj knjige koje su na stanju
                }
            }
            return availableProducts;
        }

        // Dodaje rezervaciju za određenu knjigu
        public async Task EnlistPurchase(string productId, uint quantity)
        {
            using var trx = StateManager.CreateTransaction();
            var productResult = await library.TryGetValueAsync(trx, productId);
            //Ako postoji i ako je broj zahtijevanih knjiga za kupovinu manji od broja knjiga kojih ima u biblioteci
            if (productResult.HasValue && productResult.Value.Quantity >= quantity)
            {
                var reservation = new OrderRequestDto(productResult.Value.BookId, quantity);
                reservationDictionary[productId] = reservation; // Dodajemo rezervaciju u reservationDictionary
            }
        }

        // Vraća cenu određenog proizvoda na osnovu ID-ja
        public async Task<double> GetItemPrice(string productId)
        {
            using var trx = StateManager.CreateTransaction();
            var productResult = await library.TryGetValueAsync(trx, productId);
            return productResult.HasValue ? productResult.Value.UnitPrice : 0;
        }

        // Priprema kupovinu tako što premesti stavke iz reservationDictionary u pendingReservations
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
            if (library is not null)
            {
                using var trx = StateManager.CreateTransaction();
                foreach (var reservation in pendingReservations)
                {
                    var productResult = await library.TryGetValueAsync(trx, reservation.Key);
                    if (productResult.HasValue)
                    {
                        productResult.Value.Quantity -= reservation.Value.Quantity; // Ažuriraj količinu proizvoda
                        await library.SetAsync(trx, reservation.Key, productResult.Value);
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
            library = await StateManager.GetOrAddAsync<IReliableDictionary<string, Book>>("productInventory");
            using var trx = StateManager.CreateTransaction();

            for (int i = 1; i <= 7; i++)
            {
                var product = new Book($"Knjiga {i}", (uint)new Random().Next(1, 20), new Random().NextDouble() * 50);
                await library.TryAddAsync(trx, product.BookId, product);
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
