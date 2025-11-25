using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Fleck;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Server.Domain;
using Server.Domain.Repositories.CoordinatorInstanceRepository;
using Server.Domain.Repositories.ServerInstanceRepository;
using Server.Domain.Repositories.UserRepository;
using Server.Domain.Repositories.VideoSessionRepository;
using Server.MainServer.Main.Server.Bootstrap;
using Server.MainServer.Main.Server.Coordinator;
using Server.MainServer.Main.Server.Coordinator.WebSocket;
using Server.MainServer.Main.Server.Factories.ClientConnectionFactory;
using Server.MainServer.Main.Server.Factories.CoordinatorFactory;
using Server.MainServer.Main.Server.Factories.PeerManagerFactory;
using Server.MainServer.Main.Server.Factories.VideoSessionFactory;
using Server.MainServer.Main.Server.Orchestrator;
using SIPSorceryMedia.FFmpeg;

namespace Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Configuring services...");
                
                FleckLog.LogAction = (_, _, _) => { };
                var builder = CreateWebApplicationBuilder(args);
                var configuration = BuildConfiguration(builder.Environment.ContentRootPath);
                var config = configuration.GetSection("Project").Get<InitialServerLoaderContext>()!;
                
                ConfigureServices(builder.Services, config);
                ConfigureLogging(builder.Host, configuration);
                
                var app = builder.Build();
                
                var serverLoader = app.Services.GetRequiredService<InitialServerLoader>();
                serverLoader.SetContext(config);
                if (!serverLoader.FfmpegInitialize())
                {
                    Console.WriteLine("Cannot initialize ffmpeg.");
                }
                
                Console.WriteLine("Waiting for ASP .NET...");
                ConfigureMiddleware(app);
                await app.RunAsync();
            }
            catch (SqliteException e)
            {
                Console.WriteLine("\n----------------FATAL ERROR----------------\n");
                Console.WriteLine("Database error: {0} ", e);
            }
            catch (Exception e)
            {
                Console.WriteLine("\n----------------FATAL ERROR----------------\n");
                Console.WriteLine("Unexcepted exception occured: {0} ", e);
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
            services.AddHostedService<OrchestratorService>();
            services.AddControllersWithViews();

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

            services.AddSingleton<InitialServerLoaderContext>();
            services.AddSingleton<InitialServerLoader>();
            
            services.AddSingleton<IVideoSessionFactory, VideoSessionFactory>();
            services.AddSingleton<ICoordinatorInstanceFactory, CoordinatorInstanceFactory>();
            services.AddSingleton<IPeerManagerFactory, PeerManagerFactory>();
            services.AddSingleton<IClientConnectionFactory, ClientConnectionFactory>();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton<IOrchestratorInstanceContext, OrchestratorInstanceContext>();
            services.AddSingleton<IOrchestratorInstance, OrchestratorInstance>();
            
            services.AddScoped<IOrchestratorInstanceRepository, OrchestratorInstanceRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICoordinatorInstanceRepository, CoordinatorInstanceRepository>();
            services.AddScoped<IVideoSessionRepository, VideoSessionRepository>();
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

        public class OrchestratorServices(
            ICoordinatorInstance coordinatorInstance, 
            ILogger<CoordinatorInstance> logger,
            IServiceProvider provider) : BackgroundService
        {
            private WebSocketServer? _wsServer;
            
            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                FleckLog.LogAction = (_, _, _) => { };
                var cert = X509CertificateLoader.LoadPkcs12FromFile("Certs/server.pfx", "MyPassword");
                _wsServer = new WebSocketServer("wss://0.0.0.0:26666");
                _wsServer.EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                _wsServer.Certificate = cert;

                _wsServer.Start(socket =>
                {
                    var handler = ActivatorUtilities.CreateInstance<CoordinatorWebSocketInstance>(
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
        
        public class OrchestratorService(IOrchestratorInstance orchestratorInstance, InitialServerLoader initialServerLoader) : BackgroundService
        {
            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                await orchestratorInstance.Configure(initialServerLoader);
                await orchestratorInstance.StartAsync(CancellationToken.None);
            }
        }
    }
}
