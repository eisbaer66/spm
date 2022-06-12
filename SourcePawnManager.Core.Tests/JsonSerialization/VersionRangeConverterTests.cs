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
using NuGet.Versioning;
using NUnit.Framework;
using SourcePawnManager.Core.JsonSerialization;

namespace SourcePawnManager.Core.Tests.JsonSerialization;

public class VersionRangeConverterTests
{
    [Test]
    public void ReadThrowsIfUnparsable()
    {
        var bytes = Encoding.UTF8.GetBytes("\"test\"");

        Assert.Throws<InvalidOperationException>(() =>
                                                 {
                                                     var utf8JsonReader = new Utf8JsonReader(bytes);
                                                     new VersionRangeConverter().Read(ref utf8JsonReader, null!, null!);
                                                 });
    }
    [Test]
    public void Read()
    {
        var bytes = Encoding.UTF8.GetBytes("\"1.*\"");
        
        var utf8JsonReader = new Utf8JsonReader(bytes);
        utf8JsonReader.Read();
        var versionRange   = new VersionRangeConverter().Read(ref utf8JsonReader, null!, null!);

        Assert.IsNotNull(versionRange);
        Assert.AreEqual(NuGetVersion.Parse("1.0"), versionRange!.MinVersion);
        Assert.IsNull(versionRange.MaxVersion);
    }
}