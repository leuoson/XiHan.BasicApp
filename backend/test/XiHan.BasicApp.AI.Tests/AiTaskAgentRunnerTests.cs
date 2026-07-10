#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskAgentRunnerTests
// Guid:57d3fdb9-8d61-4c7f-8760-934e2d534782
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using Microsoft.Extensions.Options;
using XiHan.BasicApp.AI.Application.Services;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.AI.Infrastructure.Configuration;
using Xunit;

namespace XiHan.BasicApp.AI.Tests;

public sealed class AiTaskAgentRunnerTests
{
    [Fact]
    public async Task PlainChatAiTaskRunner_merges_pending_guidance_before_chat_call()
    {
        var chat = new FakeAiTaskChatService("done");
        var eventService = new FakeAiTaskRunEventService();
        var runner = new PlainChatAiTaskRunner(chat, eventService);
        var context = new AiTaskRunContext(
            new SysAiTask(7) { AiTaskCode = "daily", AiTaskName = "Daily" },
            new SysAiTaskRun(42) { AiTaskId = 7, AiTaskCode = "daily" },
            "Base prompt",
            [
                new SysAiTaskRunGuidance(1) { RunId = 42, Content = "Focus on overdue items." }
            ]);

        var result = await runner.RunAsync(context);

        Assert.Equal("done", result.ResultText);
        Assert.Contains("Base prompt", chat.LastPrompt);
        Assert.Contains("Additional run guidance", chat.LastPrompt);
        Assert.Contains("Focus on overdue items.", chat.LastPrompt);
        Assert.Contains(eventService.Events, e => e.EventType == AiTaskRunEventType.GuidanceApplied);
    }

    [Fact]
    public void AiTaskRunnerResolver_returns_plain_chat_runner_by_default()
    {
        var plain = new PlainChatAiTaskRunner(new FakeAiTaskChatService("done"), new FakeAiTaskRunEventService());
        var resolver = new AiTaskRunnerResolver(
            Options.Create(new AiTaskRuntimeOptions { DefaultRunnerKind = AiTaskRunnerKind.PlainChat }),
            [plain]);

        var result = resolver.Resolve();

        Assert.Same(plain, result);
    }
}
