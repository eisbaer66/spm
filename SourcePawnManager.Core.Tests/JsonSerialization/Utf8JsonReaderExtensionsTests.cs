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

using System.Text;
using System.Text.Json;
using NUnit.Framework;
using SourcePawnManager.Core.JsonSerialization;

namespace SourcePawnManager.Core.Tests.JsonSerialization;

public class Utf8JsonReaderExtensionsTests
{
    [Test]
    public void ReadPropertyReturnsPropertyValue()
    {
        var bytes          = Encoding.UTF8.GetBytes("{\"propertyName\": \"value\"}");
        var utf8JsonReader = new Utf8JsonReader(bytes);
        utf8JsonReader.Read();
        var value = utf8JsonReader.ReadProperty("propertyName");
        Assert.AreEqual("value", value);
    }

    [Test]
    public void ReadPropertyThrowsJsonExceptionIfPositionIsNoProperty()
    {
        var bytes = Encoding.UTF8.GetBytes("{\"propertyName\": \"value\"}");
        Assert.Throws<JsonException>(() =>
                                     {
                                         var utf8JsonReader = new Utf8JsonReader(bytes);
                                         utf8JsonReader.ReadProperty("propertyName");
                                     });
    }

    [Test]
    public void ReadPropertyThrowsJsonExceptionIfPropertyNameDoesNotMatch()
    {
        var bytes = Encoding.UTF8.GetBytes("{\"notPropertyName\": \"value\"}");
        Assert.Throws<JsonException>(() =>
                                     {
                                         var utf8JsonReader = new Utf8JsonReader(bytes);
                                         utf8JsonReader.Read();
                                         utf8JsonReader.ReadProperty("propertyName");
                                     });
    }

    [Test]
    public void ReadPropertyThrowsJsonExceptionIfPropertyValueIsNoString()
    {
        var bytes          = Encoding.UTF8.GetBytes("{\"propertyName\": 123}");
        Assert.Throws<JsonException>(() =>
                                     {
                                         var utf8JsonReader = new Utf8JsonReader(bytes);
                                         utf8JsonReader.Read();
                                         utf8JsonReader.ReadProperty("propertyName");
                                     });
    }

    [Test]
    public void ReadDict()
    {
        var bytes          = Encoding.UTF8.GetBytes("{\"propertyName\": \"value\"}");
        var utf8JsonReader = new Utf8JsonReader(bytes);
        utf8JsonReader.Read();
        var dictionary = utf8JsonReader.ReadDict();

        Assert.AreEqual(1,       dictionary.Count);
        Assert.AreEqual("value", dictionary["propertyName"]);
    }

    [Test]
    public void ReadDictReadsUntilProperties()
    {
        var bytes          = Encoding.UTF8.GetBytes("{\"propertyName\": \"value\"}");
        var utf8JsonReader = new Utf8JsonReader(bytes);
        var dictionary     = utf8JsonReader.ReadDict();

        Assert.AreEqual(1,       dictionary.Count);
        Assert.AreEqual("value", dictionary["propertyName"]);
    }

    [Test]
    public void ReadDictThrowsJsonExceptionIfValueIsNull()
    {
        var bytes = Encoding.UTF8.GetBytes("{\"propertyName\": null}");
        Assert.Throws<JsonException>(() =>
                                     {
                                         var utf8JsonReader = new Utf8JsonReader(bytes);
                                         utf8JsonReader.Read();
                                         utf8JsonReader.ReadDict();
                                     });
    }
}