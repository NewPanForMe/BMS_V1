﻿using BMS_Base.Config;
using BMS_Base.Interface;
using BMS_Db.EfContext;
using Consul;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BMS;

public class Startup
{

    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        this._configuration = configuration;
    }
    // This method gets called by the runtime. Use this method to add services to the container.
    // 该方法由运行时调用，使用该方法向DI容器添加服务
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigin", builder =>
            {
                builder
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
        //注入数据库
        RegisterDb(services);
        //自动实现类注入
        RegisterIBll(services);
        //获取consulConfig
        RegisterConsul(services);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    // 该方法由运行时调用，使用该方法配置HTTP请求管道
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseRouting();
        app.UseAuthorization();
        app.UseAuthentication();
        app.UseEndpoints(x =>
        {
            x.MapControllers();
            x.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        });
        app.UseCors("AllowAllOrigin");
        //配置Consul
        RegisterConsul(appLifetime);
    }
    /// <summary>
    /// 注册consul
    /// </summary>
    /// <param name="appLifetime"></param>
    private static void RegisterConsul(IHostApplicationLifetime appLifetime)
    {
        using var client = new ConsulClient(x => x.Address = new Uri("http://127.0.0.1:8500"));
        var check = new AgentServiceCheck()
        {
            DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//服务停止后，5s开始接触注册
            HTTP = ConsulConfig.Instance.CheckApi,//健康检查
            Interval = TimeSpan.FromSeconds(10),//每10s轮询一次健康检查
            Timeout = TimeSpan.FromSeconds(5),
        };
        var service = new AgentServiceRegistration()
        {
            Checks = new[] { check },
            ID = Guid.NewGuid().ToString(),
            Name = ConsulConfig.Instance.ServiceName,
            Port = ConsulConfig.Instance.Port,
            Address = ConsulConfig.Instance.Address
        };
        client.Agent.ServiceRegister(service).Wait();
        appLifetime.ApplicationStopped.Register(() =>
        {
            Console.WriteLine("服务停止中");
            using var consulClient = new ConsulClient(x => x.Address = new Uri("http://127.0.0.1:8500"));
            consulClient.Agent.ServiceDeregister(service.ID).Wait();
        });
    }

    /// <summary>
    /// 自动依赖注入
    /// </summary>
    /// <param name="service"></param>
    private static void RegisterIBll(IServiceCollection service)
    {
        //获取所有需要注入的类
        var assemblies = Assembly.GetAssembly(typeof(IBll))?.GetTypes().ToList();
        //循环所有的类
        assemblies?.ForEach(x =>
        {
            var interfaces = x.GetInterfaces().ToList();
            interfaces.ForEach(inter =>
            {
                service.AddScoped(x);
            });

        });

        //获取所有需要注入的类
        var staticType = Assembly.GetAssembly(typeof(IStaticBll))?.GetTypes().ToList();
        //循环所有的类
        staticType?.ForEach(x =>
        {
            var interfaces = x.GetInterfaces().ToList();
            interfaces.ForEach(inter =>
            {
                service.AddSingleton(x);
            });

        });
    }

    /// <summary>
    /// 注入数据库
    /// </summary>
    /// <param name="service"></param>
    private  void RegisterDb(IServiceCollection service)
    {
        service.Configure<DbConfig>(_configuration.GetSection("DbConfig"));
        _configuration.Bind("DbConfig", DbConfig.Instance);
        Console.WriteLine("DbConfig：" + DbConfig.Instance);
        service.AddDbContext<BmsV1DbContext>(opt =>
        {
            opt.UseSqlServer(DbConfig.Instance.SqlServer);
        });
    }

    /// <summary>
    /// 获取consulConfig
    /// </summary>
    /// <param name="service"></param>
    private void RegisterConsul(IServiceCollection service)
    {
        //获取配置文件信息
        service.Configure<ConsulConfig>(_configuration.GetSection("ConsulConfig"));
        _configuration.Bind("ConsulConfig", ConsulConfig.Instance);
        Console.WriteLine("ConsulConfig：" + ConsulConfig.Instance);
    }

}