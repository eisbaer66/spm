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

using NUnit.Framework;
using SourcePawnManager.Core.Apis.Git;

namespace SourcePawnManager.Core.Tests.Apis.Git;

public class NoCredentialStoreTest
{
    [Test]
    public void Get()
    {
        var credential = new NoCredentialStore().Get("service", "account");
        Assert.IsNull(credential);
    }

    [Test]
    public void AddOrUpdate()
    {
        new NoCredentialStore().AddOrUpdate("service", "account", "secret");
    }

    [Test]
    public void Remove()
    {
        var removed = new NoCredentialStore().Remove("service", "account");
        Assert.False(removed);
    }
}