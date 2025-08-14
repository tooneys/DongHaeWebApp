namespace WebApi.Services.Report
{
    public interface IReportService
    {
        /// <summary>
        /// 매출 보고서를 생성합니다.
        /// </summary>
        /// <param name="startDate">시작 날짜</param>
        /// <param name="endDate">종료 날짜</param>
        /// <param name="type">보고서 유형 (예: "전체", "매출")</param>
        /// <param name="customer">고객 필터링 (선택적)</param>
        /// <param name="manager">담당자 필터링 (선택적)</param>
        /// <returns>보고서 데이터</returns>
        Task<IEnumerable<Dictionary<string, object>>> GenerateSalesReportAsync(
            DateTime startDate,
            DateTime endDate,
            string type = "",
            string? customer = null,
            string? manager = null
        );

        /// <summary>
        /// 계획별 주문현황 보고서를 생성합니다.
        /// </summary>
        /// <param name="searchMonth"></param>
        /// <param name="manager"></param>
        /// <param name="valueDiv"></param>
        /// <param name="itemDiv"></param>
        /// <returns>보고서 데이터</returns>
        Task<IEnumerable<Dictionary<string, object>>> GenerateUserPlanReportAsync(
            string searchMonth,
            string manager,
            string? valueDiv,
            string? itemDiv
        );
    }
}
