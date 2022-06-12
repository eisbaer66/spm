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
using SourcePawnManager.Core.Apis.GitHub;

namespace SourcePawnManager.Core.Tests.Apis;

public class DryRunGitHubApiTests
{
    private static readonly string Owner      = nameof(Owner);
    private static readonly string Repository = nameof(Repository);
    private static readonly string AssetName  = nameof(AssetName);
    private static readonly string Tag        = nameof(Tag);

    private static readonly object[] ConstructorCases =
    {
        new object[]
        {
            null!,
            Substitute.For<ILogger<DryRunGitHubApi>>(),
        },
        new object[]
        {
            Substitute.For<IGitHubApi>(),
            null!,
        },
    };

    private static readonly object[] DelegatesToFileSystemCases =
    {
        new object[] { (IGitHubApi s) => { s.GetVersions(Owner, Repository, CancellationToken.None); }, true },
        new object[] { (IGitHubApi s) => { s.Download(Owner, Repository, AssetName, Tag, CancellationToken.None); }, false },
    };
    
    [Test]
    [TestCaseSource(nameof(ConstructorCases))]
    public void ConstructorThrowsArgumentNullException(IGitHubApi api, ILogger<DryRunGitHubApi> logger)
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new DryRunGitHubApi(api, logger));
    }

    [Test]
    [TestCaseSource(nameof(DelegatesToFileSystemCases))]
    public void DelegatesToFileSystem(Action<IGitHubApi> action, bool delegates)
    {
        var api = Substitute.For<IGitHubApi>();

        var dryRunVersionStore = new DryRunGitHubApi(api, Substitute.For<ILogger<DryRunGitHubApi>>());
        action(dryRunVersionStore);

#pragma warning disable NS5000 // Received check.
        var received = delegates ? api.Received() : api.DidNotReceive();
#pragma warning restore NS5000 // Received check.
        action(received);
    }
}