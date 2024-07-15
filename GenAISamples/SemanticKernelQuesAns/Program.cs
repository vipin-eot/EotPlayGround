#pragma warning disable SKEXP0001, SKEXP0003, SKEXP0010, SKEXP0011, SKEXP0050, SKEXP0052

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// Create kernel with a custom http address
var builder = Kernel.CreateBuilder();

builder.AddOpenAIChatCompletion(
    modelId: "phi3",
    endpoint: new Uri("http://localhost:1234"),
    apiKey: "apikey");

var kernel = builder.Build();
 

//define prompt execution settings
var settings = new OpenAIPromptExecutionSettings
{
    //MaxTokens = 200,  // Response words
    Temperature = 1
};
var kernelArguments = new KernelArguments(settings);

//var prompt = "Write a short joke about kittens. Use Emojis";


while(true)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write($"Ques: ");
    var prompt = Console.ReadLine();

    var response = kernel.InvokePromptStreamingAsync(prompt, kernelArguments);

    Console.ForegroundColor = ConsoleColor.Green;

    Console.Write($"Answer:");

    await foreach (var message in response)
    {
        Console.Write(message.ToString());
    }

    Console.WriteLine();
}


// if Wants complete answer in one single chunks
//var result = await kernel.InvokePromptAsync("Give me a list of breakfast foods with eggs and cheese. Only names");
//Console.WriteLine(result);