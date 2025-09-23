using Polly;
using Polly.CircuitBreaker;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;

namespace PlantBasedPizza.Shared.Policies;

public static class Retry
{
    public const string RETRYPOLICYASYNC = "PlantBasedPizza.Shared.Policies.RetryPolicyAsync";
    public const string EXPONENTIAL_RETRYPOLICYASYNC = "PlantBasedPizza.Shared.Policies.ExponentialRetryPolicyAsync";

    public static AsyncRetryPolicy GetSimpleHandlerRetryPolicy()
    {
        return Policy.Handle<Exception>().WaitAndRetryAsync(new[]
        {
            TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(150)
        });
    }

    public static IAsyncPolicy<HttpResponseMessage> DefaultHttpRetryPolicy()
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);
        
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(delay);
    }

    public static AsyncRetryPolicy GetExponentialHandlerRetryPolicy()
    {
        var delay = Backoff.ExponentialBackoff(TimeSpan.FromMilliseconds(100), retryCount: 5, fastFirst: true);
        return Policy.Handle<Exception>().WaitAndRetryAsync(delay);
    }
        
    public static AsyncRetryPolicy GetDefaultRetryPolicy()
    {
        return Policy.Handle<Exception>()
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(150)
            }); 
    }
        
    public static AsyncCircuitBreakerPolicy GetDefaultCircuitBreakerPolicy()
    {
        return Policy.Handle<Exception>().CircuitBreakerAsync(
            1, TimeSpan.FromMilliseconds(500)
        );
    }
}