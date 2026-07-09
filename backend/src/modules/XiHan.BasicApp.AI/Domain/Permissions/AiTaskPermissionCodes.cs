#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskPermissionCodes
// Guid:eb8e39c5-364d-42b4-9ca7-9b0343a9e76b
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

namespace XiHan.BasicApp.AI.Domain.Permissions;

/// <summary>
/// AI 任务权限码常量
/// </summary>
public static class AiTaskPermissionCodes
{
    /// <summary>资源编码</summary>
    public const string Resource = "ai_task";

    /// <summary>查看</summary>
    public const string Read = "ai_task:read";

    /// <summary>创建</summary>
    public const string Create = "ai_task:create";

    /// <summary>更新</summary>
    public const string Update = "ai_task:update";

    /// <summary>删除</summary>
    public const string Delete = "ai_task:delete";

    /// <summary>执行</summary>
    public const string Execute = "ai_task:execute";
}
