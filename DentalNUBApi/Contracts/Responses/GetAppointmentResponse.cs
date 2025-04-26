using DentalNUB.Api.Contracts.Requests;

namespace DentalNUB.Api.Contracts.Responses;

public class GetAppointmentResponse
{
    public CreatePatientRequest? CreatePatientRequest { get; set; }
    public string Complaint { get; set; } = string.Empty;
    public string? XRayImage { get; set; }
}
