#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiToolAppService
// Guid:e085527c-cafa-45ea-b2a4-a8e4c9d9b78d
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Application.Contracts;
using XiHan.BasicApp.AI.Application.Dtos;
using XiHan.BasicApp.AI.Application.Mappers;
using XiHan.BasicApp.AI.Domain.DomainServices;
using XiHan.BasicApp.AI.Domain.Permissions;
using XiHan.Framework.Application.Attributes;
using XiHan.Framework.Authorization.AspNetCore;
using XiHan.Framework.Uow.Attributes;

namespace XiHan.BasicApp.AI.Application.AppServices;

/// <summary>
/// AI 技能命令应用服务
/// </summary>
[DynamicApi(Group = "BasicApp.AI", GroupName = "AI 服务", Tag = "AI技能")]
public sealed class AiToolAppService : AiApplicationService, IAiToolAppService
{
    private readonly IAiToolDomainService _toolDomainService;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AiToolAppService(IAiToolDomainService toolDomainService)
    {
        _toolDomainService = toolDomainService;
    }

    /// <inheritdoc />
    [UnitOfWork(true)]
    [PermissionAuthorize(AiToolPermissionCodes.Create)]
    public async Task<AiToolDetailDto> CreateAsync(AiToolCreateDto input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _toolDomainService.CreateToolAsync(AiToolApplicationMapper.ToCreateCommand(input), cancellationToken);
        return AiToolApplicationMapper.ToDetailDto(result.Tool);
    }

    /// <inheritdoc />
    [UnitOfWork(true)]
    [PermissionAuthorize(AiToolPermissionCodes.Update)]
    public async Task<AiToolDetailDto> UpdateAsync(AiToolUpdateDto input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _toolDomainService.UpdateToolAsync(AiToolApplicationMapper.ToUpdateCommand(input), cancellationToken);
        return AiToolApplicationMapper.ToDetailDto(result.Tool);
    }

    /// <inheritdoc />
    [UnitOfWork(true)]
    [PermissionAuthorize(AiToolPermissionCodes.Update)]
    public async Task<AiToolDetailDto> UpdateStatusAsync(AiToolStatusUpdateDto input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _toolDomainService.UpdateToolStatusAsync(AiToolApplicationMapper.ToStatusCommand(input), cancellationToken);
        return AiToolApplicationMapper.ToDetailDto(result.Tool);
    }
}
