// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs.Processor.Samples.Infrastructure;
using EventHubWorkload;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Messaging.EventHubs.Processor.Samples
{
    /// <summary>
    ///   The main entry point for executing the samples.
    /// </summary>
    ///
    public class Program
    {
        /// <summary>
        ///   Serves as the main entry point of the application.
        /// </summary>
        ///
        /// <param name="args">The set of command line arguments passed.</param>
        ///
        public static async Task Main(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) ||
                                devEnvironmentVariable.ToLower() == "development";
            //Determines the working environment as IHostingEnvironment is unavailable in a console app

            if (isDevelopment)
            {
                configurationBuilder.AddUserSecrets<Program>();
            }
            var configuration = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();

            //Map the implementations of your classes here ready for DI
            var provider = services
                .Configure<EventHubWorkloadConfig>(configuration.GetSection(nameof(EventHubWorkloadConfig)))
                .AddOptions()
                //.AddLogging()
                .AddSingleton<SampleRunner, SampleRunner>()
                .BuildServiceProvider();

            var runner = provider.GetService<SampleRunner>();
            await runner.Run(args, default);
        }
    }
}
