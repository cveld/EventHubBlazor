using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventHubWorkload
{
    public class EventsReceiver
    {
        EventProcessorClient processor;
        private readonly EventHubWorkloadConfig eventHubWorkloadConfig;
        Func<ProcessEventArgs, Task> processEventHandler;
        Func<ProcessErrorEventArgs, Task> processErrorHandler;

        public EventsReceiver(IOptions<EventHubWorkloadConfig> sampleRunnerConfig)
        {
            this.eventHubWorkloadConfig = sampleRunnerConfig.Value;            
        }
        public void Setup(Func<ProcessEventArgs, Task> processEventHandler, Func<ProcessErrorEventArgs, Task> processErrorHandler)
        {
            this.processEventHandler = processEventHandler;
            this.processErrorHandler = processErrorHandler;
        }
        public async Task Start(CancellationToken token)
        {
            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
            BlobContainerClient storageClient = new BlobContainerClient(eventHubWorkloadConfig.StorageAccountConnectionString, eventHubWorkloadConfig.StorageAccountContainer);
            processor = new EventProcessorClient(storageClient, consumerGroup, eventHubWorkloadConfig.EventHubsNamespaceConnectionString, eventHubWorkloadConfig.EventHub);

            processor.ProcessEventAsync += processEventHandler;
            processor.ProcessErrorAsync += processErrorHandler;
            await processor.StartProcessingAsync(token);
        }

        public async Task Stop()
        {
            await processor.StopProcessingAsync();
            processor.ProcessEventAsync -= processEventHandler;
            processor.ProcessErrorAsync -= processErrorHandler;

        }
    }
}
