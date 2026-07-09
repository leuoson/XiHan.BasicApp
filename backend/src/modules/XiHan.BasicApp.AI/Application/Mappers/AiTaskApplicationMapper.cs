#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskApplicationMapper
// Guid:91e625b7-02bb-4c34-a4fb-19de4469f5c2
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using System.Text.Json;
using XiHan.BasicApp.AI.Application.Contracts;
using XiHan.BasicApp.AI.Application.Dtos;
using XiHan.BasicApp.AI.Application.Services;
using XiHan.BasicApp.AI.Domain.DomainServices;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.AI.Infrastructure.Tasks;
using XiHan.BasicApp.Saas.Application.Dtos;
using XiHan.BasicApp.Saas.Domain.Entities;
using XiHan.BasicApp.Saas.Domain.Enums;

namespace XiHan.BasicApp.AI.Application.Mappers;

/// <summary>
/// AI 任务应用层映射器
/// </summary>
public static class AiTaskApplicationMapper
{
    /// <summary>
    /// AI 任务对应的系统任务分组
    /// </summary>
    public const string BackingTaskGroup = "ai-task";

    /// <summary>
    /// AI 任务对应的系统任务编码前缀
    /// </summary>
    public const string BackingTaskCodePrefix = "ai-task:";

    /// <summary>
    /// 映射为内部 SysTask 创建 DTO
    /// </summary>
    public static TaskCreateDto ToBackingTaskCreateDto(SysAiTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        return new TaskCreateDto
        {
            TaskCode = ToBackingTaskCode(task.AiTaskCode),
            TaskName = task.AiTaskName,
            TaskDescription = task.Description,
            TaskGroup = BackingTaskGroup,
            TaskClass = typeof(AiTaskJobExecutor).FullName ?? nameof(AiTaskJobExecutor),
            TaskMethod = nameof(AiTaskJobExecutor.ExecuteAsync),
            TaskParams = JsonSerializer.Serialize(new { aiTaskId = task.BasicId }),
            TriggerType = task.TriggerType,
            CronExpression = task.CronExpression,
            StartTime = task.StartTime,
            EndTime = task.EndTime,
            IntervalSeconds = task.IntervalSeconds,
            RepeatCount = task.RepeatCount,
            TimeoutSeconds = task.TimeoutSeconds,
            RunTaskStatus = RunTaskStatus.Pending,
            Priority = task.Priority,
            AllowConcurrent = task.AllowConcurrent,
            MaxRetryCount = task.MaxRetryCount,
            Status = task.Status,
            Remark = "AI 任务系统托管调度行，请在 AI 任务页面管理。"
        };
    }

    /// <summary>
    /// 映射为内部 SysTask 更新 DTO
    /// </summary>
    public static TaskUpdateDto ToBackingTaskUpdateDto(SysAiTask task, long backingTaskId)
    {
        ArgumentNullException.ThrowIfNull(task);

        var create = ToBackingTaskCreateDto(task);
        return new TaskUpdateDto
        {
            BasicId = backingTaskId,
            TaskName = create.TaskName,
            TaskDescription = create.TaskDescription,
            TaskGroup = create.TaskGroup,
            TaskClass = create.TaskClass,
            TaskMethod = create.TaskMethod,
            TaskParams = create.TaskParams,
            TriggerType = create.TriggerType,
            CronExpression = create.CronExpression,
            StartTime = create.StartTime,
            EndTime = create.EndTime,
            IntervalSeconds = create.IntervalSeconds,
            RepeatCount = create.RepeatCount,
            TimeoutSeconds = create.TimeoutSeconds,
            Priority = create.Priority,
            AllowConcurrent = create.AllowConcurrent,
            MaxRetryCount = create.MaxRetryCount,
            Remark = create.Remark
        };
    }

    /// <summary>
    /// 映射创建命令
    /// </summary>
    public static AiTaskCreateCommand ToCreateCommand(AiTaskCreateDto input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new AiTaskCreateCommand(
            input.AiTaskCode,
            input.AiTaskName,
            input.Description,
            input.Category,
            input.PromptMode,
            input.PromptText,
            input.PromptCode,
            input.PromptVersion,
            input.TriggerType,
            input.CronExpression,
            input.StartTime,
            input.EndTime,
            input.IntervalSeconds,
            input.RepeatCount,
            input.TimeoutSeconds,
            input.Priority,
            input.AllowConcurrent,
            input.MaxRetryCount,
            input.Sort,
            input.Status,
            input.ProviderId,
            input.ToolPolicies.Select(ToPolicyCommand).ToList(),
            input.Remark);
    }

    /// <summary>
    /// 映射更新命令
    /// </summary>
    public static AiTaskUpdateCommand ToUpdateCommand(AiTaskUpdateDto input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new AiTaskUpdateCommand(
            input.BasicId,
            input.AiTaskName,
            input.Description,
            input.Category,
            input.PromptMode,
            input.PromptText,
            input.PromptCode,
            input.PromptVersion,
            input.TriggerType,
            input.CronExpression,
            input.StartTime,
            input.EndTime,
            input.IntervalSeconds,
            input.RepeatCount,
            input.TimeoutSeconds,
            input.Priority,
            input.AllowConcurrent,
            input.MaxRetryCount,
            input.Sort,
            input.ProviderId,
            input.ToolPolicies.Select(ToPolicyCommand).ToList(),
            input.Remark);
    }

    /// <summary>
    /// 映射状态命令
    /// </summary>
    public static AiTaskStatusChangeCommand ToStatusCommand(AiTaskStatusUpdateDto input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new AiTaskStatusChangeCommand(input.BasicId, input.Status, input.Remark);
    }

    /// <summary>
    /// 映射策略命令
    /// </summary>
    private static AiTaskToolPolicyCommand ToPolicyCommand(AiTaskToolPolicyInputDto input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new AiTaskToolPolicyCommand(
            input.ToolId,
            input.Remark);
    }

    /// <summary>
    /// 实体映射为列表项 DTO
    /// </summary>
    public static AiTaskListItemDto ToListItemDto(SysAiTask entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new AiTaskListItemDto
        {
            BasicId = entity.BasicId,
            AiTaskCode = entity.AiTaskCode,
            AiTaskName = entity.AiTaskName,
            Description = entity.Description,
            Category = entity.Category,
            PromptMode = entity.PromptMode,
            ProviderId = entity.ProviderId,
            BackingTaskId = entity.BackingTaskId,
            TriggerType = entity.TriggerType,
            CronExpression = entity.CronExpression,
            IntervalSeconds = entity.IntervalSeconds,
            RepeatCount = entity.RepeatCount,
            TimeoutSeconds = entity.TimeoutSeconds,
            Priority = entity.Priority,
            AllowConcurrent = entity.AllowConcurrent,
            MaxRetryCount = entity.MaxRetryCount,
            Sort = entity.Sort,
            Status = entity.Status,
            CreatedTime = entity.CreatedTime,
            ModifiedTime = entity.ModifiedTime
        };
    }

    /// <summary>
    /// 实体映射为详情 DTO
    /// </summary>
    public static AiTaskDetailDto ToDetailDto(SysAiTask entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var item = ToListItemDto(entity);
        return new AiTaskDetailDto
        {
            BasicId = item.BasicId,
            AiTaskCode = item.AiTaskCode,
            AiTaskName = item.AiTaskName,
            Description = item.Description,
            Category = item.Category,
            PromptMode = item.PromptMode,
            ProviderId = item.ProviderId,
            BackingTaskId = item.BackingTaskId,
            TriggerType = item.TriggerType,
            CronExpression = item.CronExpression,
            IntervalSeconds = item.IntervalSeconds,
            RepeatCount = item.RepeatCount,
            TimeoutSeconds = item.TimeoutSeconds,
            Priority = item.Priority,
            AllowConcurrent = item.AllowConcurrent,
            MaxRetryCount = item.MaxRetryCount,
            Sort = item.Sort,
            Status = item.Status,
            CreatedTime = item.CreatedTime,
            ModifiedTime = item.ModifiedTime,
            PromptText = entity.PromptText,
            PromptCode = entity.PromptCode,
            PromptVersion = entity.PromptVersion,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            Remark = entity.Remark
        };
    }

    /// <summary>
    /// 策略实体映射为 DTO
    /// </summary>
    public static AiTaskToolPolicyDto ToToolPolicyDto(SysAiTaskToolPolicy policy, SysAiTool? tool)
    {
        ArgumentNullException.ThrowIfNull(policy);

        return new AiTaskToolPolicyDto
        {
            BasicId = policy.BasicId,
            ToolId = policy.ToolId,
            ToolCode = tool?.ToolCode ?? string.Empty,
            ToolName = tool?.ToolName ?? string.Empty,
            ToolType = tool?.ToolType ?? AiToolType.BuiltInSkill,
            RiskLevel = tool?.RiskLevel ?? AiToolRiskLevel.Low,
            SafetyLevel = tool?.SafetyLevel ?? AiToolSafetyLevel.ReadOnly,
            IsAvailable = tool?.Status == EnableStatus.Enabled,
            Remark = policy.Remark
        };
    }

    /// <summary>
    /// 映射执行结果
    /// </summary>
    public static AiTaskExecutionResultDto ToExecutionResultDto(AiTaskExecutionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new AiTaskExecutionResultDto
        {
            Succeeded = result.Succeeded,
            ResultText = result.ResultText,
            ErrorMessage = result.ErrorMessage
        };
    }

    /// <summary>
    /// 获取内部 SysTask 编码
    /// </summary>
    public static string ToBackingTaskCode(string aiTaskCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(aiTaskCode);
        return $"{BackingTaskCodePrefix}{aiTaskCode.Trim()}";
    }
}
