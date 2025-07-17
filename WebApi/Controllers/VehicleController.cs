using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Models;
using WebApi.Services.Vehicle;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<VehicleController> _logger;

        public VehicleController(IVehicleService vehicleService, ILogger<VehicleController> logger)
        {
            _vehicleService = vehicleService;
            _logger = logger;
        }

        [HttpGet("{empCode}")]
        public async Task<IActionResult> GetVehiclesByIdAsync(string empCode)
        {
            try
            {
                _logger.LogInformation($"차량일지 내역 조회 시작: empCode={empCode}");

                var response = await _vehicleService.GetVehiclesByIdAsync(empCode);

                if (response == null || !response.Any())
                {
                    _logger.LogWarning($"차량일지 내역을 찾을 수 없음: empCode={empCode}");
                    return NotFound("차량일지 내역을 찾을 수 없습니다.");
                }

                _logger.LogInformation($"차량일지 내역 조회 완료 Count: {response.ToList().Count}");

                // DTO를 List로 변환하여 반환
                return Ok(new ApiResponse<List<VehicleDto>>
                {
                    Message = "차량일지 내역이 조회가 완료되었습니다.",
                    Data = response.ToList() ?? new List<VehicleDto>(),
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "차량일지 내역 조회 중 오류 발생");
                return StatusCode(500, "서버 오류가 발생했습니다.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicleAsync([FromBody] VehicleDto vehicleDto)
        {
            if (vehicleDto == null)
            {
                return BadRequest("차량 정보가 제공되지 않았습니다.");
            }
            try
            {
                var result = await _vehicleService.CreateVehicleAsync(vehicleDto);

                _logger.LogInformation($"차량 생성 요청: NO_CAR={vehicleDto.NO_CAR}, CD_EMP={vehicleDto.CD_EMP}");

                return Ok(new ApiResponse<VehicleDto>
                {
                    Message = "차량이 성공적으로 생성되었습니다.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "차량 생성 중 오류 발생");
                return StatusCode(500, "서버 오류가 발생했습니다.");
            }
        }
    }
}
