using DentalNUB.Api.Entities;

namespace DentalNUB.Api.Services;

public interface ICaseDistributionService
{
    Task<Doctor?> DistributeCaseToDoctorAsync(string assignedClinicName);
}