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
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NuGet.Versioning;
using NUnit.Framework;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag.DownloadGitHubTagFile;

namespace SourcePawnManager.Core.Tests.DependencyStrategy.GitHubTag.DownloadGitHubTagFile;

public class DryRunDownloadGitHubTagFileQueryHandlerTests
{
    [Test]
    public void ConstructorThrowsArgumentNullException()
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new DryRunDownloadGitHubTagFileQueryHandler(null!));
    }
    
    [Test]
    public async Task Handle()
    {
        var handler = new DryRunDownloadGitHubTagFileQueryHandler(Substitute.For<ILogger<DryRunDownloadGitHubTagFileQueryHandler>>());
        await handler.Handle(new(new("owner", "repository", VersionRange.All, "assetName"), DependencyVersion.Parse("1.0")), CancellationToken.None);
    }
    
}