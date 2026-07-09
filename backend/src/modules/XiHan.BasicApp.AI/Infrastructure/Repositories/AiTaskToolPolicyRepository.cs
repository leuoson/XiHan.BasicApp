#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskToolPolicyRepository
// Guid:b66b0361-b158-4219-bd2f-85b1a6e4430b
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.BasicApp.Saas.Infrastructure.Repositories;
using XiHan.Framework.Data.SqlSugar.Clients;

namespace XiHan.BasicApp.AI.Infrastructure.Repositories;

/// <summary>
/// AI 任务工具策略仓储实现
/// </summary>
public sealed class AiTaskToolPolicyRepository(ISqlSugarClientResolver clientResolver)
    : SaasRepository<SysAiTaskToolPolicy>(clientResolver), IAiTaskToolPolicyRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<SysAiTaskToolPolicy>> GetByTaskIdAsync(long aiTaskId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await CreateQueryable()
            .Where(policy => policy.AiTaskId == aiTaskId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SysAiTaskToolPolicy>> GetByTaskIdsAsync(IReadOnlyList<long> aiTaskIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aiTaskIds);
        cancellationToken.ThrowIfCancellationRequested();
        if (aiTaskIds.Count == 0)
        {
            return [];
        }

        return await CreateQueryable()
            .Where(policy => aiTaskIds.Contains(policy.AiTaskId))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task ReplaceByTaskIdAsync(long aiTaskId, IReadOnlyList<SysAiTaskToolPolicy> policies, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(policies);
        cancellationToken.ThrowIfCancellationRequested();

        await DbClient.Updateable<SysAiTaskToolPolicy>()
            .SetColumns(policy => policy.IsDeleted == true)
            .Where(policy => policy.AiTaskId == aiTaskId && !policy.IsDeleted)
            .ExecuteCommandAsync(cancellationToken);

        if (policies.Count > 0)
        {
            await DbClient.Insertable(policies.ToList()).ExecuteCommandAsync(cancellationToken);
        }
    }
}
