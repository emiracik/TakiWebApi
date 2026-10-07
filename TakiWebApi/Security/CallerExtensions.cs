using System.Security.Claims;

namespace TakiWebApi.Security;

// Token sahibinin (yolcu/sürücü) kimliğini okuyan yardımcılar. Kimlik her
// zaman token'dan alınır; URL veya gövdede gelen id'ye güvenilmez.
public static class CallerExtensions
{
    public static int? CallerId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public static bool IsPassenger(this ClaimsPrincipal user, int? passengerId) =>
        passengerId.HasValue &&
        user.FindFirstValue("user_type") == "user" &&
        user.CallerId() == passengerId;

    public static bool IsDriver(this ClaimsPrincipal user, int? driverId) =>
        driverId.HasValue &&
        user.FindFirstValue("user_type") == "driver" &&
        user.CallerId() == driverId;

    public static bool IsAnyDriver(this ClaimsPrincipal user) =>
        user.FindFirstValue("user_type") == "driver";

    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole("Admin");

    // Yolculuğun/talebin tarafı mı (yolcusu veya sürücüsü)?
    public static bool IsPartyOf(this ClaimsPrincipal user, int? passengerId, int? driverId) =>
        user.IsAdmin() || user.IsPassenger(passengerId) || user.IsDriver(driverId);

    // Mesajlaşmada sürücüler negatif kimlikle temsil ediliyor (driverId=6 -> -6).
    public static bool IsMessageParty(this ClaimsPrincipal user, int messagingId) =>
        user.IsAdmin() || user.IsPassenger(messagingId) || user.IsDriver(-messagingId);
}
