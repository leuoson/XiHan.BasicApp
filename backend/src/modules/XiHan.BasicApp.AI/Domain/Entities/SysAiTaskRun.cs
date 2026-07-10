#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:SysAiTaskRun
// Guid:1304638b-cdb9-4ad6-9b3c-a8bdcaec1185
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
/// AI 任务运行记录
/// </summary>
[SugarTable(TableName = "Sys_Ai_Task_Run", TableDescription = "系统 AI 任务运行记录表")]
[SugarIndex("IX_{table}_TeId_AiTaId_CrTi", nameof(TenantId), OrderByType.Asc, nameof(AiTaskId), OrderByType.Asc, nameof(CreatedTime), OrderByType.Desc)]
[SugarIndex("IX_{table}_TeId_RunSt", nameof(TenantId), OrderByType.Asc, nameof(RunStatus), OrderByType.Asc)]
public partial class SysAiTaskRun : BasicAppFullAuditedEntity
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public SysAiTaskRun()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="basicId"></param>
    public SysAiTaskRun(long basicId)
        : base(basicId)
    {
    }

    /// <summary>
    /// AI 任务主键
    /// </summary>
    [SugarColumn(ColumnName = "Ai_Task_Id", ColumnDescription = "AI任务主键")]
    public virtual long AiTaskId { get; set; }

    /// <summary>
    /// 任务编码
    /// </summary>
    [SugarColumn(ColumnName = "Ai_Task_Code", ColumnDescription = "AI任务编码", Length = 100, IsNullable = false)]
    public virtual string AiTaskCode { get; set; } = string.Empty;

    /// <summary>
    /// 开始时间
    /// </summary>
    [SugarColumn(ColumnName = "Started_Time", ColumnDescription = "开始时间")]
    public virtual DateTimeOffset StartedTime { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// 结束时间
    /// </summary>
    [SugarColumn(ColumnName = "Ended_Time", ColumnDescription = "结束时间", IsNullable = true)]
    public virtual DateTimeOffset? EndedTime { get; set; }

    /// <summary>
    /// 运行状态
    /// </summary>
    [SugarColumn(ColumnName = "Run_Status", ColumnDescription = "运行状态")]
    public virtual AiTaskRunStatus RunStatus { get; set; } = AiTaskRunStatus.Queued;

    /// <summary>
    /// 执行租约持有者
    /// </summary>
    [SugarColumn(ColumnName = "Lease_Owner", ColumnDescription = "执行租约持有者", Length = 200, IsNullable = true)]
    public virtual string? LeaseOwner { get; set; }

    /// <summary>
    /// 执行租约过期时间
    /// </summary>
    [SugarColumn(ColumnName = "Lease_Expires_At", ColumnDescription = "执行租约过期时间", IsNullable = true)]
    public virtual DateTimeOffset? LeaseExpiresAt { get; set; }

    /// <summary>
    /// 最后心跳时间
    /// </summary>
    [SugarColumn(ColumnName = "Last_Heartbeat_Time", ColumnDescription = "最后心跳时间", IsNullable = true)]
    public virtual DateTimeOffset? LastHeartbeatTime { get; set; }

    /// <summary>
    /// 执行尝试次数
    /// </summary>
    [SugarColumn(ColumnName = "Attempt_Count", ColumnDescription = "执行尝试次数")]
    public virtual int AttemptCount { get; set; }

    /// <summary>
    /// 运行器类型
    /// </summary>
    [SugarColumn(ColumnName = "Runner_Kind", ColumnDescription = "运行器类型", Length = 100, IsNullable = true)]
    public virtual string? RunnerKind { get; set; }

    /// <summary>
    /// 运行器版本
    /// </summary>
    [SugarColumn(ColumnName = "Runner_Version", ColumnDescription = "运行器版本", Length = 100, IsNullable = true)]
    public virtual string? RunnerVersion { get; set; }

    /// <summary>
    /// Agent 会话标识
    /// </summary>
    [SugarColumn(ColumnName = "Agent_Session_Id", ColumnDescription = "Agent会话标识", Length = 200, IsNullable = true)]
    public virtual string? AgentSessionId { get; set; }

    /// <summary>
    /// 提示词快照
    /// </summary>
    [SugarColumn(ColumnName = "Prompt_Snapshot", ColumnDescription = "提示词快照", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public virtual string? PromptSnapshot { get; set; }

    /// <summary>
    /// 结果文本
    /// </summary>
    [SugarColumn(ColumnName = "Result_Text", ColumnDescription = "结果文本", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public virtual string? ResultText { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    [SugarColumn(ColumnName = "Error_Message", ColumnDescription = "错误消息", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public virtual string? ErrorMessage { get; set; }

    /// <summary>
    /// 耗时毫秒
    /// </summary>
    [SugarColumn(ColumnName = "Duration_Milliseconds", ColumnDescription = "耗时毫秒", IsNullable = true)]
    public virtual long? DurationMilliseconds { get; set; }
}
