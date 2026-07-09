#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskRepository
// Guid:4b8c8f5e-7b12-43d6-8d9d-62689361a663
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
/// AI 任务仓储实现
/// </summary>
public sealed class AiTaskRepository(ISqlSugarClientResolver clientResolver)
    : SaasRepository<SysAiTask>(clientResolver), IAiTaskRepository
{
    /// <inheritdoc />
    public async Task<SysAiTask?> GetByCodeAsync(string aiTaskCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(aiTaskCode);
        cancellationToken.ThrowIfCancellationRequested();

        var code = aiTaskCode.Trim();
        return await CreateQueryable()
            .Where(task => task.AiTaskCode == code)
            .FirstAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsCodeAsync(string aiTaskCode, long? excludeId = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(aiTaskCode);
        cancellationToken.ThrowIfCancellationRequested();

        var code = aiTaskCode.Trim();
        var query = CreateQueryable().Where(task => task.AiTaskCode == code);
        if (excludeId.HasValue)
        {
            query = query.Where(task => task.BasicId != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SysAiTask>> GetListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await CreateQueryable()
            .OrderBy(task => task.Sort)
            .OrderByDescending(task => task.CreatedTime)
            .ToListAsync(cancellationToken);
    }
}
