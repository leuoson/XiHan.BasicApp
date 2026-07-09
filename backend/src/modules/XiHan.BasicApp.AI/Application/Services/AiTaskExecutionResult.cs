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

namespace XiHan.BasicApp.AI.Application.Services;

/// <summary>
/// AI 任务执行结果
/// </summary>
public sealed record AiTaskExecutionResult(bool Succeeded, string? ResultText, string? ErrorMessage)
{
    /// <summary>
    /// 成功结果
    /// </summary>
    public static AiTaskExecutionResult Success(string? resultText) => new(true, resultText, null);

    /// <summary>
    /// 失败结果
    /// </summary>
    public static AiTaskExecutionResult Failure(string errorMessage) => new(false, null, errorMessage);
}
