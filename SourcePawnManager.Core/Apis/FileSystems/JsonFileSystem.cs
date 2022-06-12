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

using System.Text.Json;
using Microsoft.Extensions.Logging;
using SourcePawnManager.Core.JsonSerialization.Schemas;

namespace SourcePawnManager.Core.Apis.FileSystems;

public class JsonFileSystem : IJsonFileSystem
{
    private readonly IFileSystem             _fileSystem;
    private readonly IJSchemaStore           _jSchemaStore;
    private readonly JsonDocumentOptions     _jsonDocumentOptions;
    private readonly JsonSerializerOptions   _jsonSerializerOptions;
    private readonly ILogger<JsonFileSystem> _logger;

    public JsonFileSystem(JsonSerializerOptions   jsonSerializerOptions,
                          IFileSystem             fileSystem,
                          IJSchemaStore           jSchemaStore,
                          ILogger<JsonFileSystem> logger)
    {
        _jsonSerializerOptions =
            jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        _fileSystem          = fileSystem   ?? throw new ArgumentNullException(nameof(fileSystem));
        _jSchemaStore        = jSchemaStore ?? throw new ArgumentNullException(nameof(jSchemaStore));
        _logger              = logger       ?? throw new ArgumentNullException(nameof(logger));
        _jsonDocumentOptions = new();
    }

    public async Task<T?> ReadJson<T>(string path, CancellationToken cancellationToken = default)
    {
        if (!_fileSystem.FileExists(path))
        {
            _logger.LogDebug("file {FilePath} was not found. returning null", path);
            return default;
        }

        Stream stream;
        try
        {
            stream = _fileSystem.Read(path);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "could not read file {FilePath}. returning null", path);
            return default;
        }

        try
        {
            var jsonSchema = await _jSchemaStore.Get<T>();
            if (jsonSchema is null)
            {
                _logger.LogWarning("schema not found for {JsonTypeName}. returning null", typeof(T).FullName);
                return default;
            }

            try
            {
                using var jsonDocument = await JsonDocument.ParseAsync(stream, _jsonDocumentOptions, cancellationToken);
                var       results      = jsonSchema.Validate(jsonDocument.RootElement);
                if (!results.IsValid)
                {
                    var messages =
                        results.NestedResults.Select(e =>
                                                         $"{e.Message} (absoluteSchemaLocation {e.AbsoluteSchemaLocation} instanceLocation {e.InstanceLocation} schemaLocation {e.SchemaLocation})");
                    _logger.LogWarning("could not read file {FilePath}: {@ValidationErrors}. returning null",
                                       path,
                                       string.Join("; ", messages));
                    return default;
                }

                stream.Position = 0;
                return await JsonSerializer.DeserializeAsync<T>(stream, _jsonSerializerOptions, cancellationToken);
            }
            catch (JsonException e)
            {
                _logger.LogWarning(e, "could not read file {FilePath} cant be read as JSON. returning null", path);
                return default;
            }
        }
        finally
        {
            await stream.DisposeAsync();
        }
    }

    public async Task WriteJson<T>(T obj, string path, CancellationToken cancellationToken = default)
    {
        await using Stream stream = new MemoryStream();

        await WriteJsonToStream(obj, stream, cancellationToken);

        await _fileSystem.Write(stream, path);
    }

    public bool HasExtension(string path) => _fileSystem.HasExtension(path);

    public bool FileExists(string path) => _fileSystem.FileExists(path);

    public string[] GetFiles(string path, string searchPattern) => _fileSystem.GetFiles(path, searchPattern);

    public Stream Read(string path) => _fileSystem.Read(path);

    public Task Write(Stream stream, string path) => _fileSystem.Write(stream, path);

    public void Delete(string path)
    {
        _fileSystem.Delete(path);
    }

    private async Task WriteJsonToStream<T>(T obj, Stream stream, CancellationToken cancellationToken)
    {
        if (obj == null)
        {
            return;
        }

        await JsonSerializer.SerializeAsync(stream, obj, _jsonSerializerOptions, cancellationToken);
        stream.Position = 0;
    }
}