using WebApi.DTOs;

namespace WebApi.Services.Dashboard
{
    public interface IDashboardSalesService
    {
        Task<List<OpticalStoreSalesDto>> GetTopStoresSalesAsync(int count, string month, string userId, CancellationToken cancellationToken = default);
        Task<List<OpticalStoreSalesDto>> GetCurrentMonthSalesAsync(string month, string userId, CancellationToken cancellationToken = default);
        Task<List<OpticalStoreSalesDeclineDto>> GetCurrentMonthSalesDeclineAsync(string month, string userId, CancellationToken cancellationToken = default);
        Task<List<ItemGroupSalesDto>> GetItemGroupSalesAsync(string month, string userId, CancellationToken cancellationToken = default);
    }
}
