using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Models;
using WebApi.Repositories;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketingDataController : ControllerBase
    {
        private readonly IMarketingDataRepository _repository;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<MarketingDataController> _logger;

        public MarketingDataController(
            IMarketingDataRepository repository,
            IFileUploadService fileUploadService,
            ILogger<MarketingDataController> logger)
        {
            _repository = repository;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        /// <summary>
        /// 마케팅 데이터를 등록합니다.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<MarketingDataResponseDto>>> CreateMarketingData(
            MarketingDataCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "입력 데이터가 유효하지 않습니다.",
                    });
                }

                var data = new MarketingData { Name = dto.Name, TabType = dto.TabType, Description = dto.Description };

                // 파일 업로드 (상대 경로로 저장)
                if (dto.CoverImage != null)
                {
                    data.CoverImageUrl = await _fileUploadService.UploadFileAsync(dto.CoverImage, "covers");
                }

                if (dto.DownloadFile != null)
                {
                    data.DownloadFileUrl = await _fileUploadService.UploadFileAsync(dto.DownloadFile, "downloads");
                }

                var id = await _repository.CreateAsync(data);
                var createdData = await _repository.GetByIdAsync(id);

                var response = MapToResponseDto(createdData!);

                return CreatedAtAction(
                    nameof(GetMarketingData),
                    new { id = response.Id },
                    new ApiResponse<MarketingDataResponseDto>
                    {
                        Success = true,
                        Message = "마케팅 데이터가 성공적으로 등록되었습니다.",
                        Data = response
                    });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "마케팅 데이터 생성 중 오류가 발생했습니다.");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "마케팅 데이터 등록 중 오류가 발생했습니다."
                });
            }
        }

        /// <summary>
        /// 모든 마케팅 데이터 목록을 조회합니다.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<MarketingDataResponseDto>>>> GetAllMarketingData()
        {
            try
            {
                var dataList = await _repository.GetAllAsync();
                var response = dataList.Select(MapToResponseDto);

                return Ok(new ApiResponse<IEnumerable<MarketingDataResponseDto>>
                {
                    Success = true,
                    Message = "마케팅 데이터 목록을 성공적으로 조회했습니다.",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "마케팅 데이터 목록 조회 중 오류가 발생했습니다.");
                return StatusCode(500, new ApiResponse<IEnumerable<MarketingDataResponseDto>>
                {
                    Success = false,
                    Message = "마케팅 데이터 목록 조회 중 오류가 발생했습니다."
                });
            }
        }

        /// <summary>
        /// 특정 마케팅 데이터를 조회합니다.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<MarketingDataResponseDto>>> GetMarketingData(int id)
        {
            try
            {
                var data = await _repository.GetByIdAsync(id);
                if (data == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "마케팅 데이터를 찾을 수 없습니다."
                    });
                }

                var response = MapToResponseDto(data);

                return Ok(new ApiResponse<MarketingDataResponseDto>
                {
                    Success = true,
                    Message = "마케팅 데이터를 성공적으로 조회했습니다.",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "마케팅 데이터 조회 중 오류가 발생했습니다. ID: {Id}", id);
                return StatusCode(500, new ApiResponse<MarketingDataResponseDto>
                {
                    Success = false,
                    Message = "마케팅 데이터 조회 중 오류가 발생했습니다."
                });
            }
        }

        /// <summary>
        /// 마케팅 데이터를 삭제합니다.
        /// </summary>
        [HttpPost("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteMarketingData(int id)
        {
            try
            {
                var data = await _repository.GetByIdAsync(id);
                if (data == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "마케팅 데이터를 찾을 수 없습니다."
                    });
                }

                // 파일 삭제 (상대 경로로 삭제)
                var deleteResults = await Task.WhenAll(
                    _fileUploadService.DeleteFileAsync(data.CoverImageUrl),
                    _fileUploadService.DeleteFileAsync(data.DownloadFileUrl)
                );

                // DB에서 삭제
                var success = await _repository.DeleteAsync(id);
                if (!success)
                {
                    return StatusCode(500, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "마케팅 데이터 삭제에 실패했습니다."
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "마케팅 데이터가 성공적으로 삭제되었습니다."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "마케팅 데이터 삭제 중 오류가 발생했습니다. ID: {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "마케팅 데이터 삭제 중 오류가 발생했습니다."
                });
            }
        }

        /// <summary>
        /// 마케팅 데이터를 응답 DTO로 변환합니다.
        /// </summary>
        private MarketingDataResponseDto MapToResponseDto(MarketingData data)
        {
            return new MarketingDataResponseDto
            {
                Id = data.Id,
                Name = data.Name,
                TabType = data.TabType,
                Description = data.Description,
                CoverImageUrl = _fileUploadService.GetFullUrl(data.CoverImageUrl), // 전체 URL로 변환
                DownloadFileUrl = _fileUploadService.GetFullUrl(data.DownloadFileUrl), // 전체 URL로 변환
                CreatedDate = data.CreatedDate,
                UpdatedDate = data.UpdatedDate
            };
        }
    }
}
