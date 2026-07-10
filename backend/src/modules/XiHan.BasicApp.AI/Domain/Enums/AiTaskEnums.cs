#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskEnums
// Guid:723d89a8-08ec-4e64-9a09-4f41efecdd46
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using System.ComponentModel;

namespace XiHan.BasicApp.AI.Domain.Enums;

/// <summary>
/// AI 任务提示词来源
/// </summary>
public enum AiTaskPromptMode
{
    /// <summary>
    /// 内联提示词
    /// </summary>
    [Description("内联提示词")]
    Inline = 0,

    /// <summary>
    /// 引用提示词库
    /// </summary>
    [Description("引用提示词库")]
    PromptStore = 1
}

/// <summary>
/// AI 技能来源类型
/// </summary>
public enum AiToolType
{
    /// <summary>
    /// 应用内置技能
    /// </summary>
    [Description("内置技能")]
    BuiltInSkill = 0
}

/// <summary>
/// AI 技能风险等级
/// </summary>
public enum AiToolRiskLevel
{
    /// <summary>
    /// 低风险
    /// </summary>
    [Description("低")]
    Low = 0,

    /// <summary>
    /// 中风险
    /// </summary>
    [Description("中")]
    Medium = 1,

    /// <summary>
    /// 高风险
    /// </summary>
    [Description("高")]
    High = 2
}

/// <summary>
/// AI 技能安全级别
/// </summary>
public enum AiToolSafetyLevel
{
    /// <summary>
    /// 只读
    /// </summary>
    [Description("只读")]
    ReadOnly = 0,

    /// <summary>
    /// 写入
    /// </summary>
    [Description("写入")]
    Write = 1,

    /// <summary>
    /// 外部网络访问
    /// </summary>
    [Description("外部网络")]
    ExternalNetwork = 2,

    /// <summary>
    /// 密钥访问
    /// </summary>
    [Description("密钥访问")]
    SecretAccess = 3
}

/// <summary>
/// AI 任务运行状态
/// </summary>
public enum AiTaskRunStatus
{
    /// <summary>
    /// 执行中
    /// </summary>
    [Description("执行中")]
    Running = 0,

    /// <summary>
    /// 成功
    /// </summary>
    [Description("成功")]
    Success = 1,

    /// <summary>
    /// 失败
    /// </summary>
    [Description("失败")]
    Failed = 2,

    /// <summary>
    /// 已取消
    /// </summary>
    [Description("已取消")]
    Canceled = 3,

    /// <summary>
    /// 排队中
    /// </summary>
    [Description("排队中")]
    Queued = 4
}

/// <summary>
/// AI 任务运行事件类型
/// </summary>
public enum AiTaskRunEventType
{
    /// <summary>
    /// 已排队
    /// </summary>
    [Description("已排队")]
    RunQueued = 0,

    /// <summary>
    /// 开始执行
    /// </summary>
    [Description("开始执行")]
    RunStarted = 1,

    /// <summary>
    /// 提示词已渲染
    /// </summary>
    [Description("提示词已渲染")]
    PromptRendered = 2,

    /// <summary>
    /// Agent 步骤开始
    /// </summary>
    [Description("Agent 步骤开始")]
    AgentStepStarted = 3,

    /// <summary>
    /// Agent 消息片段
    /// </summary>
    [Description("Agent 消息片段")]
    AgentMessageDelta = 4,

    /// <summary>
    /// 工具调用开始
    /// </summary>
    [Description("工具调用开始")]
    ToolCallStarted = 5,

    /// <summary>
    /// 工具调用完成
    /// </summary>
    [Description("工具调用完成")]
    ToolCallFinished = 6,

    /// <summary>
    /// 收到运行引导
    /// </summary>
    [Description("收到运行引导")]
    GuidanceReceived = 7,

    /// <summary>
    /// 运行引导已应用
    /// </summary>
    [Description("运行引导已应用")]
    GuidanceApplied = 8,

    /// <summary>
    /// 运行引导已忽略
    /// </summary>
    [Description("运行引导已忽略")]
    GuidanceIgnored = 9,

    /// <summary>
    /// 执行成功
    /// </summary>
    [Description("执行成功")]
    RunSucceeded = 10,

    /// <summary>
    /// 执行失败
    /// </summary>
    [Description("执行失败")]
    RunFailed = 11,

    /// <summary>
    /// 执行取消
    /// </summary>
    [Description("执行取消")]
    RunCanceled = 12,

    /// <summary>
    /// 已重新排队
    /// </summary>
    [Description("已重新排队")]
    RunRequeued = 13
}

/// <summary>
/// AI 任务运行事件角色
/// </summary>
public enum AiTaskRunEventRole
{
    /// <summary>
    /// 系统
    /// </summary>
    [Description("系统")]
    System = 0,

    /// <summary>
    /// 用户
    /// </summary>
    [Description("用户")]
    User = 1,

    /// <summary>
    /// AI
    /// </summary>
    [Description("AI")]
    Assistant = 2,

    /// <summary>
    /// 工具
    /// </summary>
    [Description("工具")]
    Tool = 3
}

/// <summary>
/// AI 任务运行引导状态
/// </summary>
public enum AiTaskRunGuidanceStatus
{
    /// <summary>
    /// 待应用
    /// </summary>
    [Description("待应用")]
    Pending = 0,

    /// <summary>
    /// 已应用
    /// </summary>
    [Description("已应用")]
    Applied = 1,

    /// <summary>
    /// 已忽略
    /// </summary>
    [Description("已忽略")]
    Ignored = 2
}
