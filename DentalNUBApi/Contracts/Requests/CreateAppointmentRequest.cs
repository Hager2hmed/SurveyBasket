using Azure.Core;
using DentalNUB.Api.Entities;  // إضافة المسار الصحيح للـ enum

namespace DentalNUB.Api.Contracts.Requests
{
    public record CreateAppointmentRequest
    {
        public CreatePatientRequest? PatientData { get; set; }
        public string Complaint { get; set; } = string.Empty;
        public IFormFile? XRayImage { get; set; }
        public Appointment.BookingTypeEnum BookingType { get; set; } = Appointment.BookingTypeEnum.Normal;  // استخدام BookingType
      


    }
}
