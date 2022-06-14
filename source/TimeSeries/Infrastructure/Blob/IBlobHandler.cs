using System.Threading.Tasks;

namespace Energinet.DataHub.TimeSeries.Infrastructure.Blob;

public interface IBlobHandler
{
    Task SaveAsync(string fileName, string content, string connectionString, string blobContainerName);
}
