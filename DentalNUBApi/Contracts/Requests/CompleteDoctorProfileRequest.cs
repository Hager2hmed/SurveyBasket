using System.ComponentModel.DataAnnotations;

namespace DentalNUB.Api.Contracts.Requests;

public class CompleteDoctorProfileRequest
{
    public int UserId { get; set; }
   
    [Required]
    public string ClinicName { get; set; } = string.Empty;

    [Required]
    [Range(3, 5, ErrorMessage = "DoctorYear must be 3, 4, or 5")]
    public int DoctorYear { get; set; }

    [Required]
    [RegularExpression(@"^\+?\d{10,15}$", ErrorMessage = "Invalid phone number format")]
    public string DoctorPhone { get; set; } = string.Empty;

    [Required]
    public int UniversityID { get; set; }
}
