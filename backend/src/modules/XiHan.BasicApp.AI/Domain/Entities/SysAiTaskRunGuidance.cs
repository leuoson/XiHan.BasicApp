#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:SysAiTaskRunGuidance
// Guid:6f217f60-1098-42d8-91b5-293404fc3dc2
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
/// AI 任务运行引导
/// </summary>
[SugarTable(TableName = "Sys_Ai_Task_Run_Guidance", TableDescription = "系统 AI 任务运行引导表")]
[SugarIndex("IX_{table}_TeId_RunId_CrTi", nameof(TenantId), OrderByType.Asc, nameof(RunId), OrderByType.Asc, nameof(CreatedTime), OrderByType.Asc)]
[SugarIndex("IX_{table}_TeId_RunId_ClientReq", nameof(TenantId), OrderByType.Asc, nameof(RunId), OrderByType.Asc, nameof(ClientRequestId), OrderByType.Asc)]
public partial class SysAiTaskRunGuidance : BasicAppFullAuditedEntity
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public SysAiTaskRunGuidance()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="basicId"></param>
    public SysAiTaskRunGuidance(long basicId)
        : base(basicId)
    {
    }

    /// <summary>
    /// 运行记录主键
    /// </summary>
    [SugarColumn(ColumnName = "Run_Id", ColumnDescription = "运行记录主键")]
    public virtual long RunId { get; set; }

    /// <summary>
    /// 引导内容
    /// </summary>
    [SugarColumn(ColumnName = "Content", ColumnDescription = "引导内容", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = false)]
    public virtual string Content { get; set; } = string.Empty;

    /// <summary>
    /// 引导状态
    /// </summary>
    [SugarColumn(ColumnName = "Status", ColumnDescription = "引导状态")]
    public virtual AiTaskRunGuidanceStatus Status { get; set; } = AiTaskRunGuidanceStatus.Pending;

    /// <summary>
    /// 客户端幂等键
    /// </summary>
    [SugarColumn(ColumnName = "Client_Request_Id", ColumnDescription = "客户端幂等键", Length = 100, IsNullable = true)]
    public virtual string? ClientRequestId { get; set; }

    /// <summary>
    /// 应用时间
    /// </summary>
    [SugarColumn(ColumnName = "Applied_Time", ColumnDescription = "应用时间", IsNullable = true)]
    public virtual DateTimeOffset? AppliedTime { get; set; }

    /// <summary>
    /// 忽略原因
    /// </summary>
    [SugarColumn(ColumnName = "Ignored_Reason", ColumnDescription = "忽略原因", Length = 500, IsNullable = true)]
    public virtual string? IgnoredReason { get; set; }
}
