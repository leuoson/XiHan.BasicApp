#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiToolRepository
// Guid:beeb9295-7568-4ffd-952f-b796d8c7e7ac
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.BasicApp.Core.Dtos;
using XiHan.BasicApp.Saas.Domain.Enums;
using XiHan.BasicApp.Saas.Infrastructure.Repositories;
using XiHan.Framework.Data.SqlSugar.Clients;
using XiHan.Framework.Domain.Shared.Paging.Dtos;

namespace XiHan.BasicApp.AI.Infrastructure.Repositories;

/// <summary>
/// AI 工具仓储实现
/// </summary>
public sealed class AiToolRepository(ISqlSugarClientResolver clientResolver)
    : SaasRepository<SysAiTool>(clientResolver), IAiToolRepository
{
    /// <inheritdoc />
    public new async Task<SysAiTool> AddAsync(SysAiTool entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        cancellationToken.ThrowIfCancellationRequested();
        return await DbClient.Insertable(entity).ExecuteReturnEntityAsync();
    }

    /// <inheritdoc />
    public async Task<bool> ExistsCodeAsync(string toolCode, long? excludeId = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolCode);
        cancellationToken.ThrowIfCancellationRequested();

        var code = toolCode.Trim();
        var query = CreateQueryable().Where(tool => tool.ToolCode == code);
        if (excludeId.HasValue)
        {
            query = query.Where(tool => tool.BasicId != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PageResultDtoBase<SysAiTool>> GetPagedAsync(BasicAppPRDto input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();
        return await base.GetPagedAsync(input, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SysAiTool?> GetByCodeAsync(string toolCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolCode);
        cancellationToken.ThrowIfCancellationRequested();

        var code = toolCode.Trim();
        return await CreateQueryable()
            .Where(tool => tool.ToolCode == code)
            .FirstAsync(cancellationToken);
    }

    /// <inheritdoc />
    public new async Task<SysAiTool?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await CreateQueryable()
            .Where(tool => tool.BasicId == id)
            .FirstAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SysAiTool>> GetEnabledListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await CreateQueryable()
            .Where(tool => tool.Status == EnableStatus.Enabled)
            .OrderBy(tool => tool.ToolName)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public new async Task<SysAiTool> UpdateAsync(SysAiTool entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        cancellationToken.ThrowIfCancellationRequested();
        await DbClient.Updateable(entity).ExecuteCommandAsync(cancellationToken);
        return entity;
    }
}
