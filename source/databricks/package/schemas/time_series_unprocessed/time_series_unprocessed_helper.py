# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.


class TimeSeriesUnprocessedColname:
    BusinessReasonCode = "BusinessReasonCode"
    CreatedDateTime = "CreatedDateTime"
    DocumentId = "DocumentId"
    MeasureUnit = "MeasureUnit"
    MeteringPointId = "MeteringPointId"
    MeteringPointType = "MeteringPointType"
    Period = "Period"
    EndDateTime = "EndDateTime"
    Points = "Points"
    Position = "Position"
    Quality = "Quality"
    Quantity = "Quantity"
    Resolution = "Resolution"
    StartDateTime = "StartDateTime"
    Product = "Product"
    Receiver = "Receiver"
    BusinessProcessRole = "BusinessProcessRole"
    Id = "Id"
    RegistrationDateTime = "RegistrationDateTime"
    Sender = "Sender"
    SeriesId = "SeriesId"
    TransactionId = "TransactionId"
    year = "year"
    month = "month"
    day = "day"


class Period:
    EndDateTime = f"{TimeSeriesUnprocessedColname.Period}.{TimeSeriesUnprocessedColname.EndDateTime}"
    Points = (
        f"{TimeSeriesUnprocessedColname.Period}.{TimeSeriesUnprocessedColname.Points}"
    )
    Resolution = f"{TimeSeriesUnprocessedColname.Period}.{TimeSeriesUnprocessedColname.Resolution}"
    StartDateTime = f"{TimeSeriesUnprocessedColname.Period}.{TimeSeriesUnprocessedColname.StartDateTime}"


class Points:
    Position = (
        f"{TimeSeriesUnprocessedColname.Points}.{TimeSeriesUnprocessedColname.Position}"
    )
    Quality = (
        f"{TimeSeriesUnprocessedColname.Points}.{TimeSeriesUnprocessedColname.Quality}"
    )
    Quantity = (
        f"{TimeSeriesUnprocessedColname.Points}.{TimeSeriesUnprocessedColname.Quantity}"
    )


class Reciver:
    BusinessProcessRole = f"{TimeSeriesUnprocessedColname.Receiver}.{TimeSeriesUnprocessedColname.BusinessProcessRole}"
    Id = f"{TimeSeriesUnprocessedColname.Receiver}.{TimeSeriesUnprocessedColname.Id}"


class Sender:
    BusinessProcessRole = f"{TimeSeriesUnprocessedColname.Sender}.{TimeSeriesUnprocessedColname.BusinessProcessRole}"
    Id = f"{TimeSeriesUnprocessedColname.Sender}.{TimeSeriesUnprocessedColname.Id}"
