#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:IAiTaskRepository
// Guid:b2aec4d6-88b4-43fc-ad8e-83f4b3648768
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;

namespace XiHan.BasicApp.AI.Domain.Repositories;

/// <summary>
/// AI 任务仓储接口
/// </summary>
public interface IAiTaskRepository
{
    /// <summary>
    /// 按主键获取
    /// </summary>
    Task<SysAiTask?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 按编码获取
    /// </summary>
    Task<SysAiTask?> GetByCodeAsync(string aiTaskCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取列表
    /// </summary>
    Task<IReadOnlyList<SysAiTask>> GetListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查编码是否存在
    /// </summary>
    Task<bool> ExistsCodeAsync(string aiTaskCode, long? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 新增
    /// </summary>
    Task<SysAiTask> AddAsync(SysAiTask entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新
    /// </summary>
    Task<SysAiTask> UpdateAsync(SysAiTask entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除
    /// </summary>
    Task<bool> DeleteAsync(SysAiTask entity, CancellationToken cancellationToken = default);
}
