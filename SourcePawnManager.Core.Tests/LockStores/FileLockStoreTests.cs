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
using NuGet.Versioning;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.FileSystems;
using SourcePawnManager.Core.LockStores;

namespace SourcePawnManager.Core.Tests.LockStores;

public class FileLockStoreTests
{
    private static readonly object[] SetCallsWriteJsonCases =
    {
        new object[] { new LockDefinition() },
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
        },
        new object[] { null! },
    };

    private IJsonFileSystem  _fileSystem      = null!;
    private IServiceProvider _serviceProvider = null!;

    [SetUp]
    public void Setup()
    {
        _fileSystem = Substitute.For<IJsonFileSystem>();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSerilog();
        serviceCollection.AddSingleton(_fileSystem);
        serviceCollection.AddSingleton<FileLockStore>();
        serviceCollection.AddSourcePawnManager();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Test]
    public async Task GetReturnsEmptyLockDefinitionIfFileSystemReturnsNull()
    {
        var path = "path";

        _fileSystem.HasExtension(path)
                   .Returns(true);
        _fileSystem.ReadJson<LockDefinition>(path, CancellationToken.None)
                   .Returns((LockDefinition)null!);

        var store          = _serviceProvider.GetRequiredService<FileLockStore>();
        var lockDefinition = await store.Get(path, CancellationToken.None);

        Assert.IsNotNull(lockDefinition);
        Assert.IsNotNull(lockDefinition.IncludeLocks);
        Assert.AreEqual(0, lockDefinition.IncludeLocks.Count);

        await _fileSystem.Received().ReadJson<LockDefinition>(path, CancellationToken.None);
    }

    [Test]
    public async Task GetAddsDefaultFileNameToPath()
    {
        var path     = "path";
        var filePath = Path.Combine(path, FileLockStore.DefaultFileName);

        _fileSystem.HasExtension(filePath)
                   .Returns(false);
        _fileSystem.ReadJson<LockDefinition>(filePath, CancellationToken.None)
                   .Returns((LockDefinition)null!);

        var store          = _serviceProvider.GetRequiredService<FileLockStore>();
        var lockDefinition = await store.Get(path, CancellationToken.None);

        Assert.IsNotNull(lockDefinition);
        Assert.IsNotNull(lockDefinition.IncludeLocks);
        Assert.AreEqual(0, lockDefinition.IncludeLocks.Count);

        await _fileSystem.Received().ReadJson<LockDefinition>(filePath, CancellationToken.None);
    }

    [Test]
    public async Task GetReadsFile()
    {
        var path       = "path\\spm.lock.json";
        var definition = new LockDefinition();

        _fileSystem.HasExtension(path)
                   .Returns(true);
        _fileSystem.FileExists(path)
                   .Returns(true);
        _fileSystem.ReadJson<LockDefinition>(path, CancellationToken.None)
                   .Returns(definition);

        var store          = _serviceProvider.GetRequiredService<FileLockStore>();
        var readDefinition = await store.Get(path, CancellationToken.None);

        Assert.AreSame(definition, readDefinition);
        await _fileSystem.Received().ReadJson<LockDefinition>(path, CancellationToken.None);
    }

    [Test]
    [TestCaseSource(nameof(SetCallsWriteJsonCases))]
    public async Task SetCallsWriteJson(LockDefinition lockDefinition)
    {
        const string path = "path\\spm.lock.json";

        _fileSystem.HasExtension(path)
                   .Returns(true);

        var store = _serviceProvider.GetRequiredService<FileLockStore>();
        await store.Set(lockDefinition, path, CancellationToken.None);

        await _fileSystem.Received().WriteJson(lockDefinition, path, CancellationToken.None);
    }
}