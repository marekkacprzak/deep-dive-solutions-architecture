using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Kitchen.Core;
using PlantBasedPizza.Kitchen.Core.Services; 
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Kitchen.Infrastructure.Controllers
{
    [Route("kitchen")]
    public class KitchenController(
        IKitchenDomainService domainService,
        ILogger<KitchenController> logger)
        : ControllerBase
    {
        /// <summary>
        /// Get a list of all new kitchen requests.
        /// </summary>
        /// <returns></returns>
        [HttpGet("new")]
        public async Task<IEnumerable<KitchenRequestDTO>> GetNew()
        {
            try
            {
                return await domainService.GetNewRequestsAsync();
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
        public async Task<KitchenRequestDTO> Preparing(string orderIdentifier)
        {
            logger.LogInformation("Received request to prepare order");

            var kitchenRequest = await domainService.StartPreparingAsync(orderIdentifier);

            return kitchenRequest;
        }

        /// <summary>
        /// List all orders that are currently being prepared.
        /// </summary>
        /// <returns></returns>
        [HttpGet("prep")]
        public async Task<IEnumerable<KitchenRequestDTO>> GetPrep()
        {
            try
            {
                return await domainService.GetPreparingRequestAsync();
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
        public async Task<KitchenRequestDTO> PrepComplete(string orderIdentifier)
        {
            var request = await domainService.CompletePreparationAsync(orderIdentifier);
            
            return request;
        }

        /// <summary>
        /// List all orders currently baking.
        /// </summary>
        /// <returns></returns>
        [HttpGet("baking")]
        public async Task<IEnumerable<KitchenRequestDTO>> GetBaking()
        {
            try
            {
                return await domainService.GetBakingAsync();
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
        public async Task<KitchenRequestDTO> BakeComplete(string orderIdentifier)
        {
            return await domainService.CompleteBakingAsync(orderIdentifier);
        }

        /// <summary>
        /// Mark an order as quality check completed.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpPut("{orderIdentifier}/quality-check")]
        public async Task<KitchenRequestDTO> QualityCheckComplete(string orderIdentifier)
        {
            return await domainService.CompleteQualityCheckAsync(orderIdentifier);
        }

        /// <summary>
        /// List all orders awaiting quality check.
        /// </summary>
        /// <returns></returns>
        [HttpGet("quality-check")]
        public async Task<IEnumerable<KitchenRequestDTO>> GetAwaitingQualityCheck()
        {
            try
            {
                return await domainService.GetAwaitingQualityCheckAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing");
                return new List<KitchenRequestDTO>();
            }
        }
    }
}