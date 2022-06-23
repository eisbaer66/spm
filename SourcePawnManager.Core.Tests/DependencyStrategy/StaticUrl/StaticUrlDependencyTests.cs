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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet.Versioning;
using NUnit.Framework;
using SourcePawnManager.Core.DependencyStrategy;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;
using SourcePawnManager.Core.DependencyStrategy.Shared;
using SourcePawnManager.Core.DependencyStrategy.StaticUrl;

namespace SourcePawnManager.Core.Tests.DependencyStrategy.StaticUrl;

public class StaticUrlDependencyTests
{
    private readonly Dictionary<string, string> _fromDictionary = new()
                                                                  {
                                                                      {"url", "url/asset.inc"},
                                                                      {"downloadPath", "downloadPath/asset.inc"},
                                                                  };

    [Test]
    public void FromSetsData()
    {
        var dependency = StaticUrlDependency.From(_fromDictionary);

        Assert.AreEqual($"StaticUrl:{_fromDictionary["url"]}", dependency.Id);
        Assert.AreEqual(DependencyType.StaticUrl,              dependency.Type);
        Assert.AreEqual(VersionRange.All,                      dependency.VersionRange);
        Assert.AreEqual(_fromDictionary["url"],                dependency.Url);
        Assert.AreEqual(_fromDictionary["downloadPath"],       dependency.DownloadPath);
        
        Assert.IsTrue(dependency.GetVersions() is GetStaticVersionsQuery);
        Assert.IsTrue(dependency.Download(DependencyVersion.Parse("0.0")) is DownloadStaticUrlQuery);
    }

    [Test]
    public void FromSetsDownloadPathToDefaultIfNull()
    {
        var dictionary = _fromDictionary.ToDictionary(x => x.Key, x => x.Value);
        dictionary.Remove("downloadPath");
        var dependency = StaticUrlDependency.From(dictionary);
        Assert.AreEqual(Path.Combine("include", "asset.inc"), dependency.DownloadPath);
    }

    [Test]
    public void FromSetsDownloadPathToDefaultIfEmpty()
    {
        var dictionary = _fromDictionary.ToDictionary(x => x.Key, x => x.Value);
        dictionary["downloadPath"] = string.Empty;
        var dependency = StaticUrlDependency.From(dictionary);
        Assert.AreEqual(Path.Combine("include", "asset.inc"), dependency.DownloadPath);
    }
}