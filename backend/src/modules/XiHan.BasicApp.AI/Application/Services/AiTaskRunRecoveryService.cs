#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskRunRecoveryService
// Guid:0581f553-7d8e-4a84-a819-a22520a62d50
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/09 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using Microsoft.Extensions.Logging;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Repositories;

namespace XiHan.BasicApp.AI.Application.Services;

/// <summary>
/// AI 任务运行恢复服务
/// </summary>
public sealed class AiTaskRunRecoveryService
{
    private static readonly TimeSpan QueuedRecoveryDelay = TimeSpan.FromSeconds(10);
    private const int RecoveryBatchSize = 100;

    private readonly IAiTaskRunRepository _runRepository;
    private readonly IAiTaskRepository _taskRepository;
    private readonly IAiTaskRunQueue _runQueue;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<AiTaskRunRecoveryService> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskRunRecoveryService(
        IAiTaskRunRepository runRepository,
        IAiTaskRepository taskRepository,
        IAiTaskRunQueue runQueue,
        TimeProvider timeProvider,
        ILogger<AiTaskRunRecoveryService> logger)
    {
        _runRepository = runRepository;
        _taskRepository = taskRepository;
        _runQueue = runQueue;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>
    /// 恢复丢失队列消息或执行器中断留下的运行记录
    /// </summary>
    public async Task RecoverAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var now = _timeProvider.GetUtcNow();
        await RecoverQueuedRunsAsync(now, cancellationToken);
        await RecoverRunningRunsAsync(now, cancellationToken);
    }

    private async Task RecoverQueuedRunsAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var queuedBefore = now.Subtract(QueuedRecoveryDelay);
        var ids = await _runRepository.GetStaleQueuedIdsAsync(queuedBefore, RecoveryBatchSize, cancellationToken);
        foreach (var id in ids)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _runQueue.EnqueueAsync(id, cancellationToken);
        }

        if (ids.Count > 0)
        {
            _logger.LogInformation("AI 任务运行恢复：重投 {Count} 个排队运行记录", ids.Count);
        }
    }

    private async Task RecoverRunningRunsAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var candidates = await _runRepository.GetRunningRecoveryCandidatesAsync(RecoveryBatchSize, cancellationToken);
        foreach (var run in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var task = await _taskRepository.GetByIdAsync(run.AiTaskId, cancellationToken);
            if (task is null)
            {
                await FailRunAsync(run, now, "AI 任务运行恢复失败：任务定义不存在。", cancellationToken);
                continue;
            }

            if (!IsExpired(run, task, now))
            {
                continue;
            }

            if (CanRetry(run, task))
            {
                await _runRepository.RequeueAsync(run.BasicId, now, "执行器中断或租约过期，已重新排队。", cancellationToken);
                await _runQueue.EnqueueAsync(run.BasicId, cancellationToken);
                _logger.LogWarning("AI 任务运行恢复：运行记录 {RunId} 已重新排队，AttemptCount={AttemptCount}, MaxRetryCount={MaxRetryCount}",
                    run.BasicId, run.AttemptCount, task.MaxRetryCount);
                continue;
            }

            await FailRunAsync(run, now, "AI 任务执行器中断或租约过期，已超过最大重试次数。", cancellationToken);
        }
    }

    private static bool IsExpired(SysAiTaskRun run, SysAiTask task, DateTimeOffset now)
    {
        if (run.LeaseExpiresAt.HasValue)
        {
            return run.LeaseExpiresAt.Value <= now;
        }

        var timeout = TimeSpan.FromSeconds(Math.Max(1, task.TimeoutSeconds));
        return run.StartedTime.Add(timeout) <= now;
    }

    private static bool CanRetry(SysAiTaskRun run, SysAiTask task)
    {
        var maxAttempts = Math.Max(1, task.MaxRetryCount + 1);
        return run.AttemptCount < maxAttempts;
    }

    private async Task FailRunAsync(SysAiTaskRun run, DateTimeOffset now, string message, CancellationToken cancellationToken)
    {
        var duration = Math.Max(0, (long)(now - run.StartedTime).TotalMilliseconds);
        await _runRepository.FailAsync(run.BasicId, now, duration, message, cancellationToken);
        _logger.LogWarning("AI 任务运行恢复：运行记录 {RunId} 已标记失败，原因：{Message}", run.BasicId, message);
    }
}
