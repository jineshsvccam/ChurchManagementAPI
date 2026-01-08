using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;


namespace ChurchData
{
    public class Parish
    {
        public int ParishId { get; set; }
        public required string ParishName { get; set; }
        public string? ParishLocation { get; set; }
        public string? Photo { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Place { get; set; }
        public string? Pincode { get; set; }
        public string? VicarName { get; set; }
        public int DistrictId { get; set; }

        // Map to the database column "geo_location" of type geography(Point,4326)
        [Column("geo_location", TypeName = "geography (Point,4326)")]
        public Point? GeoLocation { get; set; }

        public District? District { get; set; }
        public ICollection<Unit> Units { get; set; } = new List<Unit>();
        public ICollection<Family> Families { get; set; } = new List<Family>();
        public ICollection<TransactionHead> TransactionHeads { get; set; } = new List<TransactionHead>();
        public ICollection<Bank> Banks { get; set; } = new List<Bank>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<FinancialYear> FinancialYears { get; set; } = new List<FinancialYear>();
        [JsonIgnore]
        public ICollection<User> Users { get; set; } = new List<User>();
    }

}
