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

namespace SourcePawnManager.Core.Apis.Git;

public class DryRunGitCredentials : IGitCredentials
{
    private readonly IGitCredentials               _credentials;
    private readonly ILogger<DryRunGitCredentials> _logger;

    public DryRunGitCredentials(IGitCredentials credentials, ILogger<DryRunGitCredentials> logger)
    {
        _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
        _logger      = logger      ?? throw new ArgumentNullException(nameof(logger));
    }

    public string? Fill(string url) => _credentials.Fill(url);

    public void Update(HttpResponseMessage response)
    {
        _logger.LogInformation("would have updated git credentials from {Successful} response",
                               response.IsSuccessStatusCode ? "successful" : "failed");
    }
}