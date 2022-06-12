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
using System.Threading;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using SourcePawnManager.Core.DependencyStrategy;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;
using SourcePawnManager.Core.VersionStores;

namespace SourcePawnManager.Core.Tests.VersionStores;

public class DryRunVersionStoreTests
{
    private static readonly IDependency Dependency = Substitute.For<IDependency>();

    private static readonly object[] ConstructorCases =
    {
        new object[]
        {
            null!,
            Substitute.For<ILogger<DryRunVersionStore>>(),
        },
        new object[]
        {
            Substitute.For<IVersionStore>(),
            null!,
        },
    };

    private static readonly object[] DelegatesToFileSystemCases =
    {
        new object[] { (IVersionStore s) => { s.GetExistingVersion(Dependency, CancellationToken.None); }, true },
        new object[] { (IVersionStore s) => { s.Clean(new List<string>(), new List<IDependency>()); }, false },
        new object[] { (IVersionStore s) => { s.Set(Dependency, DependencyVersion.Parse("1.0"), CancellationToken.None); }, false },
        new object[] { (IVersionStore s) => { s.Delete(Dependency); }, false },
    };

    [Test]
    [TestCaseSource(nameof(ConstructorCases))]
    public void ConstructorThrowsArgumentNullException(IVersionStore versionStore, ILogger<DryRunVersionStore> logger)
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new DryRunVersionStore(versionStore, logger));
    }

    [Test]
    [TestCaseSource(nameof(DelegatesToFileSystemCases))]
    public void DelegatesToFileSystem(Action<IVersionStore> action, bool delegates)
    {
        var versionStore = Substitute.For<IVersionStore>();

        var dryRunVersionStore = new DryRunVersionStore(versionStore, Substitute.For<ILogger<DryRunVersionStore>>());
        action(dryRunVersionStore);

#pragma warning disable NS5000 // Received check.
        var received = delegates ? versionStore.Received() : versionStore.DidNotReceive();
#pragma warning restore NS5000 // Received check.
        action(received);
    }
}