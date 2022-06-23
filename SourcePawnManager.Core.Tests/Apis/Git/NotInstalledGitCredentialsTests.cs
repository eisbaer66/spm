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
using Microsoft.Extensions.Logging;using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.Git;

namespace SourcePawnManager.Core.Tests.Apis.Git;

public class NotInstalledGitCredentialsTests
{
    private const           string              Url                 = "url";
    private static readonly HttpResponseMessage HttpResponseMessage = new();

    private static readonly object[] ConstructorCases =
    {
        new object[]
        {
            null!,
            Substitute.For<ILogger<NotInstalledGitCredentials>>(),
        },
        new object[]
        {
            Substitute.For<IGitCredentials>(),
            null!,
        },
    };

    private static readonly object[] DelegatesCases =
    {
        new object[] { (IGitCredentials c) => { c.Fill(Url); } },
        new object[] { (IGitCredentials c) => { c.Update(HttpResponseMessage); } },
    };

    [Test]
    [TestCaseSource(nameof(ConstructorCases))]
    public void ConstructorThrowsArgumentNullException(IGitCredentials credentials, ILogger<NotInstalledGitCredentials> logger)
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new NotInstalledGitCredentials(credentials, logger));
    }
    
    [Test]
    [TestCaseSource(nameof(DelegatesCases))]
    public void Delegates(Action<IGitCredentials> action)
    {
        IGitCredentials                     credentials                = Substitute.For<IGitCredentials>();

        var dryRunVersionStore = new NotInstalledGitCredentials(credentials, Substitute.For<ILogger<NotInstalledGitCredentials>>());
        action(dryRunVersionStore);

#pragma warning disable NS5000 // Received check.
        action(credentials.Received());
#pragma warning restore NS5000 // Received check.
    }
    
    [Test]
    public void FillReturnNullIfCredentialsThrows()
    {
        IGitCredentials credentials = Substitute.For<IGitCredentials>();
        credentials.Fill(Url).Throws(new Exception("No credential backing store has been selected."));

        var dryRunVersionStore = new NotInstalledGitCredentials(credentials, Substitute.For<ILogger<NotInstalledGitCredentials>>());
        var fill               = dryRunVersionStore.Fill(Url);

        Assert.IsNull(fill);
        credentials.Received().Fill(Url);
    }
    
    [Test]
    public void FillReturnsIfCredentialsThrows()
    {
        IGitCredentials credentials = Substitute.For<IGitCredentials>();
        credentials.When( x => x.Update(HttpResponseMessage)).Throw(new Exception("No credential backing store has been selected."));

        var dryRunVersionStore = new NotInstalledGitCredentials(credentials, Substitute.For<ILogger<NotInstalledGitCredentials>>());
        dryRunVersionStore.Update(HttpResponseMessage);

        credentials.Received().Update(HttpResponseMessage);
    }
    
}