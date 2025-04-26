using DentalNUB.Api.Contracts.Requests;
using DentalNUB.Api.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DentalNUB.Api.Services
{
    public interface IDiagnoseService
    {
        Task<bool> CreateDiagnoseAsync(CreateDiagnoseRequest request);
        Task<IEnumerable<Diagnose>> GetAllDiagnosesAsync();
        Task<Diagnose?> GetDiagnoseByIdAsync(int id);
        Task<bool> UpdateDiagnoseAsync(int id, CreateDiagnoseRequest request);
        Task<bool> DeleteDiagnoseAsync(int id);
    }
}