#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskSchemaSeeder
// Guid:4ef5f6d6-9d64-42e9-9763-9ff41c124a0b
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/09 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using Microsoft.Extensions.Logging;
using XiHan.Framework.Data.SqlSugar.Clients;
using XiHan.Framework.Data.SqlSugar.Seeders;

namespace XiHan.BasicApp.AI.Infrastructure.Seeders.System;

/// <summary>
/// AI 任务当前结构补齐种子
/// </summary>
public sealed class AiTaskSchemaSeeder : DataSeederBase
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public AiTaskSchemaSeeder(ISqlSugarClientResolver clientResolver, ILogger<AiTaskSchemaSeeder> logger, IServiceProvider serviceProvider)
        : base(clientResolver, logger, serviceProvider)
    {
    }

    /// <inheritdoc />
    public override int Order => 216;

    /// <inheritdoc />
    public override string Name => "[Ai]AI任务结构补齐";

    /// <inheritdoc />
    protected override async Task SeedInternalAsync()
    {
        const string sql = """
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "ai_task_code" varchar(100) NOT NULL DEFAULT '';
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "ai_task_name" varchar(200) NOT NULL DEFAULT '';
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "description" varchar(500) NULL;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "category" varchar(100) NULL;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "prompt_mode" int4 NOT NULL DEFAULT 0;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "prompt_text" text NULL;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "prompt_code" varchar(100) NULL;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "prompt_version" varchar(100) NULL;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "provider_id" int8 NULL;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "backing_task_id" int8 NULL;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "trigger_type" int4 NOT NULL DEFAULT 0;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "cron_expression" varchar(100) NULL;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "start_time" timestamptz NULL;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "end_time" timestamptz NULL;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "interval_seconds" int4 NULL;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "repeat_count" int4 NOT NULL DEFAULT -1;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "timeout_seconds" int4 NOT NULL DEFAULT 300;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "priority" int4 NOT NULL DEFAULT 0;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "allow_concurrent" bool NOT NULL DEFAULT false;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "max_retry_count" int4 NOT NULL DEFAULT 3;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "sort" int4 NOT NULL DEFAULT 0;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "status" int4 NOT NULL DEFAULT 1;
            ALTER TABLE IF EXISTS "sys_ai_task" ADD COLUMN IF NOT EXISTS "remark" varchar(500) NULL;

            ALTER TABLE IF EXISTS "sys_ai_task_run" ADD COLUMN IF NOT EXISTS "ai_task_id" int8 NOT NULL DEFAULT 0;
            ALTER TABLE IF EXISTS "sys_ai_task_run" ADD COLUMN IF NOT EXISTS "ai_task_code" varchar(100) NOT NULL DEFAULT '';
            ALTER TABLE IF EXISTS "sys_ai_task_run" ADD COLUMN IF NOT EXISTS "started_time" timestamptz NOT NULL DEFAULT now();
            ALTER TABLE IF EXISTS "sys_ai_task_run" ADD COLUMN IF NOT EXISTS "ended_time" timestamptz NULL;
            ALTER TABLE IF EXISTS "sys_ai_task_run" ADD COLUMN IF NOT EXISTS "run_status" int4 NOT NULL DEFAULT 0;
            ALTER TABLE IF EXISTS "sys_ai_task_run" ADD COLUMN IF NOT EXISTS "prompt_snapshot" text NULL;
            ALTER TABLE IF EXISTS "sys_ai_task_run" ADD COLUMN IF NOT EXISTS "result_text" text NULL;
            ALTER TABLE IF EXISTS "sys_ai_task_run" ADD COLUMN IF NOT EXISTS "error_message" text NULL;
            ALTER TABLE IF EXISTS "sys_ai_task_run" ADD COLUMN IF NOT EXISTS "duration_milliseconds" int8 NULL;
            ALTER TABLE IF EXISTS "sys_ai_task_run" ADD COLUMN IF NOT EXISTS "lease_owner" varchar(200) NULL;
            ALTER TABLE IF EXISTS "sys_ai_task_run" ADD COLUMN IF NOT EXISTS "lease_expires_at" timestamptz NULL;
            ALTER TABLE IF EXISTS "sys_ai_task_run" ADD COLUMN IF NOT EXISTS "last_heartbeat_time" timestamptz NULL;
            ALTER TABLE IF EXISTS "sys_ai_task_run" ADD COLUMN IF NOT EXISTS "attempt_count" int4 NOT NULL DEFAULT 0;

            CREATE TABLE IF NOT EXISTS "sys_ai_task_run_event" (
                "basic_id" int8 NOT NULL,
                "tenant_id" int8 NOT NULL DEFAULT 0,
                "created_time" timestamptz NOT NULL DEFAULT now(),
                "created_by" int8 NULL,
                "modified_time" timestamptz NULL,
                "modified_by" int8 NULL,
                "is_deleted" bool NOT NULL DEFAULT false,
                "deleted_time" timestamptz NULL,
                "deleted_by" int8 NULL,
                "run_id" int8 NOT NULL,
                "sequence" int8 NOT NULL,
                "event_type" int4 NOT NULL,
                "role" int4 NOT NULL DEFAULT 0,
                "content" text NULL,
                "payload_json" text NULL,
                CONSTRAINT "pk_sys_ai_task_run_event" PRIMARY KEY ("basic_id")
            );
            CREATE INDEX IF NOT EXISTS "ix_sys_ai_task_run_event_tenant_run_seq"
                ON "sys_ai_task_run_event" ("tenant_id", "run_id", "sequence");

            ALTER TABLE IF EXISTS "sys_ai_task_tool_policy" ADD COLUMN IF NOT EXISTS "ai_task_id" int8 NOT NULL DEFAULT 0;
            ALTER TABLE IF EXISTS "sys_ai_task_tool_policy" ADD COLUMN IF NOT EXISTS "tool_id" int8 NOT NULL DEFAULT 0;
            ALTER TABLE IF EXISTS "sys_ai_task_tool_policy" ADD COLUMN IF NOT EXISTS "remark" varchar(500) NULL;

            ALTER TABLE IF EXISTS "sys_ai_tool" ADD COLUMN IF NOT EXISTS "tool_code" varchar(100) NOT NULL DEFAULT '';
            ALTER TABLE IF EXISTS "sys_ai_tool" ADD COLUMN IF NOT EXISTS "tool_name" varchar(200) NOT NULL DEFAULT '';
            ALTER TABLE IF EXISTS "sys_ai_tool" ADD COLUMN IF NOT EXISTS "tool_type" int4 NOT NULL DEFAULT 0;
            ALTER TABLE IF EXISTS "sys_ai_tool" ADD COLUMN IF NOT EXISTS "source_key" varchar(200) NOT NULL DEFAULT '';
            ALTER TABLE IF EXISTS "sys_ai_tool" ADD COLUMN IF NOT EXISTS "category" varchar(100) NULL;
            ALTER TABLE IF EXISTS "sys_ai_tool" ADD COLUMN IF NOT EXISTS "description" varchar(500) NULL;
            ALTER TABLE IF EXISTS "sys_ai_tool" ADD COLUMN IF NOT EXISTS "risk_level" int4 NOT NULL DEFAULT 0;
            ALTER TABLE IF EXISTS "sys_ai_tool" ADD COLUMN IF NOT EXISTS "safety_level" int4 NOT NULL DEFAULT 0;
            ALTER TABLE IF EXISTS "sys_ai_tool" ADD COLUMN IF NOT EXISTS "status" int4 NOT NULL DEFAULT 1;
            ALTER TABLE IF EXISTS "sys_ai_tool" ADD COLUMN IF NOT EXISTS "remark" varchar(500) NULL;
            """;

        await DbClient.Ado.ExecuteCommandAsync(sql);
        Logger.LogInformation("AI 任务当前结构补齐完成");
    }
}
