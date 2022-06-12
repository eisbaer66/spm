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
using System.Net.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.Git;

namespace SourcePawnManager.Core.Tests.Apis.Git;

public class DryRunGitCredentialsTests
{
    private const           string              Url                 = "url";
    private static readonly HttpResponseMessage HttpResponseMessage = new();

    private static readonly object[] ConstructorCases =
    {
        new object[]
        {
            null!,
            Substitute.For<ILogger<DryRunGitCredentials>>(),
        },
        new object[]
        {
            Substitute.For<IGitCredentials>(),
            null!,
        },
    };

    private static readonly object[] DelegatesToFileSystemCases =
    {
        new object[] { (IGitCredentials c) => { c.Fill(Url); }, true },
        new object[] { (IGitCredentials c) => { c.Update(HttpResponseMessage); }, false },
    };
    
    [Test]
    [TestCaseSource(nameof(ConstructorCases))]
    public void ConstructorThrowsArgumentNullException(IGitCredentials credentials, ILogger<DryRunGitCredentials> logger)
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new DryRunGitCredentials(credentials, logger));
    }

    [Test]
    [TestCaseSource(nameof(DelegatesToFileSystemCases))]
    public void DelegatesToFileSystem(Action<IGitCredentials> action, bool delegates)
    {
        var credentials = Substitute.For<IGitCredentials>();

        var dryRunGitCredentials = new DryRunGitCredentials(credentials, Substitute.For<ILogger<DryRunGitCredentials>>());
        action(dryRunGitCredentials);

#pragma warning disable NS5000 // Received check.
        var received = delegates ? credentials.Received() : credentials.DidNotReceive();
#pragma warning restore NS5000 // Received check.
        action(received);
    }
}