#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskExecutor
// Guid:0fb4cc81-c798-4a8c-9f1e-d1d75f321d14
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using System.Diagnostics;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.BasicApp.Saas.Domain.Enums;

namespace XiHan.BasicApp.AI.Application.Services;

/// <summary>
/// AI 任务执行器
/// </summary>
public sealed class AiTaskExecutor
{
    private static readonly string ProcessLeaseOwnerPrefix = $"{Environment.MachineName}:{Guid.NewGuid():N}";

    private readonly IAiTaskRepository _taskRepository;
    private readonly IAiTaskRunRepository _runRepository;
    private readonly AiTaskRunnerResolver _runnerResolver;
    private readonly AiTaskPromptRenderer _promptRenderer;
    private readonly IAiTaskRunEventService _eventService;
    private readonly IAiTaskRunGuidanceRepository _guidanceRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskExecutor(
        IAiTaskRepository taskRepository,
        IAiTaskRunRepository runRepository,
        AiTaskRunnerResolver runnerResolver,
        AiTaskPromptRenderer promptRenderer,
        IAiTaskRunEventService eventService,
        IAiTaskRunGuidanceRepository guidanceRepository)
    {
        _taskRepository = taskRepository;
        _runRepository = runRepository;
        _runnerResolver = runnerResolver;
        _promptRenderer = promptRenderer;
        _eventService = eventService;
        _guidanceRepository = guidanceRepository;
    }

    /// <summary>
    /// 执行 AI 任务
    /// </summary>
    public async Task<AiTaskExecutionResult> ExecuteAsync(long aiTaskId, CancellationToken cancellationToken = default)
    {
        var run = await StartRunAsync(aiTaskId, cancellationToken);
        return await ExecuteRunAsync(run.BasicId, cancellationToken);
    }

    /// <summary>
    /// 创建运行记录但不执行 AI 调用
    /// </summary>
    public async Task<SysAiTaskRun> StartRunAsync(long aiTaskId, CancellationToken cancellationToken = default)
    {
        var task = await GetRunnableTaskOrThrowAsync(aiTaskId, cancellationToken);
        var run = await _runRepository.AddAsync(new SysAiTaskRun
        {
            AiTaskId = task.BasicId,
            AiTaskCode = task.AiTaskCode,
            StartedTime = DateTimeOffset.Now,
            RunStatus = AiTaskRunStatus.Queued
        }, cancellationToken);
        _ = await _eventService.AppendAsync(
            run.BasicId,
            AiTaskRunEventType.RunQueued,
            AiTaskRunEventRole.System,
            "AI 任务运行已排队。",
            null,
            cancellationToken);
        return run;
    }

    /// <summary>
    /// 执行已创建的运行记录
    /// </summary>
    public async Task<AiTaskExecutionResult> ExecuteRunAsync(long runId, CancellationToken cancellationToken = default)
    {
        if (runId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(runId), "AI 任务运行记录主键必须大于 0。");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var run = await _runRepository.GetByIdAsync(runId, cancellationToken)
            ?? throw new InvalidOperationException("AI 任务运行记录不存在。");
        if (run.RunStatus != AiTaskRunStatus.Queued)
        {
            return run.RunStatus == AiTaskRunStatus.Success
                ? AiTaskExecutionResult.Success(run.ResultText, run.BasicId, run.RunStatus)
                : AiTaskExecutionResult.Failure(run.ErrorMessage ?? "AI 任务运行记录不是执行中状态。", run.BasicId, run.RunStatus);
        }

        var task = await GetRunnableTaskOrThrowAsync(run.AiTaskId, cancellationToken);
        var now = DateTimeOffset.Now;
        var leaseExpiresAt = now.AddSeconds(Math.Max(1, task.TimeoutSeconds));
        var leaseOwner = CreateLeaseOwner();
        run = await _runRepository.ClaimQueuedAsync(runId, leaseOwner, leaseExpiresAt, now, cancellationToken);
        if (run is null)
        {
            var current = await _runRepository.GetByIdAsync(runId, cancellationToken)
                ?? throw new InvalidOperationException("AI 任务运行记录不存在。");
            return current.RunStatus == AiTaskRunStatus.Success
                ? AiTaskExecutionResult.Success(current.ResultText, current.BasicId, current.RunStatus)
                : AiTaskExecutionResult.Failure(current.ErrorMessage ?? "AI 任务运行记录已被其他执行器领取。", current.BasicId, current.RunStatus);
        }

        return await ExecuteStartedRunAsync(run, cancellationToken);
    }

    private async Task<AiTaskExecutionResult> ExecuteStartedRunAsync(SysAiTaskRun run, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var leaseOwner = run.LeaseOwner;

        if (string.IsNullOrWhiteSpace(leaseOwner))
        {
            return AiTaskExecutionResult.Failure("AI 任务运行记录缺少执行租约。", run.BasicId, run.RunStatus);
        }

        try
        {
            var task = await GetRunnableTaskOrThrowAsync(run.AiTaskId, cancellationToken);
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, task.TimeoutSeconds)));
            var executionToken = timeoutCts.Token;

            await _eventService.AppendAsync(
                run.BasicId,
                AiTaskRunEventType.RunStarted,
                AiTaskRunEventRole.System,
                "AI 任务开始执行。",
                null,
                executionToken);

            var prompt = await _promptRenderer.RenderAsync(task, executionToken);
            run.PromptSnapshot = prompt;
            await _eventService.AppendAsync(
                run.BasicId,
                AiTaskRunEventType.PromptRendered,
                AiTaskRunEventRole.System,
                "AI 任务提示词已渲染。",
                null,
                executionToken);

            var pendingGuidance = await _guidanceRepository.GetPendingByRunIdAsync(run.BasicId, executionToken);
            var runner = _runnerResolver.Resolve();
            var runnerResult = await runner.RunAsync(new AiTaskRunContext(task, run, prompt, pendingGuidance), executionToken);
            if (pendingGuidance.Count > 0)
            {
                await _guidanceRepository.MarkAppliedAsync(
                    pendingGuidance.Select(g => g.BasicId).ToList(),
                    DateTimeOffset.Now,
                    CancellationToken.None);
            }

            var resultText = runnerResult.ResultText;
            stopwatch.Stop();

            var completedRun = await _runRepository.CompleteRunningAsync(
                run.BasicId,
                leaseOwner,
                DateTimeOffset.Now,
                stopwatch.ElapsedMilliseconds,
                prompt,
                resultText,
                runner.Kind.ToString(),
                runner.Version,
                runnerResult.AgentSessionId,
                cancellationToken);
            if (completedRun is null)
            {
                return await LeaseLostResultAsync(run.BasicId, cancellationToken);
            }

            await IgnorePendingGuidanceAsync(run.BasicId, "运行已结束，未被执行器消费。", CancellationToken.None);
            await _eventService.AppendAsync(
                run.BasicId,
                AiTaskRunEventType.RunSucceeded,
                AiTaskRunEventRole.System,
                "AI 任务执行成功。",
                null,
                cancellationToken);
            return AiTaskExecutionResult.Success(resultText, completedRun.BasicId, completedRun.RunStatus);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();

            const string errorMessage = "AI 任务执行超时。";
            var failedRun = await _runRepository.FailRunningAsync(
                run.BasicId,
                leaseOwner,
                DateTimeOffset.Now,
                stopwatch.ElapsedMilliseconds,
                run.PromptSnapshot,
                errorMessage,
                CancellationToken.None);
            if (failedRun is null)
            {
                return await LeaseLostResultAsync(run.BasicId, CancellationToken.None);
            }

            await IgnorePendingGuidanceAsync(run.BasicId, "运行失败，未被执行器消费。", CancellationToken.None);
            await _eventService.AppendAsync(
                run.BasicId,
                AiTaskRunEventType.RunFailed,
                AiTaskRunEventRole.System,
                errorMessage,
                null,
                CancellationToken.None);
            return AiTaskExecutionResult.Failure(errorMessage, run.BasicId, AiTaskRunStatus.Failed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var failedRun = await _runRepository.FailRunningAsync(
                run.BasicId,
                leaseOwner,
                DateTimeOffset.Now,
                stopwatch.ElapsedMilliseconds,
                run.PromptSnapshot,
                ex.Message,
                cancellationToken);
            if (failedRun is null)
            {
                return await LeaseLostResultAsync(run.BasicId, cancellationToken);
            }

            await IgnorePendingGuidanceAsync(run.BasicId, "运行失败，未被执行器消费。", CancellationToken.None);
            await _eventService.AppendAsync(
                run.BasicId,
                AiTaskRunEventType.RunFailed,
                AiTaskRunEventRole.System,
                ex.Message,
                null,
                CancellationToken.None);
            return AiTaskExecutionResult.Failure(ex.Message, failedRun.BasicId, failedRun.RunStatus);
        }
    }

    private static string CreateLeaseOwner()
    {
        return $"{ProcessLeaseOwnerPrefix}:{Guid.NewGuid():N}";
    }

    private async Task<AiTaskExecutionResult> LeaseLostResultAsync(long runId, CancellationToken cancellationToken)
    {
        var current = await _runRepository.GetByIdAsync(runId, cancellationToken);
        return AiTaskExecutionResult.Failure(
            "AI 任务运行记录租约已失效，执行结果已忽略。",
            runId,
            current?.RunStatus ?? AiTaskRunStatus.Failed);
    }

    private async Task IgnorePendingGuidanceAsync(long runId, string reason, CancellationToken cancellationToken)
    {
        var pendingGuidance = await _guidanceRepository.GetPendingByRunIdAsync(runId, cancellationToken);
        if (pendingGuidance.Count == 0)
        {
            return;
        }

        await _guidanceRepository.MarkIgnoredAsync(pendingGuidance.Select(g => g.BasicId).ToList(), reason, cancellationToken);
    }

    private async Task<SysAiTask> GetRunnableTaskOrThrowAsync(long aiTaskId, CancellationToken cancellationToken)
    {
        if (aiTaskId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(aiTaskId), "AI 任务主键必须大于 0。");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var task = await _taskRepository.GetByIdAsync(aiTaskId, cancellationToken)
            ?? throw new InvalidOperationException("AI 任务不存在。");
        if (task.Status != EnableStatus.Enabled)
        {
            throw new InvalidOperationException("AI 任务已禁用。");
        }

        return task;
    }
}
