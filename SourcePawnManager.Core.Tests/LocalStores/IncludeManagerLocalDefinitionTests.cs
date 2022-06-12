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
using SourcePawnManager.Core.LocalStores;

namespace SourcePawnManager.Core.Tests.LocalStores;

public class IncludeManagerLocalDefinitionTests
{
    [Test]
    public void EqualsNullReturnsFalse()
    {
        var definition = new IncludeManagerLocalDefinition();
        Assert.False(definition.Equals(null));
    }

    [Test]
    public void EqualsItselfReturnsTrue()
    {
        var definition = new IncludeManagerLocalDefinition();
        Assert.True(definition.Equals(definition));
    }

    [Test]
    public void EqualsDifferentTypeReturnsFalse()
    {
        var definition = new IncludeManagerLocalDefinition();
        Assert.False(definition.Equals(123));
    }

    [Test]
    public void EqualsWithSameDependenciesReturnsTrue()
    {
        var previousDownloadPaths = new List<string>();
        var definition            = new IncludeManagerLocalDefinition() { PreviousDownloadPaths = previousDownloadPaths };
        var definition2           = new IncludeManagerLocalDefinition() { PreviousDownloadPaths = previousDownloadPaths };
        Assert.True(definition.Equals(definition2));
    }

    [Test]
    public void EqualsWithOwnDependenciesNullReturnsFalse()
    {
        var previousDownloadPaths = new List<string>();
        var definition            = new IncludeManagerLocalDefinition() { PreviousDownloadPaths = null! };
        var definition2           = new IncludeManagerLocalDefinition() { PreviousDownloadPaths = previousDownloadPaths };
        Assert.False(definition.Equals(definition2));
    }

    [Test]
    public void EqualsWithOtherDependenciesNullReturnsFalse()
    {
        var previousDownloadPaths = new List<string>();
        var definition            = new IncludeManagerLocalDefinition() { PreviousDownloadPaths = previousDownloadPaths };
        var definition2           = new IncludeManagerLocalDefinition() { PreviousDownloadPaths = null! };
        Assert.False(definition.Equals(definition2));
    }

    [Test]
    public void EqualsWithSimilarDependenciesNullReturnsTrue()
    {
        var definition = new IncludeManagerLocalDefinition()
                         { PreviousDownloadPaths = new List<string> { "previousDownloadPath" } };
        var definition2 = new IncludeManagerLocalDefinition()
                          { PreviousDownloadPaths = new List<string> { "previousDownloadPath" } };
        Assert.True(definition.Equals(definition2));
    }
}