namespace WebApi.Models
{
    public class SalesRecord
    {
        public int Id { get; set; }
        public int OpticianId { get; set; }
        public decimal SalesAmount { get; set; }
        public DateTime SalesDate { get; set; }
        public string SalesType { get; set; } = string.Empty; // "안경", "선글라스", "렌즈" 등
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public virtual Optician OpticalStore { get; set; } = null!;
    }
}
