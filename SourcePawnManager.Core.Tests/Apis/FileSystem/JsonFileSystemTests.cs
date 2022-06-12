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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Json.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NuGet.Versioning;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.FileSystems;
using SourcePawnManager.Core.DependencyStrategy;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;
using SourcePawnManager.Core.JsonSerialization.Schemas;
using SourcePawnManager.Core.LocalStores;
using SourcePawnManager.Core.LockStores;

namespace SourcePawnManager.Core.Tests.Apis.FileSystem;

public class JsonFileSystemTests
{
    private const string TestPath = "path";
    private static readonly object[] ConstructorCases =
    {
        new object[]
        {
            null!,
            Substitute.For<IFileSystem>(),
            Substitute.For<IJSchemaStore>(),
            Substitute.For<ILogger<JsonFileSystem>>(),
        },
        new object[]
        {
            new JsonSerializerOptions(),
            null!,
            Substitute.For<IJSchemaStore>(),
            Substitute.For<ILogger<JsonFileSystem>>(),
        },
        new object[]
        {
            new JsonSerializerOptions(),
            Substitute.For<IFileSystem>(),
            null!,
            Substitute.For<ILogger<JsonFileSystem>>(),
        },
        new object[]
        {
            new JsonSerializerOptions(),
            Substitute.For<IFileSystem>(),
            Substitute.For<IJSchemaStore>(),
            null!,
        },
    };

    private static readonly object[] JsonSerializationCases =
    {
        new object[]
        {
            new DependencyVersion(NuGetVersion.Parse("1.0.0"), "1.0.0"),
            "{\r\n  \"version\": \"1.0.0\",\r\n  \"tag\": \"1.0.0\"\r\n}".ToEnvironmentString(),
        },
        new object[]
        {
            new DependencyVersion(NuGetVersion.Parse("1.7.1"), "1.7.1"),
            "{\r\n  \"version\": \"1.7.1\",\r\n  \"tag\": \"1.7.1\"\r\n}".ToEnvironmentString(),
        },
        new object[]
        {
            new DependencyVersion(NuGetVersion.Parse("1.7.1.1"), "1.7.1.1"),
            "{\r\n  \"version\": \"1.7.1.1\",\r\n  \"tag\": \"1.7.1.1\"\r\n}".ToEnvironmentString(),
        },
        new object[]
        {
            new IncludeManagerLocalDefinition
            {
                PreviousDownloadPaths = new[] { "firstPath", "secondPath" },
            },
            "{\r\n  \"previousDownloadPaths\": [\r\n    \"firstPath\",\r\n    \"secondPath\"\r\n  ]\r\n}"
                .ToEnvironmentString(),
        },
        new object[]
        {
            new IncludeManagerDefinition
            {
                Dependencies = new IDependency[]
                               {
                                   new GitHubTagFileDependency("nosoop",
                                                               "tf2attributes",
                                                               VersionRange.Parse("1.7.1.1"),
                                                               "tf2attributes.inc"),
                                   new GitHubTagZipDependency("nosoop",
                                                              "SM-TFCustAttr",
                                                              VersionRange.Parse("8.0"),
                                                              "package.zip",
                                                              "scripting/include/tf_custom_attributes.inc"),
                               },
            },
            @"{
  ""dependencies"": [
    {
      ""type"": ""GitHubTagFile"",
      ""versionRange"": ""[1.7.1.1, )"",
      ""owner"": ""nosoop"",
      ""repository"": ""tf2attributes"",
      ""assetName"": ""tf2attributes.inc"",
      ""downloadPath"": ""include\\tf2attributes.inc""
    },
    {
      ""type"": ""GitHubTagZip"",
      ""versionRange"": ""[8.0.0, )"",
      ""owner"": ""nosoop"",
      ""repository"": ""SM-TFCustAttr"",
      ""assetName"": ""package.zip"",
      ""fileInZip"": ""scripting/include/tf_custom_attributes.inc"",
      ""downloadPath"": ""include\\tf_custom_attributes.inc""
    }
  ]
}".ToEnvironmentString(),
        },
        new object[] { Substitute.For<LockDefinition>(), "{\r\n  \"includeLocks\": []\r\n}".ToEnvironmentString() },
        new object[]
        {
            new LockDefinition
            {
                IncludeLocks = new[]
                               {
                                   new IncludeLockDefinition("1",
                                                             new(NuGetVersion.Parse("1.0"), "1.0")),
                                   new IncludeLockDefinition("2",
                                                             new(NuGetVersion.Parse("1.1"), "1.1")),
                               },
            },
            @"{
  ""includeLocks"": [
    {
      ""id"": ""1"",
      ""version"": {
        ""version"": ""1.0"",
        ""tag"": ""1.0""
      }
    },
    {
      ""id"": ""2"",
      ""version"": {
        ""version"": ""1.1"",
        ""tag"": ""1.1""
      }
    }
  ]
}".ToEnvironmentString(),
        },
    };

    private static readonly object[] ReadCases = JsonSerializationCases
                                                 .Concat(new object[]
                                                         {
                                                             new object[]
                                                             {
                                                                 new DependencyVersion(NuGetVersion.Parse("1.7.1.1"),
                                                                                       "1.7.1.1"),
                                                                 "{\"version\": \"1.7.1.1\", \"tag\": \"1.7.1.1\"}"
                                                                     .ToEnvironmentString(),
                                                             },
                                                             new object[]
                                                             {
                                                                 new DependencyVersion(NuGetVersion.Parse("1.7.1.1"),
                                                                                       "1.7.1.1"),
                                                                 "{\"version\":\"1.7.1.1\",\"tag\":\"1.7.1.1\"}"
                                                                     .ToEnvironmentString(),
                                                             },
                                                         })
                                                 .ToArray();

    private static readonly object[] DelegatesToFileSystemCases =
    {
        new object[] { (IFileSystem d) => { d.HasExtension(TestPath); } },
        new object[] { (IFileSystem d) => { d.FileExists(TestPath); } },
        new object[] { (IFileSystem d) => { d.GetFiles(TestPath, "searchPattern"); } },
        new object[] { (IFileSystem d) => { d.Read(TestPath); } },
        new object[] { (IFileSystem d) => { d.Write(Stream.Null, TestPath); } },
        new object[] { (IFileSystem d) => { d.Delete(TestPath); } },
    };

    private IFileSystem      _fileSystem      = null!;
    private IServiceProvider _serviceProvider = null!;
    private IJSchemaStore    _jSchemaStore    = null!;

    [SetUp]
    public void Setup()
    {
        _fileSystem   = Substitute.For<IFileSystem>();
        _jSchemaStore = Substitute.For<IJSchemaStore>();
        _jSchemaStore.Get<DependencyVersion>().Returns(_ => _serviceProvider.GetRequiredService<EmbeddedFileJSchemaStore>().Get<DependencyVersion>());
        _jSchemaStore.Get<IncludeManagerDefinition>().Returns(_ => _serviceProvider.GetRequiredService<EmbeddedFileJSchemaStore>().Get<IncludeManagerDefinition>());
        _jSchemaStore.Get<IncludeManagerLocalDefinition>().Returns(_ => _serviceProvider.GetRequiredService<EmbeddedFileJSchemaStore>().Get<IncludeManagerLocalDefinition>());
        _jSchemaStore.Get<LockDefinition>().Returns(_ => _serviceProvider.GetRequiredService<EmbeddedFileJSchemaStore>().Get<LockDefinition>());

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSerilog();
        serviceCollection.AddSingleton(_fileSystem);
        serviceCollection.AddSingleton(_jSchemaStore);
        serviceCollection.AddSourcePawnManager();
        serviceCollection.AddSingleton<JsonFileSystem>();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Test]
    [TestCaseSource(nameof(ConstructorCases))]
    public void ConstructorThrowsArgumentNullException(JsonSerializerOptions   jsonSerializerOptions,
                                                       IFileSystem             fileSystem,
                                                       IJSchemaStore           jSchemaStore,
                                                       ILogger<JsonFileSystem> logger)
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new JsonFileSystem(jsonSerializerOptions, fileSystem, jSchemaStore, logger));
    }

    [Test]
    public async Task WriteJsonDelegatesToFileSystem()
    {
        var expectedPath = Path.Combine("path", "spm.json");
        var definition   = Substitute.For<IncludeManagerDefinition>();

        var jsonFileSystem = _serviceProvider.GetRequiredService<JsonFileSystem>();
        await jsonFileSystem.WriteJson(definition, expectedPath, CancellationToken.None);

        await _fileSystem.Received().Write(Arg.Any<Stream>(), expectedPath);
    }

    [Test]
    public async Task WriteJsonSerializesNullToEmptyString()
    {
        var     expectedPath = Path.Combine("path", "spm.json");
        string? output       = null;
        _fileSystem.When(fs => fs.Write(Arg.Any<Stream>(), expectedPath))
                   .Do(cb =>
                       {
                           using var reader = new StreamReader(cb.Arg<Stream>());
                           output = reader.ReadToEnd();
                       });

        var jsonFileSystem = _serviceProvider.GetRequiredService<JsonFileSystem>();
        await jsonFileSystem.WriteJson<DependencyVersion>(null!, expectedPath, CancellationToken.None);

        Assert.AreEqual(string.Empty, output);
    }

    [Test]
    [TestCaseSource(nameof(JsonSerializationCases))]
    public async Task WriteJsonSerializesTypeToString(object obj, string expected)
    {
        var     expectedPath = Path.Combine("path", "spm.json");
        string? output       = null;
        _fileSystem.When(fs => fs.Write(Arg.Any<Stream>(), expectedPath))
                   .Do(cb =>
                       {
                           using var reader = new StreamReader(cb.Arg<Stream>());
                           output = reader.ReadToEnd();
                       });

        var jsonFileSystem = _serviceProvider.GetRequiredService<JsonFileSystem>();
        await jsonFileSystem.WriteJson(obj, expectedPath, CancellationToken.None);

        Assert.AreEqual(expected, output);
    }

    [Test]
    public async Task ReadReturnsNullIfFileDoesNotExist()
    {
        var path = Path.Combine("path", "spm.json");
        _fileSystem.FileExists(path).Returns(false);

        var jsonFileSystem = _serviceProvider.GetRequiredService<JsonFileSystem>();
        await jsonFileSystem.ReadJson<IncludeManagerDefinition>(path, CancellationToken.None);

        _fileSystem.DidNotReceive().Read(path);
    }

    [Test]
    public async Task ReadReturnsNullIfFileDoesNotExistAfterCheck()
    {
        var path = Path.Combine("path", "spm.json");
        _fileSystem.FileExists(path).Returns(true);
        _fileSystem.Read(path).Throws(new FileNotFoundException());

        var jsonFileSystem = _serviceProvider.GetRequiredService<JsonFileSystem>();
        var definition     = await jsonFileSystem.ReadJson<IncludeManagerDefinition>(path, CancellationToken.None);

        Assert.IsNull(definition);
    }

    [Test]
    public async Task ReadReturnsNullIfFileIsEmpty()
    {
        var path = Path.Combine("path", "spm.json");
        _fileSystem.FileExists(path).Returns(true);
        _fileSystem.Read(path).Returns(Stream.Null);

        var jsonFileSystem = _serviceProvider.GetRequiredService<JsonFileSystem>();
        var definition     = await jsonFileSystem.ReadJson<IncludeManagerDefinition>(path, CancellationToken.None);

        Assert.IsNull(definition);
    }

    [Test]
    public async Task ReadReturnsNullIfFileIsEmptyObject()
    {
        var path = Path.Combine("path", "spm.json");
        _fileSystem.FileExists(path).Returns(true);
        _fileSystem.Read(path).Returns("{}".ToMemoryStream());

        var jsonFileSystem = _serviceProvider.GetRequiredService<JsonFileSystem>();
        var definition     = await jsonFileSystem.ReadJson<IncludeManagerDefinition>(path, CancellationToken.None);

        Assert.IsNull(definition);
    }

    [Test]
    public async Task ReadJsonDelegatesToFileSystem()
    {
        var expectedPath = Path.Combine("path", "spm.json");
        _fileSystem.FileExists(expectedPath).Returns(true);
        _fileSystem.Read(expectedPath).Returns(new MemoryStream());

        var jsonFileSystem = _serviceProvider.GetRequiredService<JsonFileSystem>();
        await jsonFileSystem.ReadJson<IncludeManagerDefinition>(expectedPath, CancellationToken.None);

        _fileSystem.Received().Read(expectedPath);
    }

    [Test]
    [TestCase("{version: \"1.0.0\"}")]
    [TestCase("{tag: \"1.0.0\"}")]
    public async Task DependencyVersionReadAsNullTests(string input)
    {
        var expectedPath = Path.Combine("path", "spm.json");
        _fileSystem.FileExists(expectedPath).Returns(true);
        _fileSystem.Read(expectedPath).Returns(input.ToMemoryStream());

        var jsonFileSystem = _serviceProvider.GetRequiredService<JsonFileSystem>();
        var version        = await jsonFileSystem.ReadJson<DependencyVersion>(expectedPath, CancellationToken.None);

        Assert.IsNull(version);
    }

    [Test]
    [TestCaseSource(nameof(ReadCases))]
    public async Task ReadJsonSerializesTypeToString<T>(T expected, string input)
    {
        var expectedPath = Path.Combine("path", "spm.json");
        _fileSystem.FileExists(expectedPath).Returns(true);
        _fileSystem.Read(expectedPath).Returns(input.ToMemoryStream());

        var jsonFileSystem = _serviceProvider.GetRequiredService<JsonFileSystem>();
        var obj            = await jsonFileSystem.ReadJson<T>(expectedPath, CancellationToken.None);

        Assert.AreEqual(expected, obj);
    }

    [Test]
    public async Task ReadJsonReturnsNullIfNoSchemaAvailable()
    {
        var expectedPath = Path.Combine("path", "spm.json");
        _fileSystem.FileExists(expectedPath).Returns(true);
        _fileSystem.Read(expectedPath)
                   .Returns("{\"version\":\"1.7.1.1\",\"tag\":\"1.7.1.1\"}"
                            .ToEnvironmentString()
                            .ToMemoryStream());
        _jSchemaStore.Get<DependencyVersion>().Returns(Task.FromResult((JsonSchema?)null));

        var jsonFileSystem = _serviceProvider.GetRequiredService<JsonFileSystem>();
        var obj            = await jsonFileSystem.ReadJson<DependencyVersion>(expectedPath, CancellationToken.None);

        Assert.IsNull(obj);
    }

    [Test]
    [TestCaseSource(nameof(DelegatesToFileSystemCases))]
    public void DelegatesToFileSystem(Action<IFileSystem> action)
    {
        var jsonFileSystem = _serviceProvider.GetRequiredService<IJsonFileSystem>();
        action(jsonFileSystem);

#pragma warning disable NS5000 // Received check.
        action(_fileSystem.Received());
#pragma warning restore NS5000 // Received check.
    }
}