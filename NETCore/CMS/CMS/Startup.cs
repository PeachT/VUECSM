﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace CMS
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region 设置跨域请求
            var urls = "http://localhost:/"; // 还没有研究这个什么我设置的是我的访问路径
            services.AddCors(options =>
            options.AddPolicy("MyDomain",
                builder => builder.WithOrigins(urls).AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin().AllowCredentials()));
            #endregion
            
            //services.AddCors(); //全局跨域请求
            // Add framework services.
            services.AddMvc();

            // 身份验证
            services.AddAuthorization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            // 身份验证
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "Cookie",
                LoginPath = new PathString("/Account/Login"),// 登陆路径 , 如果没有登陆跳转到的登陆路径
                AccessDeniedPath = new PathString("/Account/Forbidden"), //禁止访问路径：当用户试图访问资源时，但未通过该资源的任何授权策略，请求将被重定向到这个相对路径。
                AutomaticAuthenticate = true, //自动认证：这个标志表明中间件应该会在每个请求上进行验证和重建他创建的序列化主体。  
                AutomaticChallenge = true, //自动挑战：这个标志标明当中间件认证失败时应该重定向浏览器到登录路径或者禁止访问路径。
                SlidingExpiration = true,  //Cookie可以分为永久性的和临时性的。 临时性的是指只在当前浏览器进程里有效，浏览器一旦关闭就失效（被浏览器删除）。 永久性的是指Cookie指定了一个过期时间，在这个时间到达之前，此cookie一直有效（浏览器一直记录着此cookie的存在）。 slidingExpriation的作用是，指示浏览器把cookie作为永久性cookie存储，但是会自动更改过期时间，以使用户不会在登录后并一直活动，但是一段时间后却自动注销。也就是说，你10点登录了，服务器端设置的TimeOut为30分钟，如果slidingExpriation为false,那么10:30以后，你就必须重新登录。如果为true的话，你10:16分时打开了一个新页面，服务器就会通知浏览器，把过期时间修改为10:46。 更详细的说明还是参考MSDN的文档。
                
                
            });
            //app.UseCors("AllowSameDomain");
            // 全局跨域请求
            //app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials());
            // 静态文件
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
