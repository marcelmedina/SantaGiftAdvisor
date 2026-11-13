using Azure.AI.AgentServer.Responses;
using Azure.AI.AgentServer.Responses.Models;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SantaGiftAdvisor;

ResponsesServer.Run<SantaGiftAdvisorHandler>(configure: builder =>
{
    if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
        Console.Error.WriteLine(
            "[WARNING] APPLICATIONINSIGHTS_CONNECTION_STRING is not set; telemetry will not be sent to Application Insights.");

    var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT");
    if (string.IsNullOrWhiteSpace(endpoint))
        throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT environment variable is not set.");

    var model = Environment.GetEnvironmentVariable("AZURE_AI_MODEL_DEPLOYMENT_NAME")
        ?? Environment.GetEnvironmentVariable("FOUNDRY_MODEL")
        ?? "gpt-4.1";

    // Foundry's local agent uses the Responses API and executes the gift tools.
    AIAgent agent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
        .AsAIAgent(
            model: model,
            name: "SantaGiftAdvisor",
            instructions: """
                You are Santa's Gift Advisor, a friendly and practical gift recommendation assistant.
                Ask clarifying questions when age, interests, or budget are missing.
                Use SearchGifts to find suitable gift ideas and CheckGiftAvailability before claiming
                that a gift is available. Respect the user's maximum budget and explain each suggestion.
                The catalogue is sample data, so be transparent.
                """,
            tools: [GiftTools.SearchGiftsFunction, GiftTools.CheckGiftAvailabilityFunction]);

    builder.Services.AddSingleton(agent);
});

public sealed class SantaGiftAdvisorHandler(
    AIAgent agent,
    ILogger<SantaGiftAdvisorHandler> logger) : ResponseHandler
{
    public override IAsyncEnumerable<ResponseStreamEvent> CreateAsync(
        CreateResponse request,
        ResponseContext context,
        CancellationToken cancellationToken)
    {
        return new TextResponse(context, request,
            createText: ct => GenerateTextAsync(context, ct));
    }

    private async Task<string> GenerateTextAsync(
        ResponseContext context,
        CancellationToken cancellationToken)
    {
        var userInput = await context.GetInputTextAsync(cancellationToken: cancellationToken) ?? "Hello!";
        var history = await context.GetHistoryAsync(cancellationToken);
        var messages = new List<ChatMessage>();

        foreach (var item in history)
        {
            if (item is OutputItemMessage { Content: { } contents })
            {
                foreach (var content in contents)
                {
                    switch (content)
                    {
                        case MessageContentOutputTextContent { Text: { } assistantText }:
                            messages.Add(new ChatMessage(ChatRole.Assistant, assistantText));
                            break;
                        case MessageContentInputTextContent { Text: { } userText }:
                            messages.Add(new ChatMessage(ChatRole.User, userText));
                            break;
                    }
                }
            }
        }

        messages.Add(new ChatMessage(ChatRole.User, userInput));
        logger.LogInformation("Processing request {ResponseId}", context.ResponseId);

        var session = await agent.CreateSessionAsync(cancellationToken: cancellationToken);
        var response = await agent.RunAsync(messages, session, cancellationToken: cancellationToken);
        return response.Text;
    }
}