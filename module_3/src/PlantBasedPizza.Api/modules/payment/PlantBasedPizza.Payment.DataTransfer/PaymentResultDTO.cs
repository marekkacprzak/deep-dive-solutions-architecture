



using PlantBasedPizza.Payment.Core;

namespace PlantBasedPizza.Payment.DataTransfer;

public record PaymentResultDTO
{
    public PaymentResultDTO(PaymentResult result)
    {
        PaymentId = result.PaymentId;
        Message = result.Message;
    }
    
    public string PaymentId { get; set; } = "";
    
    public string Message { get; set; } = "";
}