#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:IAiTaskDomainService
// Guid:61291460-0300-4392-8d63-c6cc6d54a332
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

namespace XiHan.BasicApp.AI.Domain.DomainServices;

/// <summary>
/// AI 任务领域服务接口
/// </summary>
public interface IAiTaskDomainService
{
    /// <summary>
    /// 创建 AI 任务
    /// </summary>
    Task<AiTaskCommandResult> CreateTaskAsync(AiTaskCreateCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新 AI 任务
    /// </summary>
    Task<AiTaskCommandResult> UpdateTaskAsync(AiTaskUpdateCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新 AI 任务状态
    /// </summary>
    Task<AiTaskCommandResult> UpdateTaskStatusAsync(AiTaskStatusChangeCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除 AI 任务
    /// </summary>
    Task<AiTaskCommandResult> DeleteTaskAsync(long id, CancellationToken cancellationToken = default);
}
