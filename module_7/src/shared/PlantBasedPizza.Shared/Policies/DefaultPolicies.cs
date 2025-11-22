using System.Reflection;
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

    public static ResiliencePipelineRegistry<string> AddDefaultRetries(this ResiliencePipelineRegistry<string> registry, IEnumerable<Type> requestTypes)
    {
        registry.AddDefaultRetries();

        foreach (var type in requestTypes)
        {
            RegisterGenericRetry(registry, type, Retry.EXPONENTIAL_RETRYPOLICYASYNC, options => 
            {
                options.Delay = TimeSpan.FromMilliseconds(100);
                options.MaxRetryAttempts = 5;
                options.BackoffType = DelayBackoffType.Exponential;
                options.UseJitter = true;
            });
            
             RegisterGenericRetry(registry, type, Retry.RETRYPOLICYASYNC, options => 
            {
                options.Delay = TimeSpan.FromMilliseconds(50);
                options.MaxRetryAttempts = 3;
                options.BackoffType = DelayBackoffType.Linear;
            });
        }
        return registry;
    }

    private static void RegisterGenericRetry(ResiliencePipelineRegistry<string> registry, Type type, string key, Action<RetryStrategyOptions> configureRetry)
    {
        var method = typeof(DefaultPolicieExtensions).GetMethod(nameof(Register), BindingFlags.NonPublic | BindingFlags.Static);
        var generic = method!.MakeGenericMethod(type);
        generic.Invoke(null, new object[] { registry, key, configureRetry });
    }

    private static void Register<T>(ResiliencePipelineRegistry<string> registry, string key, Action<RetryStrategyOptions> configureRetry)
    {
        registry.TryAddBuilder<T>(key, (builder, _) => 
        {
            var nonGenericOptions = new RetryStrategyOptions();
            configureRetry(nonGenericOptions);

            var options = new RetryStrategyOptions<T>
            {
                Delay = nonGenericOptions.Delay,
                MaxRetryAttempts = nonGenericOptions.MaxRetryAttempts,
                BackoffType = nonGenericOptions.BackoffType,
                UseJitter = nonGenericOptions.UseJitter
            };
            
            builder.AddRetry(options)
                   .AddTimeout(TimeSpan.FromMilliseconds(500))
                   .AddCircuitBreaker(new CircuitBreakerStrategyOptions<T>
                   {
                       BreakDuration = TimeSpan.FromSeconds(2)
                   });
        });
    }
}