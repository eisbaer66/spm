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

public class NotInstalledGitCredentials : IGitCredentials
{
    private readonly IGitCredentials                     _credentials;
    private readonly ILogger<NotInstalledGitCredentials> _logger;

    public NotInstalledGitCredentials(IGitCredentials credentials, ILogger<NotInstalledGitCredentials> logger)
    {
        _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
        _logger      = logger      ?? throw new ArgumentNullException(nameof(logger));
    }

    public string? Fill(string url)
    {
        try
        {
            return _credentials.Fill(url);
        }
        catch (Exception e) when ((e.Message.StartsWith("Failed to locate '") &&
                                   e.Message.EndsWith(" executable on the path.")) ||
                                  e.Message.StartsWith("No credential backing store has been selected."))
        {
            _logger.LogDebug(e, "git not found");
            return null;
        }
    }

    public void Update(HttpResponseMessage response)
    {
        try
        {
            _credentials.Update(response);
        }
        catch (Exception e) when ((e.Message.StartsWith("Failed to locate '") &&
                                   e.Message.EndsWith(" executable on the path.")) ||
                                  e.Message.StartsWith("No credential backing store has been selected."))
        {
            _logger.LogDebug(e, "git not found");
        }
    }
}