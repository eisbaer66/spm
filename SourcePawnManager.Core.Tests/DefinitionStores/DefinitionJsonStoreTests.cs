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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.FileSystems;
using SourcePawnManager.Core.DefinitionStores;

namespace SourcePawnManager.Core.Tests.DefinitionStores;

public class DefinitionJsonStoreTests
{
    private IJsonFileSystem  _fileSystem      = null!;
    private IServiceProvider _serviceProvider = null!;

    [SetUp]
    public void Setup()
    {
        _fileSystem = Substitute.For<IJsonFileSystem>();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSerilog();
        serviceCollection.AddSingleton(_fileSystem);
        serviceCollection.AddSourcePawnManager();
        serviceCollection.AddSingleton<DefinitionJsonStore>();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Test]
    public void ReadThrowsIfPathIsNull()
    {
        var store = _serviceProvider.GetRequiredService<DefinitionJsonStore>();
        Assert.ThrowsAsync<ArgumentNullException>(async () => await store.Read(null!, CancellationToken.None));
    }

    [Test]
    public void ReadThrowsIfPathIsEmpty()
    {
        var store = _serviceProvider.GetRequiredService<DefinitionJsonStore>();
        Assert.ThrowsAsync<ArgumentException>(async () => await store.Read(string.Empty, CancellationToken.None));
    }

    [Test]
    public async Task ReadDelegatesToJsonFileSystem()
    {
        var path       = "path\\spm.json";
        var definition = new IncludeManagerDefinition();

        _fileSystem.HasExtension(path)
                   .Returns(true);
        _fileSystem.ReadJson<IncludeManagerDefinition>(path, CancellationToken.None)
                   .Returns(definition);

        var store          = _serviceProvider.GetRequiredService<DefinitionJsonStore>();
        var readDefinition = await store.Read(path, CancellationToken.None);
        Assert.AreSame(definition, readDefinition);

        _fileSystem.Received()
                   .HasExtension(path);
        await _fileSystem.Received()
                         .ReadJson<IncludeManagerDefinition>(path, CancellationToken.None);
    }

    [Test]
    public void WriteThrowsIfPathIsNull()
    {
        var store = _serviceProvider.GetRequiredService<DefinitionJsonStore>();
        Assert.ThrowsAsync<ArgumentNullException>(async () => await store.Write(null!,
                                                                                Substitute.For<IncludeManagerDefinition>(),
                                                                                CancellationToken.None));
    }

    [Test]
    public void WriteThrowsIfPathIsEmpty()
    {
        var store = _serviceProvider.GetRequiredService<DefinitionJsonStore>();
        Assert.ThrowsAsync<ArgumentException>(async () => await store.Write(string.Empty,
                                                                            Substitute.For<IncludeManagerDefinition>(),
                                                                            CancellationToken.None));
    }

    [Test]
    public void WriteThrowsIfDefinitionIsNull()
    {
        var store = _serviceProvider.GetRequiredService<DefinitionJsonStore>();
        Assert.ThrowsAsync<ArgumentNullException>(async () => await store.Write("path", null!, CancellationToken.None));
    }

    [Test]
    public async Task WritesToDefaultPath()
    {
        var path         = "path";
        var expectedPath = Path.Combine(path, "spm.json");
        var definition   = Substitute.For<IncludeManagerDefinition>();
        _fileSystem.HasExtension(path)
                   .Returns(false);

        var store = _serviceProvider.GetRequiredService<DefinitionJsonStore>();
        await store.Write(path, definition, CancellationToken.None);

        await _fileSystem.Received().WriteJson(definition, expectedPath, CancellationToken.None);
    }

    [Test]
    public async Task WritesToSpecifiedFile()
    {
        var path       = "path";
        var definition = Substitute.For<IncludeManagerDefinition>();
        _fileSystem.HasExtension(path)
                   .Returns(true);

        var store = _serviceProvider.GetRequiredService<DefinitionJsonStore>();
        await store.Write(path, definition, CancellationToken.None);

        await _fileSystem.Received().WriteJson(definition, path, CancellationToken.None);
    }
}