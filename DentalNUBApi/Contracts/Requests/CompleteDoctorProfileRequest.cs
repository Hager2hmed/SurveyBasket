namespace DentalNUB.Api.Contracts.Requests;

public class CompleteDoctorProfileRequest
{
    public int UserId { get; set; }
    public string DoctorPhone { get; set; } = string.Empty;
    public int DoctorYear { get; set; }
    public int UniversityID { get; set; }
    public string ClinicName { get; set; } = string.Empty;


}
