using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TakiWebApi.Security;

// Bu uçta URL'deki userId/driverId başka bir kullanıcıya ait olabilir
// (ör. yolcunun sürücü puanını veya konumunu görmesi).
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AllowAnyCallerAttribute : Attribute { }

// Controller, mesajlaşma kimliklerini kullanıyor: sürücü = -driverId.
[AttributeUsage(AttributeTargets.Class)]
public sealed class MessagingIdsAttribute : Attribute { }

// URL/sorgu parametresi olarak userId, passengerId veya driverId alan her
// uçta bu kimliğin token sahibine ait olduğunu doğrular (IDOR koruması).
// Kayıt id'si ile çalışan uçların (ör. /trips/{id}) sahiplik kontrolü
// controller içinde yapılır.
public sealed class ResourceOwnerFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var metadata = context.ActionDescriptor.EndpointMetadata;
        if (metadata.OfType<IAllowAnonymous>().Any() || metadata.OfType<AllowAnyCallerAttribute>().Any())
            return;

        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true || user.IsAdmin())
            return;

        var messaging = metadata.OfType<MessagingIdsAttribute>().Any();

        foreach (var (name, value) in context.ActionArguments)
        {
            if (value is not int id)
                continue;

            var allowed = name.ToLowerInvariant() switch
            {
                "userid" or "passengerid" => messaging ? user.IsMessageParty(id) : user.IsPassenger(id),
                "driverid" => user.IsDriver(id),
                _ => true
            };

            if (!allowed)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
