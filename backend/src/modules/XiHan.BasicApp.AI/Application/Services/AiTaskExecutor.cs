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
using System.Text.Json;
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
    private readonly IAiTaskRepository _taskRepository;
    private readonly IAiTaskRunRepository _runRepository;
    private readonly IAiTaskChatService _chatService;
    private readonly AiTaskPromptRenderer _promptRenderer;
    private readonly IAiTaskToolPolicyRepository _policyRepository;
    private readonly IAiToolRepository _toolRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskExecutor(
        IAiTaskRepository taskRepository,
        IAiTaskRunRepository runRepository,
        IAiTaskChatService chatService,
        AiTaskPromptRenderer promptRenderer,
        IAiTaskToolPolicyRepository policyRepository,
        IAiToolRepository toolRepository)
    {
        _taskRepository = taskRepository;
        _runRepository = runRepository;
        _chatService = chatService;
        _promptRenderer = promptRenderer;
        _policyRepository = policyRepository;
        _toolRepository = toolRepository;
    }

    /// <summary>
    /// 执行 AI 任务
    /// </summary>
    public async Task<AiTaskExecutionResult> ExecuteAsync(long aiTaskId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var task = await _taskRepository.GetByIdAsync(aiTaskId, cancellationToken)
            ?? throw new InvalidOperationException("AI 任务不存在。");
        if (task.Status != EnableStatus.Enabled)
        {
            throw new InvalidOperationException("AI 任务已禁用。");
        }

        var stopwatch = Stopwatch.StartNew();
        var prompt = _promptRenderer.Render(task);
        var policySnapshot = await BuildCapabilityPolicySnapshotAsync(task.BasicId, cancellationToken);
        var run = await _runRepository.AddAsync(new SysAiTaskRun
        {
            AiTaskId = task.BasicId,
            AiTaskCode = task.AiTaskCode,
            StartedTime = DateTimeOffset.Now,
            RunStatus = AiTaskRunStatus.Running,
            PromptSnapshot = prompt,
            CapabilityPolicySnapshotJson = policySnapshot
        }, cancellationToken);

        try
        {
            var resultText = await _chatService.CompleteAsync(task, prompt, cancellationToken);
            stopwatch.Stop();

            run.EndedTime = DateTimeOffset.Now;
            run.DurationMilliseconds = stopwatch.ElapsedMilliseconds;
            run.RunStatus = AiTaskRunStatus.Success;
            run.ResultText = resultText;
            await _runRepository.UpdateAsync(run, cancellationToken);

            return AiTaskExecutionResult.Success(resultText);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            run.EndedTime = DateTimeOffset.Now;
            run.DurationMilliseconds = stopwatch.ElapsedMilliseconds;
            run.RunStatus = AiTaskRunStatus.Failed;
            run.ErrorMessage = ex.Message;
            await _runRepository.UpdateAsync(run, cancellationToken);

            return AiTaskExecutionResult.Failure(ex.Message);
        }
    }

    private async Task<string?> BuildCapabilityPolicySnapshotAsync(long aiTaskId, CancellationToken cancellationToken)
    {
        var policies = await _policyRepository.GetByTaskIdAsync(aiTaskId, cancellationToken);
        var enabledPolicies = policies.Where(policy => policy.IsEnabled).ToList();
        if (enabledPolicies.Count == 0)
        {
            return null;
        }

        var snapshots = new List<object>();
        foreach (var policy in enabledPolicies)
        {
            var tool = await _toolRepository.GetByIdAsync(policy.ToolId, cancellationToken);
            snapshots.Add(new
            {
                policy.ToolId,
                ToolCode = tool?.ToolCode,
                ToolName = tool?.ToolName,
                policy.AccessMode,
                policy.MaxCalls,
                policy.ArgumentPolicyJson,
                policy.IsEnabled,
                IsAvailable = tool?.Status == EnableStatus.Enabled
            });
        }

        return JsonSerializer.Serialize(snapshots);
    }
}
