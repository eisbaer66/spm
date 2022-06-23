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

using NuGet.Versioning;
using NUnit.Framework;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;

namespace SourcePawnManager.Core.Tests.DependencyStrategy.GitHubTag;

public class DependencyVersionTests
{
    [Test]
    public void EqualsNullReturnsFalse()
    {
        var version = new DependencyVersion(NuGetVersion.Parse("1.0"), "1.0");
        Assert.False(version.Equals(null));
    }

    [Test]
    public void EqualsItselfReturnsTrue()
    {
        var version = new DependencyVersion(NuGetVersion.Parse("1.0"), "1.0");
        Assert.True(version.Equals(version));
    }

    [Test]
    public void EqualsDifferentTypeReturnsFalse()
    {
        var version = new DependencyVersion(NuGetVersion.Parse("1.0"), "1.0");
        Assert.False(version.Equals(123));
    }

    [Test]
    public void EqualsWithSimilarDependenciesNullReturnsTrue()
    {
        var version = new DependencyVersion(NuGetVersion.Parse("1.0"), "1.0");
        var version2 = new DependencyVersion(NuGetVersion.Parse("1.0"), "1.0");
        Assert.True(version.Equals(version2));
    }

    [Test]
    public void Deconstruct()
    {
        var version = new DependencyVersion(NuGetVersion.Parse("1.0"), "1.0");
        (NuGetVersion v, string tag) = version;

        Assert.AreEqual(version.Version, v);
        Assert.AreEqual(version.Tag, tag);
    }
}