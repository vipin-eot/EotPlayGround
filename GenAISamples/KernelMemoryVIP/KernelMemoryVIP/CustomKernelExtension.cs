using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable SKEXP0001 

namespace KernelMemoryVIP
{
    public static class CustomKernelExtension
    {
        public static IKernelBuilder AddLocalEmbeddingGeneration(this IKernelBuilder builder)
        {
            builder.Services.AddKeyedSingleton<ITextEmbeddingGenerationService>(null, new LocalEmbeddingService());
            return builder;
        }
    }
}
