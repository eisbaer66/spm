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

using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Schema.Generation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Git.CredentialManager;
using Microsoft.Git.CredentialManager.Interop.MacOS;
using Microsoft.Git.CredentialManager.Interop.Windows;
using SourcePawnManager.Core.Apis.FileSystems;
using SourcePawnManager.Core.Apis.Git;
using SourcePawnManager.Core.Apis.GitHub;
using SourcePawnManager.Core.Apis.Http;
using SourcePawnManager.Core.DefinitionStores;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag.DownloadGitHubTagFile;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag.DownloadGitHubTagZip;
using SourcePawnManager.Core.DependencyStrategy.StaticUrl;
using SourcePawnManager.Core.JsonSerialization;
using SourcePawnManager.Core.JsonSerialization.Schemas;
using SourcePawnManager.Core.LocalStores;
using SourcePawnManager.Core.LockStores;
using SourcePawnManager.Core.VersionStores;
using FileSystem = SourcePawnManager.Core.Apis.FileSystems.FileSystem;
using IFileSystem = SourcePawnManager.Core.Apis.FileSystems.IFileSystem;

namespace SourcePawnManager.Core;

public static class ServiceCollectionServiceExtensions
{
    private const string GitHubBaseAddress   = "https://api.github.com/";
    private const string HttpClientUserAgent = "icebear.SourcePawnManager";

    public static IServiceCollection AddSourcePawnManager(this IServiceCollection services,
                                                          bool                    dryRun      = false,
                                                          string?                 githubToken = null)
    {
        services.AddOptions<GitHubToken>(GitHubToken.UserSpecified)
                .Configure(t =>
                           {
                               t.Url   = GitHubBaseAddress;
                               t.Value = githubToken;
                           });
        services.AddOptions<GitHubToken>()
                .Configure((GitHubToken t, IGitCredentialsReader credentials) =>
                           {
                               t.Url   = GitHubBaseAddress;
                               t.Value = githubToken ?? credentials.Fill(GitHubBaseAddress);
                           });

        services.AddMediatR(typeof(IncludeManager));
        services.AddHttpClient(GitHubApi.ApiHttpClientName,
                               (sp, client) => ConfigureGitHubClient(client, sp, "application/json"));
        services.AddHttpClient(GitHubApi.DownloadHttpClientName,
                               (sp, client) => ConfigureGitHubClient(client, sp, "application/octet-stream"));
        services.AddHttpClient(HttpApi.ApiHttpClientName,
                               (_, client) => client.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse(HttpClientUserAgent)));

        if (dryRun)
        {
            services.AddTransient<IRequestHandler<DownloadGitHubTagZipQuery, Unit>, DryRunDownloadGitHubTagZipQueryHandler>();
            services.AddTransient<IRequestHandler<DownloadGitHubTagFileQuery, Unit>, DryRunDownloadGitHubTagFileQueryHandler>();
            services.AddTransient<IRequestHandler<DownloadStaticUrlQuery, Unit>, DryRunDownloadStaticUrlQueryHandler>();
        }

        services.AddDryRun<IGitHubApi, GitHubApi, DryRunGitHubApi>(dryRun, (impl,                  logger) => new DryRunGitHubApi(impl, logger));
        services.AddDryRun<IFileSystem, FileSystem, DryRunFileSystem>(dryRun, (impl,               logger) => new DryRunFileSystem(impl, logger));
        services.AddDryRun<IVersionStore, SideFileVersionStore, DryRunVersionStore>(dryRun, (impl, logger) => new DryRunVersionStore(impl, logger));
        services.AddDryRun<ILocalStore, FileLocalStore, DryRunLocalStore>(dryRun, (impl,           logger) => new DryRunLocalStore(impl, logger));
        services.AddDryRun<ILockStore, FileLockStore, DryRunLockStore>(dryRun, (impl,              logger) => new DryRunLockStore(impl, logger));
        services.AddDryRun<IDefinitionStore, DefinitionJsonStore, DryRunDefinitionStore>(dryRun,
                                                                                         (impl, logger) => new DryRunDefinitionStore(impl, logger));
        services.AddDryRun<IHttpApi, HttpApi, DryRunHttpApi>(dryRun, (_, logger) => new DryRunHttpApi(logger));

        services.TryAddSingleton(_ =>
                                 {
                                     try
                                     {
                                         if (PlatformUtils.IsWindows())
                                         {
                                             return new WindowsCredentialManager();
                                         }

                                         if (PlatformUtils.IsMacOS())
                                         {
                                             return new MacOSKeychain();
                                         }

                                         if (PlatformUtils.IsLinux())
                                         {
                                             return new CommandContext(GetApplicationPath()).CredentialStore;
                                         }
                                     }
                                     catch (Exception e) when (e.Message.StartsWith("Failed to locate '") &&
                                                               e.Message.EndsWith(" executable on the path."))
                                     {
                                         return new NoCredentialStore();
                                     }

                                     throw new PlatformNotSupportedException();
                                 });
        services.TryAddSingleton<GitCredentialsReader>();
        services.TryAddSingleton<IGitCredentialsReader>(sp => new NotInstalledGitCredentialsReader(sp.GetRequiredService<GitCredentialsReader>(),
                                                                                                   sp.GetRequiredService<ILogger<NotInstalledGitCredentialsReader>>()));
        services.TryAddSingleton<IGitCredentialsWriter, GitCredentialsWriter>();
        services.TryAddSingleton<GitCredentials>();
        services.AddDryRun<IGitCredentials, NotInstalledGitCredentials, DryRunGitCredentials>(dryRun,
                                                                                              (impl, logger) => new DryRunGitCredentials(impl, logger),
                                                                                              sp => new(sp.GetRequiredService<GitCredentials>(),
                                                                                                        sp.GetRequiredService<ILogger<NotInstalledGitCredentials>>()));

        services.TryAddSingleton<IJsonSerializationService, JsonSerializationService>();
        services.TryAddSingleton<IJsonFileSystem, JsonFileSystem>();
        services.TryAddSingleton<EmbeddedFileJSchemaStore, EmbeddedFileJSchemaStore>();
        services.TryAddSingleton<DynamicJSchemaStore, DynamicJSchemaStore>();
        services.TryAddSingleton<IJSchemaStore>(sp => new CompositeJSchemaStore(sp.GetRequiredService<EmbeddedFileJSchemaStore>(),
                                                                                sp.GetRequiredService<DynamicJSchemaStore>()));
        services.TryAddSingleton<IDefinitionStore, DefinitionJsonStore>();
        services.TryAddSingleton<IIncludeManager, IncludeManager>();

        services.TryAddSingleton(_ => new SchemaGeneratorConfiguration
                                      {
                                          PropertyNamingMethod = PropertyNamingMethods.CamelCase,
                                      });
        services.TryAddSingleton(_ => new JsonSerializerOptions(JsonSerializerDefaults.Web)
                                      {
                                          AllowTrailingCommas    = true,
                                          WriteIndented          = true,
                                          DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                          Converters =
                                          {
                                              new JsonStringEnumConverter(),
                                              new NuGetVersionConverter(),
                                              new VersionRangeConverter(),
                                              new DependencyConverter(),
                                          },
                                      });

        return services;
    }

    private static void ConfigureGitHubClient(HttpClient client, IServiceProvider serviceProvider, string acceptHeader)
    {
        client.BaseAddress = new(GitHubBaseAddress);
        client.DefaultRequestHeaders.Accept.ParseAdd(acceptHeader);
        client.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse(HttpClientUserAgent));

        var token = serviceProvider.GetRequiredService<IOptions<GitHubToken>>().Value;
        if (token.Value is not null)
        {
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"token {token.Value}");
        }
    }

    public static IServiceCollection AddDryRun<TService, TImplementation, TDryRunImplementation>(
        this IServiceCollection                                  services,
        bool                                                     dryRun,
        Func<TService, ILogger<TDryRunImplementation>, TService> factory,
        Func<IServiceProvider, TImplementation>?                 implFactory = null)
        where TService : class
        where TImplementation : class, TService
        where TDryRunImplementation : class, TService
    {
        if (implFactory == null)
        {
            services.TryAddSingleton<TImplementation>();
        }
        else
        {
            services.TryAddSingleton(implFactory);
        }

        if (dryRun)
        {
            services.TryAddSingleton(sp =>
                                     {
                                         var impl   = sp.GetRequiredService<TImplementation>();
                                         var logger = sp.GetRequiredService<ILogger<TDryRunImplementation>>();
                                         return factory(impl, logger);
                                     });
        }
        else
        {
            if (implFactory == null)
            {
                services.TryAddSingleton<TService, TImplementation>();
            }
            else
            {
                services.TryAddSingleton<TService>(implFactory);
            }
        }

        return services;
    }

    // See GCM's Program.cs
    private static string GetApplicationPath()
    {
        // Assembly::Location always returns an empty string if the application was published as a single file
        var isSingleFile = string.IsNullOrEmpty(Assembly.GetEntryAssembly()?.Location);

        // Use "argv[0]" to get the full path to the entry executable - this is consistent across
        // .NET Framework and .NET >= 5 when published as a single file.
        var args          = Environment.GetCommandLineArgs();
        var candidatePath = args[0];

        // If we have not been published as a single file on .NET 5 then we must strip the ".dll" file extension
        // to get the default AppHost/SuperHost name.
        if (!isSingleFile && Path.HasExtension(candidatePath))
        {
            return Path.ChangeExtension(candidatePath, null);
        }

        return candidatePath;
    }
}