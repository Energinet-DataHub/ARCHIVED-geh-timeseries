// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.TimeSeries.Application;
using Energinet.DataHub.TimeSeries.Application.CimDeserialization.TimeSeriesBundle;
using Energinet.DataHub.TimeSeries.Infrastructure.Functions;
using Energinet.DataHub.TimeSeries.MessageReceiver.SimpleInjector;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SimpleInjector;

namespace Energinet.DataHub.TimeSeries.MessageReceiver
{
    public abstract class Startup
    {
        private readonly Container _container;

        protected Startup()
            : this(new Container())
        {
        }

        private Startup(Container container)
        {
            _container = container;
        }

        protected async Task ExecuteApplicationAsync(IHost host)
        {
            await host.RunAsync().ConfigureAwait(false);
            await _container.DisposeContainerAsync().ConfigureAwait(false);
        }

        protected IHost ConfigureApplication()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults(ConfigureFunctionsWorkerDefaults)
                .ConfigureServices(ConfigureServices)
                .Build()
                .UseSimpleInjector(_container);

            ConfigureContainer(_container);

            return host;
        }

        protected void VerifyContainer() => _container.Verify();

        protected virtual void ConfigureContainer(Container container) { }

        protected virtual void ConfigureFunctionsWorkerDefaults(IFunctionsWorkerApplicationBuilder options)
        {
            options.UseMiddleware<SimpleInjectorScopedRequest>();
            options.UseMiddleware<CorrelationIdMiddleware>();
        }

        private static void ReplaceServiceDescriptor(IServiceCollection serviceCollection)
        {
            var descriptor = new ServiceDescriptor(
                typeof(IFunctionActivator),
                typeof(SimpleInjectorActivator),
                ServiceLifetime.Singleton);
            serviceCollection.Replace(descriptor);
        }

        private void ConfigureServices(IServiceCollection serviceCollection)
        {
            ReplaceServiceDescriptor(serviceCollection);

            serviceCollection.AddApplicationInsightsTelemetryWorkerService(
                Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY"));

            serviceCollection.AddLogging();
            serviceCollection.AddSimpleInjector(_container, options =>
            {
                options.AddLogging();
            });

            serviceCollection.AddScoped<ICorrelationContext, CorrelationContext>();
            serviceCollection.AddScoped<CorrelationIdMiddleware>();
            serviceCollection.AddScoped<IHttpResponseBuilder, HttpResponseBuilder>();
            serviceCollection.AddScoped<TimeSeriesBundleDtoValidatingDeserializer>();
            serviceCollection.AddScoped<ITimeSeriesForwarder, TimeSeriesForwarder>();
            serviceCollection
                .AddScoped<ITimeSeriesBundleDtoValidatingDeserializer, TimeSeriesBundleDtoValidatingDeserializer>();
            serviceCollection.AddSingleton<IJsonSerializer, JsonSerializer>();
        }
    }
}
