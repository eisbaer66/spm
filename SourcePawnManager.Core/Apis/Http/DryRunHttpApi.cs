#region copyright

// Copyright (C) 2022 icebear <icebear@icebear.rocks>
// 
// This file is part of SourcePawnManager (spm).
// 
// SourcePawnManager (spm) is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// 
// SourcePawnManager (spm) is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with SourcePawnManager (spm). If not, see <https://www.gnu.org/licenses/>. 

#endregion

using Microsoft.Extensions.Logging;

namespace SourcePawnManager.Core.Apis.Http;

internal class DryRunHttpApi : IHttpApi
{
    private readonly ILogger<DryRunHttpApi> _logger;

    public DryRunHttpApi(ILogger<DryRunHttpApi> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Stream?> GetStream(string url, CancellationToken cancellationToken)
    {
        _logger.LogInformation("would have downloaded {Url}", url);

        return Task.FromResult<Stream?>(new MemoryStream());
    }
}