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
using Microsoft.Extensions.Logging;

namespace SourcePawnManager.Core.Results;

public class DependencyResult : IResult
{
    private readonly IList<IDependencyLock> _dependencies;
    private readonly IList<IResult>         _errors;
    private readonly string                 _verb;

    public DependencyResult(IList<IDependencyLock> dependencies, IList<IResult> errors, string verb)
    {
        Throw.IfNullOrWhitespace(verb, nameof(verb));

        _dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
        _errors       = errors       ?? throw new ArgumentNullException(nameof(errors));
        _verb         = verb;
    }

    public IReadOnlyCollection<IDependencyLock> Dependencies => _dependencies.ToImmutableList();

    public int            ExitCode => 0;
    public LogLevel       LogLevel => LogLevel.Information;
    public IList<IResult> Errors  => _errors.ToImmutableList();

    public LogMessage Log =>
        _dependencies.Count switch
        {
            0 => new("no dependencies {Verb}. {@Errors}", new object?[] { _verb, _errors, }),
            1 => new("dependency {DependencyId} {Verb} version {Version} to {DownloadPath}. {@Errors}",
                     new object?[]
                     {
                         _dependencies[0].Id,
                         _verb,
                         _dependencies[0].Version.ToString(),
                         _dependencies[0].DownloadPath,
                         _errors,
                     }),
            _ => new("{DependencyCount} dependencies {Verb}: {@Dependencies} {@Errors}",
                     new object?[]
                     {
                         _dependencies.Count,
                         _verb,
                         _dependencies,
                         _errors,
                     }),
        };
}