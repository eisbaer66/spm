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
using SourcePawnManager.Core.Apis.FileSystems;

namespace SourcePawnManager.Core.LockStores;

public class FileLockStore : ILockStore
{
    public const     string                 DefaultFileName = "spm.lock.json";
    private readonly IJsonFileSystem        _fileSystem;
    private readonly ILogger<FileLockStore> _logger;

    public FileLockStore(IJsonFileSystem fileSystem, ILogger<FileLockStore> logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger     = logger     ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LockDefinition> Get(string path, CancellationToken cancellationToken = default)
    {
        var filepath   = _fileSystem.HasExtension(path) ? path : Path.Combine(path, DefaultFileName);
        var definition = await _fileSystem.ReadJson<LockDefinition>(filepath, cancellationToken);

        if (definition == null)
        {
            _logger.LogWarning("SourcePawnManager lock {SourcePawnManagerLockPath} cant be read as JSON", filepath);
            return new();
        }

        return definition;
    }

    public async Task Set(LockDefinition lockDefinition, string path, CancellationToken cancellationToken = default)
    {
        var filepath = _fileSystem.HasExtension(path) ? path : Path.Combine(path, DefaultFileName);

        await _fileSystem.WriteJson(lockDefinition, filepath, cancellationToken);
    }
}