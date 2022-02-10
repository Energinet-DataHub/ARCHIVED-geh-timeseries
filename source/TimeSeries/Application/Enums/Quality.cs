namespace Energinet.DataHub.TimeSeries.Application.Enums
{
    public enum Quality
    {
        Unknown = 0,
        Measured = 1, // E01
        Revised = 2, // 36
        Estimated = 3, // 56
        QuantityMissing = 4, // A02
        Calculated = 5, // D01
    }
}
