#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskExecutionResult
// Guid:be6fd566-3f61-4f70-8cf7-b35bf13a1862
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Enums;

namespace XiHan.BasicApp.AI.Application.Services;

/// <summary>
/// AI 任务执行结果
/// </summary>
public sealed record AiTaskExecutionResult(bool Succeeded, string? ResultText, string? ErrorMessage, long? RunId = null, AiTaskRunStatus? RunStatus = null)
{
    /// <summary>
    /// 已提交后台执行
    /// </summary>
    public static AiTaskExecutionResult Accepted(long runId) => new(true, null, null, runId, AiTaskRunStatus.Queued);

    /// <summary>
    /// 成功结果
    /// </summary>
    public static AiTaskExecutionResult Success(string? resultText, long? runId = null, AiTaskRunStatus? runStatus = AiTaskRunStatus.Success) => new(true, resultText, null, runId, runStatus);

    /// <summary>
    /// 失败结果
    /// </summary>
    public static AiTaskExecutionResult Failure(string errorMessage, long? runId = null, AiTaskRunStatus? runStatus = AiTaskRunStatus.Failed) => new(false, null, errorMessage, runId, runStatus);
}
