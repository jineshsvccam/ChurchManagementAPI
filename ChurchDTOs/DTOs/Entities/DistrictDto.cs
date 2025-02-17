namespace ChurchDTOs.DTOs.Entities
{
    public class DistrictDto
    {
        public int DistrictId { get; set; }
        public string DistrictName { get; set; }
        public int DioceseId { get; set; }
        public string? DioceseName { get; set; } // Optional: To display diocese name without full object
        public string? Description { get; set; }
    }
}
