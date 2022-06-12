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
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SourcePawnManager.Core.Results;

namespace SourcePawnManager.Core.Tests.Results;

public class ErrorResultTests
{
    private static readonly object[] ConstructorCases =
    {
        new object[]
        {
            null!,
            123,
            new object[] { 123 },
        },
        new object[]
        {
            "message",
            123,
            null!,
        },
    };

    [Test]
    [TestCaseSource(nameof(ConstructorCases))]
    public void ConstructorThrowsArgumentNullException(string message, int exitCode, params object[] args)
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new ErrorResult(message, exitCode, args));
    }

    [Test]
    public void ConstructorThrowsArgumentOutOfRangeException()
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentOutOfRangeException>(() => new ErrorResult("message", 0, 123));
    }

    [Test]
    public void LogMessages()
    {
        const string message  = nameof(message);
        const int    exitCode = 1;
        var          result   = new ErrorResult(message, exitCode, 123);

        Assert.AreEqual(exitCode,              result.ExitCode);
        Assert.AreEqual(LogLevel.Error, result.LogLevel);
        Assert.AreEqual(message,        result.Log.Message);
    }
}