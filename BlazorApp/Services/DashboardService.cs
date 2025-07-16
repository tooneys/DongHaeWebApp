using BlazorApp.Models;
using Blazored.LocalStorage;
using System.Net.Http.Json;

namespace BlazorApp.Services
{
    public class DashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public DashboardService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task<List<OpticalStoreSales>> GetTopStoresSalesAsync(int count = 10)
        {
            // 실제 API 호출 시뮬레이션을 위한 지연
            var userProfile = await _localStorage.GetItemAsync<UserProfile>("userProfile");
            var userId = userProfile!.IsAdmin ? "" : userProfile.UserId;

            var response = await _httpClient.GetFromJsonAsync<List<OpticalStoreSales>>($"api/dashboard/sales/top/{count}?userId={userId}");
            return response ?? new List<OpticalStoreSales>();
        }

        public async Task<List<OpticalStoreSales>> GetAllStoresSalesAsync()
        {
            var userProfile = await _localStorage.GetItemAsync<UserProfile>("userProfile");
            var userId = userProfile!.IsAdmin ? "" : userProfile.UserId;

            var response = await _httpClient.GetFromJsonAsync<List<OpticalStoreSales>>($"api/dashboard/sales/current-month?userId={userId}");
            return response ?? new List<OpticalStoreSales>();
        }

        public async Task<List<OpticalStoreSalesDecline>> GetAllStoresSalesDeclineAsync()
        {
            var userProfile = await _localStorage.GetItemAsync<UserProfile>("userProfile");
            var userId = userProfile!.IsAdmin ? "" : userProfile.UserId;

            var response = await _httpClient.GetFromJsonAsync<List<OpticalStoreSalesDecline>>($"api/dashboard/sales/decline?userId={userId}");
            return response ?? new List<OpticalStoreSalesDecline>();
        }

        public async Task<List<ItemGroupSales>> GetAllItemGroupSalesAsync()
        {
            var userProfile = await _localStorage.GetItemAsync<UserProfile>("userProfile");
            var userId = userProfile!.IsAdmin ? "" : userProfile.UserId;

            var response = await _httpClient.GetFromJsonAsync<List<ItemGroupSales>>($"api/dashboard/sales/itemgroup?userId={userId}");
            return response ?? new List<ItemGroupSales>();
        }
    }
}
