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

namespace SourcePawnManager.Core.DependencyStrategy.StaticUrl;

public class DryRunDownloadStaticUrlQueryHandler : IRequestHandler<DownloadStaticUrlQuery>
{
    private readonly ILogger<DryRunDownloadStaticUrlQueryHandler> _logger;

    public DryRunDownloadStaticUrlQueryHandler(ILogger<DryRunDownloadStaticUrlQueryHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Unit> Handle(DownloadStaticUrlQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("would download {DependencyId} from {DependencyStaticUrl} to {DependencyDownloadPath}",
                               request.Dependency.Id,
                               request.Dependency.Url,
                               request.Dependency.DownloadPath);

        return Task.FromResult(Unit.Value);
    }
}