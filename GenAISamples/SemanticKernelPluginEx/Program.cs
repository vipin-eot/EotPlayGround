
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelPluginEx;
using System.Net;

var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(
    modelId: "phi3",
    endpoint: new Uri("http://localhost:1234"),
    apiKey: "apikey");

// Build the kernel
Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Add a plugin (the LightsPlugin class is defined below)
kernel.Plugins.AddFromType<LightsPlugin>("Lights");

//kernel.Plugins.AddFromFunctions("time_plugin",
//[
//    KernelFunctionFactory.CreateFromMethod(
//        method: () => DateTime.Now,
//        functionName: "get_time",
//        description: "Get the current time"
//    ),
//    KernelFunctionFactory.CreateFromMethod(
//        method: (DateTime start, DateTime end) => (end - start).TotalSeconds,
//        functionName: "diff_time",
//        description: "Get the difference between two times in seconds"
//    )
//]);

// Enable planning
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

// Create a history store the conversation
var history = new ChatHistory();
history.AddUserMessage("Please turn on the lamp");
//history.AddUserMessage("Get the current time");

// Get the response from the AI
var result = await chatCompletionService.GetChatMessageContentAsync(
   history,
   executionSettings: openAIPromptExecutionSettings,
   kernel: kernel);

// Print the results
Console.WriteLine("Assistant > " + result);

// Add the message from the agent to the chat history
history.AddAssistantMessage(result.ToString());