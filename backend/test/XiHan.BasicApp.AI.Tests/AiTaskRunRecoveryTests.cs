#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskRunRecoveryTests
// Guid:e7f8d1dc-cbd5-44e6-972f-93c0a50d2a9b
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/09 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using Microsoft.Extensions.Logging.Abstractions;
using XiHan.BasicApp.AI.Application.Services;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Saas.Domain.Enums;
using Xunit;

namespace XiHan.BasicApp.AI.Tests;

public sealed class AiTaskRunRecoveryTests
{
    [Fact]
    public async Task RecoverAsync_requeues_stale_queued_run_when_queue_message_was_lost()
    {
        var now = DateTimeOffset.Parse("2026-07-09T16:00:00+08:00");
        var task = CreateTask(maxRetryCount: 1);
        var taskRepository = new InMemoryAiTaskRepository(task);
        var runRepository = new InMemoryAiTaskRunRepository();
        runRepository.Runs.Add(new SysAiTaskRun(42)
        {
            AiTaskId = task.BasicId,
            AiTaskCode = task.AiTaskCode,
            StartedTime = now.AddMinutes(-5),
            RunStatus = AiTaskRunStatus.Queued
        });
        var queue = new FakeAiTaskRunQueue();
        var eventService = new FakeAiTaskRunEventService();
        var recovery = new AiTaskRunRecoveryService(
            runRepository,
            taskRepository,
            queue,
            new FakeTimeProvider(now),
            eventService,
            NullLogger<AiTaskRunRecoveryService>.Instance);

        await recovery.RecoverAsync();

        Assert.Equal(42, Assert.Single(queue.RunIds));
        Assert.Equal(AiTaskRunStatus.Queued, runRepository.Runs[0].RunStatus);
    }

    [Fact]
    public async Task RecoverAsync_requeues_expired_running_run_when_retry_budget_remains()
    {
        var now = DateTimeOffset.Parse("2026-07-09T16:00:00+08:00");
        var task = CreateTask(maxRetryCount: 1);
        var taskRepository = new InMemoryAiTaskRepository(task);
        var runRepository = new InMemoryAiTaskRunRepository();
        runRepository.Runs.Add(new SysAiTaskRun(42)
        {
            AiTaskId = task.BasicId,
            AiTaskCode = task.AiTaskCode,
            StartedTime = now.AddMinutes(-10),
            RunStatus = AiTaskRunStatus.Running,
            AttemptCount = 1,
            LeaseOwner = "stopped-worker",
            LeaseExpiresAt = now.AddMinutes(-1)
        });
        var queue = new FakeAiTaskRunQueue();
        var eventService = new FakeAiTaskRunEventService();
        var recovery = new AiTaskRunRecoveryService(
            runRepository,
            taskRepository,
            queue,
            new FakeTimeProvider(now),
            eventService,
            NullLogger<AiTaskRunRecoveryService>.Instance);

        await recovery.RecoverAsync();

        Assert.Equal(42, Assert.Single(queue.RunIds));
        Assert.Equal(AiTaskRunStatus.Queued, runRepository.Runs[0].RunStatus);
        Assert.Null(runRepository.Runs[0].LeaseOwner);
        Assert.Null(runRepository.Runs[0].LeaseExpiresAt);
        Assert.Contains("重新排队", runRepository.Runs[0].ErrorMessage);
        Assert.Contains(eventService.Events, e => e.RunId == 42 && e.EventType == AiTaskRunEventType.RunRequeued);
    }

    [Fact]
    public async Task RecoverAsync_fails_expired_running_run_when_retry_budget_is_exhausted()
    {
        var now = DateTimeOffset.Parse("2026-07-09T16:00:00+08:00");
        var task = CreateTask(maxRetryCount: 0);
        var taskRepository = new InMemoryAiTaskRepository(task);
        var runRepository = new InMemoryAiTaskRunRepository();
        runRepository.Runs.Add(new SysAiTaskRun(42)
        {
            AiTaskId = task.BasicId,
            AiTaskCode = task.AiTaskCode,
            StartedTime = now.AddMinutes(-10),
            RunStatus = AiTaskRunStatus.Running,
            AttemptCount = 1,
            LeaseOwner = null,
            LeaseExpiresAt = null
        });
        var queue = new FakeAiTaskRunQueue();
        var eventService = new FakeAiTaskRunEventService();
        var recovery = new AiTaskRunRecoveryService(
            runRepository,
            taskRepository,
            queue,
            new FakeTimeProvider(now),
            eventService,
            NullLogger<AiTaskRunRecoveryService>.Instance);

        await recovery.RecoverAsync();

        Assert.Empty(queue.RunIds);
        Assert.Equal(AiTaskRunStatus.Failed, runRepository.Runs[0].RunStatus);
        Assert.NotNull(runRepository.Runs[0].EndedTime);
        Assert.Contains("执行器中断", runRepository.Runs[0].ErrorMessage);
        Assert.Contains(eventService.Events, e => e.RunId == 42 && e.EventType == AiTaskRunEventType.RunFailed);
    }

    private static SysAiTask CreateTask(int maxRetryCount)
    {
        return new SysAiTask(7)
        {
            AiTaskCode = "morning-news",
            AiTaskName = "Morning News",
            PromptMode = AiTaskPromptMode.Inline,
            PromptText = "Write a short brief.",
            Status = EnableStatus.Enabled,
            TimeoutSeconds = 300,
            MaxRetryCount = maxRetryCount
        };
    }
}
