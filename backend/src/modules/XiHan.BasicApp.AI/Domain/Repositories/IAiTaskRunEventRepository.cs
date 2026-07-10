#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:IAiTaskRunEventRepository
// Guid:215d1494-0f2c-4141-a529-6e47d1787b89
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;

namespace XiHan.BasicApp.AI.Domain.Repositories;

/// <summary>
/// AI 任务运行事件仓储接口
/// </summary>
public interface IAiTaskRunEventRepository
{
    /// <summary>
    /// 新增运行事件
    /// </summary>
    Task<SysAiTaskRunEvent> AddAsync(SysAiTaskRunEvent entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取运行事件
    /// </summary>
    Task<IReadOnlyList<SysAiTaskRunEvent>> GetByRunIdAsync(long runId, long afterSequence = 0, CancellationToken cancellationToken = default);
}
