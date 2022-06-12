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
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.FileSystems;

namespace SourcePawnManager.Core.Tests.Apis.FileSystem;

public class DryRunFileSystemTests
{
    private const string Path = "path";

    private static readonly object[] Cases =
    {
        new object[]
        {
            null!,
            Substitute.For<ILogger<DryRunFileSystem>>(),
        },
        new object[]
        {
            Substitute.For<IFileSystem>(),
            null!,
        },
    };

    private static readonly object[] DelegatesToFileSystemCases =
    {
        new object[] { (IFileSystem d) => { d.HasExtension(Path); }, true },
        new object[] { (IFileSystem d) => { d.FileExists(Path); }, true },
        new object[] { (IFileSystem d) => { d.GetFiles(Path, "searchPattern"); }, true },
        new object[] { (IFileSystem d) => { d.Read(Path); }, true },
        new object[] { (IFileSystem d) => { d.Write(Stream.Null, Path); }, false },
        new object[] { (IFileSystem d) => { d.Delete(Path); }, false },
    };

    [Test]
    [TestCaseSource(nameof(Cases))]
    public void ConstructorThrowsArgumentNullException(IFileSystem fileSystem, ILogger<DryRunFileSystem> logger)
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new DryRunFileSystem(fileSystem, logger));
    }

    [Test]
    [TestCaseSource(nameof(DelegatesToFileSystemCases))]
    public void DelegatesToFileSystem(Action<IFileSystem> action, bool delegates)
    {
        var fileSystem = Substitute.For<IFileSystem>();

        var dryRunFileSystem = new DryRunFileSystem(fileSystem, Substitute.For<ILogger<DryRunFileSystem>>());
        action(dryRunFileSystem);
        
#pragma warning disable NS5000 // Received check.
        var received = delegates ? fileSystem.Received() : fileSystem.DidNotReceive();
#pragma warning restore NS5000 // Received check.
        action(received);
    }
}