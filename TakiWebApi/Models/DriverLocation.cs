using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("DriverLocations")]
public class DriverLocation
{
    [Key]
    public int LocationID { get; set; }

    [Required]
    public int DriverID { get; set; }

    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }

    public bool IsAvailable { get; set; } = true;

    [StringLength(100)]
    public string? CurrentAddress { get; set; }

    public DateTime LocationTime { get; set; } = DateTime.UtcNow;

    public double? Speed { get; set; } // km/h

    public double? Heading { get; set; } // degrees

    public bool IsOnline { get; set; } = true;

    // Audit fields
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }

    // Navigation properties
    public virtual Driver? Driver { get; set; }
}

// Response models for API
public class NearbyDriverResponse
{
    public int DriverID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? VehiclePlate { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehicleColor { get; set; }
    public double Distance { get; set; } // km
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public bool IsAvailable { get; set; }
    public string EstimatedArrival { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class MatchingResponse
{
    public int MatchingRequestID { get; set; }
    public MatchingStatus Status { get; set; }
    public int? DriverID { get; set; }
    public string? DriverName { get; set; }
    public string? VehiclePlate { get; set; }
    public string? DriverPhone { get; set; }
    public double? Distance { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? EstimatedArrival { get; set; }
    public DateTime RequestTime { get; set; }
    public DateTime? AcceptedTime { get; set; }
    public string Message { get; set; } = string.Empty;

    // Talep kabul edildiğinde oluşturulan gerçek yolculuk (Trips) kaydının
    // ID'si — yolculuk bitirildiğinde/tekrar açıldığında bu kayda geri
    // dönebilmek için (geçmiş, kazanç, puanlama).
    public int? TripID { get; set; }

    // Uygulanan promosyon indirimi (varsa).
    public decimal? Discount { get; set; }

    // Yolcu iptalinde kesilecek iptal ücreti (varsa).
    public decimal? CancellationFee { get; set; }
}

public class FindNearbyDriversRequest
{
    [Required]
    public double Latitude { get; set; }
    
    [Required]
    public double Longitude { get; set; }
    
    [Range(0.1, 50.0)]
    public double Radius { get; set; } = 5.0; // km
    
    [Range(1, 100)]
    public int MaxResults { get; set; } = 10;
    
    [Range(0.0, 5.0)]
    public double? MinRating { get; set; }
}

public class CreateMatchingRequest
{
    [Required]
    public int PassengerID { get; set; }

    [Required]
    [StringLength(500)]
    public string PickupAddress { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string DropoffAddress { get; set; } = string.Empty;

    [Required]
    public double PickupLatitude { get; set; }

    [Required]
    public double PickupLongitude { get; set; }

    [Required]
    public double DropoffLatitude { get; set; }

    [Required]
    public double DropoffLongitude { get; set; }

    public double? MaxWaitTime { get; set; } = 15.0; // minutes
    public double? MinRating { get; set; } = 3.0;
    public double? MaxDistance { get; set; } = 10.0; // km

    // Araç sınıfı: "economy" (Sarı Taksi), "comfort" (Turkuaz), "premium" (Siyah/Lüks).
    // Verilmezse "economy" tarifesi uygulanır.
    public string? VehicleType { get; set; }

    // Yolcunun kendi önerdiği ücret (teklif sistemi). Doluysa ve pozitifse,
    // hesaplanan tarife yerine bu tutar geçerli olur (örn. "400 TL'ye götürür müsün?").
    public decimal? OfferedFare { get; set; }
    public string? Notes { get; set; }
    public List<int>? PreferredDrivers { get; set; }

    // Ara duraklar (en fazla 2), alış → duraklar → varış sırasıyla.
    public List<StopPoint>? Stops { get; set; }

    // Promosyon kodu; geçerliyse indirim sunucuda uygulanır.
    public string? PromoCode { get; set; }

    // Controller'ın doğruladığı promosyon (istemciden okunmaz).
    [System.Text.Json.Serialization.JsonIgnore]
    public Promotion? AppliedPromotion { get; set; }
}

public class StopPoint
{
    [StringLength(300)]
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class UpdateDriverLocationRequest
{
    [Required]
    public double Latitude { get; set; }
    
    [Required]
    public double Longitude { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    public string? CurrentAddress { get; set; }
    public double? Speed { get; set; }
    public double? Heading { get; set; }
}