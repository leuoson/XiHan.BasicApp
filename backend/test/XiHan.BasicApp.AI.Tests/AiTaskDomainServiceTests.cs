#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskDomainServiceTests
// Guid:35370a25-a474-4cd1-ac51-9ada8f4381bb
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.DomainServices;
using XiHan.BasicApp.AI.Domain.DomainServices.Implementations;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.BasicApp.Saas.Domain.Entities;
using XiHan.BasicApp.Saas.Domain.Enums;
using Xunit;

namespace XiHan.BasicApp.AI.Tests;

public sealed class AiTaskDomainServiceTests
{
    [Fact]
    public async Task CreateTaskAsync_normalizes_code_and_rejects_duplicate_code()
    {
        var repository = new InMemoryAiTaskRepository(existingCode: "morning-news");
        var service = new AiTaskDomainService(repository);
        var command = new AiTaskCreateCommand(
            " morning-news ",
            "Morning News",
            "Daily news summary",
            "news",
            AiTaskPromptMode.Inline,
            "Summarize news",
            null,
            null,
            null,
            TriggerType.Cron,
            "0 0 9 * * ?",
            null,
            null,
            null,
            -1,
            300,
            0,
            false,
            3,
            10,
            EnableStatus.Enabled,
            10001,
            [],
            null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateTaskAsync(command));

        Assert.Equal("AI 任务编码已存在。", ex.Message);
    }
}
