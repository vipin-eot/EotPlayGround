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
using System;


// Create a chat completion service
var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(
    modelId: "phi3",
    endpoint: new Uri("http://localhost:1234"),
    apiKey: "apikey");

builder.AddLocalTextEmbeddingGeneration();
 
Kernel kernel = builder.Build();

//Console.ForegroundColor = ConsoleColor.Yellow;

//var question = "Who is Matt Oberdorfer?";
//Console.WriteLine($"Question: {question}");

//Console.WriteLine("");

//Console.ForegroundColor = ConsoleColor.White;
//Console.WriteLine($"Phi-3 response (no memory/RAG):");

//Console.ForegroundColor = ConsoleColor.Green;
//var response = kernel.InvokePromptStreamingAsync(question);
//await foreach (var res in response)
//{
//    Console.Write(res);
//}

Console.ForegroundColor = ConsoleColor.White;
// separator
//Console.WriteLine("");
//Console.WriteLine("====================================================");
//Console.WriteLine("");

// get the embeddings generator service
var embeddingGenerator = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();
var memory = new SemanticTextMemory(new VolatileMemoryStore(), embeddingGenerator);

// add facts to the collection
const string MemoryCollectionName = "Oilwell";

string OilWellIntro = File.ReadAllText(@"..\..\..\OilWellIntro.txt");
string OilWellIDrilling = File.ReadAllText(@"..\..\..\OilWellIDrilling.txt");
string OilWellITypes = File.ReadAllText(@"..\..\..\OilWellITypes.txt");

//await memory.SaveInformationAsync(MemoryCollectionName, id: "info1", text: "Today is October 32nd, 2476");

await memory.SaveInformationAsync(MemoryCollectionName, id: "info1", text: OilWellIntro);
await memory.SaveInformationAsync(MemoryCollectionName, id: "info2", text: OilWellIDrilling);
await memory.SaveInformationAsync(MemoryCollectionName, id: "info3", text: OilWellITypes);

//await memory.SaveInformationAsync("About EOT", id: "info2", text: "Matt's forthcoming book, \"The Trailblazer’s Guide to Industrial IoT\" for energy and manufacturing digital transformation leaders is a comprehensive guide offers expert insights and practical advice on upgrading the OT/IT infrastructure of industrial companies to harness the power of modern AI, ML and analytics for #digitaltwins, #iiot, #industrialdatalake, and #datahistorian.");
//await memory.SaveInformationAsync(MemoryCollectionName, id: "info3", text: "In 2019, he launched 'EOT(Embassy of Things)' , a leading software company with the mission of enabling industrial enterprises to modernize and build their own operational cloud historian, industrial digital twin, or industrial data lake.");

TextMemoryPlugin memoryPlugin = new(memory);

// Import the text memory plugin into the Kernel.
kernel.ImportPluginFromObject(memoryPlugin);

OpenAIPromptExecutionSettings settings = new()
{
    Temperature = 0,
    //ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
    MaxTokens = 3330,
    ResultsPerPrompt = 10,
};

Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine($"=========== Phi-3 Response As RAG (using semantic memory) ==================");

Console.ForegroundColor = ConsoleColor.Green;

var response = await memory.SearchAsync(MemoryCollectionName, "What are the types of well?" , 1, 0.5).FirstOrDefaultAsync();
Console.WriteLine(response.Metadata.Text);

//await AnswerUsingRAGMemory(kernel, MemoryCollectionName, settings, "What's the current date?");
//await AnswerUsingRAGMemory(kernel, MemoryCollectionName, settings, "What is oil well?");
await AnswerUsingRAGMemory(kernel, MemoryCollectionName, settings, "How to drill a well?");
await AnswerUsingRAGMemory(kernel, MemoryCollectionName, settings, "What are the types of well?");
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


 
