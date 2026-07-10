#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskRunEventService
// Guid:7e9e3f75-8af3-45e2-94c4-b17173bc536a
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using Microsoft.Extensions.Logging;
using XiHan.BasicApp.AI.Application.Dtos;
using XiHan.BasicApp.AI.Application.Mappers;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.BasicApp.Saas.Hubs;
using XiHan.Framework.Web.RealTime.Services;

namespace XiHan.BasicApp.AI.Application.Services;

/// <summary>
/// AI 任务运行事件服务
/// </summary>
public sealed class AiTaskRunEventService : IAiTaskRunEventService
{
    /// <summary>
    /// 客户端 SignalR 方法名
    /// </summary>
    public const string ClientMethod = "AiTaskRunEventReceived";

    private readonly IAiTaskRunEventRepository _eventRepository;
    private readonly IRealtimeNotificationService<BasicAppNotificationHub> _realtimeNotificationService;
    private readonly ILogger<AiTaskRunEventService> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskRunEventService(
        IAiTaskRunEventRepository eventRepository,
        IRealtimeNotificationService<BasicAppNotificationHub> realtimeNotificationService,
        ILogger<AiTaskRunEventService> logger)
    {
        _eventRepository = eventRepository;
        _realtimeNotificationService = realtimeNotificationService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AiTaskRunEventDto> AppendAsync(
        long runId,
        AiTaskRunEventType eventType,
        AiTaskRunEventRole role,
        string? content,
        string? payloadJson,
        CancellationToken cancellationToken = default)
    {
        var entity = await _eventRepository.AddAsync(new SysAiTaskRunEvent
        {
            RunId = runId,
            EventType = eventType,
            Role = role,
            Content = string.IsNullOrWhiteSpace(content) ? null : content.Trim(),
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? null : payloadJson.Trim()
        }, cancellationToken);

        var dto = AiTaskApplicationMapper.ToRunEventDto(entity);
        try
        {
            await _realtimeNotificationService.SendToAllAsync(ClientMethod, dto);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "推送 AI 任务运行事件失败，RunId={RunId}, Sequence={Sequence}", runId, dto.Sequence);
        }

        return dto;
    }
}
