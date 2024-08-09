using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KernelMemoryVIP
{
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public class LocalEmbeddingService : ITextEmbeddingGenerationService
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    {
        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

        public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(
            IList<string> prompt,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default)
        {
            var requestBody = new EmbeddingRequest { Input = prompt[0] };
            var serializedBody = JsonSerializer.Serialize(requestBody);
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://127.0.0.1:8000/");

            var content = new StringContent(serializedBody, Encoding.UTF8, "application/json");

            // Send a POST request to the "/embedding" endpoint with the content.
            var response = await httpClient.PostAsync("/embedding", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            using var responseStream = await response.Content.ReadAsStreamAsync();

            var list = new List<ReadOnlyMemory<float>>();
            try
            {
                // Deserialize the response stream into an EmbeddingResponse object.
                var embeddingsResponse = await JsonSerializer.DeserializeAsync<EmbeddingResponse>(responseStream);

                foreach (var item in embeddingsResponse.Data)
                {
                    var array = item.Select(x => x).ToArray();
                    var memory = new ReadOnlyMemory<float>(array);
                    list.Add(memory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return list;
        }
    }


    public class EmbeddingRequest
    {
        public string Input { get; set; }
    }

    public class EmbeddingResponse
    {
        public List<List<float>> Data { get; set; }
    }


}
