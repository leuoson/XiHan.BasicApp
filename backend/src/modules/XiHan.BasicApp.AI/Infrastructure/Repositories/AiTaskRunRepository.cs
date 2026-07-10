#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskRunRepository
// Guid:64c420d2-ce12-4ee7-9077-6ff078710279
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.BasicApp.Saas.Infrastructure.Repositories;
using XiHan.Framework.Data.SqlSugar.Clients;

namespace XiHan.BasicApp.AI.Infrastructure.Repositories;

/// <summary>
/// AI 任务运行记录仓储实现
/// </summary>
public sealed class AiTaskRunRepository(ISqlSugarClientResolver clientResolver)
    : SaasRepository<SysAiTaskRun>(clientResolver), IAiTaskRunRepository
{
    /// <inheritdoc />
    public new async Task<SysAiTaskRun?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "AI 任务运行记录主键必须大于 0。");
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await CreateQueryable()
            .Where(run => run.BasicId == id)
            .FirstAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SysAiTaskRun?> ClaimQueuedAsync(long id, string leaseOwner, DateTimeOffset leaseExpiresAt, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "AI 任务运行记录主键必须大于 0。");
        }

        if (string.IsNullOrWhiteSpace(leaseOwner))
        {
            throw new ArgumentException("租约持有者不能为空。", nameof(leaseOwner));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var affected = await DbClient.Updateable<SysAiTaskRun>()
            .SetColumns(run => run.RunStatus == AiTaskRunStatus.Running)
            .SetColumns(run => run.LeaseOwner == leaseOwner.Trim())
            .SetColumns(run => run.LeaseExpiresAt == leaseExpiresAt)
            .SetColumns(run => run.LastHeartbeatTime == now)
            .SetColumns(run => run.StartedTime == now)
            .SetColumns(run => run.AttemptCount == run.AttemptCount + 1)
            .Where(run => run.BasicId == id && run.RunStatus == AiTaskRunStatus.Queued && !run.IsDeleted)
            .ExecuteCommandAsync(cancellationToken);

        return affected > 0 ? await GetByIdAsync(id, cancellationToken) : null;
    }

    /// <inheritdoc />
    public async Task<SysAiTaskRun?> CompleteRunningAsync(long id, string leaseOwner, DateTimeOffset endedTime, long durationMilliseconds, string? promptSnapshot, string? resultText, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "AI 任务运行记录主键必须大于 0。");
        }

        if (string.IsNullOrWhiteSpace(leaseOwner))
        {
            throw new ArgumentException("租约持有者不能为空。", nameof(leaseOwner));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var affected = await DbClient.Updateable<SysAiTaskRun>()
            .SetColumns(run => run.RunStatus == AiTaskRunStatus.Success)
            .SetColumns(run => run.EndedTime == endedTime)
            .SetColumns(run => run.DurationMilliseconds == durationMilliseconds)
            .SetColumns(run => run.LeaseOwner == null)
            .SetColumns(run => run.LeaseExpiresAt == null)
            .SetColumns(run => run.LastHeartbeatTime == null)
            .SetColumns(run => run.PromptSnapshot == promptSnapshot)
            .SetColumns(run => run.ResultText == resultText)
            .SetColumns(run => run.ErrorMessage == null)
            .Where(run => run.BasicId == id
                && run.RunStatus == AiTaskRunStatus.Running
                && run.LeaseOwner == leaseOwner.Trim()
                && !run.IsDeleted)
            .ExecuteCommandAsync(cancellationToken);

        return affected > 0 ? await GetByIdAsync(id, cancellationToken) : null;
    }

    /// <inheritdoc />
    public async Task<SysAiTaskRun?> FailRunningAsync(long id, string leaseOwner, DateTimeOffset endedTime, long durationMilliseconds, string? promptSnapshot, string errorMessage, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "AI 任务运行记录主键必须大于 0。");
        }

        if (string.IsNullOrWhiteSpace(leaseOwner))
        {
            throw new ArgumentException("租约持有者不能为空。", nameof(leaseOwner));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var affected = await DbClient.Updateable<SysAiTaskRun>()
            .SetColumns(run => run.RunStatus == AiTaskRunStatus.Failed)
            .SetColumns(run => run.EndedTime == endedTime)
            .SetColumns(run => run.DurationMilliseconds == durationMilliseconds)
            .SetColumns(run => run.LeaseOwner == null)
            .SetColumns(run => run.LeaseExpiresAt == null)
            .SetColumns(run => run.LastHeartbeatTime == null)
            .SetColumns(run => run.PromptSnapshot == promptSnapshot)
            .SetColumns(run => run.ErrorMessage == errorMessage)
            .Where(run => run.BasicId == id
                && run.RunStatus == AiTaskRunStatus.Running
                && run.LeaseOwner == leaseOwner.Trim()
                && !run.IsDeleted)
            .ExecuteCommandAsync(cancellationToken);

        return affected > 0 ? await GetByIdAsync(id, cancellationToken) : null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<long>> GetStaleQueuedIdsAsync(DateTimeOffset queuedBefore, int maxCount, CancellationToken cancellationToken = default)
    {
        if (maxCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCount), "最大数量必须大于 0。");
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await CreateQueryable()
            .Where(run => run.RunStatus == AiTaskRunStatus.Queued && run.StartedTime <= queuedBefore)
            .OrderBy(run => run.StartedTime)
            .Select(run => run.BasicId)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SysAiTaskRun>> GetRunningRecoveryCandidatesAsync(int maxCount, CancellationToken cancellationToken = default)
    {
        if (maxCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCount), "最大数量必须大于 0。");
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await CreateQueryable()
            .Where(run => run.RunStatus == AiTaskRunStatus.Running)
            .OrderBy(run => run.StartedTime)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RequeueAsync(long id, DateTimeOffset now, string message, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "AI 任务运行记录主键必须大于 0。");
        }

        cancellationToken.ThrowIfCancellationRequested();
        await DbClient.Updateable<SysAiTaskRun>()
            .SetColumns(run => run.RunStatus == AiTaskRunStatus.Queued)
            .SetColumns(run => run.LeaseOwner == null)
            .SetColumns(run => run.LeaseExpiresAt == null)
            .SetColumns(run => run.LastHeartbeatTime == null)
            .SetColumns(run => run.ErrorMessage == message)
            .Where(run => run.BasicId == id && run.RunStatus == AiTaskRunStatus.Running && !run.IsDeleted)
            .ExecuteCommandAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task FailAsync(long id, DateTimeOffset now, long durationMilliseconds, string errorMessage, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "AI 任务运行记录主键必须大于 0。");
        }

        cancellationToken.ThrowIfCancellationRequested();
        await DbClient.Updateable<SysAiTaskRun>()
            .SetColumns(run => run.RunStatus == AiTaskRunStatus.Failed)
            .SetColumns(run => run.EndedTime == now)
            .SetColumns(run => run.DurationMilliseconds == durationMilliseconds)
            .SetColumns(run => run.LeaseOwner == null)
            .SetColumns(run => run.LeaseExpiresAt == null)
            .SetColumns(run => run.LastHeartbeatTime == null)
            .SetColumns(run => run.ErrorMessage == errorMessage)
            .Where(run => run.BasicId == id && !run.IsDeleted)
            .ExecuteCommandAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SysAiTaskRun>> GetByTaskIdAsync(long aiTaskId, CancellationToken cancellationToken = default)
    {
        if (aiTaskId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(aiTaskId), "AI 任务主键必须大于 0。");
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await CreateQueryable()
            .Where(run => run.AiTaskId == aiTaskId)
            .OrderByDescending(run => run.StartedTime)
            .ToListAsync(cancellationToken);
    }
}
