using System.Diagnostics;
using FluentAssertions;
using PlantBasedPizza.FitnessFunctions.Drivers;
using Xunit;

namespace PlantBasedPizza.FitnessFunctions;

public class PerformanceTests
{
    [Fact]
    public async Task RecipeApiPerformance_ShouldRespondInside300ms()
    {
        var performanceResults = new List<long>();

        for (var x = 0; x <50; x++)
        {
            var recipeDriver = new RecipeDriver();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var response = await recipeDriver.All();
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            performanceResults.Add(elapsedMilliseconds);
        }

        var averageMilliseconds = performanceResults.Average();
        var p90Milliseconds = performanceResults.OrderBy(x => x).ElementAt((int)(performanceResults.Count * 0.9));

        averageMilliseconds.Should().BeLessOrEqualTo(300, $"Average API response after 50 iterations took too long: {averageMilliseconds} ms");
        p90Milliseconds.Should().BeLessOrEqualTo(450);
    }
}