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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.Http;

namespace SourcePawnManager.Core.Tests.Apis.Http;

public class HttpApiTests
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
    public void ConstructorThrows()
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new HttpApi(null!));
    }

    [Test]
    [Category("HttpAccess")]
    [TestCase("https://raw.githubusercontent.com/eisbaer66/spm/main/COPYING", 34523)]
    public async Task Downloads(string url, int expectedLength)
    {
        var api    = _serviceProvider.GetRequiredService<IHttpApi>();
        var stream = await api.GetStream(url, CancellationToken.None);

        Assert.IsNotNull(stream);
        var memoryStream = new MemoryStream();
        await stream!.CopyToAsync(memoryStream);
        Assert.AreEqual(expectedLength, memoryStream.Length);
    }
}