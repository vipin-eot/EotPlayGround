using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelMemoryVIP
{
    public class EotHttpMessageHandler : HttpClientHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // https://api.openai.com/v1/embeddings
            if (request.RequestUri != null && request.RequestUri.LocalPath.Contains("embeddings", StringComparison.OrdinalIgnoreCase))
            {
                request.RequestUri = new Uri($"http://localhost:1234{request.RequestUri.PathAndQuery}");
            }
            else if (request.RequestUri != null && request.RequestUri.Host.Equals("api.openai.com", StringComparison.OrdinalIgnoreCase))
            {
                request.RequestUri = new Uri($"http://localhost:1234{request.RequestUri.PathAndQuery}");
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
