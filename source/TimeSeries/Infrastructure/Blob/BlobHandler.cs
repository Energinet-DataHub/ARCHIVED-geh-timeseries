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

using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace Energinet.DataHub.TimeSeries.Infrastructure.Blob;

public class BlobHandler : IBlobHandler
{
    private readonly BlobContainerClient _blobContainerClient;

    public BlobHandler(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public async Task SaveAsync(string fileName, string content)
    {
        var blobClient = _blobContainerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(BinaryData.FromString(content), overwrite: true);
    }
}
