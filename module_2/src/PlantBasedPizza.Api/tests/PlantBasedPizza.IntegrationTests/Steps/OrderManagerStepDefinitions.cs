using System.Threading.Tasks;
using FluentAssertions;
using PlantBasedPizza.IntegrationTests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.IntegrationTests.Steps
{
    [Binding]
    public sealed class OrderManagerStepDefinitions(ScenarioContext scenarioContext)
    {
        private readonly OrderManagerDriver _driver = new();

        [Given(@"a new order is created with identifier (.*)")]
        public async Task GivenANewOrderIsCreatedWithIdentifierOrd(string p0)
        {
            var order = await this._driver.AddNewOrder().ConfigureAwait(false);

            order.OrderIdentifier.Should().NotBeNullOrEmpty();
            
            scenarioContext.Add("orderIdentifier", order.OrderIdentifier);
        }

        [When(@"a (.*) is added to order (.*)")]
        public async Task WhenAnItemIsAdded(string p0, string p1)
        {
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            await this._driver.AddItemToOrder(orderIdentifier, p0, 1);
        }

        [Then(@"there should be (.*) item on the order with identifier (.*)")]
        public async Task ThenThereShouldBeItemOnTheOrder(int p0, string p1)
        {
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            var order = await this._driver.GetOrder(orderIdentifier);

            order.Items.Count.Should().Be(p0);
        }

        [When(@"order (.*) is submitted")]
        public async Task WhenOrderOrdIsSubmitted(string p0)
        {
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            await this._driver.SubmitOrder(orderIdentifier);
        }

        [Then(@"order (.*) should be marked as (.*)")]
        public async Task ThenOrderOrdShouldBeMarkedAsCompleted(string p0, string p1)
        {
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            var order = await this._driver.GetOrder(orderIdentifier).ConfigureAwait(false);

            order.OrderCompletedOn.Should().NotBeNull();
        }

        [Then(@"order (.*) should contain a (.*) event")]
        public async Task ThenOrderOrdShouldContainAOrderQualityCheckedEvent(string p0, string p1)
        {
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            var order = await this._driver.GetOrder(orderIdentifier).ConfigureAwait(false);

            order.History.Exists(p => p.Description == p1).Should().BeTrue();
        }

        [Then(@"order (.*) should be awaiting collection")]
        public async Task ThenOrderOrdShouldBeAwaitingCollection(string p0)
        {
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            var order = await this._driver.GetOrder(orderIdentifier).ConfigureAwait(false);

            order.AwaitingCollection.Should().BeTrue();
        }

        [When(@"order (.*) is collected")]
        public async Task WhenOrderOrdIsCollected(string p0)
        {
            var orderIdentifier = scenarioContext.Get<string>("orderIdentifier");
            await this._driver.CollectOrder(orderIdentifier).ConfigureAwait(false);
        }

        [Given(@"a new delivery order is created with identifier (.*)")]
        public async Task GivenANewDeliveryOrderIsCreatedWithIdentifierDeliver(string p0)
        {
            var order = await this._driver.AddNewDeliveryOrder();
            
            order.OrderIdentifier.Should().NotBeNullOrEmpty();
            
            scenarioContext.Add("orderIdentifier", order.OrderIdentifier);
        }
    }
}