namespace WebApi.Models
{
    public class Optician
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Post { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Grade { get; set; } = string.Empty;
        public string EmpName { get; set; } = string.Empty;
        public string CeoName { get; set; } = string.Empty;
        public string BizNo { get; set; } = string.Empty;
        public string OpenAt { get; set; } = string.Empty;
        public string CloseAt { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Tel { get; set; } = string.Empty;
        public string FirstNote { get; set; } = string.Empty;
        public string CreateAt { get; set; } = string.Empty;
    }
    public class CodeDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }

    public class RegionDto
    {
        public string RegionCode { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public string ParentRegionCode { get; set; } = string.Empty;
        public int Level { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class EmplyeeDto
    {
        public string EmpCode { get; set; } = string.Empty;
        public string EmpName { get; set; } = string.Empty;
    }
}
