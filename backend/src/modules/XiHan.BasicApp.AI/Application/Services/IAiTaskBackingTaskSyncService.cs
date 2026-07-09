#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:IAiTaskBackingTaskSyncService
// Guid:29cc88b6-ad0f-4ae8-8176-ea7bdf77af9e
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;

namespace XiHan.BasicApp.AI.Application.Services;

/// <summary>
/// AI 任务内部 SysTask 同步服务接口
/// </summary>
public interface IAiTaskBackingTaskSyncService
{
    /// <summary>
    /// 创建或更新内部调度行
    /// </summary>
    Task<SysAiTask> SyncAsync(SysAiTask task, CancellationToken cancellationToken = default);

    /// <summary>
    /// 同步内部调度行启停状态
    /// </summary>
    Task SyncStatusAsync(SysAiTask task, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除内部调度行
    /// </summary>
    Task DeleteAsync(SysAiTask task, CancellationToken cancellationToken = default);
}
