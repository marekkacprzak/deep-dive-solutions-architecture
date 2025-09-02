namespace PlantBasedPizza.Kitchen.Core.Services;

public interface IKitchenDomainService
{
    Task<List<KitchenRequestDTO>> GetNewRequestsAsync();
    Task<List<KitchenRequestDTO>> GetPreparingRequestAsync();
    Task<List<KitchenRequestDTO>> GetBakingAsync();
    Task<List<KitchenRequestDTO>> GetAwaitingQualityCheckAsync();
    Task<KitchenRequestDTO> StartPreparingAsync(string orderIdentifier, string correlationId = "");
    Task<KitchenRequestDTO> CompletePreparationAsync(string orderIdentifier, string correlationId = "");
    Task<KitchenRequestDTO> CompleteBakingAsync(string orderIdentifier, string correlationId = "");
    Task<KitchenRequestDTO> CompleteQualityCheckAsync(string orderIdentifier, string correlationId = "");
}