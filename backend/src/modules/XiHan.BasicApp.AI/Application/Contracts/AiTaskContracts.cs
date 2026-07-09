#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskContracts
// Guid:a3607b1d-167f-4d55-955d-9b98a020d339
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Application.Dtos;
using XiHan.Framework.Application.Contracts.Services;

namespace XiHan.BasicApp.AI.Application.Contracts;

/// <summary>
/// AI 任务命令应用服务接口
/// </summary>
public interface IAiTaskAppService : IApplicationService
{
    /// <summary>
    /// 创建 AI 任务
    /// </summary>
    Task<AiTaskDetailDto> CreateAsync(AiTaskCreateDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新 AI 任务
    /// </summary>
    Task<AiTaskDetailDto> UpdateAsync(AiTaskUpdateDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新 AI 任务状态
    /// </summary>
    Task<AiTaskDetailDto> UpdateStatusAsync(AiTaskStatusUpdateDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除 AI 任务
    /// </summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 立即执行一次 AI 任务
    /// </summary>
    Task<AiTaskExecutionResultDto> RunAsync(AiTaskRunDto input, CancellationToken cancellationToken = default);
}

/// <summary>
/// AI 任务查询应用服务接口
/// </summary>
public interface IAiTaskQueryService : IApplicationService
{
    /// <summary>
    /// 获取 AI 任务列表
    /// </summary>
    Task<IReadOnlyList<AiTaskListItemDto>> GetListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取 AI 任务详情
    /// </summary>
    Task<AiTaskDetailDto?> GetDetailAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取 AI 任务运行记录列表
    /// </summary>
    Task<IReadOnlyList<AiTaskRunListItemDto>> GetRunListAsync(long aiTaskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取 AI 任务运行记录详情
    /// </summary>
    Task<AiTaskRunDetailDto?> GetRunDetailAsync(long id, CancellationToken cancellationToken = default);
}

/// <summary>
/// AI 任务执行结果 DTO
/// </summary>
public sealed class AiTaskExecutionResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// 结果文本
    /// </summary>
    public string? ResultText { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }
}
