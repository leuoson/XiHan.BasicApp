#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskMenuSeeder
// Guid:2701ac3e-531f-4428-94bb-34089aa1c87b
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
/// AI 任务菜单种子数据
/// </summary>
public sealed class AiTaskMenuSeeder : DataSeederBase
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskMenuSeeder(ISqlSugarClientResolver clientResolver, ILogger<AiTaskMenuSeeder> logger, IServiceProvider serviceProvider)
        : base(clientResolver, logger, serviceProvider)
    {
    }

    /// <inheritdoc />
    public override int Order => 215;

    /// <inheritdoc />
    public override string Name => "[Ai]AI任务菜单种子数据";

    /// <inheritdoc />
    protected override async Task SeedInternalAsync()
    {
        var permissions = await DbClient.Queryable<SysPermission>()
            .Where(p => p.PermissionCode == AiTaskPermissionCodes.Read || p.PermissionCode == AiToolPermissionCodes.Read)
            .ToListAsync();
        var taskReadPermission = permissions.FirstOrDefault(p => p.PermissionCode == AiTaskPermissionCodes.Read);
        var toolReadPermission = permissions.FirstOrDefault(p => p.PermissionCode == AiToolPermissionCodes.Read);
        if (taskReadPermission is null || toolReadPermission is null)
        {
            Logger.LogWarning("AI 任务/技能查看权限不存在，跳过 AI 任务菜单种子");
            return;
        }

        var exists = await DbClient.Queryable<SysMenu>().Where(m => m.MenuCode == "develop" || m.MenuCode == "ai_task" || m.MenuCode == "ai_capability").ToListAsync();
        var existsCodes = exists.Select(x => x.MenuCode).ToHashSet();
        var addList = new List<SysMenu>();

        if (!existsCodes.Contains("develop"))
        {
            addList.Add(new SysMenu { ParentId = null, PermissionId = taskReadPermission.BasicId, MenuName = "开发工具", MenuCode = "develop", MenuType = MenuType.Directory, Path = "/develop", Component = null, RouteName = null, Icon = "lucide:hammer", Title = "开发工具", I18nKey = "menu.develop", IsExternal = false, IsCache = false, IsVisible = true, IsAffix = false, Status = EnableStatus.Enabled, Sort = 801, Remark = "开发工具目录" });
        }

        if (!existsCodes.Contains("ai_task"))
        {
            addList.Add(new SysMenu { ParentId = null, PermissionId = taskReadPermission.BasicId, MenuName = "AI 任务", MenuCode = "ai_task", MenuType = MenuType.Menu, Path = "/develop/aiTask", Component = "Develop/AiTask/Index", RouteName = "DevelopAiTask", Icon = "lucide:bot", Title = "AI 任务", I18nKey = "menu.ai_task", IsExternal = false, IsCache = true, IsVisible = true, IsAffix = false, Status = EnableStatus.Enabled, Sort = 805, Remark = "AI 定时任务" });
        }

        if (!existsCodes.Contains("ai_capability"))
        {
            addList.Add(new SysMenu { ParentId = null, PermissionId = toolReadPermission.BasicId, MenuName = "AI 技能", MenuCode = "ai_capability", MenuType = MenuType.Menu, Path = "/develop/aiCapability", Component = "Develop/AiCapability/Index", RouteName = "DevelopAiCapability", Icon = "lucide:plug-zap", Title = "AI 技能", I18nKey = "menu.ai_capability", IsExternal = false, IsCache = true, IsVisible = true, IsAffix = false, Status = EnableStatus.Enabled, Sort = 806, Remark = "AI 技能目录" });
        }

        if (addList.Count > 0)
        {
            await BulkInsertAsync(addList);
        }

        var parentMenu = await DbClient.Queryable<SysMenu>().FirstAsync(m => m.MenuCode == "develop");
        if (parentMenu != null)
        {
            await DbClient.Updateable<SysMenu>()
                .SetColumns(m => m.ParentId == parentMenu.BasicId)
                .Where(m => m.MenuCode == "ai_task" || m.MenuCode == "ai_capability")
                .ExecuteCommandAsync();
        }

        await DbClient.Updateable<SysMenu>()
            .SetColumns(m => m.PermissionId == taskReadPermission.BasicId)
            .SetColumns(m => m.IsVisible == true)
            .SetColumns(m => m.Status == EnableStatus.Enabled)
            .Where(m => m.MenuCode == "ai_task")
            .ExecuteCommandAsync();

        await DbClient.Updateable<SysMenu>()
            .SetColumns(m => m.PermissionId == toolReadPermission.BasicId)
            .SetColumns(m => m.Path == "/develop/aiCapability")
            .SetColumns(m => m.Component == "Develop/AiCapability/Index")
            .SetColumns(m => m.RouteName == "DevelopAiCapability")
            .SetColumns(m => m.Icon == "lucide:plug-zap")
            .SetColumns(m => m.MenuName == "AI 技能")
            .SetColumns(m => m.Title == "AI 技能")
            .SetColumns(m => m.Remark == "AI 技能目录")
            .SetColumns(m => m.I18nKey == "menu.ai_capability")
            .SetColumns(m => m.IsVisible == true)
            .SetColumns(m => m.Status == EnableStatus.Enabled)
            .Where(m => m.MenuCode == "ai_capability")
            .ExecuteCommandAsync();

        Logger.LogInformation("初始化 AI 任务菜单：新增 {AddCount} 个", addList.Count);
    }
}
