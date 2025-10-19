using System.Xml.Linq;
using Aspire.Hosting.Yarp;

namespace PlantBasedPizza.Aspire;

public static class YarpHelper
{
    public static async Task UpdateTestEndpoint(this IResourceBuilder<YarpResource> yarpProxy)
    {
        try
        {
            // Get the allocated endpoint
            var endpoints = yarpProxy.Resource.Annotations.OfType<EndpointAnnotation>();
            var httpEndpoint = endpoints.FirstOrDefault(e => e.UriScheme == "http" || e.UriScheme == "https");

            var allocatedEndpoint = httpEndpoint?.AllocatedEndpoint;
            if (allocatedEndpoint != null)
            {
                var testUrl = $"{allocatedEndpoint.UriString}";
                await UpdateRunSettingsFile(testUrl);
                Console.WriteLine($"Updated .runsettings with TEST_URL: {testUrl}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating .runsettings: {ex.Message}");
        }
    }
    
    private static async Task UpdateRunSettingsFile(string testUrl)
    {
        // Find the solution root by looking for the .sln file
        var currentDir = new DirectoryInfo(AppContext.BaseDirectory);
        DirectoryInfo? solutionRoot = null;
        
        while (currentDir != null)
        {
            if (currentDir.GetFiles("*.sln").Any())
            {
                solutionRoot = currentDir;
                break;
            }
            currentDir = currentDir.Parent;
        }

        if (solutionRoot == null)
        {
            Console.WriteLine("Warning: Could not find solution root directory");
            return;
        }

        var runSettingsPath = Path.Combine(
            solutionRoot.FullName,
            "src", "PlantBasedPizza.Api", "tests", "PlantBasedPizza.IntegrationTests", ".runsettings");

        if (!File.Exists(runSettingsPath))
        {
            Console.WriteLine($"Warning: .runsettings file not found at {runSettingsPath}");
            return;
        }

        var doc = XDocument.Load(runSettingsPath);
        var testUrlElement = doc.Descendants("TEST_URL").FirstOrDefault();

        if (testUrlElement != null)
        {
            testUrlElement.Value = testUrl;
            await using var fileStream = new FileStream(runSettingsPath, FileMode.Create, FileAccess.Write);
            await doc.SaveAsync(fileStream, SaveOptions.None, CancellationToken.None);
            Console.WriteLine($"Successfully updated .runsettings at: {runSettingsPath}");
        }
        else
        {
            Console.WriteLine("Warning: TEST_URL element not found in .runsettings");
        }
    }
}