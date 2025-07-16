namespace BlazorApp.Models
{
    // Models/OpticalStoreSales.cs
    public class OpticalStoreSales
    {
        public string StoreName { get; set; } = string.Empty;
        public decimal SalesAmount { get; set; }
        public int Rank { get; set; }
        public string FormattedSalesAmount => SalesAmount.ToString("N0");
    }

    public class OpticalStoreSalesDecline
    {
        public int Rank { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public decimal PreyearAvgSales { get; set; }
        public decimal PremonthSumSales { get; set; }
        public decimal MonthSumSales { get; set; }
        public decimal DiffAmount { get; set; }
    }
}
