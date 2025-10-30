using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Fleck;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Server.Domain;
using Server.MainServer.Main.Server.Coordinator;
using Server.MainServer.Main.Server.Factories.ClientConnectionFactory;
using Server.MainServer.Main.Server.Factories.CoordinatorFactory;
using Server.MainServer.Main.Server.Factories.PeerManagerFactory;
using Server.MainServer.Main.Server.Factories.VideoSessionFactory;
using Server.MainServer.Main.Server.Orchestrator.InitialServerLoader;
using Server.MainServer.Main.WebSocket;
using SIPSorceryMedia.FFmpeg;

namespace Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    theme: AnsiConsoleTheme.Code,
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] [Bootstrap] {Message:lj}{NewLine}")
                .CreateBootstrapLogger();

            try
            {
                if (!FfmpegInitialize())
                {
                    Log.Logger.Warning("Cannot initialize ffmpeg. Test sessions disabled.");
                }

                var builder = CreateWebApplicationBuilder(args);
                var configuration = BuildConfiguration(builder.Environment.ContentRootPath);
                var config = configuration.GetSection("Project").Get<InitialServerLoaderContext>()!;
                
                ConfigureServices(builder.Services, config);
                ConfigureLogging(builder.Host, configuration);

                var app = builder.Build();
                ConfigureMiddleware(app);

                await app.RunAsync();
            }
            catch (Exception e)
            {
                Log.Logger.Fatal($"Failed to start server. Fatal error occured. \n Exception: {e} ");
            }
        }

        private static bool FfmpegInitialize()
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_ERROR);
                }
                else if (OperatingSystem.IsLinux())
                {
                    FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_ERROR, AppContext.BaseDirectory);
                }
                return true;
            }
            catch (ApplicationException ex)
            {
                Console.WriteLine("Unable to initialize ffmpeg libraries. Server stoped.");
                return false;
            }
        }
        
        private static WebApplicationBuilder CreateWebApplicationBuilder(string[] args)
        {
            var options = new WebApplicationOptions
            {
                Args = args,
                WebRootPath = "Web/wwwroot"
            };

            return WebApplication.CreateBuilder(options);
        }

        private static IConfiguration BuildConfiguration(string contentRootPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(contentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static void ConfigureLogging(IHostBuilder host, IConfiguration configuration)
        {
            host.UseSerilog((context, services, loggerConfig) =>
            {
                loggerConfig
                    .ReadFrom.Configuration(configuration)
                    .ReadFrom.Services(services)
                    .WriteTo.Console(
                        theme: AnsiConsoleTheme.Code,
                        outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}")
                    .Enrich.FromLogContext();
            });
        }

        private static void ConfigureServices(IServiceCollection services, InitialServerLoaderContext configuratorContext)
        {
            services.AddControllersWithViews();
            services.AddHostedService<WebSocketHostedService>();

            services.AddDbContext<ServerDbContext>(options =>
                options.UseSqlite(configuratorContext.Database.ConnectionString));

            services.AddIdentity<IdentityUser, IdentityRole>(opts =>
            {
                opts.Password.RequiredLength = 6;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = false;
            })
            .AddEntityFrameworkStores<ServerDbContext>()
            .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(cookie =>
            {
                cookie.Cookie.Name = "VideoServerCookie";
                cookie.Cookie.HttpOnly = true;
                cookie.LoginPath = "/Account/Login";
                cookie.AccessDeniedPath = "/Account/AccessDenied";
                cookie.SlidingExpiration = true;
            });

            services.AddSingleton<IVideoSessionFactory, VideoSessionFactory>();
            services.AddSingleton<ICoordinatorFactory, CoordinatorFactory>();
            services.AddSingleton<IPeerManagerFactory, PeerManagerFactory>();
            services.AddSingleton<IClientConnectionFactory, ClientConnectionFactory>();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton<ICoordinatorInstance, CoordinatorInstance>();
            services.AddSingleton<ICoordinatorInstanceContext, CoordinatorInstanceContext>();
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Download}/{id?}");
        }

        public class WebSocketHostedService(
            ICoordinatorInstance coordinatorInstance, 
            ILogger<CoordinatorInstance> logger,
            IServiceProvider provider) : BackgroundService
        {
            private WebSocketServer? _wsServer;
            
            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                FleckLog.LogAction = (_, _, _) => { };
                //await coordinatorInstance.RegisterTestSessions();
                var cert = X509CertificateLoader.LoadPkcs12FromFile("Certs/server.pfx", "MyPassword");
                _wsServer = new WebSocketServer("wss://0.0.0.0:26666");
                _wsServer.EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                _wsServer.Certificate = cert;

                _wsServer.Start(socket =>
                {
                    var handler = ActivatorUtilities.CreateInstance<MainWebSocket>(
                        provider,
                        coordinatorInstance,
                        socket
                    );

                    socket.OnOpen = handler.OnOpen;
                    socket.OnClose = handler.OnClose;
                    socket.OnMessage = handler.OnMessage;
                    socket.OnError = handler.OnError;
                });                
            }
        }
    }
}
