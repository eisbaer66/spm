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

namespace SourcePawnManager.Core.Apis.Http;

public class HttpApi : IHttpApi
{
    public const     string             ApiHttpClientName = "HttpApi";
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpApi(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<Stream?> GetStream(string url, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(ApiHttpClientName);
        return await httpClient.GetStreamAsync(url, cancellationToken);
    }
}