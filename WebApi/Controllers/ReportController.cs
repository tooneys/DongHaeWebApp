using Microsoft.AspNetCore.Mvc;
using WebApi.Services.Report;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesReport(
            DateTime startDate, 
            DateTime endDate, 
            string type = "", 
            string? customer = null, 
            string? manager = null
        )
        {
            // 예: startDate~endDate의 월 목록 생성
            var months = new List<string>();
            var current = new DateTime(startDate.Year, startDate.Month, 1);
            while (current <= endDate)
            {
                months.Add(current.ToString("yyyy년MM월"));
                current = current.AddMonths(1);
            }

            var records = await _reportService.GenerateSalesReportAsync(
                startDate,
                endDate,
                type,
                customer,
                manager
            );

            // 컬럼 정의 동적 생성 (첫 행 기준)
            var columns = new List<object>();
            if (records.Any())
            {
                foreach (var key in records.First().Keys)
                {
                    columns.Add(new { field = key, title = key });
                }
            }

            // 거래처 필터
            if (!string.IsNullOrEmpty(customer))
                records = records.Where(r => r["안경원"].ToString()?.Contains(customer) ?? false).ToList();

            return Ok(new
            {
                columns,
                rows = records
            });
        }
    }
}
