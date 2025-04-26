using DentalNUB.Api.Contracts.Requests;
using DentalNUB.Api.Contracts.Responses;
using DentalNUB.Api.Entities;
using Mapster;

namespace DentalNUB.Api.Mapping;

public class MapsterConfigurations : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Appointment, GetAppointmentResponse>()
            .Map(dest => dest.CreatePatientRequest, src => src.Patient.Adapt<CreatePatientRequest>());
    }
}
