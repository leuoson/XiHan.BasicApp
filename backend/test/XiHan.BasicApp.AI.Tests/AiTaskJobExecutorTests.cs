#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskJobExecutorTests
// Guid:f4d43b09-0d9d-4afa-bab3-c8b8b7bbb2a6
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Application.Services;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Saas.Domain.Enums;
using Xunit;

namespace XiHan.BasicApp.AI.Tests;

public sealed class AiTaskJobExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_records_success_run_for_plain_chat_task()
    {
        var task = new SysAiTask(7)
        {
            AiTaskCode = "morning-news",
            AiTaskName = "Morning News",
            PromptMode = AiTaskPromptMode.Inline,
            PromptText = "Write a short brief.",
            Status = EnableStatus.Enabled
        };
        var repository = new InMemoryAiTaskRepository(task);
        var runRepository = new InMemoryAiTaskRunRepository();
        var chat = new FakeAiTaskChatService("Brief result");
        var executor = new AiTaskExecutor(repository, runRepository, chat, new AiTaskPromptRenderer(new FakeAiPromptStore()));

        var result = await executor.ExecuteAsync(7);

        Assert.True(result.Succeeded);
        Assert.Single(runRepository.Runs);
        Assert.Equal(AiTaskRunStatus.Success, runRepository.Runs[0].RunStatus);
        Assert.Equal(1, runRepository.Runs[0].AttemptCount);
        Assert.Null(runRepository.Runs[0].LeaseOwner);
        Assert.Null(runRepository.Runs[0].LeaseExpiresAt);
        Assert.Equal("Brief result", runRepository.Runs[0].ResultText);
        Assert.Equal("Write a short brief.", runRepository.Runs[0].PromptSnapshot);
    }

    [Fact]
    public async Task ExecuteAsync_records_success_run_for_prompt_store_task()
    {
        var store = new FakeAiPromptStore();
        store.Add("daily-summary", "Write the daily summary.", "v1");
        var task = new SysAiTask(7)
        {
            AiTaskCode = "morning-news",
            AiTaskName = "Morning News",
            PromptMode = AiTaskPromptMode.PromptStore,
            PromptCode = "daily-summary",
            PromptVersion = "v1",
            Status = EnableStatus.Enabled
        };
        var repository = new InMemoryAiTaskRepository(task);
        var runRepository = new InMemoryAiTaskRunRepository();
        var chat = new FakeAiTaskChatService("Brief result");
        var executor = new AiTaskExecutor(repository, runRepository, chat, new AiTaskPromptRenderer(store));

        var result = await executor.ExecuteAsync(7);

        Assert.True(result.Succeeded);
        Assert.Equal("Write the daily summary.", chat.LastPrompt);
        Assert.Single(runRepository.Runs);
        Assert.Equal(AiTaskRunStatus.Success, runRepository.Runs[0].RunStatus);
        Assert.Equal("Write the daily summary.", runRepository.Runs[0].PromptSnapshot);
    }

    [Fact]
    public async Task ExecuteAsync_records_failed_run_when_prompt_store_prompt_is_missing()
    {
        var task = new SysAiTask(7)
        {
            AiTaskCode = "morning-news",
            AiTaskName = "Morning News",
            PromptMode = AiTaskPromptMode.PromptStore,
            PromptCode = "missing-prompt",
            Status = EnableStatus.Enabled
        };
        var repository = new InMemoryAiTaskRepository(task);
        var runRepository = new InMemoryAiTaskRunRepository();
        var chat = new FakeAiTaskChatService("Brief result");
        var executor = new AiTaskExecutor(repository, runRepository, chat, new AiTaskPromptRenderer(new FakeAiPromptStore()));

        var result = await executor.ExecuteAsync(7);

        Assert.False(result.Succeeded);
        Assert.Single(runRepository.Runs);
        Assert.Equal(AiTaskRunStatus.Failed, runRepository.Runs[0].RunStatus);
        Assert.Equal("AI 任务引用的提示词不存在或已禁用。", runRepository.Runs[0].ErrorMessage);
        Assert.Null(chat.LastPrompt);
    }

    [Fact]
    public async Task ExecuteRunAsync_completes_existing_running_run()
    {
        var task = new SysAiTask(7)
        {
            AiTaskCode = "morning-news",
            AiTaskName = "Morning News",
            PromptMode = AiTaskPromptMode.Inline,
            PromptText = "Write a short brief.",
            Status = EnableStatus.Enabled
        };
        var repository = new InMemoryAiTaskRepository(task);
        var runRepository = new InMemoryAiTaskRunRepository();
        runRepository.Runs.Add(new SysAiTaskRun(42)
        {
            AiTaskId = 7,
            AiTaskCode = "morning-news",
            RunStatus = AiTaskRunStatus.Queued
        });
        var chat = new FakeAiTaskChatService("Brief result");
        var executor = new AiTaskExecutor(repository, runRepository, chat, new AiTaskPromptRenderer(new FakeAiPromptStore()));

        var result = await executor.ExecuteRunAsync(42);

        Assert.True(result.Succeeded);
        Assert.Equal(42, result.RunId);
        Assert.Equal(AiTaskRunStatus.Success, result.RunStatus);
        Assert.Equal(AiTaskRunStatus.Success, runRepository.Runs[0].RunStatus);
        Assert.Equal(1, runRepository.Runs[0].AttemptCount);
        Assert.Null(runRepository.Runs[0].LeaseOwner);
        Assert.Null(runRepository.Runs[0].LeaseExpiresAt);
        Assert.Equal("Brief result", runRepository.Runs[0].ResultText);
        Assert.Equal("Write a short brief.", chat.LastPrompt);
    }

    [Fact]
    public async Task ExecuteRunAsync_ignores_result_when_lease_was_requeued()
    {
        var task = new SysAiTask(7)
        {
            AiTaskCode = "morning-news",
            AiTaskName = "Morning News",
            PromptMode = AiTaskPromptMode.Inline,
            PromptText = "Write a short brief.",
            Status = EnableStatus.Enabled
        };
        var repository = new InMemoryAiTaskRepository(task);
        var runRepository = new InMemoryAiTaskRunRepository();
        runRepository.Runs.Add(new SysAiTaskRun(42)
        {
            AiTaskId = 7,
            AiTaskCode = "morning-news",
            RunStatus = AiTaskRunStatus.Queued
        });
        var chat = new FakeAiTaskChatService("Late result", onComplete: () =>
        {
            runRepository.Runs[0].RunStatus = AiTaskRunStatus.Queued;
            runRepository.Runs[0].LeaseOwner = null;
            runRepository.Runs[0].LeaseExpiresAt = null;
            runRepository.Runs[0].ErrorMessage = "执行器中断或租约过期，已重新排队。";
        });
        var executor = new AiTaskExecutor(repository, runRepository, chat, new AiTaskPromptRenderer(new FakeAiPromptStore()));

        var result = await executor.ExecuteRunAsync(42);

        Assert.False(result.Succeeded);
        Assert.Equal(AiTaskRunStatus.Queued, runRepository.Runs[0].RunStatus);
        Assert.Null(runRepository.Runs[0].ResultText);
        Assert.Contains("租约", result.ErrorMessage);
    }
}
