using System.Threading.Tasks;
using Energinet.DataHub.TimeSeries.Application.Dtos;
using Energinet.DataHub.TimeSeries.MessageReceiver;

namespace Energinet.DataHub.TimeSeries.Application
{
    public class TimeSeriesBundleHandler : ITimeSeriesBundleHandler
    {
        public Task HandleAsync(TimeSeriesBundleDto inboundMessageValidatedMessage)
        {
            throw new System.NotImplementedException();
        }
    }
}
