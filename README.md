# Santa's Gift Advisor

A C# Responses protocol server that helps users find gift ideas based on age, interests, and budget. It uses Microsoft Agent Framework with a Microsoft Foundry Responses model and two function tools: `SearchGifts` and `CheckGiftAvailability`. The protocol SDK handles HTTP responses, SSE streaming, health probes, and telemetry.

## Requirements
- .NET 11 SDK and x64 runtime
- An Azure AI Foundry project and a deployed model
- Azure CLI authentication (`az login`) or another credential supported by `DefaultAzureCredential`

## Configuration
Set these environment variables:

- `FOUNDRY_PROJECT_ENDPOINT` — your Foundry project endpoint
- `AZURE_AI_MODEL_DEPLOYMENT_NAME` — model deployment name (falls back to `FOUNDRY_MODEL`, then `gpt-4.1`)
- `APPLICATIONINSIGHTS_CONNECTION_STRING` — optional; enables Application Insights telemetry

For local development, set these in your shell before starting the server. The application does not load a `.env` file.

## Packages
The project references `Azure.AI.AgentServer.Responses` and `Microsoft.Agents.AI.Foundry`. Restore and run:

```bash
dotnet restore
dotnet run
```

The server listens on port `8088`. In a separate PowerShell terminal:

```powershell
$body = @{ input = "Find a gift for a 12-year-old who likes art, under $50."; stream = $false } | ConvertTo-Json
$response = Invoke-RestMethod -Uri http://localhost:8088/responses -Method Post -ContentType "application/json" -Body $body
$response.output

$followUp = @{ input = "Is it in stock?"; previous_response_id = $response.id; stream = $false } | ConvertTo-Json
Invoke-RestMethod -Uri http://localhost:8088/responses -Method Post -ContentType "application/json" -Body $followUp
```

Set `stream` to `true` for SSE events. The handler reconstructs conversation history from `previous_response_id` and creates a separate agent session per request. Errors and cancellation are handled by the protocol SDK rather than returned as successful text.

## Debugging in VS Code
Select **SantaGiftAdvisor** in Run and Debug, then press F5. The server runs in
the integrated terminal; send requests from a separate terminal.

On Windows, the build task and debugger use the system .NET installation under
`%ProgramFiles%\dotnet`. The debug configuration sets both `DOTNET_ROOT` and
`DOTNET_ROOT_X64` so an inherited Azure Logic Apps private SDK path cannot select
an older runtime. Install the .NET 11 SDK and x64 runtime in that system location.
The project launch profile in `Properties/launchSettings.json` also sets these
variables for the C# Run/Debug button and `dotnet run` on Windows.

The gift catalogue is an in-memory sample and availability is illustrative. Replace the sample data and availability logic with a real catalogue/API before production use.

## Project structure
- `Program.cs` — Responses server, conversation handler, and Foundry agent setup
- `Tools/GiftTools.cs` — gift search and availability function tools
- `Models/Gift.cs` — gift model
