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

namespace SourcePawnManager.Core.Apis.FileSystems;

public class DryRunFileSystem : IFileSystem
{
    private readonly IFileSystem               _fileSystem;
    private readonly ILogger<DryRunFileSystem> _logger;

    public DryRunFileSystem(IFileSystem fileSystem, ILogger<DryRunFileSystem> logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger     = logger     ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool HasExtension(string path) => _fileSystem.HasExtension(path);

    public bool FileExists(string path) => _fileSystem.FileExists(path);

    public string[] GetFiles(string path, string searchPattern) => _fileSystem.GetFiles(path, searchPattern);

    public Stream Read(string path) => _fileSystem.Read(path);

    public Task Write(Stream stream, string path)
    {
        _logger.LogInformation("would have written {StreamLength} bytes to {Path}", stream.Length, path);
        return Task.CompletedTask;
    }

    public void Delete(string path)
    {
        _logger.LogInformation("would have deleted {Path}", path);
    }
}