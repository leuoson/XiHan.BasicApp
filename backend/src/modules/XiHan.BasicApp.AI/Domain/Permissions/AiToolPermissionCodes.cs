#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiToolPermissionCodes
// Guid:5bfe9858-2798-4e3c-b753-b40d026a316f
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

namespace XiHan.BasicApp.AI.Domain.Permissions;

/// <summary>
/// AI 技能权限码常量
/// </summary>
public static class AiToolPermissionCodes
{
    /// <summary>资源编码</summary>
    public const string Resource = "ai_tool";

    /// <summary>查看</summary>
    public const string Read = "ai_tool:read";

    /// <summary>创建</summary>
    public const string Create = "ai_tool:create";

    /// <summary>更新</summary>
    public const string Update = "ai_tool:update";
}
