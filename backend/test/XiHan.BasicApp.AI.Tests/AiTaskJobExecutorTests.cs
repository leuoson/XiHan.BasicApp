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
        var executor = new AiTaskExecutor(repository, runRepository, chat, new AiTaskPromptRenderer());

        var result = await executor.ExecuteAsync(7);

        Assert.True(result.Succeeded);
        Assert.Single(runRepository.Runs);
        Assert.Equal(AiTaskRunStatus.Success, runRepository.Runs[0].RunStatus);
        Assert.Equal("Brief result", runRepository.Runs[0].ResultText);
        Assert.Equal("Write a short brief.", runRepository.Runs[0].PromptSnapshot);
    }
}
