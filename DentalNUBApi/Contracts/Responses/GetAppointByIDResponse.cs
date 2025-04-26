using DentalNUB.Api.Contracts.Requests;

namespace DentalNUB.Api.Contracts.Responses;

public class GetAppointByIDResponse
{
    public CreatePatientRequest? PatientDetails { get; set; } 
    public string Complaint { get; set; } = string.Empty;
    public string? XRayImage { get; set; }
}
