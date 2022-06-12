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

namespace SourcePawnManager.Core.Results;

public class ErrorResult : IResult
{
    private readonly object[] _args;
    private readonly string   _message;

    public ErrorResult(string message, int exitCode, params object[] args)
    {
        if (exitCode <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(exitCode));
        }

        Throw.IfNullOrWhitespace(message, nameof(message));
        _message = message;
        ExitCode = exitCode;
        _args    = args ?? throw new ArgumentNullException(nameof(args));
    }

    public int        ExitCode { get; }
    public LogLevel   LogLevel => LogLevel.Error;
    public LogMessage Log      => new(_message, _args);
}