#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskRunHistoryQueryTests
// Guid:f448e51b-b486-420a-8494-367b7fc58ae0
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/09 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Application.QueryServices;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Saas.Domain.Enums;
using Xunit;

namespace XiHan.BasicApp.AI.Tests;

public sealed class AiTaskRunHistoryQueryTests
{
    [Fact]
    public async Task GetRunListAsync_returns_task_runs_ordered_newest_first()
    {
        var runRepository = new InMemoryAiTaskRunRepository();
        runRepository.Runs.AddRange(
        [
            new SysAiTaskRun(1) { AiTaskId = 7, AiTaskCode = "morning-news", StartedTime = DateTimeOffset.Parse("2026-07-09T08:00:00+08:00"), RunStatus = AiTaskRunStatus.Success },
            new SysAiTaskRun(2) { AiTaskId = 8, AiTaskCode = "other", StartedTime = DateTimeOffset.Parse("2026-07-09T09:00:00+08:00"), RunStatus = AiTaskRunStatus.Success },
            new SysAiTaskRun(3) { AiTaskId = 7, AiTaskCode = "morning-news", StartedTime = DateTimeOffset.Parse("2026-07-09T10:00:00+08:00"), RunStatus = AiTaskRunStatus.Failed, ErrorMessage = "failed" }
        ]);
        var service = new AiTaskQueryService(
            new InMemoryAiTaskRepository(),
            new InMemoryAiTaskToolPolicyRepository([]),
            new InMemoryAiToolRepository(new SysAiTool(11) { ToolCode = "knowledge", ToolName = "Knowledge", Status = EnableStatus.Enabled }),
            runRepository);

        var runs = await service.GetRunListAsync(7);

        Assert.Equal([3L, 1L], runs.Select(run => run.BasicId).ToArray());
        Assert.Equal("failed", runs[0].ErrorMessage);
    }

    [Fact]
    public async Task GetRunDetailAsync_returns_prompt_snapshot_and_result()
    {
        var runRepository = new InMemoryAiTaskRunRepository();
        runRepository.Runs.Add(new SysAiTaskRun(3)
        {
            AiTaskId = 7,
            AiTaskCode = "morning-news",
            StartedTime = DateTimeOffset.Parse("2026-07-09T10:00:00+08:00"),
            EndedTime = DateTimeOffset.Parse("2026-07-09T10:00:01+08:00"),
            DurationMilliseconds = 1000,
            RunStatus = AiTaskRunStatus.Success,
            PromptSnapshot = "Write the daily summary.",
            ResultText = "Brief result"
        });
        var service = new AiTaskQueryService(
            new InMemoryAiTaskRepository(),
            new InMemoryAiTaskToolPolicyRepository([]),
            new InMemoryAiToolRepository(new SysAiTool(11) { ToolCode = "knowledge", ToolName = "Knowledge", Status = EnableStatus.Enabled }),
            runRepository);

        var detail = await service.GetRunDetailAsync(3);

        Assert.NotNull(detail);
        Assert.Equal("Write the daily summary.", detail.PromptSnapshot);
        Assert.Equal("Brief result", detail.ResultText);
    }
}
