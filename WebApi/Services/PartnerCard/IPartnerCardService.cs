using WebApi.Models;

namespace WebApi.Services.PartnerCard
{
    public interface IPartnerCardService
    {
        Task<PartnerCardDetail> GetPartnerCardDetailById(string opticianId);
        Task<IEnumerable<CustNote>> GetCustNotesById(string opticianId);
        Task<IEnumerable<OpticianPromotion>> GetOpticianPromotionById(string opticianId);
        Task<IEnumerable<OrderDto>> GetOrdersById(string opticianId, int year);
        Task<IEnumerable<SalesOrderDto>> GetSalesOrdersById(string opticianId);
        Task<IEnumerable<ReturnOrderDto>> GetReturnOrdersById(string opticianId);
        Task<IEnumerable<OpticianHistoryDto>> GetOpticianHistoriesById(string opticianId);
        Task<IEnumerable<OpticianClaimDto>> GetOpticianClaimsById(string opticianId);

        /// <summary>
        /// 방문이력 서비스 추가
        /// </summary>
        /// <param name="historyDto"></param>
        /// <returns></returns>
        Task<OpticianHistoryDto> AddVisitHistoryAsync(OpticianHistoryDto historyDto);
        Task<OpticianHistoryDto> GetVisitHistoryByIdAsync(int historyId); // string에서 int로 변경
        Task<OpticianHistoryDto> UpdateVisitHistoryAsync(OpticianHistoryDto historyDto);
        Task<bool> DeleteVisitHistoryAsync(int historyId); // string에서 int로 변경
        Task<OpticianPromotion> AddPromotionAsync(OpticianPromotion promotion);
        Task UpdateOpticianStoreImage(OpticianStoreImage storeImage);
    }
}
