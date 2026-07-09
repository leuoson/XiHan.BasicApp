#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:IAiTaskRunRepository
// Guid:da48c29d-3a9e-4583-8c13-5bb6c6a22047
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;

namespace XiHan.BasicApp.AI.Domain.Repositories;

/// <summary>
/// AI 任务运行记录仓储接口
/// </summary>
public interface IAiTaskRunRepository
{
    /// <summary>
    /// 新增
    /// </summary>
    Task<SysAiTaskRun> AddAsync(SysAiTaskRun entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新
    /// </summary>
    Task<SysAiTaskRun> UpdateAsync(SysAiTaskRun entity, CancellationToken cancellationToken = default);
}
