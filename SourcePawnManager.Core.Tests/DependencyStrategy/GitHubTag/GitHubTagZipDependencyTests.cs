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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet.Versioning;
using NUnit.Framework;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag.DownloadGitHubTagZip;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag.GetVersionsGitHubTag;

namespace SourcePawnManager.Core.Tests.DependencyStrategy.GitHubTag;

public class GitHubTagZipDependencyTests
{
    private readonly Dictionary<string, string> _fromDictionary = new()
                                                                  {
                                                                      {"owner", "owner"},
                                                                      {"repository", "repository"},
                                                                      {"assetName", "assetName"},
                                                                      {"fileInZip", "fileInZip"},
                                                                      {"downloadPath", "downloadPath"},
                                                                      {"versionRegEx", "versionRegEx"},
                                                                  };

    [Test]
    [TestCase(GitHubTagDependencyBase.DefaultVersionRegEx, "include")]
    [TestCase("",                                          "include")]
    [TestCase("  ",                                        "include")]
    [TestCase(null,                                        "include")]
    [TestCase(GitHubTagDependencyBase.DefaultVersionRegEx, "")]
    [TestCase(GitHubTagDependencyBase.DefaultVersionRegEx, "   ")]
    [TestCase(GitHubTagDependencyBase.DefaultVersionRegEx, null)]
    public void ValidConstructorTest(string versionRegEx, string downloadPath)
    {
        var dependency = new GitHubTagZipDependency("nosoop",
                                                     "tf2attributes",
                                                     VersionRange.Parse("1.0"),
                                                     "tf2attributes.inc",
                                                     "scripting/include/tf_custom_attributes.inc",
                                                     downloadPath,
                                                     versionRegEx);

        Assert.IsNotNull(dependency);
    }

    // ReSharper disable ObjectCreationAsStatement
    [Test]
    [TestCase("",       "tf2attributes", GitHubTagDependencyBase.DefaultVersionRegEx, "tf2attributes.inc", "scripting/include/tf_custom_attributes.inc", "include")]
    [TestCase("nosoop", "",              GitHubTagDependencyBase.DefaultVersionRegEx, "tf2attributes.inc", "scripting/include/tf_custom_attributes.inc", "include")]
    [TestCase("nosoop", "tf2attributes", GitHubTagDependencyBase.DefaultVersionRegEx, "", "scripting/include/tf_custom_attributes.inc", "include")]
    public void EmptyStringConstructorThrowsArgumentException(string owner,
                                                              string repository,
                                                              string versionRegEx,
                                                              string assetName,
                                                              string fileInZip,
                                                              string downloadPath)
    {
        Assert.Throws<ArgumentException>(() => new GitHubTagZipDependency(owner,
                                                                           repository,
                                                                           VersionRange.Parse("1.0"),
                                                                           assetName,
                                                                           fileInZip,
                                                                           downloadPath,
                                                                           versionRegEx));
    }

    [Test]
    [TestCase(null,     "tf2attributes", "1.0", GitHubTagDependencyBase.DefaultVersionRegEx, "package.zip", "scripting/include/tf_custom_attributes.inc")]
    [TestCase("nosoop", null,            "1.0", GitHubTagDependencyBase.DefaultVersionRegEx, "package.zip", "scripting/include/tf_custom_attributes.inc")]
    [TestCase("nosoop", "tf2attributes", null,  GitHubTagDependencyBase.DefaultVersionRegEx, "package.zip", "scripting/include/tf_custom_attributes.inc")]
    [TestCase("nosoop", "tf2attributes", "1.0", GitHubTagDependencyBase.DefaultVersionRegEx, null, "scripting/include/tf_custom_attributes.inc")]
    [TestCase("nosoop", "tf2attributes", "1.0", GitHubTagDependencyBase.DefaultVersionRegEx, "package.zip", null)]
    public void NullConstructorThrowsArgumentNullException(string? owner,
                                                           string? repository,
                                                           string? versionRange,
                                                           string? versionRegEx,
                                                           string? assetName,
                                                           string? fileInZip)
    {
        Assert.Throws<ArgumentNullException>(() => new GitHubTagZipDependency(owner!,
                                                                               repository!,
                                                                               versionRange != null
                                                                                   ? VersionRange.Parse(versionRange)
                                                                                   : null!,
                                                                               assetName!,
                                                                               fileInZip!,
                                                                               null,
                                                                               versionRegEx));
    }

    // ReSharper restore ObjectCreationAsStatement

    [Test]
    public void GetVersionsReturnsGetVersionsGitHubTagQuery()
    {
        var dependency = new GitHubTagZipDependency("nosoop",
                                                     "tf2attributes",
                                                     VersionRange.Parse("1.0"),
                                                     "package.zip",
                                                     "scripting/include/tf_custom_attributes.inc");
        var request = dependency.GetVersions();
        Assert.IsNotNull(request);
        var query = request as GetVersionsGitHubTagQuery;
        Assert.IsNotNull(query);
        Assert.AreSame(dependency, query!.Dependency);
    }

    [Test]
    public void DownloadReturnsDownloadGitHubTagFileQuery()
    {
        var dependency = new GitHubTagZipDependency("nosoop",
                                                    "tf2attributes",
                                                    VersionRange.Parse("1.0"),
                                                    "package.zip",
                                                    "scripting/include/tf_custom_attributes.inc");
        var version = new DependencyVersion(NuGetVersion.Parse("1.0"), "tag");
        var request = dependency.Download(version);
        Assert.IsNotNull(request);
        var query = request as DownloadGitHubTagZipQuery;
        Assert.IsNotNull(query);
        Assert.AreSame(dependency, query!.Dependency);
        Assert.AreSame(version,    query.Version);
    }

    [Test]
    public void FromThrowsArgumentExceptionIfOwnerIsNotPresent()
    {
        var dictionary = _fromDictionary.ToDictionary(x => x.Key, x => x.Value);
        dictionary.Remove("owner");
        Assert.Throws<ArgumentException>(() => GitHubTagZipDependency.From(dictionary, VersionRange.All));
    }

    [Test]
    public void FromThrowsArgumentExceptionIfRepositoryIsNotPresent()
    {
        var dictionary = _fromDictionary.ToDictionary(x => x.Key, x => x.Value);
        dictionary.Remove("repository");
        Assert.Throws<ArgumentException>(() => GitHubTagZipDependency.From(dictionary, VersionRange.All));
    }

    [Test]
    public void FromThrowsArgumentExceptionIfAssetNameIsNotPresent()
    {
        var dictionary = _fromDictionary.ToDictionary(x => x.Key, x => x.Value);
        dictionary.Remove("assetName");
        Assert.Throws<ArgumentException>(() => GitHubTagZipDependency.From(dictionary, VersionRange.All));
    }

    [Test]
    public void FromThrowsArgumentExceptionIfFileInZipIsNotPresent()
    {
        var dictionary = _fromDictionary.ToDictionary(x => x.Key, x => x.Value);
        dictionary.Remove("fileInZip");
        Assert.Throws<ArgumentException>(() => GitHubTagZipDependency.From(dictionary, VersionRange.All));
    }

    [Test]
    public void FromSetsDownloadPathToDefaultIfNotPresent()
    {
        var dictionary = _fromDictionary.ToDictionary(x => x.Key, x => x.Value);
        dictionary.Remove("downloadPath");
        var dependency = GitHubTagZipDependency.From(dictionary, VersionRange.All);
        Assert.AreEqual(Path.Combine("include", "fileInZip"), dependency.DownloadPath);
    }

    [Test]
    public void FromWorksIfNotPresent()
    {
        var dictionary = _fromDictionary.ToDictionary(x => x.Key, x => x.Value);
        dictionary.Remove("versionRegEx");
        GitHubTagZipDependency.From(dictionary, VersionRange.All);
    }

    [Test]
    public void EqualsNullReturnsFalse()
    {
        var dependency = new GitHubTagZipDependency("nosoop",
                                                     "tf2attributes",
                                                     VersionRange.Parse("1.0"),
                                                     "package.zip",
                                                     "scripting/include/tf_custom_attributes.inc");
        Assert.False(dependency.Equals(null));
    }

    [Test]
    public void EqualsItselfReturnsTrue()
    {
        var dependency = new GitHubTagZipDependency("nosoop",
                                                     "tf2attributes",
                                                     VersionRange.Parse("1.0"),
                                                     "package.zip",
                                                     "scripting/include/tf_custom_attributes.inc");
        Assert.True(dependency.Equals(dependency));
    }

    [Test]
    public void EqualsDifferentTypeReturnsFalse()
    {
        var dependency = new GitHubTagZipDependency("nosoop",
                                                     "tf2attributes",
                                                     VersionRange.Parse("1.0"),
                                                     "package.zip",
                                                     "scripting/include/tf_custom_attributes.inc");
        Assert.False(dependency.Equals(123));
    }

    [Test]
    public void TryParseVersionUsesCustomVersionRegularExpression()
    {
        var dependency = new GitHubTagZipDependency("nosoop",
                                                     "tf2attributes",
                                                     VersionRange.Parse("1.0"),
                                                     "package.zip",
                                                     "scripting/include/tf_custom_attributes.inc",
                                                     null,
                                                     @"(\d+(?:\.\d+)?(?:\.\d+)?(?:\.\d+)?)asd[-+]?(\S*)?");
        var parsed = dependency.TryParseVersion("1.0asd", out var version);
        Assert.True(parsed);
        Assert.AreEqual(new NuGetVersion(1, 0, 0), version);
    }

    [Test]
    public void TryParseVersionFailsIfCustomVersionRegularExpressionDoesContainOnlyOneGroup()
    {
        var dependency = new GitHubTagZipDependency("nosoop",
                                                     "tf2attributes",
                                                     VersionRange.Parse("1.0"),
                                                     "package.zip",
                                                     "scripting/include/tf_custom_attributes.inc",
                                                     null,
                                                     @"(\d+(?:\.\d+)?(?:\.\d+)?(?:\.\d+)?)");
        var parsed = dependency.TryParseVersion("1.0", out _);
        Assert.False(parsed);
    }
}