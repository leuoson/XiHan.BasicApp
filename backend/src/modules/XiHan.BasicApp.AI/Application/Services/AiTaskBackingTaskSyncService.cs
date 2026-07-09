#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskBackingTaskSyncService
// Guid:e89a0802-b90a-48ca-ae23-14f240f43283
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Application.Mappers;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.BasicApp.Saas.Application.Mappers;
using XiHan.BasicApp.Saas.Application.Services;
using XiHan.BasicApp.Saas.Domain.DomainServices;
using XiHan.BasicApp.Saas.Domain.Repositories;

namespace XiHan.BasicApp.AI.Application.Services;

/// <summary>
/// AI 任务内部 SysTask 同步服务
/// </summary>
public sealed class AiTaskBackingTaskSyncService : IAiTaskBackingTaskSyncService
{
    private readonly IAiTaskRepository _aiTaskRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskDomainService _taskDomainService;
    private readonly ITaskSchedulerSyncService _schedulerSyncService;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskBackingTaskSyncService(
        IAiTaskRepository aiTaskRepository,
        ITaskRepository taskRepository,
        ITaskDomainService taskDomainService,
        ITaskSchedulerSyncService schedulerSyncService)
    {
        _aiTaskRepository = aiTaskRepository;
        _taskRepository = taskRepository;
        _taskDomainService = taskDomainService;
        _schedulerSyncService = schedulerSyncService;
    }

    /// <inheritdoc />
    public async Task<SysAiTask> SyncAsync(SysAiTask task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        cancellationToken.ThrowIfCancellationRequested();

        if (task.BackingTaskId.HasValue && await _taskRepository.GetByIdAsync(task.BackingTaskId.Value, cancellationToken) is not null)
        {
            var updateResult = await _taskDomainService.UpdateTaskAsync(
                TaskApplicationMapper.ToUpdateCommand(AiTaskApplicationMapper.ToBackingTaskUpdateDto(task, task.BackingTaskId.Value)),
                cancellationToken);
            _schedulerSyncService.Apply(updateResult.Task, updateResult.SchedulerSyncAction);
            return task;
        }

        var createResult = await _taskDomainService.CreateTaskAsync(
            TaskApplicationMapper.ToCreateCommand(AiTaskApplicationMapper.ToBackingTaskCreateDto(task)),
            cancellationToken);
        _schedulerSyncService.Apply(createResult.Task, createResult.SchedulerSyncAction);

        task.BackingTaskId = createResult.Task.BasicId;
        return await _aiTaskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SyncStatusAsync(SysAiTask task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        cancellationToken.ThrowIfCancellationRequested();

        if (!task.BackingTaskId.HasValue)
        {
            _ = await SyncAsync(task, cancellationToken);
            return;
        }

        var result = await _taskDomainService.UpdateTaskStatusAsync(new TaskStatusChangeCommand(task.BackingTaskId.Value, task.Status, task.Remark), cancellationToken);
        _schedulerSyncService.Apply(result.Task, result.SchedulerSyncAction);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(SysAiTask task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        cancellationToken.ThrowIfCancellationRequested();

        if (!task.BackingTaskId.HasValue)
        {
            return;
        }

        var backingTask = await _taskRepository.GetByIdAsync(task.BackingTaskId.Value, cancellationToken);
        if (backingTask is null)
        {
            return;
        }

        var result = await _taskDomainService.DeleteTaskAsync(task.BackingTaskId.Value, cancellationToken);
        _schedulerSyncService.Apply(result.Task, result.SchedulerSyncAction);
    }
}
