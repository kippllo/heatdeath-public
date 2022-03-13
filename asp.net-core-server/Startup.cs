/*
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
*/

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Keybored.BackendServer.Network;


namespace Keybored.BackendServer.Driver {

    public class Startup {

        /// <summary> Constructor for ASP.Net Core server setup class. </summary>
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        /// <value> ASP.Net Core server config class. </value>
        public IConfiguration Configuration { get; }


        /// <summary> This method gets called by the runtime. Use this method to add services to the container. </summary>
        /// <remarks> <para> I left most of the ASP stuff to the default settings. </para>
        /// <para> But this method is where I create the singleton instance of <see cref="Master"/> that is used in DI to start the UDP game server. </para>
        /// <para> Thus, this method is the starting place for all UDP and game functionality! </para>
        /// <para> This also means that any future global DI service needs to be added here. </para>
        /// <para> P.S. ASP.Net console logging is also setup here. </para>
        /// </remarks>
        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			
			services.AddSingleton(typeof(Master)); //This must be added for DI! Help: https://www.tutorialsteacher.com/core/dependency-injection-in-aspnet-core      And: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.2

			services.AddLogging(config => { //Help: https://weblog.west-wind.com/posts/2018/Dec/31/Dont-let-ASPNET-Core-Default-Console-Logging-Slow-your-App-down#where-does-default-logging-configuration-come-from
				config.ClearProviders();
				
				config.AddConfiguration(Configuration.GetSection("Logging"));
				config.AddDebug();
				config.AddEventSourceLogger();
				
				if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == EnvironmentName.Development) {
					config.AddConsole();
				}
			});
        }

        /// <summary> This method gets called by the runtime. Use this method to configure the HTTP request pipeline. </summary>
        /// <remarks> I left this with default settings. </remarks>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {            
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
