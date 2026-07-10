#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:IAiTaskRunQueue
// Guid:44fc06bb-02c6-48ad-b15d-cad52cc3db4b
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/09 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

namespace XiHan.BasicApp.AI.Application.Services;

/// <summary>
/// AI 任务手动运行队列
/// </summary>
public interface IAiTaskRunQueue
{
    /// <summary>
    /// 入队一个 AI 任务运行记录
    /// </summary>
    Task EnqueueAsync(long runId, CancellationToken cancellationToken = default);
}
