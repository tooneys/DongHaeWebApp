namespace BlazorApp.Models
{
    public class LocationData
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
        public double Accuracy { get; set; }
    }

    public class OpticianMapDto
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
        public string OpticianManage { get; set; }

        public string OpticianManageName =>
            OpticianManage switch
            {
                "001" => "신규관리점",
                "002" => "관리제외점",
                _ => "미지정"
            };
    }

    // 마커 데이터 모델
    public class MarkerData
    {
        public int Id { get; set; }
        public string? BplcNm { get; set; }
        public double GeoX { get; set; }
        public double GeoY { get; set; }
        public bool IsReg { get; set; }
        public double Distance { get; set; }
        public string? RegStatus { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double UtmkX { get; set; }
        public double UtmkY { get; set; }
        public string CustCode { get; set; }
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
