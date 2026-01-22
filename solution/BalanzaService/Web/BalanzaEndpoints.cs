using BalanzaService.Measure;

namespace BalanzaService.Web;

public static class BalanzaEndpoints
{
    public static void MapBalanzaEndpoints(this WebApplication app)
    {
        app.MapGet("/balanza", (BalanzaManager balanzaManager) =>
        {
            var peso = balanzaManager.UltimoPeso.ToString("F2");
            return Results.Json(new { peso });
        });

        app.MapGet("/health", () => Results.Ok("OK"));
    }
}
