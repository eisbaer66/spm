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

using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.FileSystems;
using SourcePawnManager.Core.DependencyStrategy.StaticUrl;

namespace SourcePawnManager.Core.Tests.DependencyStrategy.StaticUrl;

public class DryRunDownloadStaticUrlQueryHandlerTests
{
    private ServiceProvider _serviceProvider = null!;
    private IFileSystem     _fileSystem      = null!;

    [SetUp]
    public void Setup()
    {
        _fileSystem = Substitute.For<IFileSystem>();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSerilog();
        serviceCollection.AddSingleton(_fileSystem);
        serviceCollection.AddSourcePawnManager(dryRun: true);

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Test]
    public async Task DoesNotDeleteOrWriteFiles()
    {
        var dependency = new StaticUrlDependency("url", "downloadPath");
        var mediator   = _serviceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new DownloadStaticUrlQuery(dependency));

        await _fileSystem.DidNotReceiveWithAnyArgs().Write(null!, null!);
        _fileSystem.DidNotReceiveWithAnyArgs().Delete(null!);
    }
}