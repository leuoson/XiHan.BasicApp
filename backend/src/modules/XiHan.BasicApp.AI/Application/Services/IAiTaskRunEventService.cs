#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:IAiTaskRunEventService
// Guid:836f3543-093e-42c4-a790-15493d1879bd
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Application.Dtos;
using XiHan.BasicApp.AI.Domain.Enums;

namespace XiHan.BasicApp.AI.Application.Services;

/// <summary>
/// AI 任务运行事件服务接口
/// </summary>
public interface IAiTaskRunEventService
{
    /// <summary>
    /// 追加运行事件
    /// </summary>
    Task<AiTaskRunEventDto> AppendAsync(
        long runId,
        AiTaskRunEventType eventType,
        AiTaskRunEventRole role,
        string? content,
        string? payloadJson,
        CancellationToken cancellationToken = default);
}
