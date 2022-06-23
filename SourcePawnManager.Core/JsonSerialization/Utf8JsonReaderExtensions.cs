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

namespace SourcePawnManager.Core.JsonSerialization;

public static class Utf8JsonReaderExtensions
{
    public static string ReadProperty(this ref Utf8JsonReader reader, string propertyName)
    {
        reader.Read();
        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException();
        }

        var actualPropertyName = reader.GetString();
        if (actualPropertyName != propertyName)
        {
            throw new JsonException();
        }

        reader.Read();
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        return reader.GetString()!;
    }

    public static IDictionary<string, string> ReadDict(this ref Utf8JsonReader reader)
    {
        var dictionary = new Dictionary<string, string>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dictionary;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            var propertyName = reader.GetString();
            if (propertyName == null)
            {
                throw new JsonException("propertyName not found");
            }

            reader.Read();
            var value = reader.GetString();
            if (value == null)
            {
                throw new JsonException("value not found");
            }

            dictionary.Add(propertyName, value);
        }

        throw new JsonException();
    }
}