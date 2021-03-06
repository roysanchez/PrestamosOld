﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics.Entity;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using Microsoft.Dnx.Runtime;
using Prestamos.Models;
using Prestamos.ViewModels.Cliente;
using Prestamos.ViewModels.Prestamo;
using Prestamos.Services;
using Negocios;
using System.IO;
using Microsoft.AspNet.Mvc.Razor;
using Prestamos.Config.Views.LocationExpander;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using AutoMapper;

namespace Prestamos
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Setup configuration sources.

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // This reads the configuration keys from the secret store.
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                //builder.AddUserSecrets();
            }
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            

            // Add Entity Framework services to the services container.
            var directory = CrearDirectorio();
            var filename = Configuration["Data:Sqlite:Filename"];

            var stringdb = $"Data Source={directory}/{filename}.db";

            services.AddEntityFramework()
                .AddSqlite()
                .AddDbContext<PrestamoContext>(
                    option => option.UseSqlite(stringdb)
                )
                .AddDbContext<ApplicationDbContext>(
                    option => option.UseSqlite(stringdb)
                );

            // Add Identity services to the services container.
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add MVC services to the services container.
            services.AddMvc();
            services.AddCors();

            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();

            // Register application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddSingleton<IMapper>(c =>
            {
                var config =  new MapperConfiguration(d =>
                {
                    d.CreateMap<ClienteViewModel, Cliente>().ReverseMap();
                    d.CreateMap<PrestamoViewModel, Prestamo>().ReverseMap();
                });
                return config.CreateMapper();
            });

            //TODO borrar, ya que se usara una libreria js
            services.Configure<RazorViewEngineOptions>(c => c.ViewLocationExpanders.Add(new PrestamoLocationExpander()));

        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Loggin"));

            if (env.IsDevelopment())
            {
                loggerFactory.AddDebug();
            }

            // Configure the HTTP request pipeline.

            // Add the following to the request pipeline only in development environment.
            if (env.IsDevelopment())
            {
                //app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // sends the request to the following path or controller action.
                app.UseExceptionHandler("/Home/Error");
            }

            try
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    serviceScope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();
                    serviceScope.ServiceProvider.GetService<PrestamoContext>().Database.Migrate();
                }
            }
            catch { }


            // Add the platform handler to the request pipeline.
            //app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            // Add static files to the request pipeline.
            //https://github.com/aspnet/StaticFiles/issues/10

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Add cookie-based authentication to the request pipeline.
            app.UseIdentity();

            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            });

            // Add MVC to the request pipeline.
            app.UseMvc();

            
        }

        string CrearDirectorio()
        {
            var env = CallContextServiceLocator.Locator.ServiceProvider.GetRequiredService<IApplicationEnvironment>();
            var location = Configuration["Data:Sqlite:Location"];
            var directory = $"{env.ApplicationBasePath}/{location}";

            DirectoryInfo di = Directory.CreateDirectory(directory);

            return di.FullName;
        }
        
        public static void Main(string[] args) => WebApplication.Run<Startup>();
    }
}
