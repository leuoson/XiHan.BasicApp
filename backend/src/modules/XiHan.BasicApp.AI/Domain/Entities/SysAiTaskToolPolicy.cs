#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:SysAiTaskToolPolicy
// Guid:7b7912d6-448e-41a4-a608-a46e3134903b
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using SqlSugar;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Core.Entities;

namespace XiHan.BasicApp.AI.Domain.Entities;

/// <summary>
/// AI 任务工具策略
/// </summary>
[SugarTable(TableName = "Sys_Ai_Task_Tool_Policy", TableDescription = "系统 AI 任务工具策略表")]
[SugarIndex("IX_{table}_TeId_CrTi", nameof(TenantId), OrderByType.Asc, nameof(CreatedTime), OrderByType.Desc)]
[SugarIndex("UX_{table}_TeId_AiTaId_ToId", nameof(TenantId), OrderByType.Asc, nameof(AiTaskId), OrderByType.Asc, nameof(ToolId), OrderByType.Asc, nameof(IsDeleted), OrderByType.Asc, true)]
public partial class SysAiTaskToolPolicy : BasicAppFullAuditedEntity
{
    /// <summary>
    /// AI 任务主键
    /// </summary>
    [SugarColumn(ColumnName = "Ai_Task_Id", ColumnDescription = "AI任务主键")]
    public virtual long AiTaskId { get; set; }

    /// <summary>
    /// 工具主键
    /// </summary>
    [SugarColumn(ColumnName = "Tool_Id", ColumnDescription = "工具主键")]
    public virtual long ToolId { get; set; }

    /// <summary>
    /// 访问模式
    /// </summary>
    [SugarColumn(ColumnName = "Access_Mode", ColumnDescription = "访问模式")]
    public virtual AiTaskToolAccessMode AccessMode { get; set; } = AiTaskToolAccessMode.Allow;

    /// <summary>
    /// 参数约束 JSON
    /// </summary>
    [SugarColumn(ColumnName = "Argument_Policy_Json", ColumnDescription = "参数约束JSON", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public virtual string? ArgumentPolicyJson { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [SugarColumn(ColumnName = "Is_Enabled", ColumnDescription = "是否启用")]
    public virtual bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 单次运行最大调用次数
    /// </summary>
    [SugarColumn(ColumnName = "Max_Calls", ColumnDescription = "最大调用次数")]
    public virtual int MaxCalls { get; set; } = 5;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark", ColumnDescription = "备注", Length = 500, IsNullable = true)]
    public virtual string? Remark { get; set; }
}
