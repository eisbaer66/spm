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
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using SourcePawnManager.Core.DependencyStrategy.StaticUrl;
using SourcePawnManager.Core.JsonSerialization;

namespace SourcePawnManager.Core.Tests.JsonSerialization;

public class DependencyConverterTests
{
    [Test]
    public void ReadsStaticUrlDependency()
    {
        var bytes          = Encoding.UTF8.GetBytes(@"{
  ""type"": ""StaticUrl"",
  ""versionRange"": ""(, )"",
  ""url"": ""https://raw.githubusercontent.com/JoinedSenses/SourceMod-IncludeLibrary/master/include/morecolors.inc"",
  ""downloadPath"": ""lib/morecolors_newsyntax.inc""
}");
        
        var utf8JsonReader = new Utf8JsonReader(bytes);
        utf8JsonReader.Read();
        var dependency     = new DependencyConverter().Read(ref utf8JsonReader, null!, null!);

        Assert.IsNotNull(dependency);
        Assert.IsTrue(dependency is StaticUrlDependency);
    }

    [Test]
    public void ReadPropertyThrowsIfNotExpectedPosition()
    {
        var bytes          = Encoding.UTF8.GetBytes("\"test\"");

        Assert.Throws<JsonException>(() =>
                                     {
                                         var utf8JsonReader = new Utf8JsonReader(bytes);
                                         new DependencyConverter().Read(ref utf8JsonReader, null!, null!);
                                     });
    }

    [Test]
    public void ReadPropertyThrowsIfVersionRangeIsUnparseable()
    {
        var bytes = Encoding.UTF8.GetBytes("{\"type\": \"GitHubTagFile\", \"versionRange\": \"unparseable\"}");

        Assert.Throws<JsonException>(() =>
                                     {
                                         var utf8JsonReader = new Utf8JsonReader(bytes);
                                         utf8JsonReader.Read();
                                         new DependencyConverter().Read(ref utf8JsonReader, null!, null!);
                                     });
    }

    [Test]
    public void ReadPropertyThrowsIfTypeIsUnparseable()
    {
        var bytes = Encoding.UTF8.GetBytes("{\"type\": \"unparseable\", \"versionRange\": \"1.*\", \"additionalInfo\": \"\"}");

        Assert.Throws<ArgumentException>(() =>
                                         {
                                             var utf8JsonReader = new Utf8JsonReader(bytes);
                                             utf8JsonReader.Read();
                                             new DependencyConverter().Read(ref utf8JsonReader, null!, null!);
                                         });
    }

    [Test]
    public void ReadPropertyThrowsIfTypeIsUnkown()
    {
        var bytes = Encoding.UTF8.GetBytes("{\"type\": \"Unknown\", \"versionRange\": \"1.*\", \"additionalInfo\": \"\"}");

        Assert.Throws<JsonException>(() =>
                                         {
                                             var utf8JsonReader = new Utf8JsonReader(bytes);
                                             utf8JsonReader.Read();
                                             new DependencyConverter().Read(ref utf8JsonReader, null!, null!);
                                         });
    }
}