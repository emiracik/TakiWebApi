using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TakiWebApi.Security;

namespace TakiWebApi.Controllers;

/// <summary>
/// KVKK m.11 / Aydınlatma Metni §13: "Hesap Bilgilerini Dışa Aktar".
/// Token sahibinin (yolcu ya da sürücü) kendi kişisel verilerini tek bir
/// JSON belgesi olarak döner. Şifre özeti, oturum anahtarları ve belge
/// dosyalarının kendisi dahil edilmez (belgeler uygulamadan görüntülenebilir).
/// </summary>
[ApiController]
[Route("api/my-data")]
[Authorize]
public class MyDataController : ControllerBase
{
    private readonly string _connectionString;

    public MyDataController(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("SqlServerConnection") ?? "";
    }

    [HttpGet]
    public async Task<IActionResult> Export()
    {
        if (User.CallerId() is not int id) return Forbid();
        var isDriver = User.IsAnyDriver();
        var sections = isDriver
            ? new (string, string)[]
            {
                ("hesap", "SELECT DriverID, FullName, PhoneNumber, Email, VehiclePlate, VehicleModel, VehicleColor, CreatedDate FROM Drivers WHERE DriverID = @Id"),
                ("yolculuklar", "SELECT TripID, StartAddress, EndAddress, StartTime, EndTime, Cost, PaymentMethod, Distance, Status FROM Trips WHERE DriverID = @Id ORDER BY TripID DESC"),
                ("kazanclar", "SELECT * FROM DriverEarnings WHERE DriverID = @Id"),
                ("belgeler", "SELECT DocumentID, DocumentType, UploadedDate FROM DriverDocuments WHERE DriverID = @Id"),
                ("banka_bilgisi", "SELECT BankName, IBAN, CreatedDate FROM BankInfo WHERE DriverID = @Id"),
                ("aldigim_puanlar", "SELECT TripID, Rating, Comment, CreatedDate FROM DriverRatings WHERE DriverID = @Id"),
                ("verdigim_puanlar", "SELECT TripID, RatedUserID, Rating, Comment, CreatedDate FROM UserRatings WHERE RatedByDriverID = @Id"),
                ("mesajlar", "SELECT SenderID, ReceiverID, Content, SentAt FROM Messages WHERE SenderID = -@Id OR ReceiverID = -@Id ORDER BY SentAt"),
            }
            : new (string, string)[]
            {
                ("hesap", "SELECT UserID, FullName, PhoneNumber, Email, CreatedDate FROM Users WHERE UserID = @Id"),
                ("adresler", "SELECT * FROM UserAddresses WHERE UserID = @Id AND IsDeleted = 0"),
                ("yolculuklar", "SELECT TripID, StartAddress, EndAddress, StartTime, EndTime, Cost, PaymentMethod, Distance, Status FROM Trips WHERE PassengerID = @Id ORDER BY TripID DESC"),
                ("talepler", "SELECT MatchingRequestID, PickupAddress, DropoffAddress, EstimatedCost, Status, RequestTime, Notes FROM MatchingRequests WHERE PassengerID = @Id ORDER BY MatchingRequestID DESC"),
                ("planli_yolculuklar", "SELECT * FROM ScheduledRides WHERE UserID = @Id"),
                ("cuzdan", "SELECT * FROM Wallets WHERE UserID = @Id"),
                ("kayitli_kartlar", "SELECT CardHolderName, CardNumberMasked, CardType, ExpiryMonth, ExpiryYear FROM UserCreditCards WHERE UserID = @Id AND IsDeleted = 0"),
                ("verdigim_puanlar", "SELECT TripID, DriverID, Rating, Comment, CreatedDate FROM DriverRatings WHERE UserID = @Id"),
                ("aldigim_puanlar", "SELECT TripID, Rating, Comment, CreatedDate FROM UserRatings WHERE RatedUserID = @Id"),
                ("destek_talepleri", "SELECT TripID, Comment, CreatedDate FROM Feedbacks WHERE UserID = @Id"),
                ("bildirim_tercihleri", "SELECT AllowTripUpdates, AllowPromotions, AllowNews FROM UserNotificationSettings WHERE UserID = @Id"),
                ("mesajlar", "SELECT SenderID, ReceiverID, Content, SentAt FROM Messages WHERE SenderID = @Id OR ReceiverID = @Id ORDER BY SentAt"),
            };

        var result = new Dictionary<string, object?>
        {
            ["olusturulma"] = DateTime.UtcNow,
            ["hesap_turu"] = isDriver ? "surucu" : "yolcu",
        };
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        foreach (var (name, sql) in sections)
        {
            try
            {
                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Id", id);
                using var r = await command.ExecuteReaderAsync();
                var rows = new List<Dictionary<string, object?>>();
                while (await r.ReadAsync())
                {
                    var row = new Dictionary<string, object?>();
                    for (var i = 0; i < r.FieldCount; i++)
                    {
                        var col = r.GetName(i);
                        // Hassas teknik alanlar dışarı verilmez.
                        if (col.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
                            col.Contains("Token", StringComparison.OrdinalIgnoreCase)) continue;
                        row[col] = r.IsDBNull(i) ? null : r.GetValue(i);
                    }
                    rows.Add(row);
                }
                result[name] = name == "hesap" ? rows.FirstOrDefault() : rows;
            }
            catch (SqlException)
            {
                result[name] = null; // tablo/sütun bu ortamda yoksa bölüm boş geçilir
            }
        }
        return Ok(result);
    }
}
