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
using Json.Schema;
using NSubstitute;
using NUnit.Framework;
using SourcePawnManager.Core.JsonSerialization.Schemas;

namespace SourcePawnManager.Core.Tests.JsonSerialization.Schema;

public class CompositeJSchemaStoreTests
{
    [Test]
    public async Task ReturnsFoundJSchemaIfFirst()
    {
        var a        = Substitute.For<IJSchemaStore>();
        var b        = Substitute.For<IJSchemaStore>();
        var aJSchema = JsonSchema.Empty;

        a.Get<UnknownClass>().Returns(aJSchema);

        var store   = new CompositeJSchemaStore(a, b);
        var jSchema = await store.Get<UnknownClass>();
        Assert.AreEqual(aJSchema, jSchema);

        await a.Received().Get<UnknownClass>();
        await b.DidNotReceive().Get<UnknownClass>();
    }

    [Test]
    public async Task ReturnsFoundJSchemaIfSecond()
    {
        var a        = Substitute.For<IJSchemaStore>();
        var b        = Substitute.For<IJSchemaStore>();
        var bJSchema = JsonSchema.Empty;

        a.Get<UnknownClass>().Returns((JsonSchema?)null);
        b.Get<UnknownClass>().Returns(bJSchema);

        var store   = new CompositeJSchemaStore(a, b);
        var jSchema = await store.Get<UnknownClass>();
        Assert.AreEqual(bJSchema, jSchema);

        await a.Received().Get<UnknownClass>();
        await b.Received().Get<UnknownClass>();
    }

    // ReSharper disable ObjectCreationAsStatement
    [Test]
    public void ConstructorThrowsArgumentNullExceptionIfStoresAreNull()
    {
        Assert.Throws<ArgumentNullException>(() => new CompositeJSchemaStore(null!));
    }

    [Test]
    public void ConstructorThrowsArgumentExceptionIfStoresAreEmpty()
    {
        Assert.Throws<ArgumentException>(() => new CompositeJSchemaStore(Array.Empty<IJSchemaStore>()));
    }

    // ReSharper restore ObjectCreationAsStatement
}