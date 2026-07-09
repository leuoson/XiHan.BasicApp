#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskPromptRenderer
// Guid:45782f13-7480-47de-9a8d-c818452cbd54
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.Framework.AI.Abstractions.Prompts;

namespace XiHan.BasicApp.AI.Application.Services;

/// <summary>
/// AI 任务提示词渲染器
/// </summary>
public sealed class AiTaskPromptRenderer
{
    private readonly IAiPromptStore _promptStore;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskPromptRenderer(IAiPromptStore promptStore)
    {
        _promptStore = promptStore;
    }

    /// <summary>
    /// 渲染任务提示词
    /// </summary>
    public async Task<string> RenderAsync(SysAiTask task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        cancellationToken.ThrowIfCancellationRequested();

        return task.PromptMode switch
        {
            AiTaskPromptMode.Inline when !string.IsNullOrWhiteSpace(task.PromptText) => task.PromptText,
            AiTaskPromptMode.PromptStore => await RenderPromptStoreAsync(task, cancellationToken),
            _ => throw new InvalidOperationException("AI 任务提示词不能为空。")
        };
    }

    private async Task<string> RenderPromptStoreAsync(SysAiTask task, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(task.PromptCode))
        {
            throw new InvalidOperationException("AI 任务提示词编码不能为空。");
        }

        var prompt = await _promptStore.GetAsync(task.PromptCode.Trim(), task.PromptVersion?.Trim(), cancellationToken);
        if (prompt is null || string.IsNullOrWhiteSpace(prompt.Content))
        {
            throw new InvalidOperationException("AI 任务引用的提示词不存在或已禁用。");
        }

        return prompt.Content;
    }
}
