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

namespace SourcePawnManager.Core.Results;

public class LockedVersionNotAvailableResult : ErrorResult
{
    public LockedVersionNotAvailableResult(DependencyVersion        lockedVersion,
                                           IList<DependencyVersion> versions,
                                           string                   dependencyId)
        : base("locked version {LockedVersions} is not available in {@Versions} for {DependencyId}",
               1,
               lockedVersion,
               versions,
               dependencyId)
    {
        LockedVersion = lockedVersion;
        Versions      = versions;
        DependencyId  = dependencyId;
    }

    public DependencyVersion        LockedVersion { get; }
    public IList<DependencyVersion> Versions      { get; }
    public string                   DependencyId  { get; }
}