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
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.BasicApp.Core.Dtos;
using XiHan.BasicApp.Saas.Domain.Enums;
using XiHan.Framework.AI.Abstractions.Prompts;
using XiHan.Framework.Domain.Shared.Paging.Dtos;
using XiHan.Framework.Domain.Shared.Paging.Models;

namespace XiHan.BasicApp.AI.Tests;

internal sealed class InMemoryAiTaskRepository : IAiTaskRepository
{
    private readonly Dictionary<long, SysAiTask> _tasksById = [];
    private readonly HashSet<string> _existingCodes = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryAiTaskRepository()
    {
    }

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

        if (entity.BasicId <= 0)
        {
            entity = new SysAiTaskRun(Runs.Count + 1)
            {
                AiTaskId = entity.AiTaskId,
                AiTaskCode = entity.AiTaskCode,
                StartedTime = entity.StartedTime,
                EndedTime = entity.EndedTime,
                RunStatus = entity.RunStatus,
                LeaseOwner = entity.LeaseOwner,
                LeaseExpiresAt = entity.LeaseExpiresAt,
                LastHeartbeatTime = entity.LastHeartbeatTime,
                AttemptCount = entity.AttemptCount,
                PromptSnapshot = entity.PromptSnapshot,
                ResultText = entity.ResultText,
                ErrorMessage = entity.ErrorMessage,
                DurationMilliseconds = entity.DurationMilliseconds
            };
        }

        Runs.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<SysAiTaskRun> UpdateAsync(SysAiTaskRun entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var index = Runs.FindIndex(run => run.BasicId == entity.BasicId);
        if (index >= 0)
        {
            Runs[index] = entity;
        }

        return Task.FromResult(entity);
    }

    public Task<SysAiTaskRun?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Runs.FirstOrDefault(run => run.BasicId == id));
    }

    public Task<SysAiTaskRun?> ClaimQueuedAsync(long id, string leaseOwner, DateTimeOffset leaseExpiresAt, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var run = Runs.FirstOrDefault(run => run.BasicId == id && run.RunStatus == AiTaskRunStatus.Queued);
        if (run is null)
        {
            return Task.FromResult<SysAiTaskRun?>(null);
        }

        run.RunStatus = AiTaskRunStatus.Running;
        run.LeaseOwner = leaseOwner.Trim();
        run.LeaseExpiresAt = leaseExpiresAt;
        run.LastHeartbeatTime = now;
        run.StartedTime = now;
        run.AttemptCount += 1;
        return Task.FromResult<SysAiTaskRun?>(run);
    }

    public Task<SysAiTaskRun?> CompleteRunningAsync(long id, string leaseOwner, DateTimeOffset endedTime, long durationMilliseconds, string? promptSnapshot, string? resultText, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var run = Runs.FirstOrDefault(run => run.BasicId == id
            && run.RunStatus == AiTaskRunStatus.Running
            && run.LeaseOwner == leaseOwner.Trim());
        if (run is null)
        {
            return Task.FromResult<SysAiTaskRun?>(null);
        }

        run.RunStatus = AiTaskRunStatus.Success;
        run.EndedTime = endedTime;
        run.DurationMilliseconds = durationMilliseconds;
        run.LeaseOwner = null;
        run.LeaseExpiresAt = null;
        run.LastHeartbeatTime = null;
        run.PromptSnapshot = promptSnapshot;
        run.ResultText = resultText;
        run.ErrorMessage = null;
        return Task.FromResult<SysAiTaskRun?>(run);
    }

    public Task<SysAiTaskRun?> FailRunningAsync(long id, string leaseOwner, DateTimeOffset endedTime, long durationMilliseconds, string? promptSnapshot, string errorMessage, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var run = Runs.FirstOrDefault(run => run.BasicId == id
            && run.RunStatus == AiTaskRunStatus.Running
            && run.LeaseOwner == leaseOwner.Trim());
        if (run is null)
        {
            return Task.FromResult<SysAiTaskRun?>(null);
        }

        run.RunStatus = AiTaskRunStatus.Failed;
        run.EndedTime = endedTime;
        run.DurationMilliseconds = durationMilliseconds;
        run.LeaseOwner = null;
        run.LeaseExpiresAt = null;
        run.LastHeartbeatTime = null;
        run.PromptSnapshot = promptSnapshot;
        run.ErrorMessage = errorMessage;
        return Task.FromResult<SysAiTaskRun?>(run);
    }

    public Task<IReadOnlyList<long>> GetStaleQueuedIdsAsync(DateTimeOffset queuedBefore, int maxCount, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<long>>(Runs
            .Where(run => run.RunStatus == AiTaskRunStatus.Queued && run.StartedTime <= queuedBefore)
            .OrderBy(run => run.StartedTime)
            .Take(maxCount)
            .Select(run => run.BasicId)
            .ToList());
    }

    public Task<IReadOnlyList<SysAiTaskRun>> GetRunningRecoveryCandidatesAsync(int maxCount, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<SysAiTaskRun>>(Runs
            .Where(run => run.RunStatus == AiTaskRunStatus.Running)
            .OrderBy(run => run.StartedTime)
            .Take(maxCount)
            .ToList());
    }

    public Task RequeueAsync(long id, DateTimeOffset now, string message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var run = Runs.First(run => run.BasicId == id);
        run.RunStatus = AiTaskRunStatus.Queued;
        run.LeaseOwner = null;
        run.LeaseExpiresAt = null;
        run.LastHeartbeatTime = null;
        run.ErrorMessage = message;
        return Task.CompletedTask;
    }

    public Task FailAsync(long id, DateTimeOffset now, long durationMilliseconds, string errorMessage, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var run = Runs.First(run => run.BasicId == id);
        run.RunStatus = AiTaskRunStatus.Failed;
        run.EndedTime = now;
        run.DurationMilliseconds = durationMilliseconds;
        run.LeaseOwner = null;
        run.LeaseExpiresAt = null;
        run.LastHeartbeatTime = null;
        run.ErrorMessage = errorMessage;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<SysAiTaskRun>> GetByTaskIdAsync(long aiTaskId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<SysAiTaskRun>>(Runs
            .Where(run => run.AiTaskId == aiTaskId)
            .OrderByDescending(run => run.StartedTime)
            .ToList());
    }
}

internal sealed class FakeAiPromptStore : IAiPromptStore
{
    private readonly Dictionary<string, AiPromptTemplate> _prompts = new(StringComparer.OrdinalIgnoreCase);

    public void Add(string name, string content, string? version = null)
    {
        _prompts[$"{name.Trim()}::{version?.Trim()}"] = new AiPromptTemplate
        {
            Name = name.Trim(),
            Content = content,
            Version = version
        };
    }

    public Task<AiPromptTemplate?> GetAsync(string name, string? version = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _prompts.TryGetValue($"{name.Trim()}::{version?.Trim()}", out var prompt);
        return Task.FromResult(prompt);
    }

    public Task<IReadOnlyList<AiPromptTemplate>> ListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<AiPromptTemplate>>(_prompts.Values.ToList());
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

internal sealed class FakeAiTaskRunQueue : IAiTaskRunQueue
{
    public List<long> RunIds { get; } = [];

    public Task EnqueueAsync(long runId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RunIds.Add(runId);
        return Task.CompletedTask;
    }
}

internal sealed class FakeTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _now;

    public FakeTimeProvider(DateTimeOffset now)
    {
        _now = now;
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _now.ToUniversalTime();
    }
}

internal sealed class FakeAiTaskBackingTaskSyncService : IAiTaskBackingTaskSyncService
{
    public Task<SysAiTask> SyncAsync(SysAiTask task, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(task);
    }

    public Task SyncStatusAsync(SysAiTask task, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task DeleteAsync(SysAiTask task, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}

internal sealed class FakeAiTaskChatService : IAiTaskChatService
{
    private readonly string? _resultText;
    private readonly Exception? _exception;
    private readonly Action? _onComplete;

    public string? LastPrompt { get; private set; }

    public FakeAiTaskChatService(string? resultText, Exception? exception = null, Action? onComplete = null)
    {
        _resultText = resultText;
        _exception = exception;
        _onComplete = onComplete;
    }

    public Task<string?> CompleteAsync(SysAiTask task, string prompt, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LastPrompt = prompt;
        _onComplete?.Invoke();
        return _exception is null ? Task.FromResult(_resultText) : Task.FromException<string?>(_exception);
    }
}

internal sealed class InMemoryAiTaskRunEventRepository : IAiTaskRunEventRepository
{
    public List<SysAiTaskRunEvent> Events { get; } = [];

    public Task<SysAiTaskRunEvent> AddAsync(SysAiTaskRunEvent entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sequence = Events
            .Where(e => e.RunId == entity.RunId)
            .Select(e => e.Sequence)
            .DefaultIfEmpty()
            .Max() + 1;
        if (entity.BasicId <= 0)
        {
            entity = new SysAiTaskRunEvent(Events.Count + 1)
            {
                RunId = entity.RunId,
                Sequence = sequence,
                EventType = entity.EventType,
                Role = entity.Role,
                Content = entity.Content,
                PayloadJson = entity.PayloadJson,
                CreatedTime = entity.CreatedTime == default ? DateTimeOffset.Now : entity.CreatedTime
            };
        }
        else
        {
            entity.Sequence = sequence;
        }

        Events.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<IReadOnlyList<SysAiTaskRunEvent>> GetByRunIdAsync(long runId, long afterSequence = 0, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<SysAiTaskRunEvent>>(Events
            .Where(e => e.RunId == runId && e.Sequence > afterSequence)
            .OrderBy(e => e.Sequence)
            .ToList());
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
                Category = tool.Category,
                Description = tool.Description,
                RiskLevel = tool.RiskLevel,
                SafetyLevel = tool.SafetyLevel,
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
