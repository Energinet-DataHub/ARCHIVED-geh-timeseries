using System.Threading.Tasks;
using Energinet.DataHub.TimeSeries.Application.Dtos;

namespace Energinet.DataHub.TimeSeries.MessageReceiver
{
    public interface ITimeSeriesBundleHandler
    {
        Task HandleAsync(TimeSeriesBundleDto inboundMessageValidatedMessage);
    }
}
