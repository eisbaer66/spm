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
using SourcePawnManager.Core.Results;

namespace SourcePawnManager.Core.DependencyStrategy.GitHubTag;

public class DependencyVersion : IDependencyVersion
{
    public DependencyVersion(NuGetVersion version, string tag)
    {
        Version = version;
        Tag     = tag;
    }

    public NuGetVersion Version { get; init; }
    public string       Tag     { get; init; }

    public override string ToString() => Version.ToString();

    protected bool Equals(DependencyVersion other) => Version.Equals(other.Version) && Tag == other.Tag;

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((DependencyVersion)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Version, Tag);

    public void Deconstruct(out NuGetVersion version, out string tag)
    {
        version = Version;
        tag     = Tag;
    }

    public static DependencyVersion Parse(string version) => new(NuGetVersion.Parse(version), string.Empty);
}