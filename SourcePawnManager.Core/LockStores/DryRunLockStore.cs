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

using Microsoft.Extensions.Logging;

namespace SourcePawnManager.Core.LockStores;

public class DryRunLockStore : ILockStore
{
    private readonly ILogger<DryRunLockStore> _logger;
    private readonly ILockStore               _store;

    public DryRunLockStore(ILockStore store, ILogger<DryRunLockStore> logger)
    {
        _store  = store  ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<LockDefinition> Get(string path, CancellationToken cancellationToken = default) =>
        _store.Get(path, cancellationToken);

    public Task Set(LockDefinition lockDefinition, string path, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("would have set lock-definition {LockDefinition} in {Path}", lockDefinition, path);
        return Task.CompletedTask;
    }
}