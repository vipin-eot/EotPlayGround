// This is a sample project that implement a RAG using local embeddings and Semantic Kernel.

/*
 * 
 * 
 * Scenario Overview
The demo scenario is designed to answer the question, "What is Bruno's favourite super hero?" using two different approaches:

  1. Directly asking the Phi-3 model.
  2. Adding a semantic memory object with fan facts loaded and then asking the question.
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */


#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;

Console.ForegroundColor = ConsoleColor.Yellow;

var question = "Who is Matt Oberdorfer?";
Console.WriteLine($"Question: {question}");

Console.WriteLine("");

// Create a chat completion service
var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(
    modelId: "phi3",
    endpoint: new Uri("http://localhost:1234"),
    apiKey: "apikey");

builder.AddLocalTextEmbeddingGeneration();
Kernel kernel = builder.Build();

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine($"Phi-3 response (no memory/RAG):");

Console.ForegroundColor = ConsoleColor.Green;
var response = kernel.InvokePromptStreamingAsync(question);
await foreach (var result in response)
{
    Console.Write(result);
}

Console.ForegroundColor = ConsoleColor.White;
// separator
Console.WriteLine("");
Console.WriteLine("====================================================");
Console.WriteLine("");

// get the embeddings generator service
var embeddingGenerator = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();
var memory = new SemanticTextMemory(new VolatileMemoryStore(), embeddingGenerator);

// add facts to the collection
const string MemoryCollectionName = "fanFacts";

await memory.SaveInformationAsync(MemoryCollectionName, id: "info1", text: "Matt Oberdorfer is the founder and CEO of 'EOT (Embassy of Things)', an investor, speaker, and author of five books.");
await memory.SaveInformationAsync(MemoryCollectionName, id: "info2", text: "Matt's forthcoming book, \"The Trailblazer’s Guide to Industrial IoT\" for energy and manufacturing digital transformation leaders is a comprehensive guide offers expert insights and practical advice on upgrading the OT/IT infrastructure of industrial companies to harness the power of modern AI, ML and analytics for #digitaltwins, #iiot, #industrialdatalake, and #datahistorian.");
await memory.SaveInformationAsync(MemoryCollectionName, id: "info3", text: "In 2019, he launched 'EOT(Embassy of Things)' , a leading software company with the mission of enabling industrial enterprises to modernize and build their own operational cloud historian, industrial digital twin, or industrial data lake.");

TextMemoryPlugin memoryPlugin = new(memory);

// Import the text memory plugin into the Kernel.
kernel.ImportPluginFromObject(memoryPlugin);

OpenAIPromptExecutionSettings settings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
};

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine($"Phi-3 response (using semantic memory).");

Console.ForegroundColor = ConsoleColor.Green;

await AnswerUsingRAGMemory(kernel, MemoryCollectionName, settings, "Who is Matt Oberdorfer?");

await AnswerUsingRAGMemory(kernel, MemoryCollectionName, settings, "When was EOT (Embassy of Things) launched and by whom?");

Console.WriteLine($"");
Console.WriteLine($"");

Console.ForegroundColor = ConsoleColor.White;

static async Task AnswerUsingRAGMemory(Kernel kernel, string MemoryCollectionName, OpenAIPromptExecutionSettings settings, string question)
{
    Console.WriteLine($"");

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"\n\nQues: {question}");

    var prompt = @"
    Question: {{$input}}
    Answer the question using the memory content: {{Recall}}";

    var answer = kernel.InvokePromptStreamingAsync(prompt, new KernelArguments(settings)
                                                                        {
                                                                            { "input", question },
                                                                            { "collection", MemoryCollectionName }
                                                                        });

    Console.ForegroundColor = ConsoleColor.Green;

    await foreach (var result in answer)
    {
        Console.Write(result);
    }
}