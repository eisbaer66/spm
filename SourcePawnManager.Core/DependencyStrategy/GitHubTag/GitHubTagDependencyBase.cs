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
using System.Text.RegularExpressions;
using MediatR;
using NuGet.Versioning;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag.GetVersionsGitHubTag;

namespace SourcePawnManager.Core.DependencyStrategy.GitHubTag;

public abstract class GitHubTagDependencyBase
{
    public const string DefaultVersionRegEx = @"(\d+(?:\.\d+)?(?:\.\d+)?(?:\.\d+)?)[-+]?(\S*)?";

    protected GitHubTagDependencyBase(string         owner,
                                      string         repository,
                                      VersionRange   versionRange,
                                      DependencyType type,
                                      string?        versionRegEx = null)
    {
        Throw.IfNullOrEmpty(owner,      nameof(owner));
        Throw.IfNullOrEmpty(repository, nameof(repository));

        Owner        = owner;
        Repository   = repository;
        VersionRegEx = string.IsNullOrWhiteSpace(versionRegEx) ? null : versionRegEx;
        VersionRange = versionRange ?? throw new ArgumentNullException(nameof(versionRange));
        Type         = type;
    }

    [JsonPropertyOrder(int.MinValue)]
    public DependencyType Type { get; }

    [JsonPropertyOrder(int.MinValue + 1)]
    public VersionRange VersionRange { get; }

    [JsonPropertyOrder(int.MinValue + 2)]
    public string? VersionRegEx { get; }

    [JsonPropertyOrder(int.MinValue + 3)]
    public string Owner { get; }

    [JsonPropertyOrder(int.MinValue + 4)]
    public string Repository { get; }

    public IRequest<IList<DependencyVersion>> GetVersions() => new GetVersionsGitHubTagQuery(this);
    

    public abstract override bool Equals(object? obj);
    protected bool Equals(GitHubTagDependencyBase other) => Type == other.Type &&
                                                            VersionRange.Equals(other.VersionRange) &&
                                                            VersionRegEx == other.VersionRegEx &&
                                                            Owner == other.Owner &&
                                                            Repository == other.Repository;

    public override int GetHashCode() => HashCode.Combine((int)Type, VersionRange, VersionRegEx, Owner, Repository);

    public bool TryParseVersion(string input, out NuGetVersion nuGetVersion)
    {
        var versionRegEx = string.IsNullOrWhiteSpace(VersionRegEx) ? DefaultVersionRegEx : VersionRegEx;
        return TryParseVersion(input, out nuGetVersion, versionRegEx);
    }

    private static bool TryParseVersion(string input, out NuGetVersion version, string pattern)
    {
        version = new(0, 0, 0);

        var match = Regex.Match(input, pattern); //@"(\d+(?:\.\d+)?(?:\.\d+)?(?:\.\d+)?)[-+]?(\S*)?"
        if (!match.Success)
        {
            return false;
        }

        if (match.Groups.Count < 3)
        {
            return false;
        }

        var value = match.Groups[1].Value;
        if (value.All(c => c != '.'))
        {
            value += ".0";
        }

        if (match.Groups[2].Length > 0)
        {
            value += "-" + match.Groups[2].Value;
        }

        return NuGetVersion.TryParse(value, out version);
    }
}