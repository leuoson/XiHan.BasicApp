#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskCommandModels
// Guid:e7a0f85e-ce6c-49e7-8cfe-d3f04f48ce24
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Saas.Domain.Entities;
using XiHan.BasicApp.Saas.Domain.Enums;

namespace XiHan.BasicApp.AI.Domain.DomainServices;

/// <summary>
/// AI 任务创建命令
/// </summary>
public sealed record AiTaskCreateCommand(
    string AiTaskCode,
    string AiTaskName,
    string? Description,
    string? Category,
    AiTaskPromptMode PromptMode,
    string? PromptText,
    string? PromptCode,
    string? PromptVersion,
    TriggerType TriggerType,
    string? CronExpression,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    int? IntervalSeconds,
    int RepeatCount,
    int TimeoutSeconds,
    int Priority,
    bool AllowConcurrent,
    int MaxRetryCount,
    int Sort,
    EnableStatus Status,
    long? ProviderId,
    IReadOnlyList<AiTaskToolPolicyCommand> ToolPolicies,
    string? Remark);

/// <summary>
/// AI 任务更新命令
/// </summary>
public sealed record AiTaskUpdateCommand(
    long BasicId,
    string AiTaskName,
    string? Description,
    string? Category,
    AiTaskPromptMode PromptMode,
    string? PromptText,
    string? PromptCode,
    string? PromptVersion,
    TriggerType TriggerType,
    string? CronExpression,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    int? IntervalSeconds,
    int RepeatCount,
    int TimeoutSeconds,
    int Priority,
    bool AllowConcurrent,
    int MaxRetryCount,
    int Sort,
    long? ProviderId,
    IReadOnlyList<AiTaskToolPolicyCommand> ToolPolicies,
    string? Remark);

/// <summary>
/// AI 任务技能策略命令
/// </summary>
public sealed record AiTaskToolPolicyCommand(
    long ToolId,
    string? Remark);

/// <summary>
/// AI 任务状态变更命令
/// </summary>
public sealed record AiTaskStatusChangeCommand(long BasicId, EnableStatus Status, string? Remark);

/// <summary>
/// AI 任务命令结果
/// </summary>
public sealed record AiTaskCommandResult(SysAiTask Task);
