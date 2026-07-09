#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:SysAiTask
// Guid:596f18a6-88ce-43d4-b082-e72b614aa2d2
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using SqlSugar;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Core.Entities;
using XiHan.BasicApp.Saas.Domain.Entities;
using XiHan.BasicApp.Saas.Domain.Enums;

namespace XiHan.BasicApp.AI.Domain.Entities;

/// <summary>
/// AI 定时任务定义
/// </summary>
[SugarTable(TableName = "Sys_Ai_Task", TableDescription = "系统 AI 定时任务表")]
[SugarIndex("IX_{table}_TeId_CrTi", nameof(TenantId), OrderByType.Asc, nameof(CreatedTime), OrderByType.Desc)]
[SugarIndex("IX_{table}_TeId_IsDe", nameof(TenantId), OrderByType.Asc, nameof(IsDeleted), OrderByType.Asc)]
[SugarIndex("UX_{table}_TeId_AiTaCo", nameof(TenantId), OrderByType.Asc, nameof(AiTaskCode), OrderByType.Asc, nameof(IsDeleted), OrderByType.Asc, true)]
[SugarIndex("IX_{table}_TeId_St", nameof(TenantId), OrderByType.Asc, nameof(Status), OrderByType.Asc)]
public partial class SysAiTask : BasicAppFullAuditedEntity
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public SysAiTask()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="basicId"></param>
    public SysAiTask(long basicId)
        : base(basicId)
    {
    }

    /// <summary>
    /// AI 任务编码
    /// </summary>
    [SugarColumn(ColumnName = "Ai_Task_Code", ColumnDescription = "AI任务编码", Length = 100, IsNullable = false)]
    public virtual string AiTaskCode { get; set; } = string.Empty;

    /// <summary>
    /// AI 任务名称
    /// </summary>
    [SugarColumn(ColumnName = "Ai_Task_Name", ColumnDescription = "AI任务名称", Length = 200, IsNullable = false)]
    public virtual string AiTaskName { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnName = "Description", ColumnDescription = "描述", Length = 500, IsNullable = true)]
    public virtual string? Description { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    [SugarColumn(ColumnName = "Category", ColumnDescription = "分类", Length = 100, IsNullable = true)]
    public virtual string? Category { get; set; }

    /// <summary>
    /// 提示词来源
    /// </summary>
    [SugarColumn(ColumnName = "Prompt_Mode", ColumnDescription = "提示词来源")]
    public virtual AiTaskPromptMode PromptMode { get; set; } = AiTaskPromptMode.Inline;

    /// <summary>
    /// 内联提示词
    /// </summary>
    [SugarColumn(ColumnName = "Prompt_Text", ColumnDescription = "内联提示词", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public virtual string? PromptText { get; set; }

    /// <summary>
    /// 提示词库编码
    /// </summary>
    [SugarColumn(ColumnName = "Prompt_Code", ColumnDescription = "提示词库编码", Length = 100, IsNullable = true)]
    public virtual string? PromptCode { get; set; }

    /// <summary>
    /// 提示词库版本
    /// </summary>
    [SugarColumn(ColumnName = "Prompt_Version", ColumnDescription = "提示词库版本", Length = 100, IsNullable = true)]
    public virtual string? PromptVersion { get; set; }

    /// <summary>
    /// AI 服务提供商主键
    /// </summary>
    [SugarColumn(ColumnName = "Provider_Id", ColumnDescription = "AI服务提供商主键", IsNullable = true)]
    public virtual long? ProviderId { get; set; }

    /// <summary>
    /// 后台 SysTask 主键
    /// </summary>
    [SugarColumn(ColumnName = "Backing_Task_Id", ColumnDescription = "后台系统任务主键", IsNullable = true)]
    public virtual long? BackingTaskId { get; set; }

    /// <summary>
    /// 触发类型
    /// </summary>
    [SugarColumn(ColumnName = "Trigger_Type", ColumnDescription = "触发类型")]
    public virtual TriggerType TriggerType { get; set; } = TriggerType.Immediate;

    /// <summary>
    /// Cron 表达式
    /// </summary>
    [SugarColumn(ColumnName = "Cron_Expression", ColumnDescription = "Cron表达式", Length = 100, IsNullable = true)]
    public virtual string? CronExpression { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [SugarColumn(ColumnName = "Start_Time", ColumnDescription = "开始时间", IsNullable = true)]
    public virtual DateTimeOffset? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [SugarColumn(ColumnName = "End_Time", ColumnDescription = "结束时间", IsNullable = true)]
    public virtual DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// 执行间隔秒数
    /// </summary>
    [SugarColumn(ColumnName = "Interval_Seconds", ColumnDescription = "执行间隔秒数", IsNullable = true)]
    public virtual int? IntervalSeconds { get; set; }

    /// <summary>
    /// 重复次数
    /// </summary>
    [SugarColumn(ColumnName = "Repeat_Count", ColumnDescription = "重复次数")]
    public virtual int RepeatCount { get; set; } = -1;

    /// <summary>
    /// 超时时间秒数
    /// </summary>
    [SugarColumn(ColumnName = "Timeout_Seconds", ColumnDescription = "超时时间秒数")]
    public virtual int TimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// 优先级
    /// </summary>
    [SugarColumn(ColumnName = "Priority", ColumnDescription = "优先级")]
    public virtual int Priority { get; set; }

    /// <summary>
    /// 是否允许并发
    /// </summary>
    [SugarColumn(ColumnName = "Allow_Concurrent", ColumnDescription = "是否允许并发")]
    public virtual bool AllowConcurrent { get; set; }

    /// <summary>
    /// 最大重试次数
    /// </summary>
    [SugarColumn(ColumnName = "Max_Retry_Count", ColumnDescription = "最大重试次数")]
    public virtual int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnName = "Sort", ColumnDescription = "排序")]
    public virtual int Sort { get; set; }

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
