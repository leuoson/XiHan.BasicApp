#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskTestDoubles
// Guid:d505df88-dffa-40d4-a184-de01252342ca
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Application.Services;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.BasicApp.Core.Dtos;
using XiHan.BasicApp.Saas.Domain.Enums;
using XiHan.Framework.Domain.Shared.Paging.Dtos;
using XiHan.Framework.Domain.Shared.Paging.Models;

namespace XiHan.BasicApp.AI.Tests;

internal sealed class InMemoryAiTaskRepository : IAiTaskRepository
{
    private readonly Dictionary<long, SysAiTask> _tasksById = [];
    private readonly HashSet<string> _existingCodes = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryAiTaskRepository(string? existingCode = null)
    {
        if (!string.IsNullOrWhiteSpace(existingCode))
        {
            _existingCodes.Add(existingCode.Trim());
        }
    }

    public InMemoryAiTaskRepository(SysAiTask task)
    {
        _tasksById[task.BasicId] = task;
        _existingCodes.Add(task.AiTaskCode);
    }

    public Task<SysAiTask> AddAsync(SysAiTask entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = entity.BasicId > 0 ? entity.BasicId : _tasksById.Count + 1;
        _tasksById[key] = entity;
        _existingCodes.Add(entity.AiTaskCode);
        return Task.FromResult(entity);
    }

    public Task<bool> DeleteAsync(SysAiTask entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_tasksById.Remove(entity.BasicId));
    }

    public Task<bool> ExistsCodeAsync(string aiTaskCode, long? excludeId = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_existingCodes.Contains(aiTaskCode.Trim()));
    }

    public Task<SysAiTask?> GetByCodeAsync(string aiTaskCode, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_tasksById.Values.FirstOrDefault(task => task.AiTaskCode == aiTaskCode.Trim()));
    }

    public Task<IReadOnlyList<SysAiTask>> GetListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<SysAiTask>>(_tasksById.Values.ToList());
    }

    public Task<SysAiTask?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _tasksById.TryGetValue(id, out var task);
        return Task.FromResult(task);
    }

    public Task<SysAiTask> UpdateAsync(SysAiTask entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _tasksById[entity.BasicId] = entity;
        return Task.FromResult(entity);
    }
}

internal sealed class InMemoryAiTaskRunRepository : IAiTaskRunRepository
{
    public List<SysAiTaskRun> Runs { get; } = [];

    public Task<SysAiTaskRun> AddAsync(SysAiTaskRun entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Runs.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<SysAiTaskRun> UpdateAsync(SysAiTaskRun entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(entity);
    }
}

internal sealed class InMemoryAiTaskToolPolicyRepository : IAiTaskToolPolicyRepository
{
    private IReadOnlyList<SysAiTaskToolPolicy> _policies;

    public InMemoryAiTaskToolPolicyRepository(IReadOnlyList<SysAiTaskToolPolicy> policies)
    {
        _policies = policies;
    }

    public Task<IReadOnlyList<SysAiTaskToolPolicy>> GetByTaskIdAsync(long aiTaskId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<SysAiTaskToolPolicy>>(_policies.Where(policy => policy.AiTaskId == aiTaskId).ToList());
    }

    public Task<IReadOnlyList<SysAiTaskToolPolicy>> GetByTaskIdsAsync(IReadOnlyList<long> aiTaskIds, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<SysAiTaskToolPolicy>>(_policies.Where(policy => aiTaskIds.Contains(policy.AiTaskId)).ToList());
    }

    public Task ReplaceByTaskIdAsync(long aiTaskId, IReadOnlyList<SysAiTaskToolPolicy> policies, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _policies = _policies.Where(policy => policy.AiTaskId != aiTaskId).Concat(policies).ToList();
        return Task.CompletedTask;
    }
}

internal sealed class FakeAiTaskChatService : IAiTaskChatService
{
    private readonly string _resultText;

    public FakeAiTaskChatService(string resultText)
    {
        _resultText = resultText;
    }

    public Task<string?> CompleteAsync(SysAiTask task, string prompt, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<string?>(_resultText);
    }
}

internal sealed class InMemoryAiToolRepository : IAiToolRepository
{
    public SysAiTool Tool { get; }

    public InMemoryAiToolRepository(SysAiTool tool)
    {
        if (tool.BasicId <= 0)
        {
            tool = new SysAiTool(11)
            {
                ToolCode = tool.ToolCode,
                ToolName = tool.ToolName,
                ToolType = tool.ToolType,
                SourceKey = tool.SourceKey,
                SkillName = tool.SkillName,
                Category = tool.Category,
                Description = tool.Description,
                InputSchemaJson = tool.InputSchemaJson,
                OutputSchemaJson = tool.OutputSchemaJson,
                RiskLevel = tool.RiskLevel,
                SafetyLevel = tool.SafetyLevel,
                RequiresApproval = tool.RequiresApproval,
                DefaultMaxCalls = tool.DefaultMaxCalls,
                Status = tool.Status,
                Remark = tool.Remark
            };
        }

        Tool = tool;
    }

    public Task<SysAiTool> AddAsync(SysAiTool entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(entity);
    }

    public Task<bool> ExistsCodeAsync(string toolCode, long? excludeId = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Tool.ToolCode.Equals(toolCode.Trim(), StringComparison.OrdinalIgnoreCase)
            && (!excludeId.HasValue || Tool.BasicId != excludeId.Value));
    }

    public Task<SysAiTool?> GetByCodeAsync(string toolCode, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Tool.ToolCode == toolCode.Trim() ? Tool : null);
    }

    public Task<PageResultDtoBase<SysAiTool>> GetPagedAsync(BasicAppPRDto input, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new PageResultDtoBase<SysAiTool>([Tool], new PageResultMetadata(input.Page.PageIndex, input.Page.PageSize, 1)));
    }

    public Task<SysAiTool?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Tool.BasicId == id ? Tool : null);
    }

    public Task<IReadOnlyList<SysAiTool>> GetEnabledListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<SysAiTool>>(Tool.Status == EnableStatus.Enabled ? [Tool] : []);
    }

    public Task<SysAiTool> UpdateAsync(SysAiTool entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(entity);
    }
}
