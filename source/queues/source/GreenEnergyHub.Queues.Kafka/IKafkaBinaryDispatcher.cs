﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Threading.Tasks;

namespace GreenEnergyHub.Queues.Kafka
{
    /// <summary>
    /// Kafka based message queue dispatcher
    /// </summary>
    public interface IKafkaBinaryDispatcher
    {
        /// <summary>
        /// Dispatches <see cref="data"/>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="topic"></param>
        /// <returns>Task</returns>
        Task DispatchAsync(byte[] data, string topic);

        /// <summary>
        /// Disposes instance
        /// </summary>
        void Dispose();
    }
}
