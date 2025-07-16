using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Services.Dashboard;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/dashboard/[controller]")]
    public class SalesController : Controller
    {
        private readonly IDashboardSalesService _salesService;
        private readonly ILogger<SalesController> _logger;

        public SalesController(IDashboardSalesService salesService, ILogger<SalesController> logger)
        {
            _salesService = salesService;
            _logger = logger;
        }

        [HttpGet("top/{count:int}")]
        public async Task<ActionResult<List<OpticalStoreSalesDto>>> GetTopStoresSales(
            int count = 10,
            [FromQuery] string userId = "",
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                if (count <= 0 || count > 100)
                {
                    return BadRequest("상위 랭크 선택은 1부터 100까지만 설정할 수 있습니다.");
                }

                var result = await _salesService.GetTopStoresSalesAsync(count, userId, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting top stores sales");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("current-month")]
        public async Task<ActionResult<List<OpticalStoreSalesDto>>> GetCurrentMonthSales(
            [FromQuery] string userId = "",
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var result = await _salesService.GetCurrentMonthSalesAsync(userId, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting current month sales");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("decline")]
        public async Task<ActionResult<List<OpticalStoreSalesDeclineDto>>> GetCurrentMonthSalesDecline(
            [FromQuery] string userId = "",
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var result = await _salesService.GetCurrentMonthSalesDeclineAsync(userId, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting current month sales");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
