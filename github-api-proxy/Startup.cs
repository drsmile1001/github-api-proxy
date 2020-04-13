using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Flurl.Http;

namespace github_api_proxy
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
            app.Run(async context =>
            {
                var path = context.Request.Path.Value;
                if (path.StartsWith("/github-api-proxy"))
                {
                    path = path.Substring("/github-api-proxy".Length);
                }

                var url = $"https://api.github.com{path}{context.Request.QueryString}";
                var auth = context.Request.Headers["Authorization"].ToString();
                try
                {
                    var text = await url
                        .WithHeader("User-Agent", "PostmanRuntime/7.24.1")
                        .WithHeader("Authorization", auth)
                        .GetStringAsync();
                    await context.Response.WriteAsync(text);
                }
                catch (FlurlHttpException e)
                {
                    var err = await e.GetResponseStringAsync();
                    await context.Response.WriteAsync(err);
                }
            });
        }
    }
}
