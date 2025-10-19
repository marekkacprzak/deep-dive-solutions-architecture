namespace PlantBasedPizza.Kitchen.Core.Services;

public class KitchenDomainService(IKitchenRequestRepository kitchenRequestRepository)
    : IKitchenDomainService
{
    public async Task<List<KitchenRequestDTO>> GetNewRequestsAsync()
    {
        var queryResults = await kitchenRequestRepository.GetNew();

        return queryResults.Select(p => new KitchenRequestDTO(p)).ToList();
    }

    public async Task<List<KitchenRequestDTO>> GetPreparingRequestAsync()
    {
        var queryResults = await kitchenRequestRepository.GetPrep();

        return queryResults.Select(p => new KitchenRequestDTO(p)).ToList();
    }

    public async Task<List<KitchenRequestDTO>> GetBakingAsync()
    {
        var queryResults = await kitchenRequestRepository.GetBaking();

        return queryResults.Select(p => new KitchenRequestDTO(p)).ToList();
    }

    public async Task<List<KitchenRequestDTO>> GetAwaitingQualityCheckAsync()
    {
        var queryResults = await kitchenRequestRepository.GetAwaitingQualityCheck();

        return queryResults.Select(p => new KitchenRequestDTO(p)).ToList();
    }

    public async Task<KitchenRequestDTO> StartPreparingAsync(string orderIdentifier, string correlationId = "")
    {
        ArgumentNullException.ThrowIfNull(orderIdentifier);
        
        var request = await kitchenRequestRepository.Retrieve(orderIdentifier);

        request.StartPreparing();
        
        await kitchenRequestRepository.Update(request);
        
        return new KitchenRequestDTO(request);
    }

    public async Task<KitchenRequestDTO> CompletePreparationAsync(string orderIdentifier, string correlationId = "")
    {
        var request = await kitchenRequestRepository.Retrieve(orderIdentifier);
        
        ArgumentNullException.ThrowIfNull(request);

        request.CompletePreparing();
        
        await kitchenRequestRepository.Update(request);
        
        return new KitchenRequestDTO(request);
    }

    public async Task<KitchenRequestDTO> CompleteBakingAsync(string orderIdentifier, string correlationId = "")
    {
        var request = await kitchenRequestRepository.Retrieve(orderIdentifier);
        
        ArgumentNullException.ThrowIfNull(request);

        request.CompleteBaking();
        
        await kitchenRequestRepository.Update(request);
        
        return new KitchenRequestDTO(request);
    }

    public async Task<KitchenRequestDTO> CompleteQualityCheckAsync(string orderIdentifier, string correlationId = "")
    {
        var request = await kitchenRequestRepository.Retrieve(orderIdentifier);
        
        ArgumentNullException.ThrowIfNull(request);

        request.CompleteQualityCheck();
        
        await kitchenRequestRepository.Update(request);
        
        return new KitchenRequestDTO(request);
    }
}