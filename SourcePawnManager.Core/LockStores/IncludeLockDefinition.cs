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

using SourcePawnManager.Core.DependencyStrategy.GitHubTag;

namespace SourcePawnManager.Core.LockStores;

public class IncludeLockDefinition
{
    public IncludeLockDefinition(string id, DependencyVersion version)
    {
        Id      = id      ?? throw new ArgumentNullException(nameof(id));
        Version = version ?? throw new ArgumentNullException(nameof(version));
    }

    public string            Id      { get; init; }
    public DependencyVersion Version { get; init; }

    protected bool Equals(IncludeLockDefinition other) => Id == other.Id && Equals(Version, other.Version);

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

        return Equals((IncludeLockDefinition)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Id, Version);
}