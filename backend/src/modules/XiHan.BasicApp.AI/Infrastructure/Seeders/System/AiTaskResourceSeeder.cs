#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskResourceSeeder
// Guid:be0a94d7-e990-4272-9158-58f9487231ec
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using Microsoft.Extensions.Logging;
using XiHan.BasicApp.AI.Domain.Permissions;
using XiHan.BasicApp.Saas.Domain.Entities;
using XiHan.BasicApp.Saas.Domain.Enums;
using XiHan.Framework.Data.SqlSugar.Clients;
using XiHan.Framework.Data.SqlSugar.Seeders;

namespace XiHan.BasicApp.AI.Infrastructure.Seeders.System;

/// <summary>
/// AI 任务资源种子数据
/// </summary>
public sealed class AiTaskResourceSeeder : DataSeederBase
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskResourceSeeder(ISqlSugarClientResolver clientResolver, ILogger<AiTaskResourceSeeder> logger, IServiceProvider serviceProvider)
        : base(clientResolver, logger, serviceProvider)
    {
    }

    /// <inheritdoc />
    public override int Order => 213;

    /// <inheritdoc />
    public override string Name => "[Ai]AI任务资源种子数据";

    /// <inheritdoc />
    protected override async Task SeedInternalAsync()
    {
        var targetCodes = new[] { AiTaskPermissionCodes.Resource, AiToolPermissionCodes.Resource };
        var existsCodes = (await DbClient.Queryable<SysResource>().Where(r => targetCodes.Contains(r.ResourceCode)).ToListAsync())
            .Select(r => r.ResourceCode)
            .ToHashSet();
        var addList = new List<SysResource>();

        if (!existsCodes.Contains(AiTaskPermissionCodes.Resource))
        {
            addList.Add(new SysResource
            {
                ResourceCode = AiTaskPermissionCodes.Resource,
                ResourceName = "AI 任务",
                ResourceType = ResourceType.Api,
                ResourcePath = "/api/ai-task",
                Description = "AI 定时任务 API 接口",
                AccessLevel = ResourceAccessLevel.Authorized,
                Status = EnableStatus.Enabled,
                Sort = 405
            });
        }

        if (!existsCodes.Contains(AiToolPermissionCodes.Resource))
        {
            addList.Add(new SysResource
            {
                ResourceCode = AiToolPermissionCodes.Resource,
                ResourceName = "AI 技能",
                ResourceType = ResourceType.Api,
                ResourcePath = "/api/ai-tool",
                Description = "AI 技能目录 API 接口",
                AccessLevel = ResourceAccessLevel.Authorized,
                Status = EnableStatus.Enabled,
                Sort = 406
            });
        }

        if (addList.Count == 0)
        {
            await DbClient.Updateable<SysResource>()
                .SetColumns(r => r.ResourceName == "AI 技能")
                .SetColumns(r => r.Description == "AI 技能目录 API 接口")
                .SetColumns(r => r.ResourcePath == "/api/ai-tool")
                .Where(r => r.ResourceCode == AiToolPermissionCodes.Resource)
                .ExecuteCommandAsync();
            Logger.LogInformation("AI 任务/技能资源数据已存在，已补齐资源名称");
            return;
        }

        await BulkInsertAsync(addList);
        Logger.LogInformation("成功初始化 {Count} 个 AI 任务/技能资源", addList.Count);
    }
}
