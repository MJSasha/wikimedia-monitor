using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace WikimediaMonitor.Watcher
{
    public class WikimediaWatcher
    {
        private readonly IProducer<Null, string> producer;
        private readonly WatcherSettings watcherSettings;

        public WikimediaWatcher(IProducer<Null, string> producer, IOptions<WatcherSettings> watcherSettings)
        {
            this.producer = producer;
            this.watcherSettings = watcherSettings.Value;
        }

        public async Task StartWatching()
        {
            Console.WriteLine("WikimediaWatcher is now watching...");

            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, watcherSettings.WikimediaUrl);
            request.Headers.Add("Accept", "text/event-stream");

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                {
                    // INFO: An empty line separates the events
                    continue;
                }

                if (line.StartsWith("data:"))
                {
                    var data = line["data:".Length..].Trim();
                    await ProcessChangeAsync(data);
                }
            }
        }

        private async Task ProcessChangeAsync(string change)
        {
            Console.WriteLine($"Received change: {change}");
            await producer.ProduceAsync(watcherSettings.TopicName, new Message<Null, string> { Value = change });
            Console.WriteLine($"Sent change to Kafka");
        }
    }
}
