using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System;
using System.ComponentModel;
using static Program.Startup;

static partial class Program
{
    static async Task Main(string[] args)
    {
        // Change to the directory where the executable is located
        var executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var executableDirectory = Path.GetDirectoryName(executablePath);

        if (!string.IsNullOrEmpty(executableDirectory))
        {
            Directory.SetCurrentDirectory(executableDirectory);
        }

        var builder = WebHost.CreateDefaultBuilder(args);
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Load configuration from appsettings.json and environment variables
            config
                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                  .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                  .AddEnvironmentVariables();
        })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .UseStartup<Startup>();

        var app = builder.Build();

        // Start the MCP server
        await app.RunAsync();
    }
}

static partial class Program
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            // Add application services
            services
            .AddMemoryCache()
            .AddHttpClient()
            .AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly();

            // Add our supplier search service
            services.AddScoped<IDispatchService, DispatchService>();
        }

        public interface IDispatchService
        {
        }

        public class DispatchService : IDispatchService
        {
        }

        public void Configure(
            IApplicationBuilder app,
            IHostApplicationLifetime lifetime)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMcp();
            });
        }
    }

    public class VehicleStatus
    {
        public string VehicleId { get; set; }
        public string IncidentId { get; set; }
        public string Status { get; set; }
        public Coordinate Coordinate { get; set; }

    }

    public class IncidentStatus
    {
        public string IncidentId { get; set; }
        public List<string> VehicleIds { get; set; }
        public string Status { get; set; }
        public Coordinate Coordinate { get; set; }

    }

    public class Coordinate
    { 
        public int PosX { get; set; }
        public int PosY { get; set; }
    }

    [McpServerToolType]
    public static class SupplierSearchTools
    {

        [McpServerTool, Description("Get a list of all ambulances")]
        public static async Task<List<VehicleStatus>> ListAmbulances(IDispatchService dispatchService)
        {
            return [];
        }

        [McpServerTool, Description("Get a list of all Incidents")]
        public static async Task<List<VehicleStatus>> ListIncidents(IDispatchService dispatchService)
        {
            return [];
        }
    }
}

