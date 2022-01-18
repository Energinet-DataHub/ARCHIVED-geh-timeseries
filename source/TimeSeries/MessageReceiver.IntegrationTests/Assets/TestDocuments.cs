using System.IO;
using System.Net.Http;

namespace Energinet.DataHub.TimeSeries.MessageReceiver.IntegrationTests.Assets
{
    public class TestDocuments
    {
        public string ValidTimeSeries => GetDocumentAsString("Valid_Hourly_CIM_TimeSeries.xml");

        private string GetDocumentAsString(string documentName)
        {
            var stream = GetDocumentStream(documentName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private Stream GetDocumentStream(string documentName)
        {
            var rootNamespace = GetType().Namespace;
            var assembly = GetType().Assembly;
            return assembly.GetManifestResourceStream($"{rootNamespace}.{documentName}") !;
        }
    }
}
