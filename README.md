# Oura Desktop

A desktop sidebar that lets you ask questions about your Oura Ring data in plain language, with the language model running locally. A small Avalonia app docks to the edge of the screen; it uses a local LLM (Llama 3.1 8B via LLamaSharp) to decide which Oura API tools to call through a Model Context Protocol (MCP) server, then answers from the returned sleep, activity, readiness and heart-rate data. Nothing is sent to a hosted AI service; only the Oura API is contacted.

## Stack

- C# on .NET 9
- Avalonia 11 (cross-platform desktop UI) with Fluent theme
- LLamaSharp (CPU backend) running a quantised Llama 3.1 8B Instruct GGUF model
- Model Context Protocol C# SDK (`ModelContextProtocol`): an stdio MCP server exposing Oura tools, and an MCP client inside the sidebar
- Oura API v2 with an OAuth 2.0 authorisation-code flow (local callback on port 8080)
- xUnit for tests

## Features

- Screen-edge sidebar on Windows and Linux (reserves screen space)
- MCP server tools for personal info, daily sleep, sleep sessions, activity, readiness, heart rate, workouts, sessions, tags, stress, rest mode, ring configuration and SpO2
- Local model download with progress on first run (about 5 GB of free disk space is required)
- OAuth token storage and refresh

## Running

Prerequisites: the .NET 9 SDK and an Oura developer application (to get a client ID and secret).

1. Clone the repository.
2. Provide your Oura OAuth credentials, either as environment variables:

   ```
   export OURA_CLIENT_ID=<your-client-id>
   export OURA_CLIENT_SECRET=<your-client-secret>
   ```

   or in `backend/config/oauth-config.json` (this file is git-ignored):

   ```json
   {
     "client_id": "<your-client-id>",
     "client_secret": "<your-client-secret>",
     "redirect_uri": "http://localhost:8080/callback"
   }
   ```

   Register `http://localhost:8080/callback` as the redirect URI in your Oura application.
3. Build the MCP server first, because the sidebar launches it as a child process:

   ```
   dotnet build mcp-server
   ```

4. Run the sidebar:

   ```
   dotnet run --project AvaloniaSidebar
   ```

On first run you are prompted to connect to Oura in the browser. If no local model is configured, the app creates `backend/config/llm-config.json` pointing at `models/llama-3.1-8b-instruct.Q4_K_M.gguf` and offers to download it. Model files and config are git-ignored.

Run the tests with:

```
dotnet test tests/backend/api
```

## Project layout

- `AvaloniaSidebar/` - UI, view models, local LLM service, MCP client and the advisor loop that lets the model call tools
- `backend/api/` - shared Oura client and service, OAuth service and token store, LLM and model-download helpers (compiled into each project)
- `mcp-server/` - stdio MCP server exposing the Oura tools
- `tests/backend/api/` - xUnit tests for the Oura service
- `sidebar/` - early console stub, superseded by `AvaloniaSidebar/`

## Status

Work in progress. Expect rough edges, manual setup steps and breaking changes; automating OAuth setup is planned.
