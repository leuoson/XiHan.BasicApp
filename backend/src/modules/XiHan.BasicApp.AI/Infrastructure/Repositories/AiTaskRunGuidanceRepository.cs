#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskRunGuidanceRepository
// Guid:2c2e9a90-ea47-4be2-bf38-787ed34d8742
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.BasicApp.Saas.Infrastructure.Repositories;
using XiHan.Framework.Data.SqlSugar.Clients;

namespace XiHan.BasicApp.AI.Infrastructure.Repositories;

/// <summary>
/// AI 任务运行引导仓储实现
/// </summary>
public sealed class AiTaskRunGuidanceRepository(ISqlSugarClientResolver clientResolver)
    : SaasRepository<SysAiTaskRunGuidance>(clientResolver), IAiTaskRunGuidanceRepository
{
    /// <inheritdoc />
    public new async Task<SysAiTaskRunGuidance> AddAsync(SysAiTaskRunGuidance entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (entity.RunId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(entity), "AI 任务运行记录主键必须大于 0。");
        }

        entity.Content = entity.Content.Trim();
        entity.ClientRequestId = string.IsNullOrWhiteSpace(entity.ClientRequestId) ? null : entity.ClientRequestId.Trim();
        cancellationToken.ThrowIfCancellationRequested();
        return await DbClient.Insertable(entity).ExecuteReturnEntityAsync();
    }

    /// <inheritdoc />
    public async Task<SysAiTaskRunGuidance?> GetByClientRequestIdAsync(long runId, string clientRequestId, CancellationToken cancellationToken = default)
    {
        if (runId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(runId), "AI 任务运行记录主键必须大于 0。");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(clientRequestId);
        cancellationToken.ThrowIfCancellationRequested();
        var id = clientRequestId.Trim();
        return await CreateQueryable()
            .Where(g => g.RunId == runId && g.ClientRequestId == id)
            .FirstAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SysAiTaskRunGuidance>> GetByRunIdAsync(long runId, CancellationToken cancellationToken = default)
    {
        if (runId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(runId), "AI 任务运行记录主键必须大于 0。");
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await CreateQueryable()
            .Where(g => g.RunId == runId)
            .OrderBy(g => g.CreatedTime)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SysAiTaskRunGuidance>> GetPendingByRunIdAsync(long runId, CancellationToken cancellationToken = default)
    {
        if (runId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(runId), "AI 任务运行记录主键必须大于 0。");
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await CreateQueryable()
            .Where(g => g.RunId == runId && g.Status == AiTaskRunGuidanceStatus.Pending)
            .OrderBy(g => g.CreatedTime)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task MarkAppliedAsync(IReadOnlyList<long> ids, DateTimeOffset appliedTime, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        await DbClient.Updateable<SysAiTaskRunGuidance>()
            .SetColumns(g => g.Status == AiTaskRunGuidanceStatus.Applied)
            .SetColumns(g => g.AppliedTime == appliedTime)
            .Where(g => ids.Contains(g.BasicId) && g.Status == AiTaskRunGuidanceStatus.Pending && !g.IsDeleted)
            .ExecuteCommandAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task MarkIgnoredAsync(IReadOnlyList<long> ids, string reason, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        await DbClient.Updateable<SysAiTaskRunGuidance>()
            .SetColumns(g => g.Status == AiTaskRunGuidanceStatus.Ignored)
            .SetColumns(g => g.IgnoredReason == reason)
            .Where(g => ids.Contains(g.BasicId) && g.Status == AiTaskRunGuidanceStatus.Pending && !g.IsDeleted)
            .ExecuteCommandAsync(cancellationToken);
    }
}
