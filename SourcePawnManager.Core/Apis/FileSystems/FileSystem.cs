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

namespace SourcePawnManager.Core.Apis.FileSystems;

public class FileSystem : IFileSystem
{
    public bool HasExtension(string path) => Path.GetExtension(path) == ".json";

    public bool FileExists(string path) => File.Exists(path);

    public string[] GetFiles(string path, string searchPattern) =>
        Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);

    public Stream Read(string path) => File.OpenRead(path);

    public async Task Write(Stream stream, string path)
    {
        var downloadPath  = Path.GetFullPath(path);
        var directoryName = Path.GetDirectoryName(downloadPath);
        if (directoryName != null)
        {
            Directory.CreateDirectory(directoryName);
        }

        await using var fileStream = File.Open(downloadPath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fileStream);
    }

    public void Delete(string path)
    {
        File.Delete(path);
    }
}