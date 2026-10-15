using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TakiWebApi.Security;

namespace TakiWebApi.Hubs;

// Anlık güncellemeler (/hubs/ride). Sunucu, istemciye "update" olayıyla
// yalnızca ne değiştiğini söyler ({ type, ... }); istemci ilgili veriyi
// mevcut REST uçlarından yeniden çeker. Bağlantı yoksa istemci polling'e döner.
//
// Gruplar: "user-{id}" / "driver-{id}" (token sahibi), "drivers" (bekleyen
// talep listesi), "driver-location-{id}" (o sürücünün konumunu izleyenler).
[Authorize]
public class RideHub : Hub
{
    public const string Drivers = "drivers";

    public static string Subject(string subjectType, int id) => $"{subjectType}-{id}";
    public static string DriverLocation(int driverId) => $"driver-location-{driverId}";

    public override async Task OnConnectedAsync()
    {
        var user = Context.User!;
        var type = user.FindFirstValue("user_type");
        if (user.CallerId() is int id && (type == "user" || type == "driver"))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, Subject(type, id));
            if (type == "driver")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, Drivers);
            }
        }
        await base.OnConnectedAsync();
    }

    // Sürücü konumu REST'te de her oturum açmış kullanıcıya açık
    // (GET /api/Matching/driver/{id}/location), burada da aynı kural geçerli.
    public Task WatchDriver(int driverId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, DriverLocation(driverId));

    public Task UnwatchDriver(int driverId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, DriverLocation(driverId));
}
