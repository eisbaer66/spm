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

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;
using SourcePawnManager.Core.Results;

namespace SourcePawnManager.Core.Tests.Results;

public class DependencyResultTests
{
    [SetUp]
    public void Setup()
    {
    }

    // ReSharper disable ObjectCreationAsStatement
    [Test]
    public void ConstructorThrowsArgumentNullExceptionIfDependenciesAreNull()
    {
        Assert.Throws<ArgumentNullException>(() => new DependencyResult(null!, new List<IResult>(), "verb"));
    }

    [Test]
    public void ConstructorThrowsArgumentNullExceptionIfErrorsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new DependencyResult(new List<IDependencyLock>(), null!, "verb"));
    }

    [Test]
    public void ConstructorThrowsArgumentNullExceptionIfVerbIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new DependencyResult(new List<IDependencyLock>(), new List<IResult>(), null!));
    }

    [Test]
    public void ConstructorThrowsArgumentExceptionIfVerbIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new DependencyResult(new List<IDependencyLock>(), new List<IResult>(), string.Empty));
    }

    [Test]
    public void ConstructorThrowsArgumentExceptionIfVerbIsWhitespace()
    {
        Assert.Throws<ArgumentException>(() => new DependencyResult(new List<IDependencyLock>(), new List<IResult>(), " "));
    }

    // ReSharper restore ObjectCreationAsStatement

    private static readonly object[] Cases =
    {
        new object[] { Array.Empty<IDependencyLock>(), "no dependencies {Verb}. {@Errors}" },
        new object[]
        {
            new IDependencyLock[]
            {
                new DependencyLock("id", DependencyVersion.Parse("1.0"), "downloadPath"),
            },
            "dependency {DependencyId} {Verb} version {Version} to {DownloadPath}. {@Errors}",
        },
        new object[]
        {
            new IDependencyLock[]
            {
                new DependencyLock("id", DependencyVersion.Parse("1.0"), "downloadPath"),
                new DependencyLock("id2", DependencyVersion.Parse("1.0"), "downloadPath"),
            },
            "{DependencyCount} dependencies {Verb}: {@Dependencies} {@Errors}",
        },
    };

    [Test]
    [TestCaseSource(nameof(Cases))]
    public void LogMessages(IDependencyLock[] dependencies, string expectedLogMessage)
    {
        var result = new DependencyResult(dependencies, new List<IResult>{new ErrorResult("Test", 1)}, "updated");

        Assert.AreEqual(0,                    result.ExitCode);
        Assert.AreEqual(LogLevel.Information, result.LogLevel);
        Assert.AreEqual(expectedLogMessage,   result.Log.Message);
    }
}