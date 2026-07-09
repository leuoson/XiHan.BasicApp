#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskAppService
// Guid:2a2b476e-142c-489a-89c8-0d579922f1aa
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Application.Contracts;
using XiHan.BasicApp.AI.Application.Dtos;
using XiHan.BasicApp.AI.Application.Mappers;
using XiHan.BasicApp.AI.Application.Services;
using XiHan.BasicApp.AI.Domain.DomainServices;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.AI.Domain.Permissions;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.BasicApp.Saas.Domain.Enums;
using XiHan.Framework.Application.Attributes;
using XiHan.Framework.Authorization.AspNetCore;
using XiHan.Framework.Uow.Attributes;

namespace XiHan.BasicApp.AI.Application.AppServices;

/// <summary>
/// AI 任务命令应用服务
/// </summary>
[DynamicApi(Group = "BasicApp.AI", GroupName = "AI 服务", Tag = "AI 任务")]
public sealed class AiTaskAppService : AiApplicationService, IAiTaskAppService
{
    private readonly IAiTaskDomainService _taskDomainService;
    private readonly IAiTaskRepository _taskRepository;
    private readonly IAiTaskToolPolicyRepository _policyRepository;
    private readonly IAiToolRepository _toolRepository;
    private readonly IAiTaskBackingTaskSyncService _backingTaskSyncService;
    private readonly AiTaskExecutor _executor;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskAppService(
        IAiTaskDomainService taskDomainService,
        IAiTaskRepository taskRepository,
        IAiTaskToolPolicyRepository policyRepository,
        IAiToolRepository toolRepository,
        IAiTaskBackingTaskSyncService backingTaskSyncService,
        AiTaskExecutor executor)
    {
        _taskDomainService = taskDomainService;
        _taskRepository = taskRepository;
        _policyRepository = policyRepository;
        _toolRepository = toolRepository;
        _backingTaskSyncService = backingTaskSyncService;
        _executor = executor;
    }

    /// <inheritdoc />
    [UnitOfWork(true)]
    [PermissionAuthorize(AiTaskPermissionCodes.Create)]
    public async Task<AiTaskDetailDto> CreateAsync(AiTaskCreateDto input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var command = AiTaskApplicationMapper.ToCreateCommand(input);
        var result = await _taskDomainService.CreateTaskAsync(command, cancellationToken);
        var syncedTask = await _backingTaskSyncService.SyncAsync(result.Task, cancellationToken);
        return await SaveAndMapDetailAsync(syncedTask, command.ToolPolicies, cancellationToken);
    }

    /// <inheritdoc />
    [UnitOfWork(true)]
    [PermissionAuthorize(AiTaskPermissionCodes.Update)]
    public async Task<AiTaskDetailDto> UpdateAsync(AiTaskUpdateDto input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var command = AiTaskApplicationMapper.ToUpdateCommand(input);
        var result = await _taskDomainService.UpdateTaskAsync(command, cancellationToken);
        var syncedTask = await _backingTaskSyncService.SyncAsync(result.Task, cancellationToken);
        return await SaveAndMapDetailAsync(syncedTask, command.ToolPolicies, cancellationToken);
    }

    /// <inheritdoc />
    [UnitOfWork(true)]
    [PermissionAuthorize(AiTaskPermissionCodes.Update)]
    public async Task<AiTaskDetailDto> UpdateStatusAsync(AiTaskStatusUpdateDto input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _taskDomainService.UpdateTaskStatusAsync(AiTaskApplicationMapper.ToStatusCommand(input), cancellationToken);
        await _backingTaskSyncService.SyncStatusAsync(result.Task, cancellationToken);
        return AiTaskApplicationMapper.ToDetailDto(result.Task);
    }

    /// <inheritdoc />
    [UnitOfWork(true)]
    [PermissionAuthorize(AiTaskPermissionCodes.Delete)]
    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var task = await _taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("AI 任务不存在。");
        await _backingTaskSyncService.DeleteAsync(task, cancellationToken);
        _ = await _taskDomainService.DeleteTaskAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    [PermissionAuthorize(AiTaskPermissionCodes.Execute)]
    public async Task<AiTaskExecutionResultDto> RunAsync(AiTaskRunDto input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _executor.ExecuteAsync(input.BasicId, cancellationToken);
        return AiTaskApplicationMapper.ToExecutionResultDto(result);
    }

    private async Task<AiTaskDetailDto> SaveAndMapDetailAsync(SysAiTask task, IReadOnlyList<AiTaskToolPolicyCommand> commands, CancellationToken cancellationToken)
    {
        var policies = await SaveToolPoliciesAsync(task.BasicId, commands, cancellationToken);
        var toolById = await LoadToolMapAsync(policies.Select(policy => policy.ToolId).Distinct().ToList(), cancellationToken);
        var detail = AiTaskApplicationMapper.ToDetailDto(task);
        detail.ToolPolicies = policies.Select(policy => AiTaskApplicationMapper.ToToolPolicyDto(policy, toolById.GetValueOrDefault(policy.ToolId))).ToList();
        return detail;
    }

    private async Task<IReadOnlyList<SysAiTaskToolPolicy>> SaveToolPoliciesAsync(long aiTaskId, IReadOnlyList<AiTaskToolPolicyCommand> commands, CancellationToken cancellationToken)
    {
        var policies = new List<SysAiTaskToolPolicy>();
        var seenToolIds = new HashSet<long>();
        foreach (var command in commands.Where(policy => policy.IsEnabled))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (command.ToolId <= 0)
            {
                throw new InvalidOperationException("AI 技能主键必须大于 0。");
            }

            if (!seenToolIds.Add(command.ToolId))
            {
                throw new InvalidOperationException("AI 技能不能重复绑定。");
            }

            var tool = await _toolRepository.GetByIdAsync(command.ToolId, cancellationToken)
                ?? throw new InvalidOperationException("AI 技能不存在。");
            if (tool.Status != EnableStatus.Enabled)
            {
                throw new InvalidOperationException("禁用的 AI 技能不能绑定到任务。");
            }

            policies.Add(new SysAiTaskToolPolicy
            {
                AiTaskId = aiTaskId,
                ToolId = command.ToolId,
                AccessMode = AiTaskToolAccessMode.Allow,
                MaxCalls = tool.DefaultMaxCalls,
                ArgumentPolicyJson = null,
                IsEnabled = command.IsEnabled,
                Remark = Normalize(command.Remark, 500, "备注不能超过 500 个字符。")
            });
        }

        await _policyRepository.ReplaceByTaskIdAsync(aiTaskId, policies, cancellationToken);
        return policies;
    }

    private async Task<Dictionary<long, SysAiTool>> LoadToolMapAsync(IReadOnlyList<long> toolIds, CancellationToken cancellationToken)
    {
        var result = new Dictionary<long, SysAiTool>();
        foreach (var toolId in toolIds)
        {
            var tool = await _toolRepository.GetByIdAsync(toolId, cancellationToken);
            if (tool is not null)
            {
                result[toolId] = tool;
            }
        }

        return result;
    }

    private static string? Normalize(string? value, int maxLength, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new InvalidOperationException(message);
        }

        return trimmed;
    }
}
