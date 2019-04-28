/**
 * @description Startup and AppHost class inheriting from the ServiceStack's AppHostBase base class
 * @copyright (c) 2017 HIC Limited (NZBN: 9429043400973)
 * @author Martin Stellnberger
 * @license GPL-3.0
 */

using System;
using System.Reflection;
using System.Runtime.Versioning;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using System.Web;
using Funq;
using Microsoft.Extensions.Logging;
using ServiceStack;
using ServiceStack.Admin;
using ServiceStack.Data;
using ServiceStack.Configuration;
using ServiceStack.Validation;
using ApiService.ServiceInterface;
using ApiService.ServiceModel;


namespace ApiService
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Register the service stack license key (the free license only allows for up to 10 endpoints)
            Licensing.RegisterLicenseFromFileIfExists("config/ServiceStackLicense.txt");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Call the app model config class and provide the environment variable for initialisation
            AppModelConfig.setEnvironment(env);

            // Create a log entry for the environment that has been pick up from the operating system.
            Console.WriteLine("");
            Console.WriteLine("**********************************************************************************************");
            Console.WriteLine("*** Startup time (UTC)   :  " + System.DateTime.UtcNow.ToString("yyyy-MM-dd   hh:mm:ss"));
            Console.WriteLine("**********************************************************************************************");
            Console.WriteLine("***  Application Name    :  " + env.ApplicationName);
            Console.WriteLine("***  Hosting environment :  " + env.EnvironmentName);
            Console.WriteLine("***  .NET Framework      :  " + Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName);
            Console.WriteLine("***  Web 3 URL endpoint  :  " + AppModelConfig.WEB3_URL_ENDPOINT);
            Console.WriteLine("***  Web Root Path       :  " + env.WebRootPath);
            Console.WriteLine("***  Content Root Path   :  " + env.ContentRootPath);
            Console.WriteLine("***********************************************************************************************");
            Console.WriteLine("");

            // Environment names syntax is '<operating system>'_'Development, Staging or Production>'
            // e.g. 'macOS_Development' or 'Ubuntu_Production'
            // If it is a development environment show the developer exception page
            if (env.EnvironmentName.Contains("Development"))
            {
                app.UseDeveloperExceptionPage();
            }
            
            // If the operating system is Ubuntu enable Header forwarding
            if (env.EnvironmentName.Contains("Ubuntu"))
            {
                // *** Is required for hosting of NetCore solution on Ubuntu ***
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }

            // Start the app and specify servicestack to load
            app.UseServiceStack(new AppHost());
        }
    }

    public class AppHost : AppHostBase
    {
        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public AppHost() : base("ApiService", typeof(EcosystemServices).Assembly) { }

        /// <summary>
        /// Application specific configuration
        /// This method should initialize any IoC resources utilized by your web service classes.
        /// </summary>
        public override void Configure(Container container)
        {
            // Disable configuration feator of using camel case notiation
            SetConfig(new HostConfig
            { 
                UseCamelCase = false,
                DefaultRedirectPath = "/metadata"
            });

            // Enable Postman and Cors Feature
            this.Plugins.Add(new PostmanFeature());
            this.Plugins.Add(new CorsFeature());

            // Configure Json as the default response type
            this.Config.DefaultContentType = MimeTypes.Json;
            // Configure Json as the only allowed request type; Remove all others
            this.Config.EnableFeatures = Config.EnableFeatures.Remove(Feature.Xml | Feature.Jsv | Feature.Csv | Feature.Soap11 | Feature.Soap12);

            // Initialise the Validation Feature.
            this.Plugins.Add(new ValidationFeature());
            container.RegisterValidators(typeof(GetEcosystemLogsValidator).Assembly);
        }
    }
}
