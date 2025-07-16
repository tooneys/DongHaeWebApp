using WebApi.DTOs;

namespace WebApi.Services.Dashboard
{
    public interface IDashboardSalesService
    {
        Task<List<OpticalStoreSalesDto>> GetTopStoresSalesAsync(int count, string userId, CancellationToken cancellationToken = default);
        Task<List<OpticalStoreSalesDto>> GetCurrentMonthSalesAsync(string userId, CancellationToken cancellationToken = default);
        Task<List<OpticalStoreSalesDeclineDto>> GetCurrentMonthSalesDeclineAsync(string userId, CancellationToken cancellationToken = default);
    }
}
