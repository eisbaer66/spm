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

public class InfoResultTests
{
    private static readonly object[] ConstructorCases =
    {
        new object[]
        {
            null!,
            new object[] { 123 },
        },
        new object[]
        {
            "message",
            null!,
        },
    };

    [Test]
    [TestCaseSource(nameof(ConstructorCases))]
    public void ConstructorThrowsArgumentNullException(string message, params object[] args)
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new InfoResult(message, args));
    }

    [Test]
    public void ConstructorThrowsArgumentExceptionIfMessageIsWhitespaces()
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentException>(() => new InfoResult(" ", 123));
    }

    [Test]
    public void ConstructorThrowsArgumentExceptionIfNotArgs()
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentException>(() => new InfoResult("message"));
    }

    [Test]
    public void LogMessages()
    {
        const string message = nameof(message);
        var          result  = new InfoResult(message, 123);

        Assert.AreEqual(0,                    result.ExitCode);
        Assert.AreEqual(LogLevel.Information, result.LogLevel);
        Assert.AreEqual(message,              result.Log.Message);
    }
}