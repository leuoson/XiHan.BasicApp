#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:IAiToolDomainService
// Guid:9d3975e4-3da9-428e-884b-1a44fd763f8d
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

namespace XiHan.BasicApp.AI.Domain.DomainServices;

/// <summary>
/// AI 技能领域服务接口
/// </summary>
public interface IAiToolDomainService
{
    /// <summary>
    /// 创建技能
    /// </summary>
    Task<AiToolCommandResult> CreateToolAsync(AiToolCreateCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新技能
    /// </summary>
    Task<AiToolCommandResult> UpdateToolAsync(AiToolUpdateCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新技能状态
    /// </summary>
    Task<AiToolCommandResult> UpdateToolStatusAsync(AiToolStatusChangeCommand command, CancellationToken cancellationToken = default);
}
