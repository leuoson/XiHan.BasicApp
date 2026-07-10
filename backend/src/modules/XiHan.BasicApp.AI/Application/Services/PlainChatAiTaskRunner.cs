#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:PlainChatAiTaskRunner
// Guid:ff5ae620-947f-4469-9c20-a5c844fa6095
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using System.Text;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;

namespace XiHan.BasicApp.AI.Application.Services;

/// <summary>
/// 普通聊天 AI 任务运行器
/// </summary>
public sealed class PlainChatAiTaskRunner : IAiTaskAgentRunner
{
    private readonly IAiTaskChatService _chatService;
    private readonly IAiTaskRunEventService _eventService;

    /// <summary>
    /// 构造函数
    /// </summary>
    public PlainChatAiTaskRunner(IAiTaskChatService chatService, IAiTaskRunEventService eventService)
    {
        _chatService = chatService;
        _eventService = eventService;
    }

    /// <inheritdoc />
    public AiTaskRunnerKind Kind => AiTaskRunnerKind.PlainChat;

    /// <inheritdoc />
    public string Version => "plain-chat-v1";

    /// <inheritdoc />
    public async Task<AiTaskRunnerResult> RunAsync(AiTaskRunContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var prompt = BuildPrompt(context.Prompt, context.PendingGuidance);
        if (context.PendingGuidance.Count > 0)
        {
            await _eventService.AppendAsync(
                context.Run.BasicId,
                AiTaskRunEventType.GuidanceApplied,
                AiTaskRunEventRole.System,
                $"已应用 {context.PendingGuidance.Count} 条运行引导。",
                null,
                cancellationToken);
        }

        var result = await _chatService.CompleteAsync(context.Task, prompt, cancellationToken);
        if (!string.IsNullOrWhiteSpace(result))
        {
            await _eventService.AppendAsync(
                context.Run.BasicId,
                AiTaskRunEventType.AgentMessageDelta,
                AiTaskRunEventRole.Assistant,
                result,
                null,
                cancellationToken);
        }

        return new AiTaskRunnerResult(result);
    }

    private static string BuildPrompt(string prompt, IReadOnlyList<SysAiTaskRunGuidance> pendingGuidance)
    {
        if (pendingGuidance.Count == 0)
        {
            return prompt;
        }

        var builder = new StringBuilder(prompt.Trim());
        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine("Additional run guidance:");
        foreach (var guidance in pendingGuidance)
        {
            builder.AppendLine($"- {guidance.Content.Trim()}");
        }

        return builder.ToString();
    }
}
