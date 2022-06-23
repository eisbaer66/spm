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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.FileSystems;
using SourcePawnManager.Core.LocalStores;

namespace SourcePawnManager.Core.Tests.LocalStores;

public class FileLocalStoreTests
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
        serviceCollection.AddSingleton<FileLocalStore>();
        serviceCollection.AddSourcePawnManager();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Test]
    public async Task GetDelegatesToJsonFileSystem()
    {
        var path       = "path\\spm.local.json";
        var definition = new IncludeManagerLocalDefinition();

        _fileSystem.HasExtension(path)
                   .Returns(true);
        _fileSystem.ReadJson<IncludeManagerLocalDefinition>(path, CancellationToken.None)
                   .Returns(definition);

        var fileLocalStore = _serviceProvider.GetRequiredService<FileLocalStore>();
        var readDefinition = await fileLocalStore.Get(path);

        Assert.AreSame(definition, readDefinition);
        await _fileSystem.Received()
                         .ReadJson<IncludeManagerLocalDefinition>(path, CancellationToken.None);
    }

    [Test]
    public async Task SetDelegatesToJsonFileSystem()
    {
        var path       = "path\\spm.local.json";
        var definition = new IncludeManagerLocalDefinition();

        _fileSystem.HasExtension(path)
                   .Returns(true);

        var fileLocalStore = _serviceProvider.GetRequiredService<FileLocalStore>();
        await fileLocalStore.Set(definition, path);

        await _fileSystem.Received()
                         .WriteJson(definition, path, CancellationToken.None);
    }
}