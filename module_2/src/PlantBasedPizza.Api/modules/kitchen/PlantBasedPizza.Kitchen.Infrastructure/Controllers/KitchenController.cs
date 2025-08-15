using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Infrastructure.DataTransfer;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Kitchen.Infrastructure.Controllers
{
    [Route("kitchen")]
    public class KitchenController(
        IKitchenRequestRepository kitchenRequestRepository,
        IDomainEventDispatcher domainEventDispatcher,
        ILogger<KitchenController> logger)
        : ControllerBase
    {
        /// <summary>
        /// Get a list of all new kitchen requests.
        /// </summary>
        /// <returns></returns>
        [HttpGet("new")]
        public IEnumerable<KitchenRequestDTO> GetNew()
        {
            try
            {
                var queryResults = kitchenRequestRepository.GetNew().Result;

                return queryResults.Select(p => new KitchenRequestDTO(p)).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing");
                return new List<KitchenRequestDTO>();
            }
        }

        /// <summary>
        /// Mark an order has being prepared.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpPut("{orderIdentifier}/preparing")]
        public async Task<KitchenRequest> Preparing(string orderIdentifier)
        {
            logger.LogInformation("Received request to prepare order");

            var kitchenRequest = kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.StartPreparing();

            kitchenRequestRepository.Update(kitchenRequest).Wait();
            
            await domainEventDispatcher.PublishAsync(new OrderPreparingEvent(kitchenRequest.OrderIdentifier));

            return kitchenRequest;
        }

        /// <summary>
        /// List all orders that are currently being prepared.
        /// </summary>
        /// <returns></returns>
        [HttpGet("prep")]
        public IEnumerable<KitchenRequestDTO> GetPrep()
        {
            try
            {
                var queryResults = kitchenRequestRepository.GetPrep().Result;

                return queryResults.Select(p => new KitchenRequestDTO(p)).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing");
                return new List<KitchenRequestDTO>();
            }
        }

        /// <summary>
        /// Mark an order as being prepared.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpPut("{orderIdentifier}/prep-complete")]
        public async Task<KitchenRequest> PrepComplete(string orderIdentifier)
        {
            var kitchenRequest = kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.CompletePreparing();

            kitchenRequestRepository.Update(kitchenRequest).Wait();
            
            await domainEventDispatcher.PublishAsync(new OrderPrepCompleteEvent(kitchenRequest.OrderIdentifier));

            return kitchenRequest;
        }

        /// <summary>
        /// List all orders currently baking.
        /// </summary>
        /// <returns></returns>
        [HttpGet("baking")]
        public IEnumerable<KitchenRequestDTO> GetBaking()
        {
            try
            {
                var queryResults = kitchenRequestRepository.GetBaking().Result;

                return queryResults.Select(p => new KitchenRequestDTO(p));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing");
                return new List<KitchenRequestDTO>();
            }
        }

        /// <summary>
        /// Mark an order as bake complete.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpPut("{orderIdentifier}/bake-complete")]
        public async Task<KitchenRequest> BakeComplete(string orderIdentifier)
        {
            var kitchenRequest = kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.CompleteBaking();

            kitchenRequestRepository.Update(kitchenRequest).Wait();
            
            await domainEventDispatcher.PublishAsync(new OrderBakedEvent(kitchenRequest.OrderIdentifier));

            return kitchenRequest;
        }

        /// <summary>
        /// Mark an order as quality check completed.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpPut("{orderIdentifier}/quality-check")]
        public async Task<KitchenRequest> QualityCheckComplete(string orderIdentifier)
        {
            var kitchenRequest = kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.CompleteQualityCheck();

            await kitchenRequestRepository.Update(kitchenRequest);
            
            await domainEventDispatcher.PublishAsync(new OrderQualityCheckedEvent(kitchenRequest.OrderIdentifier));

            return kitchenRequest;
        }

        /// <summary>
        /// List all orders awaiting quality check.
        /// </summary>
        /// <returns></returns>
        [HttpGet("quality-check")]
        public IEnumerable<KitchenRequestDTO> GetAwaitingQualityCheck()
        {
            try
            {
                var queryResults = kitchenRequestRepository.GetAwaitingQualityCheck().Result;

                return queryResults.Select(p => new KitchenRequestDTO(p));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing");
                return new List<KitchenRequestDTO>();
            }
        }
    }
}