using Azure.Messaging.EventHubs.Processor;
using EventHubWorkload;
using EventHubBlazor.Components;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventHubBlazor.Pages
{
    public partial class EventRunner
    {
        [Inject]
        protected EventSender Sample04_BasicCheckpointingBlazor { get; set; }

        SortedSet<string> receiverComponents = new SortedSet<string>();

        protected override void OnInitialized()
        {

        }
        string result = "";
        int currentCount = 0;
        void IncrementCount()
        {
            currentCount++;
        }

        int nextReceiverId = 0;
        int nextSenderId = 0;

        void AddReceiver()
        {
            var c = Guid.NewGuid().ToString();            
            receiverComponents.Add(nextReceiverId.ToString());
            nextReceiverId++;
        }

        void RemoveReceiver(string c)
        {
            receiverComponents.Remove(c);
        }

        async Task SendEvents()
        {
            await Sample04_BasicCheckpointingBlazor.SendEvents(nextSenderId.ToString());
            nextSenderId++;
        }
    }
}
