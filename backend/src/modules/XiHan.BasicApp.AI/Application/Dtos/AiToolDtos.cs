#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiToolDtos
// Guid:f7ec57eb-f656-44f2-90a5-50627281b760
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

#pragma warning disable CS1591

using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Core.Dtos;
using XiHan.BasicApp.Saas.Domain.Enums;

namespace XiHan.BasicApp.AI.Application.Dtos;

/// <summary>
/// AI 技能分页查询 DTO
/// </summary>
public sealed class AiToolPageQueryDto : BasicAppPRDto
{
    public string? Keyword { get; set; }
    public AiToolType? ToolType { get; set; }
    public AiToolRiskLevel? RiskLevel { get; set; }
    public EnableStatus? Status { get; set; }
}

/// <summary>
/// AI 技能列表项 DTO
/// </summary>
public class AiToolListItemDto : BasicAppDto
{
    public string ToolCode { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public AiToolType ToolType { get; set; }
    public string SourceKey { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public AiToolRiskLevel RiskLevel { get; set; }
    public AiToolSafetyLevel SafetyLevel { get; set; }
    public EnableStatus Status { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset? ModifiedTime { get; set; }
}

/// <summary>
/// AI 技能详情 DTO
/// </summary>
public sealed class AiToolDetailDto : AiToolListItemDto
{
    public string? Remark { get; set; }
}

/// <summary>
/// AI 技能选择项 DTO
/// </summary>
public sealed class AiToolSelectItemDto : BasicAppDto
{
    public string ToolCode { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public AiToolType ToolType { get; set; }
    public AiToolRiskLevel RiskLevel { get; set; }
    public AiToolSafetyLevel SafetyLevel { get; set; }
}

/// <summary>
/// AI 技能创建 DTO
/// </summary>
public sealed class AiToolCreateDto
{
    public string ToolCode { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public AiToolType ToolType { get; set; } = AiToolType.BuiltInSkill;
    public string SourceKey { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public AiToolRiskLevel RiskLevel { get; set; } = AiToolRiskLevel.Low;
    public AiToolSafetyLevel SafetyLevel { get; set; } = AiToolSafetyLevel.ReadOnly;
    public EnableStatus Status { get; set; } = EnableStatus.Enabled;
    public string? Remark { get; set; }
}

/// <summary>
/// AI 技能更新 DTO
/// </summary>
public sealed class AiToolUpdateDto : BasicAppUDto
{
    public string ToolName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public AiToolRiskLevel RiskLevel { get; set; } = AiToolRiskLevel.Low;
    public AiToolSafetyLevel SafetyLevel { get; set; } = AiToolSafetyLevel.ReadOnly;
    public string? Remark { get; set; }
}

/// <summary>
/// AI 技能状态更新 DTO
/// </summary>
public sealed class AiToolStatusUpdateDto : BasicAppDto
{
    public EnableStatus Status { get; set; } = EnableStatus.Enabled;
    public string? Remark { get; set; }
}
