using WebApi.DTOs;

namespace WebApi.Services.Vehicle
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleDto>> GetVehiclesByIdAsync(string empCode);
        Task<VehicleDto> CreateVehicleAsync(VehicleDto vehicleDto);
        Task<VehicleDto> UpdateVehicleAsync(VehicleDto vehicleDto);
        Task DeleteVehicleAsync(int Seq);
    }
}
