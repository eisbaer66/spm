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
using NUnit.Framework;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;
using SourcePawnManager.Core.LockStores;

namespace SourcePawnManager.Core.Tests.LockStores;

public class LockDefinitionTests
{
    [Test]
    public void EqualsNullReturnsFalse()
    {
        var definition = new LockDefinition();
        Assert.False(definition.Equals(null));
    }

    [Test]
    public void EqualsItselfReturnsTrue()
    {
        var definition = new LockDefinition();
        Assert.True(definition.Equals(definition));
    }

    [Test]
    public void EqualsDifferentTypeReturnsFalse()
    {
        var definition = new LockDefinition();
        Assert.False(definition.Equals(123));
    }

    [Test]
    public void EqualsWithSameDependenciesReturnsTrue()
    {
        var dependencies = new List<IncludeLockDefinition>();
        var definition   = new LockDefinition() { IncludeLocks = dependencies };
        var definition2  = new LockDefinition() { IncludeLocks = dependencies };
        Assert.True(definition.Equals(definition2));
    }

    [Test]
    public void EqualsWithOwnDependenciesNullReturnsFalse()
    {
        var dependencies = new List<IncludeLockDefinition>();
        var definition   = new LockDefinition() { IncludeLocks = null! };
        var definition2  = new LockDefinition() { IncludeLocks = dependencies };
        Assert.False(definition.Equals(definition2));
    }

    [Test]
    public void EqualsWithOtherDependenciesNullReturnsFalse()
    {
        var dependencies = new List<IncludeLockDefinition>();
        var definition   = new LockDefinition() { IncludeLocks = dependencies };
        var definition2  = new LockDefinition() { IncludeLocks = null! };
        Assert.False(definition.Equals(definition2));
    }

    [Test]
    public void EqualsWithSimilarDependenciesNullReturnsTrue()
    {
        var definition = new LockDefinition()
                         { IncludeLocks = new List<IncludeLockDefinition> { new("id", DependencyVersion.Parse("1.0")) } };
        var definition2 = new LockDefinition()
                          { IncludeLocks = new List<IncludeLockDefinition> { new ("id", DependencyVersion.Parse("1.0")) } };
        Assert.True(definition.Equals(definition2));
    }
}