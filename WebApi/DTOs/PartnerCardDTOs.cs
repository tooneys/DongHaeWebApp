namespace WebApi.DTOs
{
    public class PartnerCardInsertDTOs
    {
        public string RegDate { get; set; } = string.Empty;
        public string OpticianId { get; set; } = string.Empty;
        public string Promotion { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; }
    }
}
