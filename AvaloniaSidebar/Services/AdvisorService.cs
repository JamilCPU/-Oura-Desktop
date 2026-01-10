using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Protocol;

namespace AvaloniaSidebar.Services;

public class AdvisorService
{
    private readonly ILocalLlmService _llmService;
    private readonly IMcpClientService _mcpClient;
    private IReadOnlyList<Tool>? _availableTools;

    public AdvisorService(ILocalLlmService llmService, IMcpClientService mcpClient)
    {
        _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
        _mcpClient = mcpClient ?? throw new ArgumentNullException(nameof(mcpClient));
    }

    public async Task InitializeAsync()
    {
        await _llmService.InitializeAsync();
        await _mcpClient.InitializeAsync();
        _availableTools = await _mcpClient.ListToolsAsync();
    }

    public async Task<string> ProcessQueryAsync(string userQuery, CancellationToken cancellationToken = default)
    {
        if (_availableTools == null)
        {
            await InitializeAsync();
        }

        var toolsDescription = BuildToolsDescription();
        var systemPrompt = BuildSystemPrompt(toolsDescription);
        var fullPrompt = $"{systemPrompt}\n\nUser: {userQuery}\n\nAssistant:";

        var maxIterations = 5;
        var iteration = 0;
        var conversationHistory = new StringBuilder();
        conversationHistory.AppendLine(fullPrompt);

        while (iteration < maxIterations)
        {
            var response = await _llmService.GenerateResponseAsync(conversationHistory.ToString(), cancellationToken);
            conversationHistory.AppendLine(response);

            var toolCall = ParseToolCall(response);
            if (toolCall == null)
            {
                return response.Trim();
            }

            var toolResult = await ExecuteToolCallAsync(toolCall.Value.name, toolCall.Value.arguments, cancellationToken);
            conversationHistory.AppendLine($"\nTool Result: {toolResult}\n");
            iteration++;
        }

        return conversationHistory.ToString();
    }

    private string BuildSystemPrompt(string toolsDescription)
    {
        return $@"You are a helpful Oura health advisor. You can access the user's Oura ring data through available tools.

Available Tools:
{toolsDescription}

When the user asks a question that requires data, you should call the appropriate tool using this format:
TOOL_CALL: tool_name
ARGUMENTS: {{""key"": ""value""}}

After receiving tool results, analyze the data and provide a helpful, natural language response to the user's question.

If you don't need to call a tool, respond directly to the user's question.";
    }

    private string BuildToolsDescription()
    {
        if (_availableTools == null || _availableTools.Count == 0)
            return "No tools available.";

        var sb = new StringBuilder();
        foreach (var tool in _availableTools)
        {
            sb.AppendLine($"- {tool.Name}: {tool.Description}");
        }
        return sb.ToString();
    }

    private (string name, Dictionary<string, object?> arguments)? ParseToolCall(string llmOutput)
    {
        var toolCallPattern = @"TOOL_CALL:\s*(\w+)";
        var argumentsPattern = @"ARGUMENTS:\s*(\{.*?\})";

        var toolMatch = Regex.Match(llmOutput, toolCallPattern, RegexOptions.IgnoreCase);
        if (!toolMatch.Success)
            return null;

        var toolName = toolMatch.Groups[1].Value;

        var arguments = new Dictionary<string, object?>();
        var argsMatch = Regex.Match(llmOutput, argumentsPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (argsMatch.Success)
        {
            try
            {
                var jsonArgs = JsonSerializer.Deserialize<Dictionary<string, object?>>(argsMatch.Groups[1].Value);
                if (jsonArgs != null)
                {
                    arguments = jsonArgs;
                }
            }
            catch
            {
            }
        }

        return (toolName, arguments);
    }

    private async Task<string> ExecuteToolCallAsync(string toolName, Dictionary<string, object?> arguments, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mcpClient.CallToolAsync(toolName, arguments, cancellationToken);
            
            var textContent = result.Content
                .OfType<TextContentBlock>()
                .FirstOrDefault();

            return textContent?.Text ?? "Tool executed successfully but returned no text content.";
        }
        catch (Exception ex)
        {
            return $"Error calling tool {toolName}: {ex.Message}";
        }
    }
}


