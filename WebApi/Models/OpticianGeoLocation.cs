namespace WebApi.Models
{
    public class OpticianGeoLocation
    {
        public int Id { get; set; }
        public string OpnSfTeamCode { get; set; }
        public string MgtNo { get; set; }
        public string BplcNm { get; set; }
        public string RdnPostNo { get; set; }
        public string RdnWhlAddr { get; set; }
        public decimal GeoX { get; set; }
        public decimal GeoY { get; set; }
        public string SiteTel { get; set; }
        public string CustCode { get; set; }
        public bool IsReg { get; set; }
    }

    public class UnRegMarkerHistory
    {
        public string OpnSfTeamCode { get; set; }
        public string MgtNo { get; set; }
        public int Seq { get; set; }
        public string DT_COMP { get; set; }
        public string TX_REASON { get; set; }
        public string TX_PURPOSE { get; set; }
        public string TX_NOTE { get; set; }
    }
}
