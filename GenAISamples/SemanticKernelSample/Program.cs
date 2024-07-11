using Microsoft.SemanticKernel;

// Create kernel
var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: "[The name of your deployment]",
    endpoint: "[Your Azure endpoint]",
    apiKey: "[Your Azure OpenAI API key]",
    modelId: "[The name of the model]" // optional
);
var kernel = builder.Build();

var result = await kernel.InvokePromptAsync(
        "Give me a list of breakfast foods with eggs and cheese");
Console.WriteLine(result);