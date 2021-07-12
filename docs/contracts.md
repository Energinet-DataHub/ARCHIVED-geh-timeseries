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
| TimeSeriesStartDateTime | Timestamp | required | In UTC. The start of the time series period. The start equals the ObservationDateTime of the earliest point in the time series list |
| TimeSeriesEndDateTime | Timestamp | required | In UTC. The end of the time series period. The end is to be considered an up to (excluding) date time which equals the end of the latest point in the time series list. This is a calculated value that adds a single duration equal to the metering point's resolution, e.g. 15 minutes, to the latest Point's ObservationDateTime |
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
