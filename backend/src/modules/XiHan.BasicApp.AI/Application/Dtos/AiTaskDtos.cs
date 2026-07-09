#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskDtos
// Guid:5b68066e-511f-4e86-8eac-801b78cbe57e
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

#pragma warning disable CS1591

using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Core.Dtos;
using XiHan.BasicApp.Saas.Domain.Entities;
using XiHan.BasicApp.Saas.Domain.Enums;

namespace XiHan.BasicApp.AI.Application.Dtos;

public sealed class AiTaskCreateDto
{
    public string AiTaskCode { get; set; } = string.Empty;
    public string AiTaskName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public AiTaskPromptMode PromptMode { get; set; } = AiTaskPromptMode.Inline;
    public string? PromptText { get; set; }
    public string? PromptCode { get; set; }
    public string? PromptVersion { get; set; }
    public string? InputVariablesJson { get; set; }
    public long? ProviderId { get; set; }
    public TriggerType TriggerType { get; set; } = TriggerType.Cron;
    public string? CronExpression { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public int? IntervalSeconds { get; set; }
    public int RepeatCount { get; set; } = -1;
    public int TimeoutSeconds { get; set; } = 300;
    public int Priority { get; set; }
    public bool AllowConcurrent { get; set; }
    public int MaxRetryCount { get; set; } = 3;
    public int Sort { get; set; }
    public EnableStatus Status { get; set; } = EnableStatus.Enabled;
    public List<AiTaskToolPolicyInputDto> ToolPolicies { get; set; } = [];
    public string? Remark { get; set; }
}

public sealed class AiTaskUpdateDto : BasicAppUDto
{
    public string AiTaskName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public AiTaskPromptMode PromptMode { get; set; } = AiTaskPromptMode.Inline;
    public string? PromptText { get; set; }
    public string? PromptCode { get; set; }
    public string? PromptVersion { get; set; }
    public string? InputVariablesJson { get; set; }
    public long? ProviderId { get; set; }
    public TriggerType TriggerType { get; set; } = TriggerType.Cron;
    public string? CronExpression { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public int? IntervalSeconds { get; set; }
    public int RepeatCount { get; set; } = -1;
    public int TimeoutSeconds { get; set; } = 300;
    public int Priority { get; set; }
    public bool AllowConcurrent { get; set; }
    public int MaxRetryCount { get; set; } = 3;
    public int Sort { get; set; }
    public List<AiTaskToolPolicyInputDto> ToolPolicies { get; set; } = [];
    public string? Remark { get; set; }
}

public sealed class AiTaskStatusUpdateDto : BasicAppDto
{
    public EnableStatus Status { get; set; } = EnableStatus.Enabled;
    public string? Remark { get; set; }
}

public sealed class AiTaskRunDto : BasicAppDto
{
}

public sealed class AiTaskToolPolicyInputDto
{
    public long ToolId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? Remark { get; set; }
}

public sealed class AiTaskToolPolicyDto : BasicAppDto
{
    public long ToolId { get; set; }
    public string ToolCode { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public AiToolType ToolType { get; set; }
    public AiToolRiskLevel RiskLevel { get; set; }
    public AiToolSafetyLevel SafetyLevel { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsAvailable { get; set; }
    public string? Remark { get; set; }
}

public class AiTaskListItemDto : BasicAppDto
{
    public string AiTaskCode { get; set; } = string.Empty;
    public string AiTaskName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public AiTaskPromptMode PromptMode { get; set; }
    public long? ProviderId { get; set; }
    public long? BackingTaskId { get; set; }
    public TriggerType TriggerType { get; set; }
    public string? CronExpression { get; set; }
    public int? IntervalSeconds { get; set; }
    public int RepeatCount { get; set; }
    public int TimeoutSeconds { get; set; }
    public int Priority { get; set; }
    public bool AllowConcurrent { get; set; }
    public int MaxRetryCount { get; set; }
    public int Sort { get; set; }
    public int CapabilityCount { get; set; }
    public EnableStatus Status { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset? ModifiedTime { get; set; }
}

public sealed class AiTaskDetailDto : AiTaskListItemDto
{
    public string? PromptText { get; set; }
    public string? PromptCode { get; set; }
    public string? PromptVersion { get; set; }
    public string? InputVariablesJson { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public List<AiTaskToolPolicyDto> ToolPolicies { get; set; } = [];
    public string? Remark { get; set; }
}
