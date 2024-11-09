using Common.DTO;
using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PurchaseController : ControllerBase
    {
        private readonly ITransactionCoordinator _transactionCoordinator;
        private readonly IBank _bankService;
        private readonly IBookstore _bookstoreService;
        private readonly IValidation _validationService;

        public PurchaseController()
        {
            _transactionCoordinator = ServiceProxy.Create<ITransactionCoordinator>(new Uri("fabric:/Cloud/TransactionCoordinatorService")); //nema particija
            _bankService = ServiceProxy.Create<IBank>(new Uri("fabric:/Cloud/BankService"), new ServicePartitionKey(0), TargetReplicaSelector.Default); //ima particija
            _bookstoreService = ServiceProxy.Create<IBookstore>(new Uri("fabric:/Cloud/BookstoreService"), new ServicePartitionKey(0), TargetReplicaSelector.Default);
            _validationService = ServiceProxy.Create<IValidation>(new Uri("fabric:/Cloud/ValidationService"));
        }

        // Vraća listu dostupnih korisnika
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> ListClients()
        {
            var clients = await _bankService.ListClients();
            return Ok(clients);
        }

        // Vraća listu dostupnih proizvoda (knjiga)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> ListBooks()
        {
            var books = await _bookstoreService.ListAvailableItems();
            return Ok(books);
        }

        // Obradjuje zahtev za kupovinu
        [HttpPost]
        public async Task<IActionResult> Buy([FromBody] PurchaseRequestDto purchaseRequest)
        {
            try
            {
                bool success = await _validationService.Validate(purchaseRequest.UserId, purchaseRequest.BookId, (uint)purchaseRequest.Quantity, purchaseRequest.PricePerPC);
                return success ? Ok("Purchase successful") : BadRequest("Purchase failed. Please check your balance or stock availability.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
