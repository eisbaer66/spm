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

using System.Text.Json.Serialization;
using Json.Schema.Generation;
using MediatR;
using NuGet.Versioning;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;
using SourcePawnManager.Core.DependencyStrategy.Shared;

namespace SourcePawnManager.Core.DependencyStrategy.StaticUrl;

public class StaticUrlDependency : IDependency
{
    public StaticUrlDependency(string url, string? downloadPath)
    {
        Throw.IfNullOrWhitespace(url, nameof(url));

        Url = url;
        DownloadPath = string.IsNullOrWhiteSpace(downloadPath)
                           ? Path.Combine("include", Path.GetFileName(url))
                           : downloadPath;
    }

    [JsonIgnore]
    public string Id => $"StaticUrl:{Url}";

    [JsonPropertyOrder(int.MinValue)]
    public DependencyType Type => DependencyType.StaticUrl;
    
    [JsonPropertyOrder(int.MinValue + 1)]
    public VersionRange VersionRange => VersionRange.All;

    [Required]
    public string Url { get; }

    [Required]
    public string DownloadPath { get; }

    public IRequest<IList<DependencyVersion>> GetVersions() => new GetStaticVersionsQuery();

    public IRequest Download(DependencyVersion dependencyVersion) => new DownloadStaticUrlQuery(this);

    public static StaticUrlDependency From(IDictionary<string, string> dict)
    {
        var url        = dict.Read("url");
        var downloadPath = dict.ContainsKey("downloadPath") ? dict["downloadPath"] : null;

        return new(url, downloadPath);
    }
}