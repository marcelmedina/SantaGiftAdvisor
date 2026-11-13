# Santa's Gift Advisor

A small interactive C# console agent that helps users find gift ideas based on age, interests, and budget. It uses Microsoft Agent Framework with a Microsoft Foundry model and two function tools: `SearchGifts` and `CheckGiftAvailability`.

## Requirements
- .NET 10 SDK
- An Azure AI Foundry project and a deployed model
- Azure CLI authentication (`az login`) or another credential supported by `DefaultAzureCredential`

## Configuration
Set these environment variables:

- `FOUNDRY_PROJECT_ENDPOINT` — your Foundry project endpoint
- `FOUNDRY_MODEL` — model deployment name (defaults to `gpt-4.1-mini`)

## Packages
Install the packages used by the sample:

```bash
dotnet add package Microsoft.Agents.AI.Foundry --prerelease
dotnet add package Azure.AI.Projects
dotnet add package Azure.Identity
dotnet add package Microsoft.Extensions.AI
```

Then restore and run:

```bash
dotnet restore
dotnet run
```

The gift catalogue is an in-memory sample and availability is illustrative. Replace the sample data and availability logic with a real catalogue/API before production use.

## Project structure
- `Program.cs` — console chat loop and Foundry agent setup
- `Tools/GiftTools.cs` — gift search and availability function tools
- `Models/Gift.cs` — gift model
