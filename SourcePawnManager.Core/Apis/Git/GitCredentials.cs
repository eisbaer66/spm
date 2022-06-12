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

namespace SourcePawnManager.Core.Apis.Git;

public class GitCredentials : IGitCredentials
{
    public const     string                Key = "SourcePawnManager";
    private readonly IGitCredentialsReader _reader;
    private readonly IGitCredentialsWriter _writer;

    public GitCredentials(IGitCredentialsReader reader, IGitCredentialsWriter writer)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    public string? Fill(string url) => _reader.Fill(url);

    public void Update(HttpResponseMessage response)
    {
        _writer.Update(response);
    }
}