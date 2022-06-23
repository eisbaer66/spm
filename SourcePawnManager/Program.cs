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
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using SourcePawnManager;
using SourcePawnManager.Commands;
using SourcePawnManager.Core;

var outputTemplate = "{DryRun}[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
var logLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Error);
Log.Logger = new LoggerConfiguration()
             .MinimumLevel.ControlledBy(logLevelSwitch)
             .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
             .MinimumLevel.Override("System", LogEventLevel.Warning)
             .Enrich.FromLogContext()
             .WriteTo.Console(outputTemplate: outputTemplate)
             .CreateBootstrapLogger();

try
{
    var logger = Log.Logger.ForContext<Program>();
    logger.Information("Getting the motors running...");

    var rootCommand = new RootCommand("install, restore and update include-files from multiple sources")
                      {
                          new ListCommand(),
                          new InstallCommand(),
                          new RestoreCommand(),
                          new UpdateCommand(),
                          new RemoveCommand(),
                          new LicenseCommand(),
                      };

    var logWarningOption = new Option<bool>("-v",    "logs: shows warnings");
    var logInfoOption    = new Option<bool>("-vv",   "logs: shows information");
    var logDebugOption   = new Option<bool>("-vvv",  "logs: shows debug");
    var logVerboseOption = new Option<bool>("-vvvv", "logs: shows verbose");
    var workingDirectoryOption =
        new Option<string>(new[] { "--working-directory", "-w" }, () => ".", "directory containing the spm.json");
    var dryRunOption = new Option<bool>(new[] { "--dry-run", "-d" },
                                        "executes a dry-run without actually installing/restoring/updating/removing");
    var githubTokenOption =
        new Option<string>(new[] { "--github-token", "-t" }, "GitHub token used to authenticate API calls");
    rootCommand.AddGlobalOption(workingDirectoryOption);
    rootCommand.AddGlobalOption(dryRunOption);
    rootCommand.AddGlobalOption(githubTokenOption);
    rootCommand.AddGlobalOption(logWarningOption);
    rootCommand.AddGlobalOption(logInfoOption);
    rootCommand.AddGlobalOption(logDebugOption);
    rootCommand.AddGlobalOption(logVerboseOption);

    var     dryRun      = false;
    string? githubToken = null;
    var     builder     = new CommandLineBuilder(rootCommand);
    var parser = builder
                 .UseDefaults()
                 .UseHelp(ctx => { ctx.HelpBuilder.CustomizeLayout(_ => CustomizeHelp()); })
                 .AddMiddleware(ctx =>
                                {
                                    dryRun = ctx.ParseResult.HasOption(dryRunOption);
                                    if (dryRun)
                                    {
                                        LogContext.PushProperty("DryRun", "[DRY-RUN] ");
                                    }
                                },
                                MiddlewareOrder.Configuration)
                 .AddMiddleware(ctx =>
                                {
                                    if (ctx.ParseResult.HasOption(githubTokenOption))
                                    {
                                        githubToken = ctx.ParseResult.GetValueForOption(githubTokenOption);
                                    }
                                },
                                MiddlewareOrder.Configuration)
                 .UseHost(_ => Host.CreateDefaultBuilder(args),
                          hostBuilder =>
                          {
                              hostBuilder.UseSerilog((context, _, loggerConfiguration) =>
                                                         ConfigureLogging(loggerConfiguration,
                                                                          context,
                                                                          logLevelSwitch,
                                                                          outputTemplate))
                                         .ConfigureServices(services =>
                                                                services.AddSourcePawnManager(dryRun, githubToken))
                                         .UseNestedCommandHandlerFromAssembly<Program>();
                          })
                 .AddMiddleware(ctx =>
                                {
                                    LogEventLevel? Map()
                                    {
                                        if (ctx.ParseResult.HasOption(logVerboseOption))
                                        {
                                            return LogEventLevel.Verbose;
                                        }

                                        if (ctx.ParseResult.HasOption(logDebugOption))
                                        {
                                            return LogEventLevel.Debug;
                                        }

                                        if (ctx.ParseResult.HasOption(logInfoOption))
                                        {
                                            return LogEventLevel.Information;
                                        }

                                        if (ctx.ParseResult.HasOption(logWarningOption))
                                        {
                                            return LogEventLevel.Warning;
                                        }

                                        return null;
                                    }

                                    var level = Map();
                                    if (level == null)
                                    {
                                        return;
                                    }

                                    logLevelSwitch.MinimumLevel = level.Value;
                                })
                 .AddMiddleware(ctx =>
                                {
                                    var workingDirectory = ctx.ParseResult.GetValueForOption(workingDirectoryOption);
                                    if (workingDirectory == null)
                                    {
                                        return;
                                    }

                                    Environment.CurrentDirectory = Path.GetFullPath(workingDirectory);
                                })
                 .Build();
    return await parser.InvokeAsync(args);
}
catch (Exception exception)
{
    Log.Fatal(exception, "unexpected error");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

IEnumerable<HelpSectionDelegate> CustomizeHelp()
{
    return HelpBuilder.Default.GetLayout()
                      .Prepend(helpContext =>
                               {
                                   new ConsoleHelper(helpContext
                                                         .Output)
                                       .WriteHeader();
                               });
}

LoggerConfiguration ConfigureLogging(LoggerConfiguration loggerConfiguration,
                                     HostBuilderContext  hostBuilderContext,
                                     LoggingLevelSwitch  loggingLevelSwitch,
                                     string              s) =>
    loggerConfiguration.ReadFrom.Configuration(hostBuilderContext.Configuration)
                       .MinimumLevel.ControlledBy(loggingLevelSwitch)
                       .MinimumLevel.Override("Microsoft",
                                              LogEventLevel
                                                  .Warning)
                       .MinimumLevel.Override("System",
                                              LogEventLevel
                                                  .Warning)
                       .MinimumLevel.Override("System.CommandLine.IConsole",
                                              LogEventLevel
                                                  .Verbose)
                       .Enrich.FromLogContext()
                       .WriteTo.Console(outputTemplate: s);