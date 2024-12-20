﻿using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using Common.Interfaces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Validation
{
    internal sealed class ValidationService : StatelessService, IValidation
    {
        private readonly ITransactionCoordinator coordinator;

        public ValidationService(StatelessServiceContext context) : base(context)
        {
            coordinator = ServiceProxy.Create<ITransactionCoordinator>(new Uri("fabric:/Cloud/TransactionCoordinatorService"));
        }

        // Validira podatke o transakciji i prosleđuje zahtev TransactionCoordinator-u
        public async Task<bool> Validate(string clientId, string productId, uint quantity, double unitPrice)
        {
            try
            {
                // Provera osnovne validnosti podataka
                if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(productId) || quantity < 1 || unitPrice < 0)
                    return false;

                // Prosleđuje zahtev TransactionCoordinator servisu
                return await coordinator.BuyBook(clientId, productId, quantity, unitPrice);
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
