using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class MarketingData
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "자료명은 필수입니다.")]
        [StringLength(255, ErrorMessage = "자료명은 255자를 초과할 수 없습니다.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "표지 이미지 URL은 500자를 초과할 수 없습니다.")]
        public string? CoverImageUrl { get; set; }

        [StringLength(500, ErrorMessage = "다운로드 파일 URL은 500자를 초과할 수 없습니다.")]
        public string? DownloadFileUrl { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
