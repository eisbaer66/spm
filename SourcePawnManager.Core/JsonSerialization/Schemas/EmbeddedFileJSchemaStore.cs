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

using Json.Schema;

namespace SourcePawnManager.Core.JsonSerialization.Schemas;

public class EmbeddedFileJSchemaStore : IJSchemaStore
{
    public async Task<JsonSchema?> Get<T>()
    {
        var storeType = typeof(EmbeddedFileJSchemaStore);
        var tName     = typeof(T).Name;
        var stream    = storeType.Assembly.GetManifestResourceStream(storeType, $"{tName}.schema.json");
        if (stream == null)
        {
            return null;
        }

        return await JsonSchema.FromStream(stream);
    }
}