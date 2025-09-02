using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using PlantBasedPizza.Kitchen.Core;
using PlantBasedPizza.Kitchen.DataTransfer;

namespace PlantBasedPizza.Kitchen.Infrastructure;

public class KitchenRequestRepositoryPostgres : IKitchenRequestRepository
{
    private readonly KitchenDbContext _context;
    private readonly KitchenEventPublisher _eventPublisher;

    public KitchenRequestRepositoryPostgres(KitchenDbContext context, KitchenEventPublisher eventPublisher)
    {
        _context = context;
        _eventPublisher = eventPublisher;
    }

    public async Task AddNew(KitchenRequest kitchenRequest)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var evt in kitchenRequest.Events)
                switch (evt)
                {
                    case OrderPreparingEvent orderPreparingEvent:
                        await _eventPublisher.AddToEventOutbox(
                            new OrderPreparingEventV1(kitchenRequest.OrderIdentifier));
                        break;
                    case OrderPrepCompleteEvent orderPrepCompleteEvent:
                        await _eventPublisher.AddToEventOutbox(
                            new OrderPrepCompleteEventV1(kitchenRequest.OrderIdentifier));
                        break;
                    case OrderBakedEvent orderBakedEvent:
                        await _eventPublisher.AddToEventOutbox(new OrderBakedEventV1(kitchenRequest.OrderIdentifier));
                        break;
                    case OrderQualityCheckedEvent orderQualityCheckedEvent:
                        await _eventPublisher.AddToEventOutbox(
                            new OrderQualityCheckedEventV1(kitchenRequest.OrderIdentifier));
                        break;
                }

            await _context.KitchenRequests.AddAsync(kitchenRequest);
            await _context.SaveChangesAsync();
            
            await _eventPublisher.ClearOutbox();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            throw;
        }

        await transaction.CommitAsync();
    }

    public async Task Update(KitchenRequest kitchenRequest)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var evt in kitchenRequest.Events)
                switch (evt)
                {
                    case OrderPreparingEvent orderPreparingEvent:
                        await _eventPublisher.AddToEventOutbox(
                            new OrderPreparingEventV1(kitchenRequest.OrderIdentifier));
                        break;
                    case OrderPrepCompleteEvent orderPrepCompleteEvent:
                        await _eventPublisher.AddToEventOutbox(
                            new OrderPrepCompleteEventV1(kitchenRequest.OrderIdentifier));
                        break;
                    case OrderBakedEvent orderBakedEvent:
                        await _eventPublisher.AddToEventOutbox(new OrderBakedEventV1(kitchenRequest.OrderIdentifier));
                        break;
                    case OrderQualityCheckedEvent orderQualityCheckedEvent:
                        await _eventPublisher.AddToEventOutbox(
                            new OrderQualityCheckedEventV1(kitchenRequest.OrderIdentifier));
                        break;
                }

            _context.KitchenRequests.Update(kitchenRequest);
            var rowsAffected = await _context.SaveChangesAsync();
            
            Activity.Current?.AddTag("postgres.rowsAffected", rowsAffected);
            
            await _eventPublisher.ClearOutbox();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            throw;
        }

        await transaction.CommitAsync();
    }

    public async Task<KitchenRequest> Retrieve(string orderIdentifier)
    {
        return await _context.KitchenRequests
            .Include(k => k.Recipes)
            .FirstOrDefaultAsync(k => k.OrderIdentifier == orderIdentifier);
    }

    public async Task<IEnumerable<KitchenRequest>> GetNew()
    {
        return await _context.KitchenRequests
            .Include(k => k.Recipes)
            .Where(k => k.OrderState == OrderState.NEW)
            .ToListAsync();
    }

    public async Task<IEnumerable<KitchenRequest>> GetPrep()
    {
        return await _context.KitchenRequests
            .Include(k => k.Recipes)
            .Where(k => k.OrderState == OrderState.PREPARING)
            .ToListAsync();
    }

    public async Task<IEnumerable<KitchenRequest>> GetBaking()
    {
        return await _context.KitchenRequests
            .Include(k => k.Recipes)
            .Where(k => k.OrderState == OrderState.BAKING)
            .ToListAsync();
    }

    public async Task<IEnumerable<KitchenRequest>> GetAwaitingQualityCheck()
    {
        return await _context.KitchenRequests
            .Include(k => k.Recipes)
            .Where(k => k.OrderState == OrderState.QUALITYCHECK)
            .ToListAsync();
    }
}