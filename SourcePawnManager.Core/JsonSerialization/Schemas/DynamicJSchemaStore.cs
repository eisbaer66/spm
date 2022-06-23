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
using Json.Schema.Generation;

namespace SourcePawnManager.Core.JsonSerialization.Schemas;

public class DynamicJSchemaStore : IJSchemaStore
{
    private readonly SchemaGeneratorConfiguration _configuration;

    public DynamicJSchemaStore(SchemaGeneratorConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public Task<JsonSchema?> Get<T>()
    {
        var schema = new JsonSchemaBuilder()
                     .FromType(typeof(T), _configuration)
                     .Build();
        return Task.FromResult<JsonSchema?>(schema);
    }
}