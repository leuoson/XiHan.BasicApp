#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskRunGuidanceTests
// Guid:994d3895-0505-4408-a3ad-e6fe19e59649
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Application.AppServices;
using XiHan.BasicApp.AI.Application.Dtos;
using XiHan.BasicApp.AI.Application.Services;
using XiHan.BasicApp.AI.Domain.DomainServices.Implementations;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Saas.Domain.Enums;
using Xunit;

namespace XiHan.BasicApp.AI.Tests;

public sealed class AiTaskRunGuidanceTests
{
    [Fact]
    public async Task AppendRunGuidanceAsync_accepts_guidance_for_running_run()
    {
        var runRepository = new InMemoryAiTaskRunRepository();
        runRepository.Runs.Add(new SysAiTaskRun(42)
        {
            AiTaskId = 7,
            AiTaskCode = "daily",
            RunStatus = AiTaskRunStatus.Running
        });
        var guidanceRepository = new InMemoryAiTaskRunGuidanceRepository();
        var eventService = new FakeAiTaskRunEventService();
        var appService = CreateAppService(runRepository, guidanceRepository, eventService);

        var result = await appService.AppendRunGuidanceAsync(new AiTaskAppendGuidanceDto
        {
            RunId = 42,
            Content = "Focus on overdue items.",
            ClientRequestId = "client-1"
        });

        Assert.Equal(42, result.RunId);
        Assert.Equal(AiTaskRunGuidanceStatus.Pending, result.Status);
        Assert.Equal("Focus on overdue items.", result.Content);
        Assert.Contains(eventService.Events, e => e.RunId == 42 && e.EventType == AiTaskRunEventType.GuidanceReceived);
    }

    [Fact]
    public async Task AppendRunGuidanceAsync_is_idempotent_by_client_request_id()
    {
        var runRepository = new InMemoryAiTaskRunRepository();
        runRepository.Runs.Add(new SysAiTaskRun(42) { AiTaskId = 7, AiTaskCode = "daily", RunStatus = AiTaskRunStatus.Queued });
        var guidanceRepository = new InMemoryAiTaskRunGuidanceRepository();
        var appService = CreateAppService(runRepository, guidanceRepository, new FakeAiTaskRunEventService());

        var first = await appService.AppendRunGuidanceAsync(new AiTaskAppendGuidanceDto { RunId = 42, Content = "A", ClientRequestId = "same" });
        var second = await appService.AppendRunGuidanceAsync(new AiTaskAppendGuidanceDto { RunId = 42, Content = "A", ClientRequestId = "same" });

        Assert.Equal(first.BasicId, second.BasicId);
        Assert.Single(guidanceRepository.Guidance);
    }

    [Fact]
    public async Task AppendRunGuidanceAsync_rejects_completed_run()
    {
        var runRepository = new InMemoryAiTaskRunRepository();
        runRepository.Runs.Add(new SysAiTaskRun(42) { AiTaskId = 7, AiTaskCode = "daily", RunStatus = AiTaskRunStatus.Success });
        var appService = CreateAppService(runRepository, new InMemoryAiTaskRunGuidanceRepository(), new FakeAiTaskRunEventService());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            appService.AppendRunGuidanceAsync(new AiTaskAppendGuidanceDto { RunId = 42, Content = "Too late." }));

        Assert.Equal("AI 任务运行已结束，不能追加引导。", ex.Message);
    }

    private static AiTaskAppService CreateAppService(
        InMemoryAiTaskRunRepository runRepository,
        InMemoryAiTaskRunGuidanceRepository guidanceRepository,
        FakeAiTaskRunEventService eventService)
    {
        var taskRepository = new InMemoryAiTaskRepository(new SysAiTask(7)
        {
            AiTaskCode = "daily",
            AiTaskName = "Daily",
            PromptMode = AiTaskPromptMode.Inline,
            PromptText = "Base prompt.",
            Status = EnableStatus.Enabled
        });
        var executor = AiTaskExecutorTestFactory.Create(
            taskRepository,
            runRepository,
            new FakeAiTaskChatService("unused"),
            eventService: eventService);

        return new AiTaskAppService(
            new AiTaskDomainService(taskRepository),
            taskRepository,
            new InMemoryAiTaskToolPolicyRepository([]),
            new InMemoryAiToolRepository(new SysAiTool(11)
            {
                ToolCode = "knowledge.search",
                ToolName = "Knowledge Search",
                Status = EnableStatus.Enabled
            }),
            new FakeAiTaskBackingTaskSyncService(),
            executor,
            new FakeAiTaskRunQueue(),
            runRepository,
            guidanceRepository,
            eventService);
    }
}
