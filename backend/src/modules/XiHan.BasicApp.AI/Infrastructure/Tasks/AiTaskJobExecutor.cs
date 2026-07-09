#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskJobExecutor
// Guid:10f975af-870e-4ca4-9942-7c51a0983d30
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Application.Services;

namespace XiHan.BasicApp.AI.Infrastructure.Tasks;

/// <summary>
/// SysTask 反射桥接到 AI 任务执行器的入口
/// </summary>
public sealed class AiTaskJobExecutor
{
    private readonly AiTaskExecutor _executor;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskJobExecutor(AiTaskExecutor executor)
    {
        _executor = executor;
    }

    /// <summary>
    /// 执行 AI 任务
    /// </summary>
    public async Task ExecuteAsync(long aiTaskId, CancellationToken cancellationToken = default)
    {
        var result = await _executor.ExecuteAsync(aiTaskId, cancellationToken);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(result.ErrorMessage);
        }
    }
}
