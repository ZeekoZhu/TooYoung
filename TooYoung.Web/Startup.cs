using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using MongoDB.Driver;
using Swashbuckle.AspNetCore.Swagger;
using TooYoung.Jwt;
using TooYoung.Services;
using TooYoung.Utils;
using TooYoung.Web.Filters;
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
            connectStr.ParseEnvVarParams()
                .Select(p => (Key: $"$({p})", Val: Configuration.GetSection(p).Value))
                .ToList()
                .ForEach(p => { connectStr = connectStr.Replace(p.Key, p.Val); });
            var dbName = Configuration.GetSection("MONGO_DBNAME").Value;
            services.AddSingleton<IMongoClient>(provider => new MongoClient(connectStr));
            services.AddScoped(provider =>
                provider.GetService<IMongoClient>().GetDatabase(dbName));

            services.AddScoped<AccountService>();


            // 配置 JWT
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
            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                // c.OperationFilter<FileParamTypeFilter>();
                c.SwaggerDoc("v1", new Info { Title = "TooYoung API", Version = "v1" });
                var filePath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "TooYoung.Web.xml");
                c.IncludeXmlComments(filePath);
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
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.RoutePrefix = "doc/api";
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TooYoung API V1");
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
