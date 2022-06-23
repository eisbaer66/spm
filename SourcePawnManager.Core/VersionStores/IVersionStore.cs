﻿#region copyright

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
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;

namespace SourcePawnManager.Core.VersionStores;

public interface IVersionStore
{
    Task<DependencyVersion?> GetExistingVersion(IDependency dependency, CancellationToken cancellationToken = default);
    void Clean(IEnumerable<string> previousDownloadDirectories, IEnumerable<IDependency> dependencies);
    Task Set(IDependency dependency, DependencyVersion version, CancellationToken cancellationToken = default);
    void Delete(IDependency dependency);
}