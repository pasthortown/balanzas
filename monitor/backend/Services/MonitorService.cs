using MongoDB.Driver;
using BalanzasMonitor.Models;
using System.Text.Json;

namespace BalanzasMonitor.Services;

public class MonitorService : BackgroundService
{
    private readonly IMongoCollection<Balanza> _balanzas;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MonitorService> _logger;
    private readonly TimeSpan _intervalo = TimeSpan.FromSeconds(5);
    private readonly int _maxConcurrency = 50;

    public MonitorService(
        IMongoDatabase database,
        IHttpClientFactory httpClientFactory,
        ILogger<MonitorService> logger)
    {
        _balanzas = database.GetCollection<Balanza>("balanzas");
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Monitor de balanzas iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MonitorearBalanzasAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el ciclo de monitoreo");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }
    }

    private async Task MonitorearBalanzasAsync(CancellationToken ct)
    {
        var balanzas = await _balanzas.Find(_ => true).ToListAsync(ct);

        if (balanzas.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Monitoreando {Count} balanzas", balanzas.Count);

        var semaphore = new SemaphoreSlim(_maxConcurrency);
        var tasks = balanzas.Select(async balanza =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                await VerificarBalanzaAsync(balanza, ct);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task VerificarBalanzaAsync(Balanza balanza, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("BalanzaClient");
        var url = $"http://{balanza.Ip}/balanza";

        try
        {
            var response = await client.GetAsync(url, ct);

            var update = Builders<Balanza>.Update
                .Set(b => b.Estado, response.IsSuccessStatusCode ? "ok" : "error");

            if (response.IsSuccessStatusCode)
            {
                update = update.Set(b => b.UltimaConexion, DateTime.UtcNow);

                // Leer el peso del response
                var content = await response.Content.ReadAsStringAsync(ct);
                var pesoActual = ParsearPeso(content);

                if (pesoActual.HasValue)
                {
                    update = update.Set(b => b.UltimoPeso, pesoActual.Value);

                    // Solo actualizar la fecha de medición si el peso cambió
                    if (!balanza.UltimoPeso.HasValue || Math.Abs(balanza.UltimoPeso.Value - pesoActual.Value) > 0.001)
                    {
                        update = update.Set(b => b.UltimaMedicion, DateTime.UtcNow);
                        _logger.LogDebug("Balanza {Nombre} ({Ip}): Peso cambió de {PesoAnterior} a {PesoActual}",
                            balanza.Nombre, balanza.Ip, balanza.UltimoPeso, pesoActual.Value);
                    }
                }

                _logger.LogDebug("Balanza {Nombre} ({Ip}): OK - Peso: {Peso}", balanza.Nombre, balanza.Ip, pesoActual);
            }
            else
            {
                _logger.LogWarning("Balanza {Nombre} ({Ip}): HTTP {Status}",
                    balanza.Nombre, balanza.Ip, (int)response.StatusCode);
            }

            await _balanzas.UpdateOneAsync(
                b => b.Id == balanza.Id,
                update,
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Balanza {Nombre} ({Ip}): Error - {Message}",
                balanza.Nombre, balanza.Ip, ex.Message);

            await _balanzas.UpdateOneAsync(
                b => b.Id == balanza.Id,
                Builders<Balanza>.Update.Set(b => b.Estado, "error"),
                cancellationToken: ct);
        }
    }

    private double? ParsearPeso(string content)
    {
        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("peso", out var pesoElement))
            {
                var pesoStr = pesoElement.GetString();
                if (double.TryParse(pesoStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var peso))
                {
                    return peso;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error parseando peso: {Message}", ex.Message);
        }
        return null;
    }
}
