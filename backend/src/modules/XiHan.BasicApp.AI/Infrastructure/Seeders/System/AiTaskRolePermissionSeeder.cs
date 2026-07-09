#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskRolePermissionSeeder
// Guid:3a227789-07c6-4ff8-9016-1f9f462522f1
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using Microsoft.Extensions.Logging;
using XiHan.BasicApp.Saas.Domain.Entities;
using XiHan.Framework.Data.SqlSugar.Clients;
using XiHan.Framework.Data.SqlSugar.Seeders;

namespace XiHan.BasicApp.AI.Infrastructure.Seeders.System;

/// <summary>
/// AI 任务角色权限种子数据
/// </summary>
public sealed class AiTaskRolePermissionSeeder : DataSeederBase
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskRolePermissionSeeder(ISqlSugarClientResolver clientResolver, ILogger<AiTaskRolePermissionSeeder> logger, IServiceProvider serviceProvider)
        : base(clientResolver, logger, serviceProvider)
    {
    }

    /// <inheritdoc />
    public override int Order => 216;

    /// <inheritdoc />
    public override string Name => "[Ai]AI任务角色权限种子数据";

    /// <inheritdoc />
    protected override async Task SeedInternalAsync()
    {
        var permissions = await DbClient.Queryable<SysPermission>()
            .Where(p => p.PermissionCode.StartsWith("ai_task:") || p.PermissionCode.StartsWith("ai_tool:"))
            .ToListAsync();
        var superRole = await DbClient.Queryable<SysRole>().FirstAsync(r => r.RoleCode == "super_admin");
        if (permissions.Count == 0 || superRole is null)
        {
            Logger.LogWarning("AI 任务权限或超级管理员角色不存在，跳过角色权限种子");
            return;
        }

        var permissionIds = permissions.Select(p => p.BasicId).ToList();
        var existsSet = (await DbClient.Queryable<SysRolePermission>()
                .Where(rp => rp.RoleId == superRole.BasicId && permissionIds.Contains(rp.PermissionId))
                .ToListAsync())
            .Select(rp => rp.PermissionId)
            .ToHashSet();
        var addList = permissions
            .Where(p => !existsSet.Contains(p.BasicId))
            .Select(p => new SysRolePermission { RoleId = superRole.BasicId, PermissionId = p.BasicId })
            .ToList();
        if (addList.Count > 0)
        {
            await BulkInsertAsync(addList);
        }

        Logger.LogInformation("AI 任务授予超级管理员：新增角色权限 {Count} 条", addList.Count);
    }
}
