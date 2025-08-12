namespace BlazorApp.Models
{
    public class CustomerSalesStatusDto
    {
        public string Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal PreviousYearAmount { get; set; }
        public decimal UnpaidAmount { get; set; }
        public decimal Sales2025 { get; set; }
        public string Manager { get; set; }
        public string Category { get; set; }
        public string Grade { get; set; }
    }
}