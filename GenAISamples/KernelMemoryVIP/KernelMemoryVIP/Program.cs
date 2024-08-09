// Copyright (c) Microsoft. All rights reserved.

using KernelMemoryVIP;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Text;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable SKEXP0001,SKEXP0003, SKEXP0010, SKEXP0011, SKEXP0020, SKEXP0050, SKEXP0052, SKEXP0055, SKEXP0070  // Type is for evaluation purposes only and is subject to change or removal in future updates. 

public static class Program
{
    public static async Task Main()
    {
        // Using OpenAI for embeddings
        var openAIEmbeddingConfig = new OpenAIConfig
        {
            EmbeddingModel = "text-embedding-ada-002",
            EmbeddingModelMaxTokenTotal = 8191,
            APIKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!
        };

        HttpClient client = new HttpClient(new EotHttpMessageHandler());

        // Using LM Studio for text generation
        var lmStudioConfig = new OpenAIConfig
        {
            Endpoint = "http://localhost:1234/v1/",
            TextModel = "local-model",
            TextModelMaxTokenTotal = 4096,
            APIKey = "lm-studio"
        };

        // Initialize the Semantic kernel
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

        kernelBuilder
               .AddOpenAIChatCompletion(
                           modelId: "phi3",
                       endpoint: new Uri("http://localhost:1234"),
                       apiKey: "lm-studio")
               .AddLocalEmbeddingGeneration();


        Kernel kernel = kernelBuilder.Build();

        // Get the required service for text embedding generation registered in the kernel.
        var embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        // Download a document and create embeddings for it
        //ISemanticTextMemory memory = new MemoryBuilder()
        //    .WithLoggerFactory(kernel.LoggerFactory)
        //    //.WithMemoryStore(memoryStore)
        //    .WithTextEmbeddingGeneration(embeddingService)
        //    .Build();

        var memory = new KernelMemoryBuilder()
            //.WithOpenAITextEmbeddingGeneration(openAIEmbeddingConfig) // OpenAI
            .WithOpenAITextEmbeddingGeneration(lmStudioConfig, httpClient: client)
            //.WithSemanticKernelTextEmbeddingGenerationService(embeddingService)
            .WithOpenAITextGeneration(lmStudioConfig) // LM Studio
            .Build();

        // Generate embeddings for the provided text.
        var response = await embeddingService.GenerateEmbeddingsAsync(new string[] { "Today is October 32nd, 2476" });

        // Import some text - This will use OpenAI embeddings
        await memory.ImportTextAsync("Today is October 32nd, 2476");

        // Generate an answer - This uses OpenAI for embeddings and finding relevant data, and LM Studio to generate an answer
        var answer = await memory.AskAsync("What's the current date?");
        Console.WriteLine(answer.Question);
        Console.WriteLine(answer.Result);

        /*

        -- Output using Mixtral 8x 7B Q8_0:

        What's the current date?
        The current date is October 32nd, 2476.


        -- Server log:

        [2024-03-21 19:30:22.201] [INFO] [LM STUDIO SERVER] Processing queued request...
        [2024-03-21 19:30:22.202] [INFO] Received POST request to /v1/chat/completions with body: {
            "messages": [
                {
                    "content": "Facts:\n==== [File:content.txt;Relevance:82.6%]:\nToday is October 32nd, 2476\n======\nGiven only the facts above, provide a comprehensive/detailed answer.\nYou don't know where the knowledge comes from, just answer.\nIf you don't have sufficient information, reply with 'INFO NOT FOUND'.\nQuestion: What's the current date?\nAnswer: ",
                    "role": "system"
                }
            ],
            "max_tokens": 300,
            "temperature": 0,
            "top_p": 0,
            "presence_penalty": 0,
            "frequency_penalty": 0,
            "stream": true,
            "model": "local-model"
        }
        [2024-03-21 19:30:22.203] [INFO] [LM STUDIO SERVER] Context Overflow Policy is: Rolling Window
        [2024-03-21 19:30:22.204] [INFO] [LM STUDIO SERVER] Streaming response...
        [2024-03-21 19:30:23.023] [INFO] [LM STUDIO SERVER] First token generated. Continuing to stream response..
        [2024-03-21 19:30:25.907] [INFO] Finished streaming response

        */
    }
}
