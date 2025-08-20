using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderRepositoryPostgres : IOrderRepository
{
    private readonly ILogger<OrderRepositoryPostgres> _logger;
    private readonly OrderManagerDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IDomainEventDispatcher _dispatcher;

    public OrderRepositoryPostgres(OrderManagerDbContext context, ILogger<OrderRepositoryPostgres> logger, IConfiguration configuration, IDomainEventDispatcher dispatcher)
    {
        _logger = logger;
        _configuration = configuration;
        _dispatcher = dispatcher;
        _context = context;
    }

    public async Task Add(Order order)
    {
        // Using a transaction to ensure consistency between Orders and OutboxItems
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            foreach (var evt in order.Events)
            {
                await _context.OutboxItems.AddAsync(new OutboxItem
                {
                    EventData = evt.AsString(),
                    EventType = evt.GetType()
                        .Name,
                    Processed = false,
                    Failed = false,
                });
            }
                
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Order> Retrieve(string orderIdentifier)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.History)
            .Include(o => o.DeliveryDetails)
            .FirstOrDefaultAsync(o => o.OrderIdentifier == orderIdentifier);

        if (order == null)
        {
            Activity.Current?.AddTag("order.notFound", true);
            throw new OrderNotFoundException(orderIdentifier);
        }

        return order;
    }

    public async Task<List<Order>> GetAwaitingCollection()
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.History)
            .Include(o => o.DeliveryDetails)
            .Where(o => o.OrderType == OrderType.Pickup && o.AwaitingCollection)
            .ToListAsync();
    }

    public async Task Update(Order order)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            foreach (var evt in order.Events)
            {
                if (!_configuration.GetValue<bool>("RunWorker"))
                {
                    await _dispatcher.PublishAsync(evt);
                }
                else
                {
                    await _context.OutboxItems.AddAsync(new OutboxItem
                    {
                        EventData = evt.AsString(),
                        EventType = evt.GetType()
                            .Name,
                        Processed = false,
                        Failed = false,
                    });
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}