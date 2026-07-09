#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:SysAiTool
// Guid:5d9c3d3a-56a3-4e0b-a00d-4ef688923d3f
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using SqlSugar;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Core.Entities;
using XiHan.BasicApp.Saas.Domain.Enums;

namespace XiHan.BasicApp.AI.Domain.Entities;

/// <summary>
/// AI 工具登记表
/// </summary>
[SugarTable(TableName = "Sys_Ai_Tool", TableDescription = "系统 AI 工具表")]
[SugarIndex("IX_{table}_TeId_CrTi", nameof(TenantId), OrderByType.Asc, nameof(CreatedTime), OrderByType.Desc)]
[SugarIndex("IX_{table}_TeId_IsDe", nameof(TenantId), OrderByType.Asc, nameof(IsDeleted), OrderByType.Asc)]
[SugarIndex("UX_{table}_TeId_ToCo", nameof(TenantId), OrderByType.Asc, nameof(ToolCode), OrderByType.Asc, nameof(IsDeleted), OrderByType.Asc, true)]
[SugarIndex("IX_{table}_TeId_St", nameof(TenantId), OrderByType.Asc, nameof(Status), OrderByType.Asc)]
public partial class SysAiTool : BasicAppFullAuditedEntity
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public SysAiTool()
    {
    }

    /// <summary>
    /// 测试构造函数
    /// </summary>
    public SysAiTool(long basicId)
        : base(basicId)
    {
    }

    /// <summary>
    /// 工具编码
    /// </summary>
    [SugarColumn(ColumnName = "Tool_Code", ColumnDescription = "工具编码", Length = 100, IsNullable = false)]
    public virtual string ToolCode { get; set; } = string.Empty;

    /// <summary>
    /// 工具名称
    /// </summary>
    [SugarColumn(ColumnName = "Tool_Name", ColumnDescription = "工具名称", Length = 200, IsNullable = false)]
    public virtual string ToolName { get; set; } = string.Empty;

    /// <summary>
    /// 技能来源类型
    /// </summary>
    [SugarColumn(ColumnName = "Tool_Type", ColumnDescription = "技能来源类型")]
    public virtual AiToolType ToolType { get; set; } = AiToolType.BuiltInSkill;

    /// <summary>
    /// 来源键（内置技能名等）
    /// </summary>
    [SugarColumn(ColumnName = "Source_Key", ColumnDescription = "来源键", Length = 200, IsNullable = false)]
    public virtual string SourceKey { get; set; } = string.Empty;

    /// <summary>
    /// 分类
    /// </summary>
    [SugarColumn(ColumnName = "Category", ColumnDescription = "分类", Length = 100, IsNullable = true)]
    public virtual string? Category { get; set; }

    /// <summary>
    /// 旧技能名称字段（兼容第一阶段数据；新技能目录使用 SourceKey）
    /// </summary>
    [SugarColumn(ColumnName = "SkillName", ColumnDescription = "技能名称", Length = 200, IsNullable = true)]
    public virtual string? SkillName { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnName = "Description", ColumnDescription = "描述", Length = 500, IsNullable = true)]
    public virtual string? Description { get; set; }

    /// <summary>
    /// 输入参数 JSON Schema
    /// </summary>
    [SugarColumn(ColumnName = "Input_Schema_Json", ColumnDescription = "输入参数JSON Schema", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public virtual string? InputSchemaJson { get; set; }

    /// <summary>
    /// 输出 JSON Schema
    /// </summary>
    [SugarColumn(ColumnName = "Output_Schema_Json", ColumnDescription = "输出JSON Schema", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public virtual string? OutputSchemaJson { get; set; }

    /// <summary>
    /// 风险等级
    /// </summary>
    [SugarColumn(ColumnName = "Risk_Level", ColumnDescription = "风险等级")]
    public virtual AiToolRiskLevel RiskLevel { get; set; } = AiToolRiskLevel.Low;

    /// <summary>
    /// 安全级别
    /// </summary>
    [SugarColumn(ColumnName = "Safety_Level", ColumnDescription = "安全级别")]
    public virtual AiToolSafetyLevel SafetyLevel { get; set; } = AiToolSafetyLevel.ReadOnly;

    /// <summary>
    /// 是否需要审批
    /// </summary>
    [SugarColumn(ColumnName = "Requires_Approval", ColumnDescription = "是否需要审批")]
    public virtual bool RequiresApproval { get; set; } = false;

    /// <summary>
    /// 默认单次运行最大调用次数
    /// </summary>
    [SugarColumn(ColumnName = "Default_Max_Calls", ColumnDescription = "默认最大调用次数")]
    public virtual int DefaultMaxCalls { get; set; } = 5;

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnName = "Status", ColumnDescription = "状态")]
    public virtual EnableStatus Status { get; set; } = EnableStatus.Enabled;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark", ColumnDescription = "备注", Length = 500, IsNullable = true)]
    public virtual string? Remark { get; set; }
}
