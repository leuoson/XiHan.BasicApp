#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiToolSeeder
// Guid:0e322463-3d56-49b6-b294-7922f99d7a51
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using Microsoft.Extensions.Logging;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Saas.Domain.Enums;
using XiHan.Framework.Data.SqlSugar.Clients;
using XiHan.Framework.Data.SqlSugar.Seeders;

namespace XiHan.BasicApp.AI.Infrastructure.Seeders.System;

/// <summary>
/// AI 工具种子数据
/// </summary>
public sealed class AiToolSeeder : DataSeederBase
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public AiToolSeeder(ISqlSugarClientResolver clientResolver, ILogger<AiToolSeeder> logger, IServiceProvider serviceProvider)
        : base(clientResolver, logger, serviceProvider)
    {
    }

    /// <inheritdoc />
    public override int Order => 217;

    /// <inheritdoc />
    public override string Name => "[Ai]AI工具种子数据";

    /// <inheritdoc />
    protected override async Task SeedInternalAsync()
    {
        await EnsureSchemaAsync();

        var exists = await DbClient.Queryable<SysAiTool>().Where(t => t.ToolCode == "knowledge_retrieve").ToListAsync();
        if (exists.Count > 0)
        {
            await DbClient.Updateable<SysAiTool>()
                .SetColumns(t => t.ToolType == AiToolType.BuiltInSkill)
                .SetColumns(t => t.SourceKey == "knowledge_retrieve")
                .SetColumns(t => t.SkillName == "KnowledgeRetrieveSkill")
                .SetColumns(t => t.Category == "knowledge")
                .SetColumns(t => t.Description == "允许 AI 任务检索已接入的知识库片段。")
                .SetColumns(t => t.RiskLevel == AiToolRiskLevel.Low)
                .SetColumns(t => t.SafetyLevel == AiToolSafetyLevel.ReadOnly)
                .SetColumns(t => t.RequiresApproval == false)
                .SetColumns(t => t.DefaultMaxCalls == 5)
                .SetColumns(t => t.Status == EnableStatus.Enabled)
                .Where(t => t.ToolCode == "knowledge_retrieve")
                .ExecuteCommandAsync();
            Logger.LogInformation("AI 技能数据已存在，已补齐技能目录字段");
            return;
        }

        await BulkInsertAsync(
        [
            new SysAiTool
            {
                ToolCode = "knowledge_retrieve",
                ToolName = "知识库检索",
                ToolType = AiToolType.BuiltInSkill,
                SourceKey = "knowledge_retrieve",
                SkillName = "KnowledgeRetrieveSkill",
                Category = "knowledge",
                Description = "允许 AI 任务检索已接入的知识库片段。",
                RiskLevel = AiToolRiskLevel.Low,
                SafetyLevel = AiToolSafetyLevel.ReadOnly,
                RequiresApproval = false,
                DefaultMaxCalls = 5,
                Status = EnableStatus.Enabled
            }
        ]);
        Logger.LogInformation("成功初始化 AI 技能数据");
    }

    private async Task EnsureSchemaAsync()
    {
        const string sql = """
            ALTER TABLE "sys_ai_tool" ADD COLUMN IF NOT EXISTS "tool_type" int4 NOT NULL DEFAULT 0;
            ALTER TABLE "sys_ai_tool" ADD COLUMN IF NOT EXISTS "source_key" varchar(200) NOT NULL DEFAULT '';
            ALTER TABLE "sys_ai_tool" ADD COLUMN IF NOT EXISTS "category" varchar(100) NULL;
            ALTER TABLE "sys_ai_tool" ADD COLUMN IF NOT EXISTS "input_schema_json" text NULL;
            ALTER TABLE "sys_ai_tool" ADD COLUMN IF NOT EXISTS "output_schema_json" text NULL;
            ALTER TABLE "sys_ai_tool" ADD COLUMN IF NOT EXISTS "risk_level" int4 NOT NULL DEFAULT 0;
            ALTER TABLE "sys_ai_tool" ADD COLUMN IF NOT EXISTS "safety_level" int4 NOT NULL DEFAULT 0;
            ALTER TABLE "sys_ai_tool" ADD COLUMN IF NOT EXISTS "requires_approval" bool NOT NULL DEFAULT false;
            ALTER TABLE "sys_ai_tool" ADD COLUMN IF NOT EXISTS "default_max_calls" int4 NOT NULL DEFAULT 5;
            ALTER TABLE "sys_ai_tool" ADD COLUMN IF NOT EXISTS "remark" varchar(500) NULL;
            """;

        await DbClient.Ado.ExecuteCommandAsync(sql);
    }
}
