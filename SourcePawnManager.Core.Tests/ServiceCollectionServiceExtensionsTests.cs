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
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.FileSystems;
using SourcePawnManager.Core.Apis.GitHub;
using SourcePawnManager.Core.DefinitionStores;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag.DownloadGitHubTagFile;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag.DownloadGitHubTagZip;
using SourcePawnManager.Core.LocalStores;
using SourcePawnManager.Core.LockStores;
using SourcePawnManager.Core.VersionStores;

namespace SourcePawnManager.Core.Tests;

public class ServiceCollectionServiceExtensionsTests
{
    private static readonly (Type ServiceType, Type NormalRunImplementationType, Type DryRunImplementationType)[]
        DryRunRegistrationTypes =
        {
            new(typeof(IRequestHandler<DownloadGitHubTagZipQuery, Unit>),
                typeof(DownloadGitHubTagZipQueryHandler),
                typeof(DryRunDownloadGitHubTagZipQueryHandler)),
            new(typeof(IRequestHandler<DownloadGitHubTagFileQuery, Unit>),
                typeof(DownloadGitHubTagFileQueryHandler),
                typeof(DryRunDownloadGitHubTagFileQueryHandler)),
            new(typeof(IGitHubApi), typeof(GitHubApi), typeof(DryRunGitHubApi)),
            new(typeof(IFileSystem), typeof(FileSystem), typeof(DryRunFileSystem)),
            new(typeof(IVersionStore), typeof(SideFileVersionStore), typeof(DryRunVersionStore)),
            new(typeof(ILocalStore), typeof(FileLocalStore), typeof(DryRunLocalStore)),
            new(typeof(ILockStore), typeof(FileLockStore), typeof(DryRunLockStore)),
            new(typeof(IDefinitionStore), typeof(DefinitionJsonStore), typeof(DryRunDefinitionStore)),
        };

    private static readonly object[] DryRunRegistrationsCases =
    {
        new object[]
        {
            false,
            DryRunRegistrationTypes
                .ToDictionary(x => x.ServiceType,
                              x => x.NormalRunImplementationType),
        },
        new object[]
        {
            true,
            DryRunRegistrationTypes
                .ToDictionary(x => x.ServiceType,
                              x => x.DryRunImplementationType),
        },
    };

    [Test]
    [TestCaseSource(nameof(DryRunRegistrationsCases))]
    public void DryRunRegistrations(bool dryRun, IDictionary<Type, Type> expectedImplementations)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSourcePawnManager(dryRun);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        foreach (var (serviceType, expectedImplementationType) in expectedImplementations)
        {
            var service = serviceProvider.GetRequiredService(serviceType);
            Assert.IsInstanceOf(expectedImplementationType, service);
        }
    }
}