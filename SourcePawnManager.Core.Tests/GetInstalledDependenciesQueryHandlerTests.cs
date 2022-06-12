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
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using SourcePawnManager.Core.DefinitionStores;
using SourcePawnManager.Core.DependencyStrategy;
using SourcePawnManager.Core.Mediator.GetInstalledDependenciesQuery;

namespace SourcePawnManager.Core.Tests;

public class GetInstalledDependenciesQueryHandlerTests
{
    private IDefinitionStore _definitionStore = null!;
    private IServiceProvider _serviceProvider = null!;

    [SetUp]
    public void Setup()
    {
        _definitionStore = Substitute.For<IDefinitionStore>();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSerilog();
        serviceCollection.AddSingleton(_definitionStore);
        serviceCollection.AddSourcePawnManager();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Test]
    public async Task StopsIfStoreReturnsNull()
    {
        var path = "path";

        _definitionStore.Read(path, CancellationToken.None)
                        .Returns(Task.FromResult((IncludeManagerDefinition?)null));

        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var requests = await mediator.Send(new GetInstalledDependenciesQuery(path), CancellationToken.None);
        Assert.NotNull(requests);
        Assert.AreEqual(0, requests.Count);

        await _definitionStore.Received()
                              .Read(path, CancellationToken.None);
    }

    [Test]
    public async Task ReadsDependenciesFromStore()
    {
        var                              path         = "path\\spm.json";
        IReadOnlyCollection<IDependency> dependencies = new List<IDependency>();
        var definition = new IncludeManagerDefinition
                         {
                             Dependencies = dependencies,
                         };

        _definitionStore.Read(path, CancellationToken.None)
                        .Returns(definition);

        var mediator         = _serviceProvider.GetRequiredService<IMediator>();
        var readDependencies = await mediator.Send(new GetInstalledDependenciesQuery(path));

        Assert.AreSame(dependencies, readDependencies);

        await _definitionStore.Received()
                              .Read(path, CancellationToken.None);
    }
}