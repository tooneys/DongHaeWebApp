using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    public class MarketingDataCreateDto
    {
        [Required(ErrorMessage = "자료명은 필수입니다.")]
        [StringLength(255, ErrorMessage = "자료명은 255자를 초과할 수 없습니다.")]
        public string Name { get; set; } = string.Empty;
        
        public IFormFile? CoverImage { get; set; }
        
        public IFormFile? DownloadFile { get; set; }
    }

    public class MarketingDataUpdateDto
    {
        [Required(ErrorMessage = "자료명은 필수입니다.")]
        [StringLength(255, ErrorMessage = "자료명은 255자를 초과할 수 없습니다.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "표지 이미지 URL은 500자를 초과할 수 없습니다.")]
        public string? CoverImageUrl { get; set; }

        [StringLength(500, ErrorMessage = "다운로드 파일 URL은 500자를 초과할 수 없습니다.")]
        public string? DownloadFileUrl { get; set; }
    }

    public class MarketingDataResponseDto
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string? CoverImageUrl { get; set; }
        
        public string? DownloadFileUrl { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        public DateTime UpdatedDate { get; set; }
    }

}
