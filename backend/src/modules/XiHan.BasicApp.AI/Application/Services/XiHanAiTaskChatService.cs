#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:XiHanAiTaskChatService
// Guid:f7d2569d-d6e7-41eb-931b-a40dc616a1b5
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using Microsoft.Extensions.AI;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.Framework.AI.Abstractions.Chat;

namespace XiHan.BasicApp.AI.Application.Services;

/// <summary>
/// 基于 XiHan AI 门面的 AI 任务会话服务
/// </summary>
public sealed class XiHanAiTaskChatService : IAiTaskChatService
{
    private readonly IXiHanAiService _aiService;
    private readonly IAiProviderRepository _providerRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    public XiHanAiTaskChatService(IXiHanAiService aiService, IAiProviderRepository providerRepository)
    {
        _aiService = aiService;
        _providerRepository = providerRepository;
    }

    /// <inheritdoc />
    public async Task<string?> CompleteAsync(SysAiTask task, string prompt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);
        cancellationToken.ThrowIfCancellationRequested();

        var provider = task.ProviderId.HasValue
            ? await _providerRepository.GetByIdAsync(task.ProviderId.Value, cancellationToken)
            : null;
        var response = await _aiService.ChatAsync(
            [new ChatMessage(ChatRole.User, prompt)],
            new XiHanChatOptions { Provider = provider?.ConfigCode },
            cancellationToken);

        return response.Text;
    }
}
