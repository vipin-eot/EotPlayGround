//This is a sample project that implement a Console chat using Semantic Kernel.

#pragma warning disable SKEXP0001, SKEXP0003, SKEXP0010, SKEXP0011, SKEXP0050, SKEXP0052

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
 

// Create kernel with a custom http address
var builder = Kernel.CreateBuilder();

builder.AddOpenAIChatCompletion(
    modelId: "phi3",
    endpoint: new Uri("http://localhost:1234"),
    apiKey: "apikey");

var kernel = builder.Build();

var chat = kernel.GetRequiredService<IChatCompletionService>();

var history = new ChatHistory();

history.AddSystemMessage("You are a useful chatbot. If you don't know an answer, say 'I don't know!'." +
    " Always reply in a funny ways. Use emojis if possible.");

while (true)
{
    Console.Write("Q:");
    var userQ = Console.ReadLine();
    if (string.IsNullOrEmpty(userQ))
    {
        break;
    }
    history.AddUserMessage(userQ);

    var result = await chat.GetChatMessageContentsAsync(history);
    Console.WriteLine(result[^1].Content);
    history.Add(result[^1]);
}
