using BalanzaService.Measure;
using BalanzaService.Sap;
using BalanzaService.Web;
using Serilog;
using System.Net;
using System.Net.Sockets;

// Configurar ruta de logs junto al ejecutable
var exePath = Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
var logsPath = Path.Combine(exePath, "logs");
Directory.CreateDirectory(logsPath);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(logsPath, "balanza-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Iniciando aplicacion...");
    Log.Information("Logs en: {LogsPath}", logsPath);

    var builder = WebApplication.CreateBuilder(args);

    // Usar Serilog
    builder.Host.UseSerilog();

    // Configurar como Windows Service
    builder.Host.UseWindowsService();

    // CORS - permitir cualquier origen
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    builder.Services.AddSingleton<BalanzaManager>();
    builder.Services.AddSingleton<SapService>();
    builder.Services.AddHostedService<BalanzaWorker>();

    var app = builder.Build();

    app.UseCors();
    app.MapBalanzaEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicacion fallo al iniciar");
}
finally
{
    Log.CloseAndFlush();
}

public class BalanzaWorker : BackgroundService
{
    private readonly BalanzaManager _balanzaManager;
    private readonly SapService _sapService;
    private readonly ILogger<BalanzaWorker> _logger;
    private readonly int _intervaloMs;
    private readonly string _ipAddress;

    public BalanzaWorker(
        BalanzaManager balanzaManager,
        SapService sapService,
        IConfiguration config,
        ILogger<BalanzaWorker> logger)
    {
        _balanzaManager = balanzaManager;
        _sapService = sapService;
        _logger = logger;
        _intervaloMs = config.GetValue<int>("Worker:IntervaloMs", 3000);
        _ipAddress = ObtenerIpLocal();
    }

    private static string ObtenerIpLocal()
    {
        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            var endPoint = socket.LocalEndPoint as IPEndPoint;
            return endPoint?.Address.ToString() ?? "127.0.0.1";
        }
        catch
        {
            return "127.0.0.1";
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("===========================================");
        _logger.LogInformation("Iniciando Servicio de Balanza");
        _logger.LogInformation("IP Local: {IP}", _ipAddress);
        _logger.LogInformation("===========================================");

        try
        {
            await _balanzaManager.IniciarAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo iniciar el puerto serial. El servicio continuara solo con el servidor web.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _balanzaManager.LeerPesoAsync();

                if (_balanzaManager.PesoActual > 0)
                {
                    var enviado = await _sapService.EnviarPesoAsync(_ipAddress, _balanzaManager.PesoActual, stoppingToken);
                    if (enviado)
                    {
                        _balanzaManager.ResetearPesoActual();
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Cancelación normal, no es error
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ciclo de lectura");
            }

            await Task.Delay(_intervaloMs, stoppingToken);
        }
    }
}
