namespace Energinet.DataHub.TimeSeries.Application.Enums
{
    public enum MeteringPointType
    {
        Unknown = 0,
        Consumption = 1, // E17
        Production = 2, // E18
        Exchange = 3, // E20
        VeProduction = 4, // D01
        Analysis = 5, // D02
        SurplusProductionGroup = 6, // D04
        NetProduction = 7, // D05
        SupplyToGrid = 8, // D06
        ConsumptionFromGrid = 9, // D07
        WholesaleService = 10, // D08
        OwnProduction = 11, // D09
        NetFromGrid = 12, // D10
        NetToGrid = 13, // D11
        TotalConsumption = 14, // D12
        GridLossCorrection = 15, // D13
        ElectricalHeating = 16, // D14
        NetConsumption = 17, // D15
        OtherConsumption = 18, // D17
        OtherProduction = 19, // D18
        ExchangeReactiveEnergy = 20, // D20
        InternalUse = 21, // D99
    }
}
