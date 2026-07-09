#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiToolApplicationMapper
// Guid:93b4badc-90c4-469b-905c-fd2970184713
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Application.Dtos;
using XiHan.BasicApp.AI.Domain.DomainServices;
using XiHan.BasicApp.AI.Domain.Entities;

namespace XiHan.BasicApp.AI.Application.Mappers;

/// <summary>
/// AI 技能应用层映射器
/// </summary>
public static class AiToolApplicationMapper
{
    /// <summary>
    /// 映射创建命令
    /// </summary>
    public static AiToolCreateCommand ToCreateCommand(AiToolCreateDto input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new AiToolCreateCommand(
            input.ToolCode,
            input.ToolName,
            input.ToolType,
            input.SourceKey,
            input.Category,
            input.Description,
            input.InputSchemaJson,
            input.OutputSchemaJson,
            input.RiskLevel,
            input.SafetyLevel,
            input.RequiresApproval,
            input.DefaultMaxCalls,
            input.Status,
            input.Remark);
    }

    /// <summary>
    /// 映射更新命令
    /// </summary>
    public static AiToolUpdateCommand ToUpdateCommand(AiToolUpdateDto input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new AiToolUpdateCommand(
            input.BasicId,
            input.ToolName,
            input.Category,
            input.Description,
            input.InputSchemaJson,
            input.OutputSchemaJson,
            input.RiskLevel,
            input.SafetyLevel,
            input.RequiresApproval,
            input.DefaultMaxCalls,
            input.Remark);
    }

    /// <summary>
    /// 映射状态命令
    /// </summary>
    public static AiToolStatusChangeCommand ToStatusCommand(AiToolStatusUpdateDto input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new AiToolStatusChangeCommand(input.BasicId, input.Status, input.Remark);
    }

    /// <summary>
    /// 实体映射列表项
    /// </summary>
    public static AiToolListItemDto ToListItemDto(SysAiTool entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new AiToolListItemDto
        {
            BasicId = entity.BasicId,
            ToolCode = entity.ToolCode,
            ToolName = entity.ToolName,
            ToolType = entity.ToolType,
            SourceKey = entity.SourceKey,
            Category = entity.Category,
            Description = entity.Description,
            RiskLevel = entity.RiskLevel,
            SafetyLevel = entity.SafetyLevel,
            RequiresApproval = entity.RequiresApproval,
            DefaultMaxCalls = entity.DefaultMaxCalls,
            Status = entity.Status,
            CreatedTime = entity.CreatedTime,
            ModifiedTime = entity.ModifiedTime
        };
    }

    /// <summary>
    /// 实体映射详情
    /// </summary>
    public static AiToolDetailDto ToDetailDto(SysAiTool entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var item = ToListItemDto(entity);
        return new AiToolDetailDto
        {
            BasicId = item.BasicId,
            ToolCode = item.ToolCode,
            ToolName = item.ToolName,
            ToolType = item.ToolType,
            SourceKey = item.SourceKey,
            Category = item.Category,
            Description = item.Description,
            RiskLevel = item.RiskLevel,
            SafetyLevel = item.SafetyLevel,
            RequiresApproval = item.RequiresApproval,
            DefaultMaxCalls = item.DefaultMaxCalls,
            Status = item.Status,
            CreatedTime = item.CreatedTime,
            ModifiedTime = item.ModifiedTime,
            InputSchemaJson = entity.InputSchemaJson,
            OutputSchemaJson = entity.OutputSchemaJson,
            Remark = entity.Remark
        };
    }

    /// <summary>
    /// 实体映射选择项
    /// </summary>
    public static AiToolSelectItemDto ToSelectItemDto(SysAiTool entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new AiToolSelectItemDto
        {
            BasicId = entity.BasicId,
            ToolCode = entity.ToolCode,
            ToolName = entity.ToolName,
            ToolType = entity.ToolType,
            RiskLevel = entity.RiskLevel,
            SafetyLevel = entity.SafetyLevel,
            RequiresApproval = entity.RequiresApproval,
            DefaultMaxCalls = entity.DefaultMaxCalls
        };
    }
}
