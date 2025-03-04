namespace ChurchDTOs.DTOs.Entities
{
    public class DioceseDto
    {
        public int DioceseId { get; set; }
        public string DioceseName { get; set; }
        public string Address { get; set; }
        public string ContactInfo { get; set; }
        public string Territory { get; set; }
    }
    public class DioceseDetailDto
    {
        public int DioceseId { get; set; }
        public string DioceseName { get; set; }
        public List<DistrictSimpleDto> Districts { get; set; }        
    }

    public class  ParishesAllDto
    {
      public List<DioceseDetailDto> AllParishes { get; set; }
    }
}
