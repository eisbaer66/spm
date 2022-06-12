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

using System.Collections.Generic;
using NuGet.Versioning;
using NUnit.Framework;
using SourcePawnManager.Core.DependencyStrategy;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;

namespace SourcePawnManager.Core.Tests;

public class IncludeManagerDefinitionTests
{
    [Test]
    public void EqualsNullReturnsFalse()
    {
        var definition = new IncludeManagerDefinition();
        Assert.False(definition.Equals(null));
    }

    [Test]
    public void EqualsItselfReturnsTrue()
    {
        var definition = new IncludeManagerDefinition();
        Assert.True(definition.Equals(definition));
    }

    [Test]
    public void EqualsDifferentTypeReturnsFalse()
    {
        var definition = new IncludeManagerDefinition();
        Assert.False(definition.Equals(123));
    }

    [Test]
    public void EqualsWithSameDependenciesReturnsTrue()
    {
        var dependencies = new List<IDependency>();
        var definition   = new IncludeManagerDefinition() { Dependencies = dependencies };
        var definition2  = new IncludeManagerDefinition() { Dependencies = dependencies };
        Assert.True(definition.Equals(definition2));
    }

    [Test]
    public void EqualsWithOwnDependenciesNullReturnsFalse()
    {
        var dependencies = new List<IDependency>();
        var definition   = new IncludeManagerDefinition() { Dependencies = null! };
        var definition2  = new IncludeManagerDefinition() { Dependencies = dependencies };
        Assert.False(definition.Equals(definition2));
    }

    [Test]
    public void EqualsWithOtherDependenciesNullReturnsFalse()
    {
        var dependencies = new List<IDependency>();
        var definition   = new IncludeManagerDefinition() { Dependencies = dependencies };
        var definition2  = new IncludeManagerDefinition() { Dependencies = null! };
        Assert.False(definition.Equals(definition2));
    }

    [Test]
    public void EqualsWithSimilarDependenciesNullReturnsTrue()
    {
        const string owner        = nameof(owner);
        const string repository   = nameof(repository);
        var          versionRange = VersionRange.All;
        const string assetName    = nameof(assetName);

        var definition = new IncludeManagerDefinition()
                         { Dependencies = new List<IDependency> { new GitHubTagFileDependency(owner, repository, versionRange, assetName) } };
        var definition2 = new IncludeManagerDefinition()
                          { Dependencies = new List<IDependency> { new GitHubTagFileDependency(owner, repository, versionRange, assetName) } };
        Assert.True(definition.Equals(definition2));
    }
}