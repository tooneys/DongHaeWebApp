using WebApi.Models;

namespace WebApi.Services.OpticianMap
{
    public interface IOpticianMapService
    {
        /// <summary>
        /// 모든 안경사의 지도 위치 정보를 조회합니다.
        /// </summary>
        /// <returns>안경사 위치 정보 목록</returns>
        Task<IEnumerable<OpticianGeoLocation>> GetOpticianMapAll();

        /// <summary>
        /// 특정 지역의 안경사 위치 정보를 조회합니다.
        /// </summary>
        /// <param name="region">지역명</param>
        /// <returns>해당 지역의 안경사 위치 정보 목록</returns>
        Task<IEnumerable<OpticianGeoLocation>> GetOpticianMapByRegion(string region);

        /// <summary>
        /// 특정 안경사의 위치 정보를 조회합니다.
        /// </summary>
        /// <param name="opticianId">안경사 ID</param>
        /// <returns>안경사 위치 정보</returns>
        Task<OpticianGeoLocation> GetOpticianLocationById(string opticianId);

        /// <summary>
        /// 주어진 좌표 주변의 안경사들을 조회합니다.
        /// </summary>
        /// <param name="latitude">위도</param>
        /// <param name="longitude">경도</param>
        /// <param name="radiusKm">반경(km)</param>
        /// <returns>주변 안경사 위치 정보 목록</returns>
        Task<IEnumerable<OpticianGeoLocation>> GetNearbyOpticians(double latitude, double longitude, double radiusKm);

        /// <summary>
        /// 안경사의 위치 정보를 업데이트합니다.
        /// </summary>
        /// <param name="opticianId">안경사 ID</param>
        /// <param name="latitude">위도</param>
        /// <param name="longitude">경도</param>
        /// <returns>업데이트 성공 여부</returns>
        Task<bool> UpdateOpticianLocation(string opticianId, double latitude, double longitude);
        
        Task<UnRegMarkerHistory> AddVisitHistoryAsync(UnRegMarkerHistory historyDto);
        
        Task<IEnumerable<UnRegMarkerHistory>> GetVisitHistoryById(string OpnSfTeamCode, string MgtNo);

        Task UpdateOpticianStatus(StatusRequest statusRequest);
    }
}
