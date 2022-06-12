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
using SourcePawnManager.Core.LocalStores;

namespace SourcePawnManager.Core.Tests.LocalStores;

public class DryRunLocalStoreTests
{
    private const           string                        ExpectedPath    = "path";
    private static readonly IncludeManagerLocalDefinition Definition = new();

    private static readonly object[] ConstructorCases =
    {
        new object[]
        {
            null!,
            Substitute.For<ILogger<DryRunLocalStore>>(),
        },
        new object[]
        {
            Substitute.For<ILocalStore>(),
            null!,
        },
    };

    private static readonly object[] DelegatesToFileSystemCases =
    {
        new object[] { (ILocalStore s) => { s.Get(ExpectedPath); }, true },
        new object[] { (ILocalStore s) => { s.Set(Definition, ExpectedPath); }, false },
    };
    
    [Test]
    [TestCaseSource(nameof(ConstructorCases))]
    public void ConstructorThrowsArgumentNullException(ILocalStore api, ILogger<DryRunLocalStore> logger)
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new DryRunLocalStore(api, logger));
    }

    [Test]
    [TestCaseSource(nameof(DelegatesToFileSystemCases))]
    public void DelegatesToFileSystem(Action<ILocalStore> action, bool delegates)
    {
        var real = Substitute.For<ILocalStore>();

        var dryRunVersionStore = new DryRunLocalStore(real, Substitute.For<ILogger<DryRunLocalStore>>());
        action(dryRunVersionStore);

#pragma warning disable NS5000 // Received check.
        var received = delegates ? real.Received() : real.DidNotReceive();
#pragma warning restore NS5000 // Received check.
        action(received);
    }
}