namespace Energinet.DataHub.TimeSeries.Application.Enums
{
    public enum Quality
    {
        Unknown = 0,
        Adjusted = 1, // A01
        NotAvailable = 2, // A02
        Estimated = 3, // 56
        AsProvided = 4, // A03
        Incomplete = 5, // A05
        Calculated = 6, // Z01
    }
}
