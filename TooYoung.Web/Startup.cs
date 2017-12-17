using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using TooYoung.Jwt;
using ZeekoUtilsPack.AspNetCore.Jwt;

namespace TooYoung
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 配置 MongoDB
            var connectStr = Configuration.GetConnectionString("Mongo");
            string keyDir = PlatformServices.Default.Application.ApplicationBasePath;
            var tokenOptions = new JwtConfigOptions(keyDir, "tooyoung", "tooyoung");
            services.AddSingleton(tokenOptions.TokenOptions);
            services.AddJwtAuthorization(tokenOptions);
            // 使用 JWT 保护 API
            services.AddAuthentication().AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = tokenOptions.JwTokenValidationParameters;
                });
            // 使用 Cookie 保护页面
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login";
                    options.Cookie.Name = "tk";
                    options.Cookie.Path = "/";
                    options.TicketDataFormat = new JwtCookieDataFormat(tokenOptions.TokenOptions);
                    options.ClaimsIssuer = "TooYoung";
                });
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
