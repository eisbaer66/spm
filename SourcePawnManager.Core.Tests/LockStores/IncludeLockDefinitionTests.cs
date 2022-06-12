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
using NUnit.Framework;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;
using SourcePawnManager.Core.LockStores;

namespace SourcePawnManager.Core.Tests.LockStores;

public class IncludeLockDefinitionTests
{
    private static readonly object[] ConstructorCases =
    {
        new object[]
        {
            null!,
            DependencyVersion.Parse("1.0"),
        },
        new object[]
        {
            "id",
            null!,
        },
    };
    
    [Test]
    [TestCaseSource(nameof(ConstructorCases))]
    public void ConstructorThrowsArgumentNullException(string id, DependencyVersion version)
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new IncludeLockDefinition(id, version));
    }

    [Test]
    public void EqualsNullReturnsFalse()
    {
        var definition = new IncludeLockDefinition("id", DependencyVersion.Parse("1.0"));
        Assert.False(definition.Equals(null));
    }

    [Test]
    public void EqualsItselfReturnsTrue()
    {
        var definition = new IncludeLockDefinition("id", DependencyVersion.Parse("1.0"));
        Assert.True(definition.Equals(definition));
    }

    [Test]
    public void EqualsDifferentTypeReturnsFalse()
    {
        var definition = new IncludeLockDefinition("id", DependencyVersion.Parse("1.0"));
        Assert.False(definition.Equals(123));
    }

    [Test]
    public void EqualsWithSimilarDependenciesNullReturnsTrue()
    {
        var definition = new IncludeLockDefinition("id", DependencyVersion.Parse("1.0"));
        var definition2 = new IncludeLockDefinition("id", DependencyVersion.Parse("1.0"));
        Assert.True(definition.Equals(definition2));
    }
}