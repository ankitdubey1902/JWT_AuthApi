namespace JWT_AuthApi.Models
{
    // Sample model for demonstration purposes
    public class SampleModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
    }

    // Request DTO for creating/updating sample data
    public class SampleRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    // Response DTO for sample data
    public class SampleResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }       // Add this
        public DateTime Timestamp { get; set; } // Add this
    }

    // Filter DTO for querying sample data
    public class SampleFilter
    {
        public string NameContains { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}