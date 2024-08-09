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
using System.ComponentModel;


// Create a chat completion service
var builder = Kernel.CreateBuilder();
//builder.AddOpenAIChatCompletion(
//    modelId: "phi3",
//    endpoint: new Uri("http://localhost:1234"),
//    apiKey: "apikey");

 

//builder.AddLocalTextEmbeddingGeneration();


// Add function approval service and filter
builder.Services.AddSingleton<IFunctionApprovalService, ConsoleFunctionApprovalService>();
builder.Services.AddSingleton<IFunctionInvocationFilter, FunctionInvocationFilter>();

// Add software builder plugin
builder.Plugins.AddFromType<SoftwareBuilderPlugin>();


Kernel kernel = builder.Build();

 

OpenAIPromptExecutionSettings settings = new()
{
    Temperature = 0,
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
};


// Initialize kernel arguments.
var arguments = new KernelArguments(settings);

var requirements = await kernel.InvokeAsync<string>("SoftwareBuilderPlugin", "collect_requirements", new() {   });
Console.WriteLine(requirements);
 

// Start execution
// Try to reject invocation at each stage to compare LLM results.
//var response = kernel.InvokePromptStreamingAsync("I want to build a software. Let's start from the first step.", arguments);

//await foreach (var res in response)
//{
//    Console.Write(res);
//}



#region Plugins

public sealed class SoftwareBuilderPlugin
{
    [KernelFunction("collect_requirements")]
    [Description("Gets all the requirements for software testing.")]
    public string CollectRequirements()
    {
        Console.WriteLine("Collecting requirements...");
        return "Requirements";
    }

    [KernelFunction]
    public string Design(string requirements)
    {
        Console.WriteLine($"Designing based on: {requirements}");
        return "Design";
    }

    [KernelFunction]
    public string Implement(string requirements, string design)
    {
        Console.WriteLine($"Implementing based on {requirements} and {design}");
        return "Implementation";
    }

    [KernelFunction]
    public string Test(string requirements, string design, string implementation)
    {
        Console.WriteLine($"Testing based on {requirements}, {design} and {implementation}");
        return "Test Results";
    }

    [KernelFunction]
    public string Deploy(string requirements, string design, string implementation, string testResults)
    {
        Console.WriteLine($"Deploying based on {requirements}, {design}, {implementation} and {testResults}");
        return "Deployment";
    }
}

#endregion

#region Approval

/// <summary>
/// Service that verifies if function invocation is approved.
/// </summary>
public interface IFunctionApprovalService
{
    bool IsInvocationApproved(KernelFunction function, KernelArguments arguments);
}

/// <summary>
/// Service that verifies if function invocation is approved using console.
/// </summary>
public sealed class ConsoleFunctionApprovalService : IFunctionApprovalService
{
    public bool IsInvocationApproved(KernelFunction function, KernelArguments arguments)
    {
        Console.WriteLine("====================");
        Console.WriteLine($"Function name: {function.Name}");
        Console.WriteLine($"Plugin name: {function.PluginName ?? "N/A"}");

        if (arguments.Count == 0)
        {
            Console.WriteLine("\nArguments: N/A");
        }
        else
        {
            Console.WriteLine("\nArguments:");

            foreach (var argument in arguments)
            {
                Console.WriteLine($"{argument.Key}: {argument.Value}");
            }
        }

        Console.WriteLine("\nApprove invocation? (yes/no)");

        var input = Console.ReadLine();

        return input?.Equals("yes", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}

#endregion

#region Filter

/// <summary>
/// Filter to invoke function only if it's approved.
/// </summary>
public sealed class FunctionInvocationFilter(IFunctionApprovalService approvalService) : IFunctionInvocationFilter
{
    private readonly IFunctionApprovalService _approvalService = approvalService;

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        // Invoke the function only if it's approved.
        if (this._approvalService.IsInvocationApproved(context.Function, context.Arguments))
        {
            await next(context);
        }
        else
        {
            // Otherwise, return a result that operation was rejected.
            context.Result = new FunctionResult(context.Result, "Operation was rejected.");
        }
    }
}

#endregion
