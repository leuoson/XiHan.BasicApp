#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskRuntimeModels
// Guid:a0c2475e-2e78-4608-a36a-79e8f9f68f8c
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;

namespace XiHan.BasicApp.AI.Application.Services;

/// <summary>
/// AI 任务运行上下文
/// </summary>
public sealed record AiTaskRunContext(
    SysAiTask Task,
    SysAiTaskRun Run,
    string Prompt,
    IReadOnlyList<SysAiTaskRunGuidance> PendingGuidance);

/// <summary>
/// AI 任务运行器结果
/// </summary>
public sealed record AiTaskRunnerResult(
    string? ResultText,
    string? AgentSessionId = null);
