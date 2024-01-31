using Confluent.Kafka;
using Microsoft.Extensions.Options;
using WikimediaMonitor.Watcher;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<WatcherSettings>(builder.Configuration.GetSection("WatcherSettings"));

builder.Services.AddSingleton(sp => new ProducerBuilder<Null, string>(sp.GetRequiredService<IOptions<ProducerConfig>>().Value).Build())
    .AddTransient<WikimediaWatcher>();


var app = builder.Build();

app.MapGet("/", () => "I'm here!!!");

app.Lifetime.ApplicationStarted.Register(() =>
{
    var wikimediaWatcher = app.Services.GetRequiredService<WikimediaWatcher>();
    wikimediaWatcher.StartWatching();
});

app.Run();