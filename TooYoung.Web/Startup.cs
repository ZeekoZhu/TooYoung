using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using MongoDB.Driver;
using Swashbuckle.AspNetCore.Swagger;
using TooYoung.Core.Helpers;
using TooYoung.Core.Services;
using TooYoung.Provider.MongoDB;
using TooYoung.Web.Filters;
using TooYoung.Web.Json;
using TooYoung.Web.Jwt;
using TooYoung.Web.Services;
using ZeekoUtilsPack.AspNetCore.Jwt;

namespace TooYoung.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IImageProcessService, ImageSharpService>();
            services.AddScoped<ImageManageService>();
            services.AddYoungMongo();

            // 配置 JWT
            var jwtOptions = new EasySymmetricOptions("tooyoung")
            {
                Issuer = "too-young",
                Audience = "too-young",
                EnableCookie = true
            };
            services.AddEasyJwt(jwtOptions);
            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                // c.OperationFilter<FileParamTypeFilter>();
                c.SwaggerDoc("v1", new Info { Title = "TooYoung API", Version = "v1" });
                var filePath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "TooYoung.Web.xml");
                c.IncludeXmlComments(filePath);
            });
            services.AddMvc()
                .AddJsonOptions(opt =>
                    {
                        opt.SerializerSettings.ContractResolver = PropertyIgnoreContractResolver.Instance;
                    })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.RoutePrefix = "doc/api";
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TooYoung API V1");
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            //app.UseSpa(spa =>
            //{
            //    spa.Options.SourcePath = "ClientApp";

            //    if (env.IsDevelopment())
            //    {
            //        spa.UseReactDevelopmentServer(npmScript: "start");
            //    }
            //});
        }
    }
}
