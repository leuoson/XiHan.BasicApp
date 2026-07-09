#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:SaasTaskSystemManagedGuardTests
// Guid:7a662325-c9ae-4870-a182-73ed81a759b2
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.Saas.Application.AppServices;
using XiHan.BasicApp.Saas.Domain.Entities;
using XiHan.BasicApp.Saas.Domain.Enums;
using XiHan.Framework.Core.Exceptions;
using Xunit;

namespace XiHan.BasicApp.AI.Tests;

public sealed class SaasTaskSystemManagedGuardTests
{
    [Fact]
    public void EnsureNotSystemManagedAiTask_rejects_ai_backing_task()
    {
        var task = new SysTask
        {
            TaskCode = "ai-task:morning-news",
            TaskGroup = "ai-task",
            Status = EnableStatus.Enabled
        };

        var ex = Assert.Throws<UserFriendlyException>(() => TaskAppService.EnsureNotSystemManagedAiTask(task));

        Assert.Equal("AI 任务的系统调度行请在 AI 任务页面管理。", ex.Message);
    }
}
