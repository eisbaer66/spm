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

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SourcePawnManager.Core.JsonSerialization.Schemas;
using SourcePawnManager.Core.LocalStores;

namespace SourcePawnManager.Core.Tests.JsonSerialization.Schema;

public class DynamicJSchemaStoreTests
{
    [Test]
    public void ConstructorThrowsArgumentNullException()
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new DynamicJSchemaStore(null!));
    }

    [Test]
    public async Task Get()
    {
        var dynamicJSchemaStore = new DynamicJSchemaStore(new());
        var jsonSchema          = await dynamicJSchemaStore.Get<IncludeManagerLocalDefinition>();
        Assert.IsNotNull(jsonSchema);
    }
    
}