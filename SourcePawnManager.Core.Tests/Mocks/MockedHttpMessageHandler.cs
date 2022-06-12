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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SourcePawnManager.Core.Tests.Mocks;

public class MockedHttpMessageHandler : HttpMessageHandler
{
    private readonly IList<HttpRequestMessage> _requests = new List<HttpRequestMessage>();
    private readonly HttpResponseMessage       _response;

    public MockedHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    public IList<HttpRequestMessage> Requests => _requests.ToImmutableList();

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                           CancellationToken  cancellationToken)
    {
        _requests.Add(request);

        return Task.FromResult(_response);
    }
}