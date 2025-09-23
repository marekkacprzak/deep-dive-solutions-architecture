using System;

namespace IntegrationTests.Drivers
{
    public static class TestConstants
    {
        public static string DefaultTestUrl = Environment.GetEnvironmentVariable("TEST_URL") ?? "http://localhost:55126";
    }
}