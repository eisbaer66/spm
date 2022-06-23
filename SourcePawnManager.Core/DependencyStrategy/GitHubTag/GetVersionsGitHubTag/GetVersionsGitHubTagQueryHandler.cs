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
using SourcePawnManager.Core.Apis.GitHub;

namespace SourcePawnManager.Core.DependencyStrategy.GitHubTag.GetVersionsGitHubTag;

public class GetVersionsGitHubTagQueryHandler : IRequestHandler<GetVersionsGitHubTagQuery, IList<DependencyVersion>>
{
    private readonly IGitHubApi                                _api;
    private readonly ILogger<GetVersionsGitHubTagQueryHandler> _logger;

    public GetVersionsGitHubTagQueryHandler(IGitHubApi api, ILogger<GetVersionsGitHubTagQueryHandler> logger)
    {
        _api    = api    ?? throw new ArgumentNullException(nameof(api));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IList<DependencyVersion>> Handle(GetVersionsGitHubTagQuery request,
                                                       CancellationToken         cancellationToken)
    {
        var gitHubTags =
            await _api.GetVersions(request.Dependency.Owner, request.Dependency.Repository, cancellationToken);

        if (gitHubTags == null)
        {
            return new List<DependencyVersion>();
        }

        return gitHubTags.Select(tag =>
                                 {
                                     if (!request.Dependency.TryParseVersion(tag.Name, out var version))
                                     {
                                         _logger
                                             .LogWarning("tag name {GitHubTagName} can not be parsed as NuGetVersion",
                                                         tag);
                                         return null;
                                     }

                                     return new DependencyVersion(version, tag.Name);
                                 })
                         .Where(v => v != null)
                         .Cast<DependencyVersion>()
                         .ToList();
    }
}