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

public class InfoResult : IResult
{
    private readonly object[] _args;
    private readonly string   _message;

    public InfoResult(string message, params object[] args)
    {
        Throw.IfNullOrEmpty(args, nameof(args));
        Throw.IfNullOrWhitespace(message, nameof(message));
        _message = message;
        _args    = args;
    }

    public int        ExitCode => 0;
    public LogLevel   LogLevel => LogLevel.Information;
    public LogMessage Log      => new(_message, _args);
}