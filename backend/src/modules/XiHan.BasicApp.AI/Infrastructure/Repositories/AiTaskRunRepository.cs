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
