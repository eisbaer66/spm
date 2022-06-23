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
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SourcePawnManager.Core.DependencyStrategy.Shared;

namespace SourcePawnManager.Core.Tests.DependencyStrategy.Shared;

public class GetNoVersionsQueryHandlerTests
{
    private IServiceProvider _serviceProvider = null!;

    [SetUp]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSerilog();
        serviceCollection.AddSourcePawnManager();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Test]
    public async Task ReturnsOnlyVersion1()
    {
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var versions = await mediator.Send(new GetStaticVersionsQuery());

        Assert.NotNull(versions);
        Assert.AreEqual(1, versions.Count);
        Assert.AreEqual("1.0", versions[0].Version.ToString());
        Assert.AreEqual(string.Empty, versions[0].Tag);
    }
}