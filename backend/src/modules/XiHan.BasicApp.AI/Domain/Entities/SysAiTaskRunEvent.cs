#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:SysAiTaskRunEvent
// Guid:0f04bd1a-f84a-4c5c-95f4-cc2c9b63b9d1
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using SqlSugar;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Core.Entities;

namespace XiHan.BasicApp.AI.Domain.Entities;

/// <summary>
/// AI 任务运行事件
/// </summary>
[SugarTable(TableName = "Sys_Ai_Task_Run_Event", TableDescription = "系统 AI 任务运行事件表")]
[SugarIndex("IX_{table}_TeId_RunId_Seq", nameof(TenantId), OrderByType.Asc, nameof(RunId), OrderByType.Asc, nameof(Sequence), OrderByType.Asc)]
public partial class SysAiTaskRunEvent : BasicAppFullAuditedEntity
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public SysAiTaskRunEvent()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="basicId"></param>
    public SysAiTaskRunEvent(long basicId)
        : base(basicId)
    {
    }

    /// <summary>
    /// 运行记录主键
    /// </summary>
    [SugarColumn(ColumnName = "Run_Id", ColumnDescription = "运行记录主键")]
    public virtual long RunId { get; set; }

    /// <summary>
    /// 运行内事件序号
    /// </summary>
    [SugarColumn(ColumnName = "Sequence", ColumnDescription = "运行内事件序号")]
    public virtual long Sequence { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    [SugarColumn(ColumnName = "Event_Type", ColumnDescription = "事件类型")]
    public virtual AiTaskRunEventType EventType { get; set; }

    /// <summary>
    /// 事件角色
    /// </summary>
    [SugarColumn(ColumnName = "Role", ColumnDescription = "事件角色")]
    public virtual AiTaskRunEventRole Role { get; set; } = AiTaskRunEventRole.System;

    /// <summary>
    /// 事件内容
    /// </summary>
    [SugarColumn(ColumnName = "Content", ColumnDescription = "事件内容", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public virtual string? Content { get; set; }

    /// <summary>
    /// 事件载荷 JSON
    /// </summary>
    [SugarColumn(ColumnName = "Payload_Json", ColumnDescription = "事件载荷JSON", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public virtual string? PayloadJson { get; set; }
}
