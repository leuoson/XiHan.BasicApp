#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskManualRunHostedService
// Guid:f5f79623-f3b7-4900-ae41-9833a9f572d1
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/09 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using XiHan.BasicApp.AI.Application.Services;
using XiHan.Framework.Caching.Distributed.Abstracts;
using XiHan.Framework.Tasks.BackgroundServices;
using XiHan.Framework.Uow;

namespace XiHan.BasicApp.AI.Infrastructure.Tasks;

/// <summary>
/// AI 任务手动运行消息
/// </summary>
public sealed class AiTaskRunMessage : IBackgroundTaskItem
{
    /// <summary>
    /// AI 任务运行记录主键
    /// </summary>
    public long RunId { get; init; }

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; init; }

    /// <inheritdoc />
    public int RetryCount { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public string TaskId => RunId.ToString();

    /// <inheritdoc />
    [JsonIgnore]
    public object? Data => null;
}

/// <summary>
/// Redis AI 任务手动运行队列
/// </summary>
public sealed class AiTaskRedisRunQueue : IAiTaskRunQueue
{
    private readonly IRedisDelayQueue<AiTaskRunMessage> _queue;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskRedisRunQueue(IRedisDelayQueue<AiTaskRunMessage> queue, IUnitOfWorkManager unitOfWorkManager)
    {
        _queue = queue;
        _unitOfWorkManager = unitOfWorkManager;
    }

    /// <inheritdoc />
    public async Task EnqueueAsync(long runId, CancellationToken cancellationToken = default)
    {
        if (runId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(runId), "AI 任务运行记录主键必须大于 0。");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var message = new AiTaskRunMessage { RunId = runId, CreatedAt = DateTimeOffset.UtcNow };
        var uow = _unitOfWorkManager.Current;
        if (uow is not null)
        {
            uow.OnCompleted(() => _queue.EnqueueAsync(message, TimeSpan.Zero));
            return;
        }

        await _queue.EnqueueAsync(message, TimeSpan.Zero, cancellationToken);
    }
}

/// <summary>
/// AI 任务手动运行后台服务
/// </summary>
public sealed class AiTaskManualRunHostedService : XiHanBackgroundServiceBase<AiTaskManualRunHostedService>
{
    private static readonly TimeSpan RecoveryInterval = TimeSpan.FromSeconds(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRedisDelayQueue<AiTaskRunMessage> _queue;
    private DateTimeOffset _lastRecoveryTime = DateTimeOffset.MinValue;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskManualRunHostedService(
        IServiceScopeFactory scopeFactory,
        IRedisDelayQueue<AiTaskRunMessage> queue,
        IOptions<XiHanBackgroundServiceOptions> options,
        ILogger<AiTaskManualRunHostedService> logger)
        : base(logger, options, BuildConfig(options))
    {
        _scopeFactory = scopeFactory;
        _queue = queue;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RecoverAsync(stoppingToken);
        await base.ExecuteAsync(stoppingToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<IBackgroundTaskItem>> FetchWorkItemsAsync(int maxCount, CancellationToken cancellationToken)
    {
        await RecoverPeriodicallyAsync(cancellationToken);
        return await _queue.DequeueDueAsync(maxCount, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task ProcessItemAsync(IBackgroundTaskItem item, CancellationToken cancellationToken)
    {
        var message = (AiTaskRunMessage)item;
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var executor = scope.ServiceProvider.GetRequiredService<AiTaskExecutor>();
            _ = await executor.ExecuteRunAsync(message.RunId, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "AI 任务手动运行处理异常：{RunId}", message.RunId);
        }
    }

    private async Task RecoverPeriodicallyAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        if (now - _lastRecoveryTime < RecoveryInterval)
        {
            return;
        }

        _lastRecoveryTime = now;
        await RecoverAsync(cancellationToken);
    }

    private async Task RecoverAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var recovery = scope.ServiceProvider.GetRequiredService<AiTaskRunRecoveryService>();
            await recovery.RecoverAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "AI 任务运行恢复失败（忽略，继续后台执行）");
        }
    }

    private static IDynamicServiceConfig BuildConfig(IOptions<XiHanBackgroundServiceOptions> options)
    {
        var config = new DynamicServiceConfig(options);
        config.UpdateMaxConcurrentTasks(2);
        config.UpdateIdleDelay(1000);
        return config;
    }
}
