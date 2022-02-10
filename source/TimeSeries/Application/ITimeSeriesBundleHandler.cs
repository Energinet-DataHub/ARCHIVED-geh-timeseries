using System.Threading.Tasks;
using Energinet.DataHub.TimeSeries.Application.Dtos;

namespace Energinet.DataHub.TimeSeries.Application
{
    public interface ITimeSeriesBundleHandler
    {
        Task HandleAsync(TimeSeriesBundleDto timeSeriesBundle);
    }
}
