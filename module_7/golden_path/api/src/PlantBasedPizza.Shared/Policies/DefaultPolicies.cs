using Paramore.Brighter;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Retry;

namespace PlantBasedPizza.Shared.Policies;

public static class DefaultPolicieExtensions
{
    public static ResiliencePipelineRegistry<string> AddDefaultRetries(this ResiliencePipelineRegistry<string> registry)
    {
        registry
            .TryAddBuilder(Retry.RETRYPOLICYASYNC,
                (builder, _) => builder
                    .AddRetry(new RetryStrategyOptions
                    {
                        Delay = TimeSpan.FromMilliseconds(50),
                        MaxRetryAttempts = 3,
                        BackoffType = DelayBackoffType.Linear
                    })
                    .AddTimeout(TimeSpan.FromMilliseconds(500))
                    .AddCircuitBreaker(new CircuitBreakerStrategyOptions()
                    {
                        BreakDuration = TimeSpan.FromSeconds(2),
                    }));
        
        registry
            .TryAddBuilder(Retry.EXPONENTIAL_RETRYPOLICYASYNC,
                (builder, _) => builder
                    .AddRetry(new RetryStrategyOptions
                    {
                        Delay = TimeSpan.FromMilliseconds(100),
                        MaxRetryAttempts = 5,
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true
                    })
                    .AddTimeout(TimeSpan.FromMilliseconds(500))
                    .AddCircuitBreaker(new CircuitBreakerStrategyOptions()
                    {
                        BreakDuration = TimeSpan.FromSeconds(2),
                    }));
        return registry;
    }
}