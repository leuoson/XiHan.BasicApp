#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskDomainService
// Guid:cb06c931-319c-4340-b17d-a0d81c52ee70
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.BasicApp.Saas.Domain.Entities;
using XiHan.BasicApp.Saas.Domain.Enums;

namespace XiHan.BasicApp.AI.Domain.DomainServices.Implementations;

/// <summary>
/// AI 任务领域服务实现
/// </summary>
public sealed class AiTaskDomainService : IAiTaskDomainService
{
    private readonly IAiTaskRepository _taskRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskDomainService(IAiTaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    /// <inheritdoc />
    public async Task<AiTaskCommandResult> CreateTaskAsync(AiTaskCreateCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();

        EnsureEnum(command.PromptMode, nameof(command.PromptMode));
        EnsureEnum(command.TriggerType, nameof(command.TriggerType));
        EnsureEnum(command.Status, nameof(command.Status));
        EnsureSchedule(command.TriggerType, command.CronExpression, command.StartTime, command.EndTime, command.IntervalSeconds, command.RepeatCount, command.TimeoutSeconds, command.MaxRetryCount);

        var taskCode = Required(command.AiTaskCode, 100, nameof(command.AiTaskCode), "AI 任务编码不能超过 100 个字符。");
        EnsureCodeHasNoWhitespace(taskCode, "AI 任务编码不能包含空白字符。");
        if (await _taskRepository.ExistsCodeAsync(taskCode, null, cancellationToken))
        {
            throw new InvalidOperationException("AI 任务编码已存在。");
        }

        var task = new SysAiTask
        {
            AiTaskCode = taskCode,
            AiTaskName = Required(command.AiTaskName, 200, nameof(command.AiTaskName), "AI 任务名称不能超过 200 个字符。"),
            Description = Optional(command.Description, 500, nameof(command.Description), "AI 任务描述不能超过 500 个字符。"),
            Category = Optional(command.Category, 100, nameof(command.Category), "分类不能超过 100 个字符。"),
            PromptMode = command.PromptMode,
            PromptText = NormalizePromptText(command.PromptMode, command.PromptText),
            PromptCode = NormalizePromptCode(command.PromptMode, command.PromptCode),
            PromptVersion = Optional(command.PromptVersion, 100, nameof(command.PromptVersion), "提示词版本不能超过 100 个字符。"),
            ProviderId = command.ProviderId,
            TriggerType = command.TriggerType,
            CronExpression = Optional(command.CronExpression, 100, nameof(command.CronExpression), "Cron 表达式不能超过 100 个字符。"),
            StartTime = command.StartTime,
            EndTime = command.EndTime,
            IntervalSeconds = command.IntervalSeconds,
            RepeatCount = command.RepeatCount,
            TimeoutSeconds = command.TimeoutSeconds,
            Priority = command.Priority,
            AllowConcurrent = command.AllowConcurrent,
            MaxRetryCount = command.MaxRetryCount,
            Sort = command.Sort,
            Status = command.Status,
            Remark = Optional(command.Remark, 500, nameof(command.Remark), "备注不能超过 500 个字符。")
        };

        return new AiTaskCommandResult(await _taskRepository.AddAsync(task, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<AiTaskCommandResult> UpdateTaskAsync(AiTaskUpdateCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();

        EnsureId(command.BasicId, "AI 任务主键必须大于 0。");
        EnsureEnum(command.PromptMode, nameof(command.PromptMode));
        EnsureEnum(command.TriggerType, nameof(command.TriggerType));
        EnsureSchedule(command.TriggerType, command.CronExpression, command.StartTime, command.EndTime, command.IntervalSeconds, command.RepeatCount, command.TimeoutSeconds, command.MaxRetryCount);

        var task = await GetTaskOrThrowAsync(command.BasicId, cancellationToken);
        task.AiTaskName = Required(command.AiTaskName, 200, nameof(command.AiTaskName), "AI 任务名称不能超过 200 个字符。");
        task.Description = Optional(command.Description, 500, nameof(command.Description), "AI 任务描述不能超过 500 个字符。");
        task.Category = Optional(command.Category, 100, nameof(command.Category), "分类不能超过 100 个字符。");
        task.PromptMode = command.PromptMode;
        task.PromptText = NormalizePromptText(command.PromptMode, command.PromptText);
        task.PromptCode = NormalizePromptCode(command.PromptMode, command.PromptCode);
        task.PromptVersion = Optional(command.PromptVersion, 100, nameof(command.PromptVersion), "提示词版本不能超过 100 个字符。");
        task.ProviderId = command.ProviderId;
        task.TriggerType = command.TriggerType;
        task.CronExpression = Optional(command.CronExpression, 100, nameof(command.CronExpression), "Cron 表达式不能超过 100 个字符。");
        task.StartTime = command.StartTime;
        task.EndTime = command.EndTime;
        task.IntervalSeconds = command.IntervalSeconds;
        task.RepeatCount = command.RepeatCount;
        task.TimeoutSeconds = command.TimeoutSeconds;
        task.Priority = command.Priority;
        task.AllowConcurrent = command.AllowConcurrent;
        task.MaxRetryCount = command.MaxRetryCount;
        task.Sort = command.Sort;
        task.Remark = Optional(command.Remark, 500, nameof(command.Remark), "备注不能超过 500 个字符。");

        return new AiTaskCommandResult(await _taskRepository.UpdateAsync(task, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<AiTaskCommandResult> UpdateTaskStatusAsync(AiTaskStatusChangeCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();

        EnsureId(command.BasicId, "AI 任务主键必须大于 0。");
        EnsureEnum(command.Status, nameof(command.Status));

        var task = await GetTaskOrThrowAsync(command.BasicId, cancellationToken);
        task.Status = command.Status;
        task.Remark = Optional(command.Remark, 500, nameof(command.Remark), "备注不能超过 500 个字符。") ?? task.Remark;

        return new AiTaskCommandResult(await _taskRepository.UpdateAsync(task, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<AiTaskCommandResult> DeleteTaskAsync(long id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var task = await GetTaskOrThrowAsync(id, cancellationToken);
        if (!await _taskRepository.DeleteAsync(task, cancellationToken))
        {
            throw new InvalidOperationException("AI 任务删除失败。");
        }

        return new AiTaskCommandResult(task);
    }

    private static void EnsureCodeHasNoWhitespace(string value, string message)
    {
        if (value.Any(char.IsWhiteSpace))
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void EnsureEnum<TEnum>(TEnum value, string paramName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(paramName, "枚举值无效。");
        }
    }

    private static void EnsureId(long id, string message)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), message);
        }
    }

    private static void EnsureSchedule(
        TriggerType triggerType,
        string? cronExpression,
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        int? intervalSeconds,
        int repeatCount,
        int timeoutSeconds,
        int maxRetryCount)
    {
        if (startTime.HasValue && endTime.HasValue && endTime.Value <= startTime.Value)
        {
            throw new InvalidOperationException("结束时间必须晚于开始时间。");
        }

        if (triggerType == TriggerType.Schedule && !startTime.HasValue)
        {
            throw new InvalidOperationException("一次性定时任务必须设置执行时间。");
        }

        if (triggerType == TriggerType.Cron && string.IsNullOrWhiteSpace(cronExpression))
        {
            throw new InvalidOperationException("Cron 表达式不能为空。");
        }

        if (triggerType == TriggerType.Recurring && (!intervalSeconds.HasValue || intervalSeconds <= 0))
        {
            throw new InvalidOperationException("循环执行任务必须设置大于 0 的间隔秒数。");
        }

        if (repeatCount < -1)
        {
            throw new InvalidOperationException("重复次数不能小于 -1。");
        }

        if (timeoutSeconds <= 0)
        {
            throw new InvalidOperationException("超时时间必须大于 0。");
        }

        if (maxRetryCount < 0)
        {
            throw new InvalidOperationException("最大重试次数不能小于 0。");
        }
    }

    private static string? NormalizePromptCode(AiTaskPromptMode promptMode, string? promptCode)
    {
        return promptMode == AiTaskPromptMode.PromptStore
            ? Required(promptCode, 100, nameof(promptCode), "提示词编码不能超过 100 个字符。")
            : Optional(promptCode, 100, nameof(promptCode), "提示词编码不能超过 100 个字符。");
    }

    private static string? NormalizePromptText(AiTaskPromptMode promptMode, string? promptText)
    {
        if (promptMode == AiTaskPromptMode.Inline && string.IsNullOrWhiteSpace(promptText))
        {
            throw new InvalidOperationException("内联提示词不能为空。");
        }

        return string.IsNullOrWhiteSpace(promptText) ? null : promptText;
    }

    private static string? Optional(string? value, int maxLength, string paramName, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(paramName, message);
        }

        return normalized;
    }

    private static string Required(string? value, int maxLength, string paramName, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(paramName, message);
        }

        return normalized;
    }

    private async Task<SysAiTask> GetTaskOrThrowAsync(long id, CancellationToken cancellationToken)
    {
        EnsureId(id, "AI 任务主键必须大于 0。");
        return await _taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("AI 任务不存在。");
    }
}
