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

namespace SourcePawnManager.Core.LockStores;

public class LockDefinition
{
    public LockDefinition()
    {
        IncludeLocks = Array.Empty<IncludeLockDefinition>();
    }

    public IReadOnlyCollection<IncludeLockDefinition> IncludeLocks { get; init; }

    protected bool Equals(LockDefinition other)
    {
        if (Equals(IncludeLocks, other.IncludeLocks))
        {
            return true;
        }

        if (IncludeLocks == null)
        {
            return false;
        }

        if (other.IncludeLocks == null)
        {
            return false;
        }

        return IncludeLocks.SequenceEqual(other.IncludeLocks);
    }

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

        return Equals((LockDefinition)obj);
    }

    public override int GetHashCode() => IncludeLocks != null ? IncludeLocks.GetHashCode() : 0;
}