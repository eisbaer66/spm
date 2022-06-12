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
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using SourcePawnManager.Core.LockStores;

namespace SourcePawnManager.Core.Tests.LockStores;

public class DryRunLockStoreTests
{
    private const           string         ExpectedPath = "path";
    private static readonly LockDefinition Definition   = new();

    private static readonly object[] ConstructorCases =
    {
        new object[]
        {
            null!,
            Substitute.For<ILogger<DryRunLockStore>>(),
        },
        new object[]
        {
            Substitute.For<ILockStore>(),
            null!,
        },
    };

    private static readonly object[] DelegatesToFileSystemCases =
    {
        new object[] { (ILockStore s) => { s.Get(ExpectedPath); }, true },
        new object[] { (ILockStore s) => { s.Set(Definition, ExpectedPath); }, false },
    };
    
    [Test]
    [TestCaseSource(nameof(ConstructorCases))]
    public void ConstructorThrowsArgumentNullException(ILockStore api, ILogger<DryRunLockStore> logger)
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new DryRunLockStore(api, logger));
    }

    [Test]
    [TestCaseSource(nameof(DelegatesToFileSystemCases))]
    public void DelegatesToFileSystem(Action<ILockStore> action, bool delegates)
    {
        var real = Substitute.For<ILockStore>();

        var dryRunVersionStore = new DryRunLockStore(real, Substitute.For<ILogger<DryRunLockStore>>());
        action(dryRunVersionStore);

#pragma warning disable NS5000 // Received check.
        var received = delegates ? real.Received() : real.DidNotReceive();
#pragma warning restore NS5000 // Received check.
        action(received);
    }
}