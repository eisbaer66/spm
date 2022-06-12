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

using MediatR;
using Microsoft.Extensions.Logging;
using SourcePawnManager.Core.DefinitionStores;
using SourcePawnManager.Core.DependencyStrategy;

namespace SourcePawnManager.Core.Mediator.GetInstalledDependenciesQuery;

public class
    GetInstalledDependenciesQueryHandler : IRequestHandler<GetInstalledDependenciesQuery,
        IReadOnlyCollection<IDependency>>
{
    private readonly IDefinitionStore                              _definitionStore;
    private readonly ILogger<GetInstalledDependenciesQueryHandler> _logger;

    public GetInstalledDependenciesQueryHandler(IDefinitionStore                              definitionStore,
                                                ILogger<GetInstalledDependenciesQueryHandler> logger)
    {
        _definitionStore = definitionStore ?? throw new ArgumentNullException(nameof(definitionStore));
        _logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyCollection<IDependency>> Handle(GetInstalledDependenciesQuery request,
                                                               CancellationToken             cancellationToken)
    {
        var definition = await _definitionStore.Read(request.Path, cancellationToken);

        if (definition == null)
        {
            _logger.LogDebug("SourcePawnManager configuration {SourcePawnManagerPath} cant be read. using empty definition",
                             request.Path);
            return new List<IDependency>();
        }

        return definition.Dependencies;
    }
}