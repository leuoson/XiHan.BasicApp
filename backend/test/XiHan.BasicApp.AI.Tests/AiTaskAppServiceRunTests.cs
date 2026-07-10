#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskAppServiceRunTests
// Guid:70d863e1-43e9-4c25-8da0-83c8b49be7c0
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/09 00:00:00
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

public sealed class AiTaskAppServiceRunTests
{
    [Fact]
    public async Task RunAsync_enqueues_queued_ai_task_run_without_waiting_for_chat_completion()
    {
        var task = new SysAiTask(7)
        {
            AiTaskCode = "morning-news",
            AiTaskName = "Morning News",
            PromptMode = AiTaskPromptMode.Inline,
            PromptText = "Write a short brief.",
            Status = EnableStatus.Enabled
        };
        var taskRepository = new InMemoryAiTaskRepository(task);
        var runRepository = new InMemoryAiTaskRunRepository();
        var chat = new FakeAiTaskChatService("Brief result");
        var executor = new AiTaskExecutor(taskRepository, runRepository, chat, new AiTaskPromptRenderer(new FakeAiPromptStore()));
        var runQueue = new FakeAiTaskRunQueue();
        var appService = new AiTaskAppService(
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
            runQueue,
            runRepository,
            new InMemoryAiTaskRunGuidanceRepository(),
            new FakeAiTaskRunEventService());

        var result = await appService.RunAsync(new AiTaskRunDto { BasicId = 7 });

        Assert.True(result.Succeeded);
        Assert.True(result.RunId > 0);
        Assert.Equal(AiTaskRunStatus.Queued, result.RunStatus);
        Assert.Null(result.ResultText);
        Assert.Null(result.ErrorMessage);
        Assert.Null(chat.LastPrompt);
        var run = Assert.Single(runRepository.Runs);
        Assert.Equal(result.RunId!.Value, run.BasicId);
        Assert.Equal(AiTaskRunStatus.Queued, run.RunStatus);
        Assert.Equal(result.RunId.Value, Assert.Single(runQueue.RunIds));
    }
}
