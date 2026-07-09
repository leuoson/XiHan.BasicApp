#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskPermissionSeeder
// Guid:a0db2bc3-377a-45eb-9c20-706485f81ffc
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
/// AI 任务权限种子数据
/// </summary>
public sealed class AiTaskPermissionSeeder : DataSeederBase
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskPermissionSeeder(ISqlSugarClientResolver clientResolver, ILogger<AiTaskPermissionSeeder> logger, IServiceProvider serviceProvider)
        : base(clientResolver, logger, serviceProvider)
    {
    }

    /// <inheritdoc />
    public override int Order => 214;

    /// <inheritdoc />
    public override string Name => "[Ai]AI任务权限种子数据";

    /// <inheritdoc />
    protected override async Task SeedInternalAsync()
    {
        var resources = await DbClient.Queryable<SysResource>()
            .Where(r => r.ResourceCode == AiTaskPermissionCodes.Resource || r.ResourceCode == AiToolPermissionCodes.Resource)
            .ToListAsync();
        var resourceMap = resources.ToDictionary(r => r.ResourceCode, r => r);
        var operations = await DbClient.Queryable<SysOperation>().ToListAsync();
        if (resources.Count == 0 || operations.Count == 0)
        {
            Logger.LogWarning("AI 任务/技能资源或系统操作不存在，跳过 AI 任务权限种子数据");
            return;
        }

        var targets = new (string ResourceCode, string[] Operations)[]
        {
            (AiTaskPermissionCodes.Resource, ["read", "create", "update", "delete", "execute"]),
            (AiToolPermissionCodes.Resource, ["read", "create", "update"])
        };
        var permissionCodes = targets.SelectMany(target => target.Operations.Select(op => $"{target.ResourceCode}:{op}")).ToList();
        var existing = await DbClient.Queryable<SysPermission>().Where(p => permissionCodes.Contains(p.PermissionCode)).ToListAsync();
        var existingCodes = existing.Select(x => x.PermissionCode).ToHashSet();
        var operationMap = operations.ToDictionary(o => o.OperationCode, o => o);
        var addList = new List<SysPermission>();

        foreach (var target in targets)
        {
            if (!resourceMap.TryGetValue(target.ResourceCode, out var resource))
            {
                continue;
            }

            foreach (var opCode in target.Operations)
            {
                var permissionCode = $"{target.ResourceCode}:{opCode}";
                if (existingCodes.Contains(permissionCode) || !operationMap.TryGetValue(opCode, out var operation))
                {
                    continue;
                }

                addList.Add(new SysPermission
                {
                    ResourceId = resource.BasicId,
                    OperationId = operation.BasicId,
                    PermissionCode = permissionCode,
                    PermissionName = $"{resource.ResourceName}-{operation.OperationName}",
                    PermissionDescription = $"对{resource.ResourceName}执行{operation.OperationName}操作",
                    IsRequireAudit = operation.IsRequireAudit,
                    Tags = target.ResourceCode,
                    Status = EnableStatus.Enabled,
                    Sort = 950 + addList.Count
                });
            }
        }

        if (addList.Count == 0)
        {
            await SyncExistingToolPermissionNamesAsync(existing, resourceMap, operationMap);
            Logger.LogInformation("AI 任务/技能权限数据已存在，跳过种子数据");
            return;
        }

        await BulkInsertAsync(addList);
        await SyncExistingToolPermissionNamesAsync(existing, resourceMap, operationMap);
        Logger.LogInformation("成功初始化 {Count} 个 AI 任务/技能权限", addList.Count);
    }

    private async Task SyncExistingToolPermissionNamesAsync(
        IReadOnlyList<SysPermission> existing,
        IReadOnlyDictionary<string, SysResource> resourceMap,
        IReadOnlyDictionary<string, SysOperation> operationMap)
    {
        if (!resourceMap.TryGetValue(AiToolPermissionCodes.Resource, out var resource))
        {
            return;
        }

        foreach (var permission in existing.Where(p => p.PermissionCode.StartsWith($"{AiToolPermissionCodes.Resource}:")))
        {
            var operationCode = permission.PermissionCode[(AiToolPermissionCodes.Resource.Length + 1)..];
            if (!operationMap.TryGetValue(operationCode, out var operation))
            {
                continue;
            }

            permission.PermissionName = $"{resource.ResourceName}-{operation.OperationName}";
            permission.PermissionDescription = $"对{resource.ResourceName}执行{operation.OperationName}操作";
            permission.Tags = AiToolPermissionCodes.Resource;
            _ = await DbClient.Updateable(permission).ExecuteCommandAsync();
        }
    }
}
