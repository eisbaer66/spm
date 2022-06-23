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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.FileSystems;
using SourcePawnManager.Core.Apis.Http;
using SourcePawnManager.Core.DependencyStrategy.StaticUrl;

namespace SourcePawnManager.Core.Tests.DependencyStrategy.StaticUrl;

public class DownloadStaticUrlQueryHandlerTests
{
    private ServiceProvider _serviceProvider = null!;
    private IHttpApi        _httpApi         = null!;
    private IFileSystem     _fileSystem      = null!;

    [SetUp]
    public void Setup()
    {
        _httpApi    = Substitute.For<IHttpApi>();
        _fileSystem = Substitute.For<IFileSystem>();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSerilog();
        serviceCollection.AddSingleton(_httpApi);
        serviceCollection.AddSingleton(_fileSystem);
        serviceCollection.AddSourcePawnManager();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Test]
    public async Task DownloadStopsIfApiReturnsNull()
    {
        const string url          = "url";
        const string downloadPath = "downloadPath";

        _httpApi.GetStream(url, Arg.Any<CancellationToken>()).Returns((Stream)null!);

        var dependency = new StaticUrlDependency(url, downloadPath);

        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new DownloadStaticUrlQuery(dependency));

        await _httpApi.Received().GetStream(url, Arg.Any<CancellationToken>());
        await _fileSystem.DidNotReceive().Write(Arg.Any<Stream>(), downloadPath);
    }

    [Test]
    public async Task WritesDownloadedFile()
    {
        const string url          = "url";
        const string downloadPath = "downloadPath";
        var          stream       = Stream.Null;

        _httpApi.GetStream(url, Arg.Any<CancellationToken>()).Returns(stream);

        var dependency = new StaticUrlDependency(url, downloadPath);

        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new DownloadStaticUrlQuery(dependency));

        await _httpApi.Received().GetStream(url, Arg.Any<CancellationToken>());
        await _fileSystem.Received().Write(stream, downloadPath);
    }
}