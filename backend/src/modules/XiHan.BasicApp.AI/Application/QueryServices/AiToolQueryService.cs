#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiToolQueryService
// Guid:e6a6f7f1-8c5c-4fc1-9d8c-75d2bd77a8f2
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using Microsoft.AspNetCore.Mvc;
using XiHan.BasicApp.AI.Application.Contracts;
using XiHan.BasicApp.AI.Application.Dtos;
using XiHan.BasicApp.AI.Application.Mappers;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Permissions;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.BasicApp.Core.Dtos;
using XiHan.BasicApp.Saas.Application.Extensions;
using XiHan.BasicApp.Saas.Application.Services;
using XiHan.Framework.Application.Attributes;
using XiHan.Framework.Authorization.AspNetCore;
using XiHan.Framework.Domain.Shared.Paging.Dtos;
using XiHan.Framework.Domain.Shared.Paging.Enums;
using XiHan.Framework.Domain.Shared.Paging.Models;

namespace XiHan.BasicApp.AI.Application.QueryServices;

/// <summary>
/// AI 技能查询应用服务
/// </summary>
[DynamicApi(Group = "BasicApp.AI", GroupName = "AI 服务", Tag = "AI技能")]
public sealed class AiToolQueryService : AiApplicationService, IAiToolQueryService
{
    private readonly IAiToolRepository _toolRepository;
    private readonly IFieldSecurityService _fieldSecurity;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AiToolQueryService(IAiToolRepository toolRepository, IFieldSecurityService fieldSecurityService)
    {
        _toolRepository = toolRepository;
        _fieldSecurity = fieldSecurityService;
    }

    /// <inheritdoc />
    [PermissionAuthorize(AiToolPermissionCodes.Read)]
    [HttpPost]
    public async Task<PageResultDtoBase<AiToolListItemDto>> GetPageAsync(AiToolPageQueryDto input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var request = BuildPageRequest(input);
        await _fieldSecurity.GuardSortsAsync(request.Conditions, "SysAiTool", cancellationToken);
        await _fieldSecurity.GuardFiltersAsync(request.Conditions, "SysAiTool", cancellationToken);
        if (request.Conditions.Sorts.Count == 0)
        {
            ApplyToolSorts(request);
        }

        var toolPage = await _toolRepository.GetPagedAsync(request, cancellationToken);
        if (toolPage.Items.Count == 0)
        {
            return new PageResultDtoBase<AiToolListItemDto>([], toolPage.Page)
            {
                ExtendDatas = toolPage.ExtendDatas
            };
        }

        var items = toolPage.Items.Select(AiToolApplicationMapper.ToListItemDto).ToList();
        return new PageResultDtoBase<AiToolListItemDto>(items, toolPage.Page)
        {
            ExtendDatas = toolPage.ExtendDatas
        };
    }

    /// <inheritdoc />
    [PermissionAuthorize(AiToolPermissionCodes.Read)]
    public async Task<IReadOnlyList<AiToolSelectItemDto>> GetSelectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tools = await _toolRepository.GetEnabledListAsync(cancellationToken);
        return tools.Select(AiToolApplicationMapper.ToSelectItemDto).ToList();
    }

    /// <inheritdoc />
    [PermissionAuthorize(AiToolPermissionCodes.Read)]
    public async Task<AiToolDetailDto?> GetDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "AI 技能主键必须大于 0。");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var tool = await _toolRepository.GetByIdAsync(id, cancellationToken);
        return tool is null ? null : AiToolApplicationMapper.ToDetailDto(tool);
    }

    private static BasicAppPRDto BuildPageRequest(AiToolPageQueryDto input)
    {
        var request = new BasicAppPRDto
        {
            Page = input.Page,
            Behavior = input.Behavior,
            Conditions = new QueryConditions()
        };

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            request.Conditions.SetKeyword<SysAiTool>(
                input.Keyword.Trim(),
                tool => tool.ToolCode,
                tool => tool.ToolName,
                tool => tool.SourceKey,
                tool => tool.Category);
        }

        if (input.ToolType.HasValue)
        {
            request.Conditions.AddFilter((SysAiTool tool) => tool.ToolType, input.ToolType.Value);
        }

        if (input.RiskLevel.HasValue)
        {
            request.Conditions.AddFilter((SysAiTool tool) => tool.RiskLevel, input.RiskLevel.Value);
        }

        if (input.Status.HasValue)
        {
            request.Conditions.AddFilter((SysAiTool tool) => tool.Status, input.Status.Value);
        }

        if (input.Conditions?.Sorts is { Count: > 0 } sorts)
        {
            _ = request.Conditions.AddSorts(sorts);
        }

        if (input.Conditions?.Filters is { Count: > 0 } filters)
        {
            _ = request.Conditions.AddFilters(filters);
        }

        return request;
    }

    private static void ApplyToolSorts(BasicAppPRDto request)
    {
        request.Conditions.AddSort((SysAiTool tool) => tool.Category, SortDirection.Ascending, 0);
        request.Conditions.AddSort((SysAiTool tool) => tool.ToolName, SortDirection.Ascending, 1);
        request.Conditions.AddSort((SysAiTool tool) => tool.CreatedTime, SortDirection.Descending, 2);
    }
}
