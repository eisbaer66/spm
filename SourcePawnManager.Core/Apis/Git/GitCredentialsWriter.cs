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
using Microsoft.Extensions.Options;
using Microsoft.Git.CredentialManager;

namespace SourcePawnManager.Core.Apis.Git;

public class GitCredentialsWriter : IGitCredentialsWriter
{
    private readonly ICredentialStore              _credentialStore;
    private readonly ILogger<GitCredentialsWriter> _logger;
    private readonly IOptionsSnapshot<GitHubToken> _optionsSnapshot;

    public GitCredentialsWriter(ICredentialStore              credentialStore,
                                IOptionsSnapshot<GitHubToken> optionsSnapshot,
                                ILogger<GitCredentialsWriter> logger)
    {
        _credentialStore = credentialStore ?? throw new ArgumentNullException(nameof(credentialStore));
        _optionsSnapshot = optionsSnapshot ?? throw new ArgumentNullException(nameof(optionsSnapshot));
        _logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Update(HttpResponseMessage response)
    {
        var token = _optionsSnapshot.Get(Options.DefaultName);
        if (token.Url == null)
        {
            _logger.LogDebug("no url found. not updating");
            return;
        }

        if (token.Value == null)
        {
            _logger.LogDebug("no token found. not updating");
            return;
        }

        if (response.IsSuccessStatusCode)
        {
            Approve(token);
        }
        else
        {
            Reject(token);
        }
    }

    private void Approve(GitHubToken token)
    {
        _credentialStore.AddOrUpdate(token.Url, GitCredentials.Key, token.Value);
    }

    private void Reject(GitHubToken token)
    {
        _credentialStore.Remove(token.Url, GitCredentials.Key);
    }
}