# Time Series

## Intro

The time series domain focuses primarily on receiving, validating, storing and distributing time series data to relevant market participants to be used for billing, etc.

A time series is a message containing a collection of measurements for a given Metering Point and it documents the amount of electricity being consumed, produced or exchanged. The measurements received can come in different resolutions e.g. hourly or 15 minutes values, although the domain is built with the intend for higher resolutions like 5 minutes or higher.

Performance is essential for this time series engine as the expected through-put will be high. Current performance target is processing 16 million time series values per hour, but this figure is expected to be raised significantly.

The domain is also in charge of responding to time series data requests from market participants, and it is a key input data provider for the calculations performed by the [Aggregations domain](https://github.com/Energinet-DataHub/geh-aggregations) in order to settle the electricity market.

These are the business processes maintained by this domain.

| Processes |
| ------------- |
| [Submission of time series data](https://github.com/Energinet-DataHub/geh-timeseries/blob/main/docs/business-processes.md#submission-of-time-series-data) |
| [Request for time series data](https://github.com/Energinet-DataHub/geh-timeseries/blob/main/docs/business-processes.md#request-for-time-series-data) |
| [Request for historical time series data](https://github.com/Energinet-DataHub/geh-timeseries/blob/main/docs/business-processes.md#request-for-historical-time-series-data) |
| ... |

## Architecture

Upon receipt of a time series message, the data gets processed within [Databricks](https://databricks.com/). In Databricks, jobs and libraries implemented in Python takes care of validating the data, enriching it with Metering Point master data before storing the data in a [Delta Lake](https://delta.io/). Finally, the validated time series data together with recipient information is handed over to the [Post Office domain](https://github.com/Energinet-DataHub/geh-post-office) for distribution. Time series data failing validation will be rejected and the time series sender will be notified by a message handed over to the Post Office domain.

![design](ARCHITECTURE.png)

## Context Streams

This sections documents the dataflows to and from the time series domain.

TODO - Add dataflow diagram.

## Domain Road Map

TODO

## Getting Started

Learn how to get started with Green Energy Hub [here](https://github.com/Energinet-DataHub/green-energy-hub/blob/main/docs/getting-started.md).

## Where can I get more help?

Read about the community for Green Energy Hub [here](https://github.com/Energinet-DataHub/green-energy-hub/blob/main/COMMUNITY.md) and learn about how to get involved and get help.

Please note that we have provided a [Dictionary](https://github.com/Energinet-DataHub/green-energy-hub/tree/main/docs/dictionary-and-concepts) to help understand many of the terms used throughout the repository.
