#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:IAiTaskAgentRunner
// Guid:fc1ff673-916c-4a76-af78-564cbf0b4372
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Enums;

namespace XiHan.BasicApp.AI.Application.Services;

/// <summary>
/// AI 任务 Agent 运行器接口
/// </summary>
public interface IAiTaskAgentRunner
{
    /// <summary>
    /// 运行器类型
    /// </summary>
    AiTaskRunnerKind Kind { get; }

    /// <summary>
    /// 运行器版本
    /// </summary>
    string Version { get; }

    /// <summary>
    /// 执行运行上下文
    /// </summary>
    Task<AiTaskRunnerResult> RunAsync(AiTaskRunContext context, CancellationToken cancellationToken = default);
}
