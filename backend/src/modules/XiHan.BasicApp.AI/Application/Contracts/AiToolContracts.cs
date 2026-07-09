#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiToolContracts
// Guid:473cd1cc-0887-4ffb-a7fc-78c1550f6e4b
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Application.Dtos;
using XiHan.Framework.Application.Contracts.Services;
using XiHan.Framework.Domain.Shared.Paging.Dtos;

namespace XiHan.BasicApp.AI.Application.Contracts;

/// <summary>
/// AI 技能命令应用服务接口
/// </summary>
public interface IAiToolAppService : IApplicationService
{
    /// <summary>
    /// 创建技能
    /// </summary>
    Task<AiToolDetailDto> CreateAsync(AiToolCreateDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新技能
    /// </summary>
    Task<AiToolDetailDto> UpdateAsync(AiToolUpdateDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新技能状态
    /// </summary>
    Task<AiToolDetailDto> UpdateStatusAsync(AiToolStatusUpdateDto input, CancellationToken cancellationToken = default);
}

/// <summary>
/// AI 技能查询应用服务接口
/// </summary>
public interface IAiToolQueryService : IApplicationService
{
    /// <summary>
    /// 获取技能分页
    /// </summary>
    Task<PageResultDtoBase<AiToolListItemDto>> GetPageAsync(AiToolPageQueryDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取可选技能
    /// </summary>
    Task<IReadOnlyList<AiToolSelectItemDto>> GetSelectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取技能详情
    /// </summary>
    Task<AiToolDetailDto?> GetDetailAsync(long id, CancellationToken cancellationToken = default);
}
