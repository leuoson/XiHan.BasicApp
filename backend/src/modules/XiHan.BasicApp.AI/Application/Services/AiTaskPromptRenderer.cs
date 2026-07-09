#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskPromptRenderer
// Guid:45782f13-7480-47de-9a8d-c818452cbd54
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;

namespace XiHan.BasicApp.AI.Application.Services;

/// <summary>
/// AI 任务提示词渲染器
/// </summary>
public sealed class AiTaskPromptRenderer
{
    /// <summary>
    /// 渲染任务提示词
    /// </summary>
    public string Render(SysAiTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        return task.PromptMode switch
        {
            AiTaskPromptMode.Inline when !string.IsNullOrWhiteSpace(task.PromptText) => task.PromptText,
            AiTaskPromptMode.PromptStore => throw new NotSupportedException("提示词库模式将在接入提示词渲染上下文后启用。"),
            _ => throw new InvalidOperationException("AI 任务提示词不能为空。")
        };
    }
}
