#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskQueryService
// Guid:9c96d0a9-0a55-4f68-a3b3-88dbbd4a025f
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Application.Contracts;
using XiHan.BasicApp.AI.Application.Dtos;
using XiHan.BasicApp.AI.Application.Mappers;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Permissions;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.Framework.Application.Attributes;
using XiHan.Framework.Authorization.AspNetCore;

namespace XiHan.BasicApp.AI.Application.QueryServices;

/// <summary>
/// AI 任务查询应用服务
/// </summary>
[DynamicApi(Group = "BasicApp.AI", GroupName = "AI 服务", Tag = "AI 任务")]
public sealed class AiTaskQueryService : AiApplicationService, IAiTaskQueryService
{
    private readonly IAiTaskRepository _taskRepository;
    private readonly IAiTaskToolPolicyRepository _policyRepository;
    private readonly IAiToolRepository _toolRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskQueryService(
        IAiTaskRepository taskRepository,
        IAiTaskToolPolicyRepository policyRepository,
        IAiToolRepository toolRepository)
    {
        _taskRepository = taskRepository;
        _policyRepository = policyRepository;
        _toolRepository = toolRepository;
    }

    /// <inheritdoc />
    [PermissionAuthorize(AiTaskPermissionCodes.Read)]
    public async Task<IReadOnlyList<AiTaskListItemDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tasks = await _taskRepository.GetListAsync(cancellationToken);
        if (tasks.Count == 0)
        {
            return [];
        }

        var taskIds = tasks.Select(task => task.BasicId).ToList();
        var policies = await _policyRepository.GetByTaskIdsAsync(taskIds, cancellationToken);
        var policyCounts = policies
            .Where(policy => policy.IsEnabled)
            .GroupBy(policy => policy.AiTaskId)
            .ToDictionary(group => group.Key, group => group.Count());

        return tasks.Select(task =>
        {
            var item = AiTaskApplicationMapper.ToListItemDto(task);
            item.CapabilityCount = policyCounts.GetValueOrDefault(task.BasicId);
            return item;
        }).ToList();
    }

    /// <inheritdoc />
    [PermissionAuthorize(AiTaskPermissionCodes.Read)]
    public async Task<AiTaskDetailDto?> GetDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "AI 任务主键必须大于 0。");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return null;
        }

        var policies = await _policyRepository.GetByTaskIdAsync(task.BasicId, cancellationToken);
        var toolById = await LoadToolMapAsync(policies.Select(policy => policy.ToolId).Distinct().ToList(), cancellationToken);
        var detail = AiTaskApplicationMapper.ToDetailDto(task);
        detail.CapabilityCount = policies.Count(policy => policy.IsEnabled);
        detail.ToolPolicies = policies.Select(policy => AiTaskApplicationMapper.ToToolPolicyDto(policy, toolById.GetValueOrDefault(policy.ToolId))).ToList();
        return detail;
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
}
