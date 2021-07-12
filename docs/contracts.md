# Protocol Documentation

## Table of Contents

- [IntegrationEventContract.proto](#IntegrationEventContract.proto)
    - [TimeSeriesAccepted](#.TimeSeriesAccepted)
        - [TimeSeriesPoint](#.TimeSeriesPoint)

<a name="IntegrationEventContract.proto"></a>

## IntegrationEventContract.proto

Time Series Domain related integration events are documented here.

Note: Correlation Id is expected to be available on integration events as part of their meta data and for that reason it is not reflected below.

<a name=".TimeSeriesAccepted"></a>

### TimeSeriesAccepted

Represents an accepted time series, covering both originals as well as updates.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| TimeSeriesId | string | required | A unique time series identifier |
| MeteringPointId | string | required | A unique metering point identifier |
| TimeSeriesStartDateTime | Timestamp | required | In UTC. Time interval covering the entire time series period, where the starting point equals the earliest observation date time in the list of time series points |
| TimeSeriesEndDateTime | Timestamp | required | In UTC. Time interval covering the entire time series period, where the end point equals the latest observation date time in the list of time series points |
| Points | [TimeSeriesPoint](#.TimeSeriesPoint) | required | A list of time series points |

The Timestamp type is documented [here](https://developers.google.com/protocol-buffers/docs/reference/google.protobuf#Timestamp).

<a name=".TimeSeriesPoint"></a>

#### TimeSeriesPoint

Represents a single time series measurement also known as point.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| ObservationDateTime | Timestamp | required | In UTC. The date and time of a time series point |
| Quantity | decimal | required | Quantity of a specific type of energy. Note: 3 decimals for K3 (kVArh) and KWH (kWh) and KWT (kW) and TNE (Tonne), 6 decimals for MAW (MW) and MWH (MWh) and Z03 (MVAr) |
| Quality | enum | required | Indicates the quality of a specific quantity in a time series, e.g. estimated, measured, etc. |
