using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Models;
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

            // 담당자 필터
            if (!string.IsNullOrEmpty(manager))
                records = records.Where(r => r["담당자"].ToString()?.Contains(manager) ?? false).ToList();

            return Ok(new
            {
                columns,
                rows = records
            });
        }

        [HttpGet("userplan")]
        public async Task<IActionResult> GetUserPlanReport(
            string searchMonth,
            string manager = "",
            string? valueDiv = "",
            string? itemDiv = ""
        )
        {
            if (string.IsNullOrEmpty(searchMonth))
            {
                return BadRequest("검색 월이 지정되지 않았습니다.");
            }

            if (string.IsNullOrEmpty(manager))
            {
                return BadRequest("담당자가 지정되지 않았습니다.");
            }

            var records = await _reportService.GenerateUserPlanReportAsync(
                searchMonth,
                manager,
                valueDiv,
                itemDiv
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

            return Ok(new
            {
                columns,
                rows = records
            });
        }


        [HttpGet("userplan-target")]
        public async Task<IActionResult> GetUserPlanTargetReport(string searchMonth, string manager = "")
        {
            if (string.IsNullOrEmpty(searchMonth))
            {
                return BadRequest("검색 월이 지정되지 않았습니다.");
            }

            if (string.IsNullOrEmpty(manager))
            {
                return BadRequest("담당자가 지정되지 않았습니다.");
            }
            try
            {
                var response = await _reportService.GetUserPlanTargetReportAsync(searchMonth, manager);

                // DTO를 List로 변환하여 반환
                return Ok(new ApiResponse<UserPlanTargetReportDto>
                {
                    Message = "계획별 주문현황(달성율)이 조회가 완료되었습니다.",
                    Data = response ?? new UserPlanTargetReportDto(),
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"서버 오류가 발생했습니다. {ex.Message}");
            }
        }
    }
}
