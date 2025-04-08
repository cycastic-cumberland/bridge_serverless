using Amazon.DynamoDBv2;
using Bridge.Core.DynamoDB;
using Bridge.Domain.Configurations;
using Bridge.Infrastructure.Abstractions;
using Bridge.Serverless.Dto;
using Bridge.Serverless.Services;

namespace Bridge.Serverless;

public class Startup
{
    private const string CorsPolicy = nameof(CorsPolicy);

    
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        var settings = Configuration.GetSection(nameof(AppSettings));
        services.Configure<AppSettings>(settings);
        services.Configure<RoomConfigurations>(settings.GetSection(nameof(AppSettings.RoomConfigurations)));
        services.Configure<ItemConfigurations>(settings.GetSection(nameof(AppSettings.ItemConfigurations)));
        services.Configure<PasteConfigurations>(settings.GetSection(nameof(AppSettings.PasteConfigurations)));
        services.Configure<S3Settings>(settings.GetSection(nameof(AppSettings.S3Settings)));

        services.AddLogging();
        services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient());
        services.AddScoped<IStorageService, S3StorageService>();
        services.AddSingleton<IUrlGenerator, UrlGenerator>();
        services.AddScoped<IQrService, QrService>();

        services.AddScoped<RoomRepository>();
        services.AddScoped<ItemRepository>();
        services.AddScoped<PasteRepository>();
        services.AddScoped<IRoomRepository>(sp => sp.GetRequiredService<RoomRepository>());
        services.AddScoped<IItemRepository>(sp => sp.GetRequiredService<ItemRepository>());
        services.AddScoped<IPasteRepository>(sp => sp.GetRequiredService<PasteRepository>());
        
        services.AddControllers(cfg =>
        {
            cfg.Filters.Add<ApiExceptionFilter>();
        });
        
        var cors = settings.Get<AppSettings>()?.AllowedOrigins;
        if (!string.IsNullOrEmpty(cors))
        {
            var origins = cors.Split(";");
            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicy, policy =>
                {
                    policy.WithOrigins(origins)
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseCors(CorsPolicy);

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}