#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:IAiTaskToolPolicyRepository
// Guid:fb38a99d-4c06-4848-9d66-55027244d94e
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;

namespace XiHan.BasicApp.AI.Domain.Repositories;

/// <summary>
/// AI 任务工具策略仓储接口
/// </summary>
public interface IAiTaskToolPolicyRepository
{
    /// <summary>
    /// 获取任务工具策略
    /// </summary>
    Task<IReadOnlyList<SysAiTaskToolPolicy>> GetByTaskIdAsync(long aiTaskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取多个任务工具策略
    /// </summary>
    Task<IReadOnlyList<SysAiTaskToolPolicy>> GetByTaskIdsAsync(IReadOnlyList<long> aiTaskIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// 替换任务工具策略
    /// </summary>
    Task ReplaceByTaskIdAsync(long aiTaskId, IReadOnlyList<SysAiTaskToolPolicy> policies, CancellationToken cancellationToken = default);
}
