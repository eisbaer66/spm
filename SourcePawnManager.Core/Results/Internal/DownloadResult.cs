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

using System.Collections.Immutable;

namespace SourcePawnManager.Core.Results.Internal;

internal class DownloadResult : InfoResult
{
    private readonly IList<IDependencyLock> _dependencyLocks;
    private readonly List<IResult>          _errors;

    public DownloadResult(IList<IDependencyLock> dependencyLocks, List<IResult> errors)
        : base("downloaded {DependencyCount} dependencies, with {ErrorCount} errors", dependencyLocks.Count, errors.Count)
    {
        _dependencyLocks = dependencyLocks ?? throw new ArgumentNullException(nameof(dependencyLocks));
        _errors          = errors          ?? throw new ArgumentNullException(nameof(errors));
    }

    public IList<IDependencyLock> Dependencies => _dependencyLocks.ToImmutableList();
    public IList<IResult>         Errors      => _errors.ToImmutableList();
}