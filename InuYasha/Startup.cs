using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using AspectCore.APM.ApplicationProfiler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AspectCore.APM.AspNetCore;
using AspectCore.APM.HttpProfiler;
using AspectCore.APM.LineProtocolCollector;
using AspectCore.APM.MethodProfiler;
using AspectCore.APM.RedisProfiler;
using AspectCore.Configuration;
using AspectCore.Extensions.DataAnnotations;
using AspectCore.Extensions.DependencyInjection;
using AspectCore.Injector;
using InuYasha.Intercptor;
using InuYasha.Service;

namespace InuYasha
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            #region Metrics监控配置

            string isOpen = Configuration.GetSection("InfluxDB")["IsOpen"].ToLower();
            if (isOpen == "true")
            {
                string database = Configuration.GetSection("InfluxDB")["DataBaseName"];
                string influxDbConStr = Configuration.GetSection("InfluxDB")["ConnectionString"];
                string app = Configuration.GetSection("InfluxDB")["app"];
                string env = Configuration.GetSection("InfluxDB")["env"];
                string username = Configuration.GetSection("InfluxDB")["username"];
                string password = Configuration.GetSection("InfluxDB")["password"];

                var uri = new Uri(influxDbConStr);

                var metrics = AppMetrics.CreateDefaultBuilder()
                    .Configuration.Configure(
                        options =>
                        {
                            options.AddAppTag(app);
                            options.AddEnvTag(env);
                        })
                    .Report.ToInfluxDb(
                        options =>
                        {
                            options.InfluxDb.BaseUri = uri;
                            options.InfluxDb.Database = database;
                            options.InfluxDb.UserName = username;
                            options.InfluxDb.Password = password;
                            options.HttpPolicy.BackoffPeriod = TimeSpan.FromSeconds(30);
                            options.HttpPolicy.FailuresBeforeBackoff = 5;
                            options.HttpPolicy.Timeout = TimeSpan.FromSeconds(10);
                            options.FlushInterval = TimeSpan.FromSeconds(5);
                        })
                    .Build();

                services.AddMetrics(metrics);
                services.AddMetricsReportScheduler();
                services.AddMetricsTrackingMiddleware();
                services.AddMetricsEndpoints();

            }

            #endregion


            services.AddMvc().AddControllersAsServices();

            //IServiceContainer container = new ServiceContainer();

            //container.AddType<ICustomService, CustomService>();

            //container.AddType<IContactService, ContactService>();

            services.AddScoped<IContactService, ContactService>();

            services.AddTransient<ICustomService, CustomService>();

            services.AddTransient<ITestCheckInput, TestCheckInput>();

            //services.AddDynamicProxy();

            #region AspectAPM

            // 1、代码配置
            //services.AddAspectCoreAPM(component =>
            //{
            //    component.AddApplicationProfiler(); //注册ApplicationProfiler收集GC和ThreadPool数据
            //    component.AddHttpProfiler();        //注册HttpProfiler收集Http请求数据
            //    component.AddLineProtocolCollector(options => //注册LineProtocolCollector将数据发送到InfluxDb
            //    {
            //        options.Server = "http://192.168.1.134:8086"; //你自己的InfluxDB Http地址
            //        options.Database = "aspnetcore";    //你自己创建的Database
            //    });
            //});

            // 2、config配置
            services.AddAspectCoreAPM(component =>
            {
                component.AddLineProtocolCollector(options => Configuration.GetLineProtocolSection().Bind(options))
                    .AddHttpProfiler()
                    .AddApplicationProfiler()
                    .AddRedisProfiler(options => Configuration.GetRedisProfilerSection().Bind(options));
                component.AddMethodProfiler();
            });


            // 与微软自己的DI集成AOP
            //return services.BuildAspectCoreServiceProvider(); //返回AspectCore AOP的ServiceProvider,这句代码一定要有

            // 与AspectCore.Inject 集成
            //services.AddAspectCoreContainer();
            //var container = services.ToServiceContainer();

            ////container.Configure(x => x.Interceptors.AddTyped<TestIntercptorAttribute>());

            //return container.Build();

            var container = services.ToServiceContainer();

            //container.AddType<ICustomService, CustomService>();

            //container.AddType<IContactService, ContactService>();

            container.AddDataAnnotations();

            return container.Build();

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHttpProfiler();      //启动Http请求监控

            app.UseAspectCoreAPM();     //启动AspectCoreAPM，这句代码一定要有

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            #region 注入Metrics

            string isOpen = Configuration.GetSection("InfluxDB")["IsOpen"].ToLower();
            if (isOpen == "true")
            {
                app.UseMetricsAllMiddleware();
                // Or to cherry-pick the tracking of interest
                app.UseMetricsActiveRequestMiddleware();
                app.UseMetricsErrorTrackingMiddleware();
                app.UseMetricsPostAndPutSizeTrackingMiddleware();
                app.UseMetricsRequestTrackingMiddleware();
                app.UseMetricsOAuth2TrackingMiddleware();
                app.UseMetricsApdexTrackingMiddleware();

                app.UseMetricsAllEndpoints();
                // Or to cherry-pick endpoint of interest
                app.UseMetricsEndpoint();
                app.UseMetricsTextEndpoint();
                app.UseEnvInfoEndpoint();
            }

            #endregion


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
