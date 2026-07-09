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
/// AI 任务执行模式
/// </summary>
public enum AiTaskAgentMode
{
    /// <summary>
    /// 普通聊天
    /// </summary>
    [Description("普通聊天")]
    PlainChat = 0,

    /// <summary>
    /// 工具增强 Agent
    /// </summary>
    [Description("工具增强 Agent")]
    ToolAgent = 1
}

/// <summary>
/// AI 任务工具访问策略
/// </summary>
public enum AiTaskToolAccessMode
{
    /// <summary>
    /// 不允许使用工具
    /// </summary>
    [Description("不允许")]
    Deny = 0,

    /// <summary>
    /// 可使用
    /// </summary>
    [Description("允许")]
    Allow = 1,

    /// <summary>
    /// 需要确认
    /// </summary>
    [Description("需要确认")]
    RequireApproval = 2
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
    Canceled = 3
}
