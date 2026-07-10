#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:IAiTaskRunGuidanceRepository
// Guid:4d066dfa-f743-4130-a1c1-2d0c3502c5ac
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;

namespace XiHan.BasicApp.AI.Domain.Repositories;

/// <summary>
/// AI 任务运行引导仓储接口
/// </summary>
public interface IAiTaskRunGuidanceRepository
{
    /// <summary>
    /// 新增运行引导
    /// </summary>
    Task<SysAiTaskRunGuidance> AddAsync(SysAiTaskRunGuidance entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据客户端幂等键获取运行引导
    /// </summary>
    Task<SysAiTaskRunGuidance?> GetByClientRequestIdAsync(long runId, string clientRequestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取运行引导
    /// </summary>
    Task<IReadOnlyList<SysAiTaskRunGuidance>> GetByRunIdAsync(long runId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取待应用运行引导
    /// </summary>
    Task<IReadOnlyList<SysAiTaskRunGuidance>> GetPendingByRunIdAsync(long runId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 标记为已应用
    /// </summary>
    Task MarkAppliedAsync(IReadOnlyList<long> ids, DateTimeOffset appliedTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// 标记为已忽略
    /// </summary>
    Task MarkIgnoredAsync(IReadOnlyList<long> ids, string reason, CancellationToken cancellationToken = default);
}
