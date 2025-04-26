namespace DentalNUB.Api.Contracts.Requests;

public record CreateAppointmentRequest
{

    public CreatePatientRequest ?  PatientData { get; set; }
    public string   Complaint { get; set; } = string.Empty;

    public IFormFile? XRayImage { get; set; }

}
