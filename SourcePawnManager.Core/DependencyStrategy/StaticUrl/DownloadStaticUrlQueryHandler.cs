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

using MediatR;
using Microsoft.Extensions.Logging;
using SourcePawnManager.Core.Apis.FileSystems;
using SourcePawnManager.Core.Apis.Http;

namespace SourcePawnManager.Core.DependencyStrategy.StaticUrl;

public class DownloadStaticUrlQueryHandler : IRequestHandler<DownloadStaticUrlQuery>
{
    private readonly IHttpApi                               _api;
    private readonly IFileSystem                            _fileSystem;
    private readonly ILogger<DownloadStaticUrlQueryHandler> _logger;

    public DownloadStaticUrlQueryHandler(IHttpApi api, IFileSystem fileSystem, ILogger<DownloadStaticUrlQueryHandler> logger)
    {
        _api        = api        ?? throw new ArgumentNullException(nameof(api));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger     = logger     ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(DownloadStaticUrlQuery request, CancellationToken cancellationToken)
    {
        var stream = await _api.GetStream(request.Dependency.Url, cancellationToken);
        if (stream == null)
        {
            _logger.LogError("HttpApi did return null for url {StaticUrl} on {DependencyId}",
                             request.Dependency.Url,
                             request.Dependency.Id);
            return Unit.Value;
        }

        await _fileSystem.Write(stream, request.Dependency.DownloadPath);
        return Unit.Value;
    }
}