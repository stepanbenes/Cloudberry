using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Http;
using WebSocketManager;
using Cloudberry.Data;
using Cloudberry.DeepSpaceNetwork;

namespace Cloudberry
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRazorPages();
			services.AddServerSideBlazor(options => options.DetailedErrors = true);
			services.AddSingleton<ICpuTemperatureService, RealCpuTemperatureService>();
			services.AddScoped<MarksDiaryService>();
			services.AddScoped<DataBackupService>();
			services.AddWebSocketManager();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
		{
			app.UseForwardedHeaders(new ForwardedHeadersOptions
			{
				ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All
			});

			app.UseDeveloperExceptionPage();

			//app.UseExceptionHandler("/Home/Error");
			//// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			//app.UseHsts();

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			string rootDirectory = MarksDiaryService.SourceDirectoryPath;
			if (Directory.Exists(rootDirectory))
			{
				app.UseStaticFiles(new StaticFileOptions()
				{
					FileProvider = new PhysicalFileProvider(MarksDiaryService.SourceDirectoryPath),
					RequestPath = new PathString("/denik")
				});
			}
            app.UseWebSockets();

			app.UseRouting();

			app.MapWebSocketManager("/deep-space-network", serviceProvider.GetService<RobotMessageHandler>());

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
			});
		}
	}
}
