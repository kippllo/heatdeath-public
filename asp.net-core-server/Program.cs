/*
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
*/

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Keybored.BackendServer.Settings;

namespace Keybored.BackendServer.Driver {

    /// <summary> Driver class, where it all starts! </summary>
    public class Program {

        /// <summary> Driver method.</summary>
        /// <remarks> <para>This method calls <see cref="ServerSettings.ReadSettings()"/> before the UDP server is initialized so all setting a ready for action! </para>
        /// <para> It also calls <c>WebHost.CreateDefaultBuilder(string[])</c> to start the HTTP server on this <see cref="ServerSettings.port"/>. </para>
        /// </remarks>
        /// <param name="args"> Should not be needed. It can be left blank.</param>
        public static void Main(string[] args) {
            ServerSettings.ReadSettings();

            CreateWebHostBuilder(args)
			.UseUrls("http://*:" + ServerSettings.port + "/")
			.Build().Run();
        }

        /// <summary> ASP.Net Core server setup method. </summary>
        /// <remarks> I left a lot of stuff with the default settings. </remarks>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
            return WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();
        }
    }
}
