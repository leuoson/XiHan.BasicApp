#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiToolCommandModels
// Guid:934103f0-a6c1-48ad-af74-8e48ad04795c
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Saas.Domain.Enums;

namespace XiHan.BasicApp.AI.Domain.DomainServices;

/// <summary>
/// AI 技能创建命令
/// </summary>
public sealed record AiToolCreateCommand(
    string ToolCode,
    string ToolName,
    AiToolType ToolType,
    string SourceKey,
    string? Category,
    string? Description,
    AiToolRiskLevel RiskLevel,
    AiToolSafetyLevel SafetyLevel,
    EnableStatus Status,
    string? Remark);

/// <summary>
/// AI 技能更新命令
/// </summary>
public sealed record AiToolUpdateCommand(
    long BasicId,
    string ToolName,
    string? Category,
    string? Description,
    AiToolRiskLevel RiskLevel,
    AiToolSafetyLevel SafetyLevel,
    string? Remark);

/// <summary>
/// AI 技能状态变更命令
/// </summary>
public sealed record AiToolStatusChangeCommand(long BasicId, EnableStatus Status, string? Remark);

/// <summary>
/// AI 技能命令结果
/// </summary>
public sealed record AiToolCommandResult(SysAiTool Tool);
