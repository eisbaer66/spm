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
using System.Text.Json.Serialization;
using NuGet.Versioning;
using SourcePawnManager.Core.DependencyStrategy;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;

namespace SourcePawnManager.Core.JsonSerialization;

public class DependencyConverter : JsonConverter<IDependency>
{
    public override IDependency Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var typeName        = reader.ReadProperty("type");
        var versionRangeRaw = reader.ReadProperty("versionRange");
        if (!VersionRange.TryParse(versionRangeRaw, out var versionRange))
        {
            throw new JsonException();
        }

        var dict = reader.ReadDict();

        return Enum.Parse<DependencyType>(typeName) switch
               {
                   DependencyType.GitHubTagFile => GitHubTagFileDependency.From(dict, versionRange),
                   DependencyType.GitHubTagZip  => GitHubTagZipDependency.From(dict, versionRange),
                   _                            => throw new JsonException("unknown type '" + typeName + "'"),
               };
    }

    public override void Write(Utf8JsonWriter writer, IDependency value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}