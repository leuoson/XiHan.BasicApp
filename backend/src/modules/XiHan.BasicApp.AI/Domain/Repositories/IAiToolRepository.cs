#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:IAiToolRepository
// Guid:9c6b30d0-b0c3-4996-b3f7-8710130d97c0
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.Core.Dtos;
using XiHan.Framework.Domain.Shared.Paging.Dtos;

namespace XiHan.BasicApp.AI.Domain.Repositories;

/// <summary>
/// AI 工具仓储接口
/// </summary>
public interface IAiToolRepository
{
    /// <summary>
    /// 添加
    /// </summary>
    Task<SysAiTool> AddAsync(SysAiTool entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 编码是否存在
    /// </summary>
    Task<bool> ExistsCodeAsync(string toolCode, long? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页获取
    /// </summary>
    Task<PageResultDtoBase<SysAiTool>> GetPagedAsync(BasicAppPRDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 按编码获取
    /// </summary>
    Task<SysAiTool?> GetByCodeAsync(string toolCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 按主键获取
    /// </summary>
    Task<SysAiTool?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取启用列表
    /// </summary>
    Task<IReadOnlyList<SysAiTool>> GetEnabledListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新
    /// </summary>
    Task<SysAiTool> UpdateAsync(SysAiTool entity, CancellationToken cancellationToken = default);
}
