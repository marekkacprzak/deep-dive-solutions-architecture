using System;
using System.Threading.Tasks;
using FluentAssertions;
using PlantBasedPizza.IntegrationTests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.IntegrationTests.Steps
{
    [Binding]
    public sealed class DeliveryStepDefinitions(ScenarioContext scenarioContext)
    {
        private readonly DeliveryDriver _driver = new();

        [Then(@"order (.*) should be awaiting delivery collection")]
        public async Task ThenOrderDeliverShouldBeAwaitingDeliveryCollection(string p0)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            var ordersAwaitingDriver = await this._driver.GetAwaitingDriver();

            ordersAwaitingDriver.Exists(p => p.OrderIdentifier == orderIdentifier).Should().BeTrue();
        }

        [When(@"order (.*) is assigned to a driver named (.*)")]
        public async Task WhenOrderDeliverIsAssignedToADriverNamedJames(string p0, string p1)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            await this._driver.AssignDriver(orderIdentifier, p1);
        }

        [Then(@"order (.*) should appear in a list of (.*) deliveries")]
        public async Task ThenOrderDeliverShouldAppearInAListOfJamesDeliveries(string p0, string p1)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            
            var ordersForDriver = await this._driver.GetAssignedDeliveriesForDriver(p1);

            ordersForDriver.Exists(p => p.OrderIdentifier == orderIdentifier).Should().BeTrue();
        }

        [When(@"order (.*) is delivered")]
        public async Task WhenOrderDeliverIsDelivered(string p0)
        {
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            await this._driver.DeliverOrder(orderIdentifier);
        }

        [Then(@"order (.*) should no longer be assigned to a driver named (.*)")]
        public async Task ThenOrderDeliverShouldNoLongerBeAssignedToADriverNamedJames(string p0, string p1)
        {
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            var ordersForDriver = await this._driver.GetAssignedDeliveriesForDriver(p1);

            ordersForDriver.Exists(p => p.OrderIdentifier == orderIdentifier).Should().BeFalse();
        }
    }
}