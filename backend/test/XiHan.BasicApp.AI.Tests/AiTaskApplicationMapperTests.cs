#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskApplicationMapperTests
// Guid:ab4eaca3-82f7-4c96-983e-bae11b3e1e7d
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Application.Dtos;
using XiHan.BasicApp.AI.Application.Mappers;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.AI.Infrastructure.Tasks;
using XiHan.BasicApp.Saas.Domain.Entities;
using XiHan.BasicApp.Saas.Domain.Enums;
using Xunit;

namespace XiHan.BasicApp.AI.Tests;

public sealed class AiTaskApplicationMapperTests
{
    [Fact]
    public void ToCreateCommand_maps_capability_policy_inputs()
    {
        var input = new AiTaskCreateDto
        {
            AiTaskCode = "morning-news",
            AiTaskName = "Morning News",
            PromptMode = AiTaskPromptMode.Inline,
            PromptText = "Summarize news",
            TriggerType = TriggerType.Cron,
            CronExpression = "0 0 9 * * ?",
            RepeatCount = -1,
            TimeoutSeconds = 300,
            MaxRetryCount = 3,
            Status = EnableStatus.Enabled,
            ToolPolicies =
            [
                new AiTaskToolPolicyInputDto
                {
                    ToolId = 11,
                    IsEnabled = true,
                    Remark = "Allow knowledge"
                }
            ]
        };

        var command = AiTaskApplicationMapper.ToCreateCommand(input);

        Assert.Single(command.ToolPolicies);
        Assert.Equal(11, command.ToolPolicies[0].ToolId);
        Assert.True(command.ToolPolicies[0].IsEnabled);
    }

    [Fact]
    public void ToBackingTaskCreateDto_sets_system_managed_ai_task_shape()
    {
        var task = new SysAiTask(321)
        {
            AiTaskCode = "morning-news",
            AiTaskName = "Morning News",
            Description = "Daily news",
            TriggerType = TriggerType.Cron,
            CronExpression = "0 0 9 * * ?",
            RepeatCount = -1,
            TimeoutSeconds = 300,
            Priority = 10,
            AllowConcurrent = false,
            MaxRetryCount = 3,
            Status = EnableStatus.Enabled
        };

        var dto = AiTaskApplicationMapper.ToBackingTaskCreateDto(task);

        Assert.Equal("ai-task:morning-news", dto.TaskCode);
        Assert.Equal("ai-task", dto.TaskGroup);
        Assert.Equal(typeof(AiTaskJobExecutor).FullName, dto.TaskClass);
        Assert.Equal(nameof(AiTaskJobExecutor.ExecuteAsync), dto.TaskMethod);
        Assert.Contains("\"aiTaskId\":321", dto.TaskParams);
    }
}
