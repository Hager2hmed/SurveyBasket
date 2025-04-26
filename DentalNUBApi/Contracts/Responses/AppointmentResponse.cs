using Azure.Core;

namespace DentalNUB.Api.Contracts.Responses;

public record  AppointmentResponse
{
    public int AppointmentId { get; set; }
    public string? PatientName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public int QueueNumber { get; set; }
    public string EstimatedTime { get; set; } = string.Empty;
    public string ? ChronicalDiseases { get; set; }
    public string Message { get; set; } = string.Empty;
    

}
