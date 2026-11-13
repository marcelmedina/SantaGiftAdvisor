using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using SantaGiftAdvisor;

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT");
if (string.IsNullOrWhiteSpace(endpoint))
{
    Console.WriteLine("Set the FOUNDRY_PROJECT_ENDPOINT environment variable first.");
    return;
}

var model = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-4.1-mini";
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

var session = await agent.CreateSessionAsync();
Console.WriteLine("Santa's Gift Advisor");
Console.WriteLine("Tell me who you're shopping for, their interests, and your budget.");
Console.WriteLine("Type 'exit' to quit.\n");

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) continue;
    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

    try
    {
        var response = await agent.RunAsync(input, session);
        Console.WriteLine($"\nSanta: {response}\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nSomething went wrong: {ex.Message}\n");
    }
}
