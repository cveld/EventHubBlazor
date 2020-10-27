using Azure.Messaging.EventHubs.Processor;
using EventHubWorkload;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventHubBlazor.Components
{
    public partial class EventReceiverComponent : IDisposable
    {
        [Parameter]
        public EventCallback<MouseEventArgs> OnRemoveCallback { get; set; }
        RunningStateEnum started = RunningStateEnum.Stopped;
        List<string> result = new List<string>();

        [Inject]
        protected Func<EventsReceiver> eventReceiverFactory { get; set; }

        public EventReceiverComponent()
        {

        }

        protected override void OnInitialized()
        {
            eventsReceiver = eventReceiverFactory();
            eventsReceiver.Setup(EventReceived, EventErrorReceived);
        }

        EventsReceiver eventsReceiver;
        CancellationTokenSource token;
        async Task Start()
        {
            started = RunningStateEnum.Starting;
            token = new CancellationTokenSource();
            await eventsReceiver.Start(token.Token);
            started = RunningStateEnum.Started;
        }

        async Task Stop()
        {
            started = RunningStateEnum.Stopping;
            await eventsReceiver.Stop();
            started = RunningStateEnum.Stopped;
        }

        void Clear()
        {
            result.Clear();
        }

        int eventIndex = 0;
        int eventsSinceLastCheckpoint = 0;
        int eventsPerBatch = 3;

        async Task EventReceived(ProcessEventArgs eventArgs)
        {
            if (eventArgs.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                ++eventIndex;
                ++eventsSinceLastCheckpoint;

                if (eventsSinceLastCheckpoint >= eventsPerBatch)
                {
                    // Updating the checkpoint will interact with the Azure Storage.  As a service call,
                    // this is done asynchronously and may be long-running.  You may want to influence its behavior,
                    // such as limiting the time that it may execute in order to ensure throughput for
                    // processing events.
                    //
                    // In our case, we'll limit the checkpoint operation to a second and request cancellation
                    // if it runs longer.

                    using CancellationTokenSource cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));

                    try
                    {                        
                        await eventArgs.UpdateCheckpointAsync(cancellationSource.Token);
                        eventsSinceLastCheckpoint = 0;

                        Console.WriteLine("Created checkpoint");
                    }
                    catch (TaskCanceledException)
                    {
                        Console.WriteLine("Checkpoint creation took too long and was canceled.");
                    }

                    Console.WriteLine();
                }
                               
                result.Add($"{eventArgs.Partition.PartitionId}: Event Received: { Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray()) }");
                await InvokeAsync(() => StateHasChanged());
            }
            catch (Exception ex)
            {
                // For real-world scenarios, you should take action appropriate to your application.  For our example, we'll just log
                // the exception to the console.

                Console.WriteLine();
                Console.WriteLine($"An error was observed while processing events.  Message: { ex.Message }");
                Console.WriteLine();
            }
        }

        async Task EventErrorReceived(ProcessErrorEventArgs eventArgs)
        {
            if (eventArgs.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            Console.WriteLine("===============================");
            Console.WriteLine($"The error handler was invoked during the operation: { eventArgs.Operation ?? "Unknown" }, for Exception: { eventArgs.Exception.Message }");
            Console.WriteLine("===============================");
            Console.WriteLine();

            result.Add($"The error handler was invoked during the operation: { eventArgs.Operation ?? "Unknown" }, for Exception: { eventArgs.Exception.Message }");
            await InvokeAsync(() => StateHasChanged());
            return;
        }

        public async void Dispose()
        {
            if (started == RunningStateEnum.Started)
            {
                await eventsReceiver.Stop();
            }
            if (started == RunningStateEnum.Starting)
            {
                token.Cancel();
            }
        }
    }
}
