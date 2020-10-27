// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace EventHubWorkload
{
    public class EventHubWorkloadConfig
    {
        public string EventHubsNamespaceConnectionString { get; set; }
        public string EventHub { get; set; }
        public string StorageAccountConnectionString { get; set; }
        public string StorageAccountContainer { get; set; }
    }
}
