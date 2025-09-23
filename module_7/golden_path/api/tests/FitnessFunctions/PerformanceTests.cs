using System.Diagnostics;
using FitnessFunctions.Drivers;
using FluentAssertions;
using Xunit;

namespace FitnessFunctions;

public class PerformanceTests
{
    [Fact]
    public async Task RecipeApiPerformance_ShouldRespondInside100ms()
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

        averageMilliseconds.Should().BeLessOrEqualTo(100, $"Average API response after 50 iterations took too long: {averageMilliseconds} ms");
        p90Milliseconds.Should().BeLessOrEqualTo(450);
    }
}