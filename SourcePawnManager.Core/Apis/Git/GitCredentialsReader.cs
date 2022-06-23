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
using Microsoft.Git.CredentialManager;

namespace SourcePawnManager.Core.Apis.Git;

public class GitCredentialsReader : IGitCredentialsReader
{
    private readonly ICredentialStore              _credentialStore;
    private readonly ILogger<GitCredentialsReader> _logger;

    public GitCredentialsReader(ICredentialStore credentialStore, ILogger<GitCredentialsReader> logger)
    {
        _credentialStore = credentialStore ?? throw new ArgumentNullException(nameof(credentialStore));
        _logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
    }

    public string? Fill(string url)
    {
        var credential = _credentialStore.Get(url, GitCredentials.Key);
        if (credential == null)
        {
            _logger.LogTrace("no credentials found. using no token");
            return null;
        }

        if (credential.Account != GitCredentials.Key)
        {
            _logger.LogError("no credentials for {GitCredentialAccount} found. using no token", GitCredentials.Key);
            return null;
        }

        return credential.Password;
    }
}