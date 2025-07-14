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
        /// <param name="dto">마케팅 데이터 생성 정보</param>
        /// <returns>생성된 마케팅 데이터 정보</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<MarketingDataResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<ActionResult<ApiResponse<MarketingDataResponseDto>>> CreateMarketingData(
            [FromForm] MarketingDataCreateDto dto)
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

                var data = new MarketingData
                {
                    Name = dto.Name
                };

                // 표지 이미지 업로드
                if (dto.CoverImage != null)
                {
                    data.CoverImageUrl = await _fileUploadService.UploadFileAsync(dto.CoverImage, "covers");
                }

                // 다운로드 파일 업로드
                if (dto.DownloadFile != null)
                {
                    data.DownloadFileUrl = await _fileUploadService.UploadFileAsync(dto.DownloadFile, "downloads");
                }

                var id = await _repository.CreateAsync(data);
                var createdData = await _repository.GetByIdAsync(id);

                var response = new MarketingDataResponseDto
                {
                    Id = createdData!.Id,
                    Name = createdData.Name,
                    CoverImageUrl = createdData.CoverImageUrl,
                    DownloadFileUrl = createdData.DownloadFileUrl,
                    CreatedDate = createdData.CreatedDate,
                    UpdatedDate = createdData.UpdatedDate
                };

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
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "마케팅 데이터 등록 중 오류가 발생했습니다."
                });
            }
        }

        /// <summary>
        /// 모든 마케팅 데이터 목록을 조회합니다.
        /// </summary>
        /// <returns>마케팅 데이터 목록</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<MarketingDataResponseDto>>), 200)]
        public async Task<ActionResult<ApiResponse<IEnumerable<MarketingDataResponseDto>>>> GetAllMarketingData()
        {
            try
            {
                var dataList = await _repository.GetAllAsync();
                var response = dataList.Select(d => new MarketingDataResponseDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    CoverImageUrl = d.CoverImageUrl,
                    DownloadFileUrl = d.DownloadFileUrl,
                    CreatedDate = d.CreatedDate,
                    UpdatedDate = d.UpdatedDate
                });

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
                return BadRequest(new ApiResponse<IEnumerable<MarketingDataResponseDto>>
                {
                    Success = false,
                    Message = "마케팅 데이터 목록 조회 중 오류가 발생했습니다."
                });
            }
        }

        /// <summary>
        /// 특정 마케팅 데이터를 조회합니다.
        /// </summary>
        /// <param name="id">마케팅 데이터 ID</param>
        /// <returns>마케팅 데이터 정보</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<MarketingDataResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
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

                var response = new MarketingDataResponseDto
                {
                    Id = data.Id,
                    Name = data.Name,
                    CoverImageUrl = data.CoverImageUrl,
                    DownloadFileUrl = data.DownloadFileUrl,
                    CreatedDate = data.CreatedDate,
                    UpdatedDate = data.UpdatedDate
                };

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
                return BadRequest(new ApiResponse<MarketingDataResponseDto>
                {
                    Success = false,
                    Message = "마케팅 데이터 조회 중 오류가 발생했습니다."
                });
            }
        }

        /// <summary>
        /// 마케팅 데이터를 수정합니다.
        /// </summary>
        /// <param name="id">마케팅 데이터 ID</param>
        /// <param name="dto">수정할 마케팅 데이터 정보</param>
        /// <returns>수정된 마케팅 데이터 정보</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<MarketingDataResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<MarketingDataResponseDto>>> UpdateMarketingData(
            int id, [FromBody] MarketingDataUpdateDto dto)
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

                var existingData = await _repository.GetByIdAsync(id);
                if (existingData == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "마케팅 데이터를 찾을 수 없습니다."
                    });
                }

                var data = new MarketingData
                {
                    Id = id,
                    Name = dto.Name,
                    CoverImageUrl = dto.CoverImageUrl,
                    DownloadFileUrl = dto.DownloadFileUrl
                };

                var success = await _repository.UpdateAsync(data);
                if (!success)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "마케팅 데이터 수정에 실패했습니다."
                    });
                }

                var updatedData = await _repository.GetByIdAsync(id);
                var response = new MarketingDataResponseDto
                {
                    Id = updatedData!.Id,
                    Name = updatedData.Name,
                    CoverImageUrl = updatedData.CoverImageUrl,
                    DownloadFileUrl = updatedData.DownloadFileUrl,
                    CreatedDate = updatedData.CreatedDate,
                    UpdatedDate = updatedData.UpdatedDate
                };

                return Ok(new ApiResponse<MarketingDataResponseDto>
                {
                    Success = true,
                    Message = "마케팅 데이터가 성공적으로 수정되었습니다.",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "마케팅 데이터 수정 중 오류가 발생했습니다. ID: {Id}", id);
                return BadRequest(new ApiResponse<MarketingDataResponseDto>
                {
                    Success = false,
                    Message = "마케팅 데이터 수정 중 오류가 발생했습니다."
                });
            }
        }

        /// <summary>
        /// 마케팅 데이터를 삭제합니다.
        /// </summary>
        /// <param name="id">마케팅 데이터 ID</param>
        /// <returns>삭제 결과</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
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

                // 파일 삭제
                if (!string.IsNullOrEmpty(data.CoverImageUrl))
                {
                    await _fileUploadService.DeleteFileAsync(data.CoverImageUrl);
                }

                if (!string.IsNullOrEmpty(data.DownloadFileUrl))
                {
                    await _fileUploadService.DeleteFileAsync(data.DownloadFileUrl);
                }

                var success = await _repository.DeleteAsync(id);
                if (!success)
                {
                    return BadRequest(new ApiResponse<object>
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
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "마케팅 데이터 삭제 중 오류가 발생했습니다."
                });
            }
        }
    }
}
