using System;
using System.Threading.Tasks;
using FluentAssertions;
using IntegrationTests.Drivers;
using TechTalk.SpecFlow;
using Xunit;

namespace IntegrationTests.Steps
{
    [Binding]
    public sealed class StepDefinitions(ScenarioContext scenarioContext)
    {
        private readonly ApplicationDriver _driver = new();

        [GivenAttribute("a new entity is created with identifier {int}")]
        public void GivenANewEntityIsCreatedWithIdentifier(int p0)
        {
            // Implement step definition logic here
            scenarioContext["entityIdentifier"] = p0;
        }

        [WhenAttribute("a something else happens")]
        public void WhenASomethingElseHappens()
        {
            // Implement step definition logic here
            var identifier = scenarioContext["entityIdentifier"] as string;
            identifier.Should().NotBeNullOrEmpty();
        }

        [ThenAttribute("this thing should be true")]
        public void ThenThisThingShouldBeTrue()
        {
            Assert.Equal(true, true);
        }
    }
}