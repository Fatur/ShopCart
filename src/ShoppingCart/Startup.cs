using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShoppingCart.Models;
using Serilog;
namespace ShoppingCart
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.ColoredConsole(Serilog.Events.LogEventLevel.Verbose,
                "{NewLine}{Timestamp:HH:mm:ss} [{Level}] ({CorrelationToken}) {Message}{NewLine}{Exception}")
                .CreateLogger();
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IShoppingCartStore, InMemoryShoppingCartStore>();
            services.AddSingleton<IProductCatalogClient, ProductCatalogClient>();
             services.AddSingleton<IEventStore, InMemoryEventStore>();
            //services.AddSingleton<IEventStore, ESEventStore>();
            services.AddSingleton<ICache, Cache>();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();
            loggerFactory.AddSerilog();
           
            app.UseOwin(buildFunc =>
            {
                buildFunc(next => GlobalErrorLogging.Middleware(next, Log.Logger));
                buildFunc(next => CorrelationToken.Middleware(next));
                buildFunc(next => RequestLogging.Middleware(next, Log.Logger));
                buildFunc(next => PerformanceLogging.Middleware(next, Log.Logger));
                buildFunc(next =>new MonitoringMiddleware(next, HealthCheck).Invoke);
            });

            app.UseMvc();
        }

        private Task<bool> HealthCheck()
        {
            return Task.FromResult<bool>(false);
        }
    }
}
