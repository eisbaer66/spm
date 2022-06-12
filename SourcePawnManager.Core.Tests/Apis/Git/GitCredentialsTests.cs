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
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Git.CredentialManager;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.Git;

namespace SourcePawnManager.Core.Tests.Apis.Git;

public class GitCredentialsTests
{
    private ICredentialStore              _credentialStore = null!;
    private IOptionsSnapshot<GitHubToken> _optionsSnapshot = null!;
    private IServiceProvider              _serviceProvider = null!;
    
    private static readonly object[] ConstructorCases =
    {
        new object[]
        {
            null!,
            Substitute.For<IGitCredentialsWriter>(),
        },
        new object[]
        {
            Substitute.For<IGitCredentialsReader>(),
            null!,
        },
    };

    [SetUp]
    public void Setup()
    {
        _credentialStore = Substitute.For<ICredentialStore>();
        _optionsSnapshot = Substitute.For<IOptionsSnapshot<GitHubToken>>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_credentialStore);
        serviceCollection.AddSingleton(_optionsSnapshot);
        serviceCollection.AddSerilog();
        serviceCollection.AddSourcePawnManager();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Test]
    [TestCaseSource(nameof(ConstructorCases))]
    public void ConstructorThrowsArgumentNullException(IGitCredentialsReader reader, IGitCredentialsWriter writer)
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new GitCredentials(reader, writer));
    }

    [Test]
    public void FillReturnsNullIfGitIsNotFound()
    {
        var gitCredentials = _serviceProvider.GetRequiredService<IGitCredentials>();

        const string url = nameof(url);
        _credentialStore.Get(url, GitCredentials.Key)
                        .Throws(new Exception("Failed to locate 'git' executable on the path."));

        var filledPassword = gitCredentials.Fill(url);

        Assert.IsNull(filledPassword);
    }

    [Test]
    public void FillReturnsNullIfGitCredentialStoreIsNotSet()
    {
        var gitCredentials = _serviceProvider.GetRequiredService<IGitCredentials>();

        const string url = nameof(url);
        _credentialStore.Get(url, GitCredentials.Key)
                        .Throws(new Exception("No credential backing store has been selected."));

        var filledPassword = gitCredentials.Fill(url);

        Assert.IsNull(filledPassword);
    }

    [Test]
    public void FillReturnsPasswordAsTokenWhenUsernameIsGitCredentialsKey()
    {
        var gitCredentials = _serviceProvider.GetRequiredService<IGitCredentials>();

        const string url      = nameof(url);
        const string password = nameof(password);
        _credentialStore.Get(url, GitCredentials.Key)
                        .Returns(new GitCredential(GitCredentials.Key, password));

        var filledPassword = gitCredentials.Fill(url);

        Assert.AreEqual(password, filledPassword);
    }

    [Test]
    public void FillDoesNotReturnPasswordAsTokenWhenUsernameIsSetToUnexpectedValue()
    {
        var gitCredentials = _serviceProvider.GetRequiredService<IGitCredentials>();

        const string url      = nameof(url);
        const string username = nameof(username);
        const string password = nameof(password);
        _credentialStore.Get(url, GitCredentials.Key)
                        .Returns(new GitCredential(username, password));

        var filledPassword = gitCredentials.Fill(url);

        Assert.IsNull(filledPassword);
    }

    [Test]
    public void UpdateSkippedIfGitHubTokenUrlIsNotSet()
    {
        var gitCredentials = _serviceProvider.GetRequiredService<IGitCredentials>();

        var gitHubToken = new GitHubToken();

        _optionsSnapshot.Get(Options.DefaultName).Returns(gitHubToken);
        var response = new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                       };
        gitCredentials.Update(response);

        _credentialStore.DidNotReceiveWithAnyArgs().AddOrUpdate(null, null, null);
        _credentialStore.DidNotReceiveWithAnyArgs().Remove(null, null);
    }

    [Test]
    public void UpdatesSkipsNullIfGitIsNotFound()
    {
        var gitCredentials = _serviceProvider.GetRequiredService<IGitCredentials>();

        const string url         = nameof(url);
        const string token       = nameof(token);
        var          gitHubToken = new GitHubToken { Url                = url, Value = token };
        var          response    = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
        _credentialStore.When(x => x.AddOrUpdate(url, GitCredentials.Key, token))
                        .Do(_ => throw new("Failed to locate 'git' executable on the path."));

        _optionsSnapshot.Get(Options.DefaultName).Returns(gitHubToken);
        gitCredentials.Fill(url);
        gitCredentials.Update(response);

        _credentialStore.Received().AddOrUpdate(url, GitCredentials.Key, token);
        _credentialStore.DidNotReceiveWithAnyArgs().Remove(null, null);
    }

    [Test]
    public void UpdatesOnSuccessfulResponse()
    {
        var gitCredentials = _serviceProvider.GetRequiredService<IGitCredentials>();

        const string url         = nameof(url);
        const string token       = nameof(token);
        var          gitHubToken = new GitHubToken { Url                = url, Value = token };
        var          response    = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

        _optionsSnapshot.Get(Options.DefaultName).Returns(gitHubToken);
        gitCredentials.Fill(url);
        gitCredentials.Update(response);

        _credentialStore.Received().AddOrUpdate(url, GitCredentials.Key, token);
        _credentialStore.DidNotReceiveWithAnyArgs().Remove(null, null);
    }

    [Test]
    public void UpdatesOnUnsuccessfulResponse()
    {
        var gitCredentials = _serviceProvider.GetRequiredService<IGitCredentials>();

        const string url         = nameof(url);
        const string token       = nameof(token);
        var          gitHubToken = new GitHubToken { Url                = url, Value = token };
        var          response    = new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };

        _optionsSnapshot.Get(Options.DefaultName).Returns(gitHubToken);
        gitCredentials.Fill(url);
        gitCredentials.Update(response);

        _credentialStore.DidNotReceiveWithAnyArgs().AddOrUpdate(null, null, null);
        _credentialStore.Received().Remove(url, GitCredentials.Key);
    }
}