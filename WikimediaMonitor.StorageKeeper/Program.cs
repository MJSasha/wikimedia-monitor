using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WikimediaMonitor.StorageKeeper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")));
});

builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<WikimediaConsumerSettings>(builder.Configuration.GetSection("WikimediaConsumerSettings"));

builder.Services.AddTransient(sp => new ConsumerBuilder<Null, string>(sp.GetRequiredService<IOptions<ConsumerConfig>>().Value).Build());
builder.Services.AddTransient<WikimediaChangesConsumer>();
builder.Services.AddTransient<WikimediasRepository>();

builder.Services.AddHostedService<StartupTasks>();

var app = builder.Build();

app.MapGet("/", () => "I'm here!!!");

app.Run();

public class StartupTasks : IHostedService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<StartupTasks> logger;

    public StartupTasks(IServiceProvider serviceProvider, ILogger<StartupTasks> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var serviceProviderInScope = scope.ServiceProvider;

            var context = serviceProviderInScope.GetRequiredService<AppDbContext>();
            context.Database.Migrate();

            var wikimediaChangesConsumer = serviceProviderInScope.GetRequiredService<WikimediaChangesConsumer>();
            await wikimediaChangesConsumer.StartConsuming(cancellationToken);
        }

        logger.LogInformation("Startup tasks completed.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
