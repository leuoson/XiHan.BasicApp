#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskRunnerResolver
// Guid:0b727e68-7f2e-44e9-936c-1f05852cc5d0
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using Microsoft.Extensions.Options;
using XiHan.BasicApp.AI.Infrastructure.Configuration;

namespace XiHan.BasicApp.AI.Application.Services;

/// <summary>
/// AI 任务运行器解析器
/// </summary>
public sealed class AiTaskRunnerResolver
{
    private readonly AiTaskRuntimeOptions _options;
    private readonly IReadOnlyList<IAiTaskAgentRunner> _runners;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskRunnerResolver(IOptions<AiTaskRuntimeOptions> options, IEnumerable<IAiTaskAgentRunner> runners)
    {
        _options = options.Value;
        _runners = runners.ToList();
    }

    /// <summary>
    /// 解析当前运行器
    /// </summary>
    public IAiTaskAgentRunner Resolve()
    {
        var runner = _runners.FirstOrDefault(r => r.Kind == _options.DefaultRunnerKind);
        return runner ?? throw new InvalidOperationException($"AI 任务运行器未注册：{_options.DefaultRunnerKind}");
    }
}
