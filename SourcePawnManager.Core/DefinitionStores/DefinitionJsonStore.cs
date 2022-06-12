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

using SourcePawnManager.Core.Apis.FileSystems;

namespace SourcePawnManager.Core.DefinitionStores;

public class DefinitionJsonStore : IDefinitionStore
{
    public const     string          DefaultFileName = "spm.json";
    private readonly IJsonFileSystem _fileSystem;

    public DefinitionJsonStore(IJsonFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task<IncludeManagerDefinition?> Read(string path, CancellationToken cancellationToken = default)
    {
        Throw.IfNullOrEmpty(path, nameof(path));

        var filepath   = _fileSystem.HasExtension(path) ? path : Path.Combine(path, DefaultFileName);
        var definition = await _fileSystem.ReadJson<IncludeManagerDefinition>(filepath, cancellationToken);

        return definition;
    }

    public async Task Write(string                   path,
                            IncludeManagerDefinition definition,
                            CancellationToken        cancellationToken = default)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        Throw.IfNullOrEmpty(path, nameof(path));

        var filepath = _fileSystem.HasExtension(path) ? path : Path.Combine(path, DefaultFileName);

        await _fileSystem.WriteJson(definition, filepath, cancellationToken);
    }
}