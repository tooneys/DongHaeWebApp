namespace BlazorApp.Models
{
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

    public class EmployeeDto
    {
        public string EmpCode { get; set; } = string.Empty;
        public string EmpName { get; set; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int? TotalCount { get; set; }
        public bool Success { get; set; } = true;
        public DateTime Timestamp { get; set; }
    }

    public class ApiErrorResponse : ApiResponse<object>
    {
        public string ErrorCode { get; set; } = string.Empty;
        public object? Details { get; set; }
    }

    public class FormResult
    {
        public bool Successed { get; set; }
        public string[] Errors { get; set; } = Array.Empty<string>();
    }
}
