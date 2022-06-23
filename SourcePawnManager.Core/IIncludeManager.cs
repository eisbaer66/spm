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

using SourcePawnManager.Core.DependencyStrategy;
using SourcePawnManager.Core.Results;

namespace SourcePawnManager.Core;

public interface IIncludeManager
{
    Task<IResult> Install(string basePath, IDependency       dependency, CancellationToken cancellationToken = default);
    Task<IResult> Restore(string basePath, CancellationToken cancellationToken = default);
    Task<IResult> Update(string  basePath, CancellationToken cancellationToken = default);

    Task<IResult> Remove(string                       basePath,
                         Func<IDependency, int, bool> predicate,
                         CancellationToken            cancellationToken = default);
}