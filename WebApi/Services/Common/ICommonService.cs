using WebApi.Models;

namespace WebApi.Services.Common
{
    public interface ICommonService
    {
        Task<List<Optician>> GetOpticiansAsync();
        Task<Optician> GetOpticiansByIdAsync(string opticianId);
        Task<List<CodeDto>> GetCodeListAsync(string codeType);
        Task<List<RegionDto>> GetRegionsAsync();
    }
}
