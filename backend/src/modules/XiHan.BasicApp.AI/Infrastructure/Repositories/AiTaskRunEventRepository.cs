#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskRunEventRepository
// Guid:2c834d80-2e88-4d3a-a4c7-4bf0384be707
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.BasicApp.Saas.Infrastructure.Repositories;
using XiHan.Framework.Data.SqlSugar.Clients;

namespace XiHan.BasicApp.AI.Infrastructure.Repositories;

/// <summary>
/// AI 任务运行事件仓储实现
/// </summary>
public sealed class AiTaskRunEventRepository(ISqlSugarClientResolver clientResolver)
    : SaasRepository<SysAiTaskRunEvent>(clientResolver), IAiTaskRunEventRepository
{
    /// <inheritdoc />
    public new async Task<SysAiTaskRunEvent> AddAsync(SysAiTaskRunEvent entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (entity.RunId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(entity), "AI 任务运行记录主键必须大于 0。");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var maxSequence = await CreateQueryable()
            .Where(e => e.RunId == entity.RunId)
            .MaxAsync(e => e.Sequence, cancellationToken);

        entity.Sequence = maxSequence + 1;
        return await DbClient.Insertable(entity).ExecuteReturnEntityAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SysAiTaskRunEvent>> GetByRunIdAsync(long runId, long afterSequence = 0, CancellationToken cancellationToken = default)
    {
        if (runId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(runId), "AI 任务运行记录主键必须大于 0。");
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await CreateQueryable()
            .Where(e => e.RunId == runId && e.Sequence > afterSequence)
            .OrderBy(e => e.Sequence)
            .ToListAsync(cancellationToken);
    }
}
