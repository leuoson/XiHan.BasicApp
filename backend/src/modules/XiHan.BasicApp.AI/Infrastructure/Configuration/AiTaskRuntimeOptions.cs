#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskRuntimeOptions
// Guid:82bb18f7-53c5-45dc-b60f-2747676da8b1
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Enums;

namespace XiHan.BasicApp.AI.Infrastructure.Configuration;

/// <summary>
/// AI 任务运行时配置
/// </summary>
public sealed class AiTaskRuntimeOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "XiHan:AI:TaskRuntime";

    /// <summary>
    /// 默认运行器
    /// </summary>
    public AiTaskRunnerKind DefaultRunnerKind { get; set; } = AiTaskRunnerKind.PlainChat;

    /// <summary>
    /// 是否允许任务级覆盖
    /// </summary>
    public bool AllowTaskOverride { get; set; } = true;
}
