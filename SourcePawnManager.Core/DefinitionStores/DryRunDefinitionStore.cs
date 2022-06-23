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

namespace SourcePawnManager.Core.DefinitionStores;

public class DryRunDefinitionStore : IDefinitionStore
{
    private readonly ILogger<DryRunDefinitionStore> _logger;
    private readonly IDefinitionStore               _store;

    public DryRunDefinitionStore(IDefinitionStore store, ILogger<DryRunDefinitionStore> logger)
    {
        _store  = store  ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<IncludeManagerDefinition?> Read(string path, CancellationToken cancellationToken = default) =>
        _store.Read(path, cancellationToken);

    public Task Write(string path, IncludeManagerDefinition definition, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("would have set definition {Definition} in {Path}", definition, path);
        return Task.CompletedTask;
    }
}