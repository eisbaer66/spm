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

public class CompositeJSchemaStore : IJSchemaStore
{
    private readonly IJSchemaStore[] _stores;

    public CompositeJSchemaStore(params IJSchemaStore[] stores)
    {
        if (stores == null)
        {
            throw new ArgumentNullException(nameof(stores));
        }

        if (stores.Length == 0)
        {
            throw new ArgumentException("at least one store must be defined", nameof(stores));
        }

        _stores = stores;
    }

    public async Task<JsonSchema?> Get<T>()
    {
        foreach (var store in _stores)
        {
            var jsonSchema = await store.Get<T>();
            if (jsonSchema != null)
            {
                return jsonSchema;
            }
        }

        return null;
    }
}