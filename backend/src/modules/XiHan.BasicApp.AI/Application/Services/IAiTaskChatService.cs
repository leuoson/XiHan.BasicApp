#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:IAiTaskChatService
// Guid:6d3408f8-18d7-43aa-ad26-2b2fb0d72ce7
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;

namespace XiHan.BasicApp.AI.Application.Services;

/// <summary>
/// AI 任务会话服务接口
/// </summary>
public interface IAiTaskChatService
{
    /// <summary>
    /// 执行单轮聊天
    /// </summary>
    Task<string?> CompleteAsync(SysAiTask task, string prompt, CancellationToken cancellationToken = default);
}
