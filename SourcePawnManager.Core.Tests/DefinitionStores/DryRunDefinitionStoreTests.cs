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
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using SourcePawnManager.Core.DefinitionStores;

namespace SourcePawnManager.Core.Tests.DefinitionStores;

public class DryRunDefinitionStoreTests
{
    private const           string                   ExpectedPath = "ExpectedPath";
    private static readonly IncludeManagerDefinition Definition   = new();

    private static readonly object[] ConstructorCases =
    {
        new object[]
        {
            null!,
            Substitute.For<ILogger<DryRunDefinitionStore>>(),
        },
        new object[]
        {
            Substitute.For<IDefinitionStore>(),
            null!,
        },
    };

    private static readonly object[] DelegatesToFileSystemCases =
    {
        new object[] { (IDefinitionStore s) => { s.Read(ExpectedPath, CancellationToken.None); }, true },
        new object[] { (IDefinitionStore s) => { s.Write(ExpectedPath, Definition); }, false },
    };
    
    [Test]
    [TestCaseSource(nameof(ConstructorCases))]
    public void ConstructorThrowsArgumentNullException(IDefinitionStore credentials, ILogger<DryRunDefinitionStore> logger)
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new DryRunDefinitionStore(credentials, logger));
    }

    [Test]
    [TestCaseSource(nameof(DelegatesToFileSystemCases))]
    public void DelegatesToFileSystem(Action<IDefinitionStore> action, bool delegates)
    {
        var store = Substitute.For<IDefinitionStore>();

        var dryRunDefinitionStore = new DryRunDefinitionStore(store, Substitute.For<ILogger<DryRunDefinitionStore>>());
        action(dryRunDefinitionStore);

#pragma warning disable NS5000 // Received check.
        var received = delegates ? store.Received() : store.DidNotReceive();
#pragma warning restore NS5000 // Received check.
        action(received);
    }
}