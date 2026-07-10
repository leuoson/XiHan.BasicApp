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

    /// <summary>
    /// 根据主键获取运行记录
    /// </summary>
    Task<SysAiTaskRun?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 原子领取排队中的运行记录
    /// </summary>
    Task<SysAiTaskRun?> ClaimQueuedAsync(long id, string leaseOwner, DateTimeOffset leaseExpiresAt, DateTimeOffset now, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据执行租约原子完成运行记录
    /// </summary>
    Task<SysAiTaskRun?> CompleteRunningAsync(long id, string leaseOwner, DateTimeOffset endedTime, long durationMilliseconds, string? promptSnapshot, string? resultText, string? runnerKind, string? runnerVersion, string? agentSessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据执行租约原子标记运行记录失败
    /// </summary>
    Task<SysAiTaskRun?> FailRunningAsync(long id, string leaseOwner, DateTimeOffset endedTime, long durationMilliseconds, string? promptSnapshot, string errorMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取需要重新投递的排队运行记录主键
    /// </summary>
    Task<IReadOnlyList<long>> GetStaleQueuedIdsAsync(DateTimeOffset queuedBefore, int maxCount, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取运行中恢复候选记录
    /// </summary>
    Task<IReadOnlyList<SysAiTaskRun>> GetRunningRecoveryCandidatesAsync(int maxCount, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将运行记录重新排队
    /// </summary>
    Task RequeueAsync(long id, DateTimeOffset now, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将运行记录标记失败
    /// </summary>
    Task FailAsync(long id, DateTimeOffset now, long durationMilliseconds, string errorMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 AI 任务主键获取运行记录
    /// </summary>
    Task<IReadOnlyList<SysAiTaskRun>> GetByTaskIdAsync(long aiTaskId, CancellationToken cancellationToken = default);
}
