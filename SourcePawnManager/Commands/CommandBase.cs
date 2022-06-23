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

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;
using Serilog;
using SourcePawnManager.Core.Results;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SourcePawnManager.Commands;

public abstract class CommandBase : Command
{
    protected CommandBase(string name, string? description) : base(name, description)
    {
        AddOption(new Option<bool>(new[] { "--github-action", "-g" }, "write github-action outputs"));
    }

    public abstract class CommandHandlerBase : ICommandHandler
    {
        private readonly ILogger _logger;

        protected CommandHandlerBase(ILogger<IConsole> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // injected by System.CommandLine
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
        public bool GithubAction { get; set; } = false;

        public IConsole Console { get; set; } = null!;

        // ReSharper restore UnusedAutoPropertyAccessor.Global
        // ReSharper restore AutoPropertyCanBeMadeGetOnly.Global

        public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var result = await InvokeAsync(context.GetCancellationToken());

            _logger.Log(result.LogLevel, result.Log.Message, result.Log.Args);

            if (GithubAction)
            {
                if (!Log.Logger.BindMessageTemplate(result.Log.Message,
                                                    result.Log.Args,
                                                    out var parsedTemplate,
                                                    out var boundProperties))
                {
                    throw new InvalidOperationException("could not render log message");
                }

                var renderedMessage = parsedTemplate.Render(boundProperties.ToDictionary(p => p.Name, p => p.Value));
                Console.WriteLine("::set-output name=message::"   + renderedMessage);
                Console.WriteLine("::set-output name=exit-code::" + result.ExitCode);
                Console.WriteLine("::set-output name=dependency-count::" +
                                  result switch
                                  {
                                      DependencyResult dependencyResult => dependencyResult.Dependencies.Count,
                                      _                                 => 0,
                                  });
            }

            return result.ExitCode;
        }

        protected abstract Task<IResult> InvokeAsync(CancellationToken cancellationToken);
    }
}