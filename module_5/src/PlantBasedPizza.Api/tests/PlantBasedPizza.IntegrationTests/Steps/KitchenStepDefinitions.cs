using System;
using System.Threading.Tasks;
using FluentAssertions;
using PlantBasedPizza.IntegrationTests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.IntegrationTests.Steps
{
    [Binding]
    public sealed class KitchenStepDefinitions(ScenarioContext scenarioContext)
    {
        private readonly KitchenDriver _kitchenDriver = new();

        [Then(@"an order with identifier (.*) should be added to the new kitchen requests")]
        public async Task ThenAnOrderWithIdentifierOrdShouldBeAddedToTheNewKitchenRequests(string p0)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            var newKitchenRequests = await this._kitchenDriver.GetNew();

            newKitchenRequests.Exists(p => p.OrderIdentifier == orderIdentifier).Should().BeTrue();
        }

        [When(@"order (.*) is processed by the kitchen")]
        public async Task WhenOrderOrdIsProcessedByTheKitchen(string p0)
        {
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            await this._kitchenDriver.Preparing(orderIdentifier);
            await this._kitchenDriver.PrepComplete(orderIdentifier);
            await this._kitchenDriver.BakeComplete(orderIdentifier);
            await this._kitchenDriver.QualityChecked(orderIdentifier);
        }

        [When(@"order (.*) is marked as preparing")]
        public async Task WhenOrderOrdIsMarkedAsPreparing(string p0)
        {
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            await this._kitchenDriver.Preparing(orderIdentifier);
        }

        [When(@"order (.*) is marked as prep-complete")]
        public async Task WhenOrderOrdIsMarkedAsPrepComplete(string p0)
        {
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            await this._kitchenDriver.PrepComplete(orderIdentifier);
        }
        
        [When(@"order (.*) is marked as bake-complete")]
        public async Task WhenOrderOrdIsMarkedAsBakeComplete(string p0)
        {
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            await this._kitchenDriver.BakeComplete(orderIdentifier);
        }
        
        [When(@"order (.*) is marked as quality-checked")]
        public async Task WhenOrderOrdIsMarkedAsQualityChecked(string p0)
        {
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            await this._kitchenDriver.QualityChecked(orderIdentifier);
        }

        [Then(@"order (.*) should appear in the preparing queue")]
        public async Task ThenOrderOrdShouldAppearInThePreparingQueue(string p0)
        {
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            var requests = await this._kitchenDriver.GetPreparing();

            requests.Exists(p => p.OrderIdentifier == orderIdentifier).Should().BeTrue();
        }

        [Then(@"order (.*) should appear in the baking queue")]
        public async Task ThenOrderOrdShouldAppearInTheBakingQueueQueue(string p0)
        {
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            var requests = await this._kitchenDriver.GetBaking();

            requests.Exists(p => p.OrderIdentifier == orderIdentifier).Should().BeTrue();
        }

        [Then(@"order (.*) should appear in the quality check queue")]
        public async Task ThenOrderOrdShouldAppearInTheQualityCheckQueue(string p0)
        {
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            var requests = await this._kitchenDriver.GetQualityChecked();

            requests.Exists(p => p.OrderIdentifier == orderIdentifier).Should().BeTrue();
        }
    }
}