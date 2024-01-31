using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace WikimediaMonitor.StorageKeeper
{
    public class WikimediaChangesConsumer
    {
        private readonly WikimediasRepository wikimediasRepository;
        private readonly IConsumer<Null, string> consumer;
        private readonly WikimediaConsumerSettings consumerSettings;

        public WikimediaChangesConsumer(WikimediasRepository wikimediasRepository, IConsumer<Null, string> consumer, IOptions<WikimediaConsumerSettings> consumerSettings)
        {
            this.wikimediasRepository = wikimediasRepository;
            this.consumer = consumer;
            this.consumerSettings = consumerSettings.Value;
        }

        public async Task StartConsuming(CancellationToken cancellationToken = default)
        {
            consumer.Subscribe(consumerSettings.TopicName);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cancellationToken);

                        if (consumeResult != null)
                        {
                            var message = consumeResult.Message.Value;
                            await ProcessMessageAsync(message);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        Console.WriteLine($"Error consuming message: {ex.Error.Reason}");
                    }
                }
            }
            finally
            {
                consumer.Close();
            }
        }

        private async Task ProcessMessageAsync(string message)
        {
            try
            {
                await wikimediasRepository.Create(new Wikimedia { Data = message });

                Console.WriteLine($"Message processed and saved to the database: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        }

    }
}
