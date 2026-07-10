# AI Task Agent Runtime Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the production foundation for AI Task run observability, live run preview, non-blocking prompt guidance, and configurable runner boundaries without changing the existing AI chat feature.

**Architecture:** Persist AI Task run events and guidance as AI-module-owned records, publish live updates through the existing notification SignalR channel, and refactor task execution behind `IAiTaskAgentRunner`. The first implementation keeps the current one-shot chat behavior as `PlainChat`, while snapshotting runner metadata so Microsoft Agent Framework can be added later as a separate adapter.

**Tech Stack:** .NET 10, XiHan.Framework Dynamic API application services, SqlSugar entities/repositories, existing Saas SignalR notification hub, Vue 3, TypeScript, Naive UI, `useSignalR`, existing `frontend/src/api/modules/ai` typed API modules.

## Global Constraints

- AI Task is automation-first: additional user input is guidance, not a required reply.
- Future interactive AI Agent behavior must stay separate from AI Task and can later reuse the same persistence primitives.
- Existing AI conversation/chat API, storage, UI route, and chat SignalR hub must not be modified for this feature.
- Persisted run events are the source of truth; SignalR is only live delivery.
- The UI must show auditable execution records, not hidden model chain-of-thought.
- Microsoft Agent Framework is a configurable future runner adapter, not the AI Task domain model.
- Do not add a Microsoft Agent Framework package in this plan; this plan only creates the extension point.
- Preserve `/api/AiTask/Run` caller behavior: it creates/enqueues a run and returns the run id/status without waiting for final AI output.
- Work against the current dirty workspace state that already contains async run/recovery changes; do not revert unrelated changes.

---

## Scope Boundary

This plan implements Phase 1 and Phase 2 from the design:

- run event persistence;
- guidance persistence and append API;
- real-time run detail preview;
- `IAiTaskAgentRunner`;
- `PlainChatAiTaskRunner`;
- runner metadata snapshot fields.

This plan does not implement:

- real Microsoft Agent Framework execution;
- tool-call loop execution;
- `WaitingForUserInput`;
- future interactive AI Agent product UI.

## File Structure

Backend new files:

- `backend/src/modules/XiHan.BasicApp.AI/Domain/Entities/SysAiTaskRunEvent.cs`: persisted append-only event row.
- `backend/src/modules/XiHan.BasicApp.AI/Domain/Entities/SysAiTaskRunGuidance.cs`: persisted user guidance row.
- `backend/src/modules/XiHan.BasicApp.AI/Domain/Repositories/IAiTaskRunEventRepository.cs`: event repository contract.
- `backend/src/modules/XiHan.BasicApp.AI/Domain/Repositories/IAiTaskRunGuidanceRepository.cs`: guidance repository contract.
- `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Repositories/AiTaskRunEventRepository.cs`: SqlSugar event repository.
- `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Repositories/AiTaskRunGuidanceRepository.cs`: SqlSugar guidance repository.
- `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Configuration/AiTaskRuntimeOptions.cs`: runner configuration options.
- `backend/src/modules/XiHan.BasicApp.AI/Application/Services/IAiTaskRunEventService.cs`: event append/publish service contract.
- `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskRunEventService.cs`: event append and SignalR publish service.
- `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskRuntimeModels.cs`: runner context/result models.
- `backend/src/modules/XiHan.BasicApp.AI/Application/Services/IAiTaskAgentRunner.cs`: runner interface.
- `backend/src/modules/XiHan.BasicApp.AI/Application/Services/PlainChatAiTaskRunner.cs`: current chat behavior behind runner abstraction.
- `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskRunnerResolver.cs`: resolves configured runner.

Backend modified files:

- `backend/src/modules/XiHan.BasicApp.AI/Domain/Enums/AiTaskEnums.cs`: add runner, event, event role, and guidance status enums.
- `backend/src/modules/XiHan.BasicApp.AI/Domain/Entities/SysAiTaskRun.cs`: add runner snapshot fields.
- `backend/src/modules/XiHan.BasicApp.AI/Domain/Repositories/IAiTaskRunRepository.cs`: extend completion methods with runner snapshot updates if needed.
- `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Repositories/AiTaskRunRepository.cs`: persist runner metadata.
- `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Seeders/System/AiTaskSchemaSeeder.cs`: add columns and create event/guidance tables idempotently.
- `backend/src/modules/XiHan.BasicApp.AI/Application/Dtos/AiTaskDtos.cs`: add event/guidance DTOs and run detail collections/metadata.
- `backend/src/modules/XiHan.BasicApp.AI/Application/Contracts/AiTaskContracts.cs`: add `AppendRunGuidanceAsync` and `GetRunEventsAsync`.
- `backend/src/modules/XiHan.BasicApp.AI/Application/AppServices/AiTaskAppService.cs`: add guidance command method.
- `backend/src/modules/XiHan.BasicApp.AI/Application/QueryServices/AiTaskQueryService.cs`: return run events and include run detail events/guidance.
- `backend/src/modules/XiHan.BasicApp.AI/Application/Mappers/AiTaskApplicationMapper.cs`: map new entities/fields.
- `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskExecutor.cs`: emit events, apply guidance checkpoints, use runner resolver.
- `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskRunRecoveryService.cs`: emit recovery requeue/fail events.
- `backend/src/modules/XiHan.BasicApp.AI/Extensions/ServiceCollectionExtensions.cs`: register new repositories/services/options/runners.

Backend tests:

- `backend/test/XiHan.BasicApp.AI.Tests/AiTaskRunEventTests.cs`: event sequencing, mapping, query filtering.
- `backend/test/XiHan.BasicApp.AI.Tests/AiTaskRunGuidanceTests.cs`: guidance append/idempotency/state rules.
- `backend/test/XiHan.BasicApp.AI.Tests/AiTaskAgentRunnerTests.cs`: runner resolution and PlainChat prompt guidance behavior.
- Modify `backend/test/XiHan.BasicApp.AI.Tests/AiTaskJobExecutorTests.cs`: event emission around execution.
- Modify `backend/test/XiHan.BasicApp.AI.Tests/AiTaskTestDoubles.cs`: in-memory event/guidance repos and fake event service.

Frontend modified files:

- `frontend/src/api/modules/ai/task.enums.ts`: add event/guidance/runner enums.
- `frontend/src/api/modules/ai/task.types.ts`: add event/guidance DTOs and extend run detail.
- `frontend/src/api/modules/ai/task.ts`: add `runEvents` and `appendRunGuidance`.
- `frontend/src/views/develop/ai-task/index.vue`: show event timeline, guidance input, live event subscription, and reconnect fetch.
- `frontend/packages/locales/langs/zh-CN/develop.ts`: add Chinese labels/messages.
- `frontend/packages/locales/langs/en-US/develop.ts`: add English labels/messages.

---

### Task 1: Backend Run Event Domain And Repository

**Files:**
- Create: `backend/src/modules/XiHan.BasicApp.AI/Domain/Entities/SysAiTaskRunEvent.cs`
- Create: `backend/src/modules/XiHan.BasicApp.AI/Domain/Repositories/IAiTaskRunEventRepository.cs`
- Create: `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Repositories/AiTaskRunEventRepository.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Domain/Enums/AiTaskEnums.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Seeders/System/AiTaskSchemaSeeder.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Extensions/ServiceCollectionExtensions.cs`
- Modify: `backend/test/XiHan.BasicApp.AI.Tests/AiTaskTestDoubles.cs`
- Create: `backend/test/XiHan.BasicApp.AI.Tests/AiTaskRunEventTests.cs`

**Interfaces:**
- Produces: `SysAiTaskRunEvent`, `AiTaskRunEventType`, `AiTaskRunEventRole`, `IAiTaskRunEventRepository.AddAsync`, `IAiTaskRunEventRepository.GetByRunIdAsync`.
- Consumes: existing `BasicAppFullAuditedEntity`, `SaasRepository<T>`, `ISqlSugarClientResolver`.

- [ ] **Step 1: Add failing event repository tests**

Add `backend/test/XiHan.BasicApp.AI.Tests/AiTaskRunEventTests.cs`:

```csharp
using XiHan.BasicApp.AI.Application.Mappers;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using Xunit;

namespace XiHan.BasicApp.AI.Tests;

public sealed class AiTaskRunEventTests
{
    [Fact]
    public async Task AddAsync_assigns_monotonic_sequence_per_run()
    {
        var repository = new InMemoryAiTaskRunEventRepository();

        var first = await repository.AddAsync(new SysAiTaskRunEvent
        {
            RunId = 10,
            EventType = AiTaskRunEventType.RunQueued,
            Role = AiTaskRunEventRole.System,
            Content = "queued"
        });
        var second = await repository.AddAsync(new SysAiTaskRunEvent
        {
            RunId = 10,
            EventType = AiTaskRunEventType.RunStarted,
            Role = AiTaskRunEventRole.System,
            Content = "started"
        });
        var otherRun = await repository.AddAsync(new SysAiTaskRunEvent
        {
            RunId = 11,
            EventType = AiTaskRunEventType.RunQueued,
            Role = AiTaskRunEventRole.System,
            Content = "queued"
        });

        Assert.Equal(1, first.Sequence);
        Assert.Equal(2, second.Sequence);
        Assert.Equal(1, otherRun.Sequence);
    }

    [Fact]
    public async Task GetByRunIdAsync_returns_events_after_sequence()
    {
        var repository = new InMemoryAiTaskRunEventRepository();
        await repository.AddAsync(new SysAiTaskRunEvent { RunId = 10, EventType = AiTaskRunEventType.RunQueued, Role = AiTaskRunEventRole.System });
        await repository.AddAsync(new SysAiTaskRunEvent { RunId = 10, EventType = AiTaskRunEventType.RunStarted, Role = AiTaskRunEventRole.System });
        await repository.AddAsync(new SysAiTaskRunEvent { RunId = 10, EventType = AiTaskRunEventType.PromptRendered, Role = AiTaskRunEventRole.System });

        var result = await repository.GetByRunIdAsync(10, afterSequence: 1);

        Assert.Equal([2, 3], result.Select(e => e.Sequence).ToArray());
    }

    [Fact]
    public void ToRunEventDto_maps_auditable_event_fields()
    {
        var created = DateTimeOffset.Parse("2026-07-10T08:00:00+08:00");
        var dto = AiTaskApplicationMapper.ToRunEventDto(new SysAiTaskRunEvent(55)
        {
            RunId = 10,
            Sequence = 3,
            EventType = AiTaskRunEventType.PromptRendered,
            Role = AiTaskRunEventRole.System,
            Content = "prompt rendered",
            PayloadJson = "{\"length\":42}",
            CreatedTime = created
        });

        Assert.Equal(55, dto.BasicId);
        Assert.Equal(10, dto.RunId);
        Assert.Equal(3, dto.Sequence);
        Assert.Equal(AiTaskRunEventType.PromptRendered, dto.EventType);
        Assert.Equal(AiTaskRunEventRole.System, dto.Role);
        Assert.Equal("prompt rendered", dto.Content);
        Assert.Equal("{\"length\":42}", dto.PayloadJson);
        Assert.Equal(created, dto.CreatedTime);
    }
}
```

- [ ] **Step 2: Run the new tests and verify they fail**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore --filter AiTaskRunEventTests
```

Expected: FAIL because `InMemoryAiTaskRunEventRepository`, `SysAiTaskRunEvent`, `AiTaskRunEventType`, `AiTaskRunEventRole`, and `ToRunEventDto` do not exist.

- [ ] **Step 3: Add event enums**

Append these enums to `backend/src/modules/XiHan.BasicApp.AI/Domain/Enums/AiTaskEnums.cs`:

```csharp
/// <summary>
/// AI 任务运行事件类型
/// </summary>
public enum AiTaskRunEventType
{
    [Description("已排队")]
    RunQueued = 0,

    [Description("开始执行")]
    RunStarted = 1,

    [Description("提示词已渲染")]
    PromptRendered = 2,

    [Description("Agent 步骤开始")]
    AgentStepStarted = 3,

    [Description("Agent 消息片段")]
    AgentMessageDelta = 4,

    [Description("工具调用开始")]
    ToolCallStarted = 5,

    [Description("工具调用完成")]
    ToolCallFinished = 6,

    [Description("收到运行引导")]
    GuidanceReceived = 7,

    [Description("运行引导已应用")]
    GuidanceApplied = 8,

    [Description("运行引导已忽略")]
    GuidanceIgnored = 9,

    [Description("执行成功")]
    RunSucceeded = 10,

    [Description("执行失败")]
    RunFailed = 11,

    [Description("执行取消")]
    RunCanceled = 12,

    [Description("已重新排队")]
    RunRequeued = 13
}

/// <summary>
/// AI 任务运行事件角色
/// </summary>
public enum AiTaskRunEventRole
{
    [Description("系统")]
    System = 0,

    [Description("用户")]
    User = 1,

    [Description("AI")]
    Assistant = 2,

    [Description("工具")]
    Tool = 3
}
```

- [ ] **Step 4: Add `SysAiTaskRunEvent` entity**

Create `backend/src/modules/XiHan.BasicApp.AI/Domain/Entities/SysAiTaskRunEvent.cs`:

```csharp
#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:SysAiTaskRunEvent
// Guid:0f04bd1a-f84a-4c5c-95f4-cc2c9b63b9d1
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using SqlSugar;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Core.Entities;

namespace XiHan.BasicApp.AI.Domain.Entities;

/// <summary>
/// AI 任务运行事件
/// </summary>
[SugarTable(TableName = "Sys_Ai_Task_Run_Event", TableDescription = "系统 AI 任务运行事件表")]
[SugarIndex("IX_{table}_TeId_RunId_Seq", nameof(TenantId), OrderByType.Asc, nameof(RunId), OrderByType.Asc, nameof(Sequence), OrderByType.Asc)]
public partial class SysAiTaskRunEvent : BasicAppFullAuditedEntity
{
    public SysAiTaskRunEvent()
    {
    }

    public SysAiTaskRunEvent(long basicId)
        : base(basicId)
    {
    }

    [SugarColumn(ColumnName = "Run_Id", ColumnDescription = "运行记录主键")]
    public virtual long RunId { get; set; }

    [SugarColumn(ColumnName = "Sequence", ColumnDescription = "运行内事件序号")]
    public virtual long Sequence { get; set; }

    [SugarColumn(ColumnName = "Event_Type", ColumnDescription = "事件类型")]
    public virtual AiTaskRunEventType EventType { get; set; }

    [SugarColumn(ColumnName = "Role", ColumnDescription = "事件角色")]
    public virtual AiTaskRunEventRole Role { get; set; } = AiTaskRunEventRole.System;

    [SugarColumn(ColumnName = "Content", ColumnDescription = "事件内容", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public virtual string? Content { get; set; }

    [SugarColumn(ColumnName = "Payload_Json", ColumnDescription = "事件载荷JSON", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public virtual string? PayloadJson { get; set; }
}
```

- [ ] **Step 5: Add event repository contract**

Create `backend/src/modules/XiHan.BasicApp.AI/Domain/Repositories/IAiTaskRunEventRepository.cs`:

```csharp
#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:IAiTaskRunEventRepository
// Guid:215d1494-0f2c-4141-a529-6e47d1787b89
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;

namespace XiHan.BasicApp.AI.Domain.Repositories;

/// <summary>
/// AI 任务运行事件仓储接口
/// </summary>
public interface IAiTaskRunEventRepository
{
    Task<SysAiTaskRunEvent> AddAsync(SysAiTaskRunEvent entity, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SysAiTaskRunEvent>> GetByRunIdAsync(long runId, long afterSequence = 0, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 6: Add SqlSugar event repository**

Create `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Repositories/AiTaskRunEventRepository.cs`:

```csharp
#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskRunEventRepository
// Guid:2c834d80-2e88-4d3a-a4c7-4bf0384be707
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.BasicApp.Saas.Infrastructure.Repositories;
using XiHan.Framework.Data.SqlSugar.Clients;

namespace XiHan.BasicApp.AI.Infrastructure.Repositories;

/// <summary>
/// AI 任务运行事件仓储实现
/// </summary>
public sealed class AiTaskRunEventRepository(ISqlSugarClientResolver clientResolver)
    : SaasRepository<SysAiTaskRunEvent>(clientResolver), IAiTaskRunEventRepository
{
    public async Task<SysAiTaskRunEvent> AddAsync(SysAiTaskRunEvent entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (entity.RunId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(entity), "AI 任务运行记录主键必须大于 0。");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var maxSequence = await CreateQueryable()
            .Where(e => e.RunId == entity.RunId)
            .MaxAsync(e => (long?)e.Sequence, cancellationToken) ?? 0;

        entity.Sequence = maxSequence + 1;
        return await base.AddAsync(entity, cancellationToken);
    }

    public async Task<IReadOnlyList<SysAiTaskRunEvent>> GetByRunIdAsync(long runId, long afterSequence = 0, CancellationToken cancellationToken = default)
    {
        if (runId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(runId), "AI 任务运行记录主键必须大于 0。");
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await CreateQueryable()
            .Where(e => e.RunId == runId && e.Sequence > afterSequence)
            .OrderBy(e => e.Sequence)
            .ToListAsync(cancellationToken);
    }
}
```

- [ ] **Step 7: Update schema seeder for event table**

In `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Seeders/System/AiTaskSchemaSeeder.cs`, add this SQL inside the existing raw SQL string:

```sql
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
```

- [ ] **Step 8: Register repository**

In `backend/src/modules/XiHan.BasicApp.AI/Extensions/ServiceCollectionExtensions.cs`, inside `AddAiTaskServices`, add:

```csharp
services.AddScoped<IAiTaskRunEventRepository, AiTaskRunEventRepository>();
```

- [ ] **Step 9: Add in-memory event repository test double**

Append to `backend/test/XiHan.BasicApp.AI.Tests/AiTaskTestDoubles.cs`:

```csharp
internal sealed class InMemoryAiTaskRunEventRepository : IAiTaskRunEventRepository
{
    public List<SysAiTaskRunEvent> Events { get; } = [];

    public Task<SysAiTaskRunEvent> AddAsync(SysAiTaskRunEvent entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        entity.Sequence = Events.Where(e => e.RunId == entity.RunId).Select(e => e.Sequence).DefaultIfEmpty().Max() + 1;
        if (entity.BasicId <= 0)
        {
            entity = new SysAiTaskRunEvent(Events.Count + 1)
            {
                RunId = entity.RunId,
                Sequence = entity.Sequence,
                EventType = entity.EventType,
                Role = entity.Role,
                Content = entity.Content,
                PayloadJson = entity.PayloadJson,
                CreatedTime = entity.CreatedTime == default ? DateTimeOffset.Now : entity.CreatedTime
            };
        }

        Events.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<IReadOnlyList<SysAiTaskRunEvent>> GetByRunIdAsync(long runId, long afterSequence = 0, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<SysAiTaskRunEvent>>(Events
            .Where(e => e.RunId == runId && e.Sequence > afterSequence)
            .OrderBy(e => e.Sequence)
            .ToList());
    }
}
```

- [ ] **Step 10: Run tests**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore --filter AiTaskRunEventTests
```

Expected: PASS.

- [ ] **Step 11: Commit**

```bash
git add backend/src/modules/XiHan.BasicApp.AI/Domain/Entities/SysAiTaskRunEvent.cs \
  backend/src/modules/XiHan.BasicApp.AI/Domain/Repositories/IAiTaskRunEventRepository.cs \
  backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Repositories/AiTaskRunEventRepository.cs \
  backend/src/modules/XiHan.BasicApp.AI/Domain/Enums/AiTaskEnums.cs \
  backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Seeders/System/AiTaskSchemaSeeder.cs \
  backend/src/modules/XiHan.BasicApp.AI/Extensions/ServiceCollectionExtensions.cs \
  backend/test/XiHan.BasicApp.AI.Tests/AiTaskTestDoubles.cs \
  backend/test/XiHan.BasicApp.AI.Tests/AiTaskRunEventTests.cs
git commit -m "feat: add ai task run events"
```

---

### Task 2: Backend Guidance Domain, Repository, And Command API

**Files:**
- Create: `backend/src/modules/XiHan.BasicApp.AI/Domain/Entities/SysAiTaskRunGuidance.cs`
- Create: `backend/src/modules/XiHan.BasicApp.AI/Domain/Repositories/IAiTaskRunGuidanceRepository.cs`
- Create: `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Repositories/AiTaskRunGuidanceRepository.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Domain/Enums/AiTaskEnums.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Seeders/System/AiTaskSchemaSeeder.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/Dtos/AiTaskDtos.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/Contracts/AiTaskContracts.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/AppServices/AiTaskAppService.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/Mappers/AiTaskApplicationMapper.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Extensions/ServiceCollectionExtensions.cs`
- Modify: `backend/test/XiHan.BasicApp.AI.Tests/AiTaskTestDoubles.cs`
- Create: `backend/test/XiHan.BasicApp.AI.Tests/AiTaskRunGuidanceTests.cs`

**Interfaces:**
- Produces: `AiTaskAppendGuidanceDto`, `AiTaskRunGuidanceDto`, `IAiTaskAppService.AppendRunGuidanceAsync`.
- Consumes: `IAiTaskRunRepository.GetByIdAsync`, `IAiTaskRunEventRepository.AddAsync`.

- [ ] **Step 1: Add failing guidance command tests**

Create `backend/test/XiHan.BasicApp.AI.Tests/AiTaskRunGuidanceTests.cs`:

```csharp
using XiHan.BasicApp.AI.Application.AppServices;
using XiHan.BasicApp.AI.Application.Dtos;
using XiHan.BasicApp.AI.Application.Services;
using XiHan.BasicApp.AI.Domain.DomainServices.Implementations;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Saas.Domain.Enums;
using Xunit;

namespace XiHan.BasicApp.AI.Tests;

public sealed class AiTaskRunGuidanceTests
{
    [Fact]
    public async Task AppendRunGuidanceAsync_accepts_guidance_for_running_run()
    {
        var runRepository = new InMemoryAiTaskRunRepository();
        runRepository.Runs.Add(new SysAiTaskRun(42)
        {
            AiTaskId = 7,
            AiTaskCode = "daily",
            RunStatus = AiTaskRunStatus.Running
        });
        var guidanceRepository = new InMemoryAiTaskRunGuidanceRepository();
        var eventService = new FakeAiTaskRunEventService();
        var appService = CreateAppService(runRepository: runRepository, guidanceRepository: guidanceRepository, runEventService: eventService);

        var result = await appService.AppendRunGuidanceAsync(new AiTaskAppendGuidanceDto
        {
            RunId = 42,
            Content = "Focus on overdue items.",
            ClientRequestId = "client-1"
        });

        Assert.Equal(42, result.RunId);
        Assert.Equal(AiTaskRunGuidanceStatus.Pending, result.Status);
        Assert.Equal("Focus on overdue items.", result.Content);
        Assert.Contains(eventService.Events, e => e.RunId == 42 && e.EventType == AiTaskRunEventType.GuidanceReceived);
    }

    [Fact]
    public async Task AppendRunGuidanceAsync_is_idempotent_by_client_request_id()
    {
        var runRepository = new InMemoryAiTaskRunRepository();
        runRepository.Runs.Add(new SysAiTaskRun(42) { AiTaskId = 7, AiTaskCode = "daily", RunStatus = AiTaskRunStatus.Queued });
        var guidanceRepository = new InMemoryAiTaskRunGuidanceRepository();
        var appService = CreateAppService(runRepository: runRepository, guidanceRepository: guidanceRepository);

        var first = await appService.AppendRunGuidanceAsync(new AiTaskAppendGuidanceDto { RunId = 42, Content = "A", ClientRequestId = "same" });
        var second = await appService.AppendRunGuidanceAsync(new AiTaskAppendGuidanceDto { RunId = 42, Content = "A", ClientRequestId = "same" });

        Assert.Equal(first.BasicId, second.BasicId);
        Assert.Single(guidanceRepository.Guidance);
    }

    [Fact]
    public async Task AppendRunGuidanceAsync_rejects_completed_run()
    {
        var runRepository = new InMemoryAiTaskRunRepository();
        runRepository.Runs.Add(new SysAiTaskRun(42) { AiTaskId = 7, AiTaskCode = "daily", RunStatus = AiTaskRunStatus.Success });
        var appService = CreateAppService(runRepository: runRepository);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            appService.AppendRunGuidanceAsync(new AiTaskAppendGuidanceDto { RunId = 42, Content = "Too late." }));

        Assert.Equal("AI 任务运行已结束，不能追加引导。", ex.Message);
    }

    private static AiTaskAppService CreateAppService(
        InMemoryAiTaskRunRepository? runRepository = null,
        InMemoryAiTaskRunGuidanceRepository? guidanceRepository = null,
        FakeAiTaskRunEventService? runEventService = null)
    {
        var taskRepository = new InMemoryAiTaskRepository(new SysAiTask(7)
        {
            AiTaskCode = "daily",
            AiTaskName = "Daily",
            PromptMode = AiTaskPromptMode.Inline,
            PromptText = "Base prompt.",
            Status = EnableStatus.Enabled
        });
        runRepository ??= new InMemoryAiTaskRunRepository();
        guidanceRepository ??= new InMemoryAiTaskRunGuidanceRepository();
        runEventService ??= new FakeAiTaskRunEventService();
        var executor = new AiTaskExecutor(
            taskRepository,
            runRepository,
            new FakeAiTaskChatService("unused"),
            new AiTaskPromptRenderer(new FakeAiPromptStore()));

        return new AiTaskAppService(
            new AiTaskDomainService(taskRepository),
            taskRepository,
            new InMemoryAiTaskToolPolicyRepository([]),
            new InMemoryAiToolRepository(new SysAiTool(11)
            {
                ToolCode = "knowledge.search",
                ToolName = "Knowledge Search",
                Status = EnableStatus.Enabled
            }),
            new FakeAiTaskBackingTaskSyncService(),
            executor,
            new FakeAiTaskRunQueue(),
            runRepository,
            guidanceRepository,
            runEventService);
    }
}
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore --filter AiTaskRunGuidanceTests
```

Expected: FAIL because guidance entity/repository/DTO/API do not exist.

- [ ] **Step 3: Add guidance status enum**

Append to `backend/src/modules/XiHan.BasicApp.AI/Domain/Enums/AiTaskEnums.cs`:

```csharp
/// <summary>
/// AI 任务运行引导状态
/// </summary>
public enum AiTaskRunGuidanceStatus
{
    [Description("待应用")]
    Pending = 0,

    [Description("已应用")]
    Applied = 1,

    [Description("已忽略")]
    Ignored = 2
}
```

- [ ] **Step 4: Add guidance entity**

Create `backend/src/modules/XiHan.BasicApp.AI/Domain/Entities/SysAiTaskRunGuidance.cs`:

```csharp
#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:SysAiTaskRunGuidance
// Guid:6f217f60-1098-42d8-91b5-293404fc3dc2
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using SqlSugar;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Core.Entities;

namespace XiHan.BasicApp.AI.Domain.Entities;

/// <summary>
/// AI 任务运行引导
/// </summary>
[SugarTable(TableName = "Sys_Ai_Task_Run_Guidance", TableDescription = "系统 AI 任务运行引导表")]
[SugarIndex("IX_{table}_TeId_RunId_CrTi", nameof(TenantId), OrderByType.Asc, nameof(RunId), OrderByType.Asc, nameof(CreatedTime), OrderByType.Asc)]
[SugarIndex("IX_{table}_TeId_RunId_ClientReq", nameof(TenantId), OrderByType.Asc, nameof(RunId), OrderByType.Asc, nameof(ClientRequestId), OrderByType.Asc)]
public partial class SysAiTaskRunGuidance : BasicAppFullAuditedEntity
{
    public SysAiTaskRunGuidance()
    {
    }

    public SysAiTaskRunGuidance(long basicId)
        : base(basicId)
    {
    }

    [SugarColumn(ColumnName = "Run_Id", ColumnDescription = "运行记录主键")]
    public virtual long RunId { get; set; }

    [SugarColumn(ColumnName = "Content", ColumnDescription = "引导内容", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = false)]
    public virtual string Content { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "Status", ColumnDescription = "引导状态")]
    public virtual AiTaskRunGuidanceStatus Status { get; set; } = AiTaskRunGuidanceStatus.Pending;

    [SugarColumn(ColumnName = "Client_Request_Id", ColumnDescription = "客户端幂等键", Length = 100, IsNullable = true)]
    public virtual string? ClientRequestId { get; set; }

    [SugarColumn(ColumnName = "Applied_Time", ColumnDescription = "应用时间", IsNullable = true)]
    public virtual DateTimeOffset? AppliedTime { get; set; }

    [SugarColumn(ColumnName = "Ignored_Reason", ColumnDescription = "忽略原因", Length = 500, IsNullable = true)]
    public virtual string? IgnoredReason { get; set; }
}
```

- [ ] **Step 5: Add guidance repository contract and implementation**

Create `backend/src/modules/XiHan.BasicApp.AI/Domain/Repositories/IAiTaskRunGuidanceRepository.cs`:

```csharp
using XiHan.BasicApp.AI.Domain.Entities;

namespace XiHan.BasicApp.AI.Domain.Repositories;

public interface IAiTaskRunGuidanceRepository
{
    Task<SysAiTaskRunGuidance> AddAsync(SysAiTaskRunGuidance entity, CancellationToken cancellationToken = default);

    Task<SysAiTaskRunGuidance?> GetByClientRequestIdAsync(long runId, string clientRequestId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SysAiTaskRunGuidance>> GetByRunIdAsync(long runId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SysAiTaskRunGuidance>> GetPendingByRunIdAsync(long runId, CancellationToken cancellationToken = default);

    Task MarkAppliedAsync(IReadOnlyList<long> ids, DateTimeOffset appliedTime, CancellationToken cancellationToken = default);

    Task MarkIgnoredAsync(IReadOnlyList<long> ids, string reason, CancellationToken cancellationToken = default);
}
```

Create `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Repositories/AiTaskRunGuidanceRepository.cs` with the same header style:

```csharp
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.BasicApp.Saas.Infrastructure.Repositories;
using XiHan.Framework.Data.SqlSugar.Clients;

namespace XiHan.BasicApp.AI.Infrastructure.Repositories;

public sealed class AiTaskRunGuidanceRepository(ISqlSugarClientResolver clientResolver)
    : SaasRepository<SysAiTaskRunGuidance>(clientResolver), IAiTaskRunGuidanceRepository
{
    public new async Task<SysAiTaskRunGuidance> AddAsync(SysAiTaskRunGuidance entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.Content = entity.Content.Trim();
        entity.ClientRequestId = string.IsNullOrWhiteSpace(entity.ClientRequestId) ? null : entity.ClientRequestId.Trim();
        return await base.AddAsync(entity, cancellationToken);
    }

    public async Task<SysAiTaskRunGuidance?> GetByClientRequestIdAsync(long runId, string clientRequestId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await CreateQueryable()
            .Where(g => g.RunId == runId && g.ClientRequestId == clientRequestId.Trim())
            .FirstAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SysAiTaskRunGuidance>> GetByRunIdAsync(long runId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await CreateQueryable()
            .Where(g => g.RunId == runId)
            .OrderBy(g => g.CreatedTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SysAiTaskRunGuidance>> GetPendingByRunIdAsync(long runId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await CreateQueryable()
            .Where(g => g.RunId == runId && g.Status == AiTaskRunGuidanceStatus.Pending)
            .OrderBy(g => g.CreatedTime)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAppliedAsync(IReadOnlyList<long> ids, DateTimeOffset appliedTime, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        await DbClient.Updateable<SysAiTaskRunGuidance>()
            .SetColumns(g => g.Status == AiTaskRunGuidanceStatus.Applied)
            .SetColumns(g => g.AppliedTime == appliedTime)
            .Where(g => ids.Contains(g.BasicId) && g.Status == AiTaskRunGuidanceStatus.Pending && !g.IsDeleted)
            .ExecuteCommandAsync(cancellationToken);
    }

    public async Task MarkIgnoredAsync(IReadOnlyList<long> ids, string reason, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        await DbClient.Updateable<SysAiTaskRunGuidance>()
            .SetColumns(g => g.Status == AiTaskRunGuidanceStatus.Ignored)
            .SetColumns(g => g.IgnoredReason == reason)
            .Where(g => ids.Contains(g.BasicId) && g.Status == AiTaskRunGuidanceStatus.Pending && !g.IsDeleted)
            .ExecuteCommandAsync(cancellationToken);
    }
}
```

- [ ] **Step 6: Add DTOs and mapper methods**

In `backend/src/modules/XiHan.BasicApp.AI/Application/Dtos/AiTaskDtos.cs`, add:

```csharp
public sealed class AiTaskAppendGuidanceDto
{
    public long RunId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ClientRequestId { get; set; }
}

public sealed class AiTaskRunGuidanceDto : BasicAppDto
{
    public long RunId { get; set; }
    public string Content { get; set; } = string.Empty;
    public AiTaskRunGuidanceStatus Status { get; set; }
    public string? ClientRequestId { get; set; }
    public DateTimeOffset? AppliedTime { get; set; }
    public string? IgnoredReason { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
}
```

In `AiTaskRunDetailDto`, add:

```csharp
public string? RunnerKind { get; set; }
public string? RunnerVersion { get; set; }
public string? AgentSessionId { get; set; }
public List<AiTaskRunEventDto> Events { get; set; } = [];
public List<AiTaskRunGuidanceDto> Guidance { get; set; } = [];
```

In `backend/src/modules/XiHan.BasicApp.AI/Application/Mappers/AiTaskApplicationMapper.cs`, add:

```csharp
public static AiTaskRunGuidanceDto ToRunGuidanceDto(SysAiTaskRunGuidance entity)
{
    ArgumentNullException.ThrowIfNull(entity);
    return new AiTaskRunGuidanceDto
    {
        BasicId = entity.BasicId,
        RunId = entity.RunId,
        Content = entity.Content,
        Status = entity.Status,
        ClientRequestId = entity.ClientRequestId,
        AppliedTime = entity.AppliedTime,
        IgnoredReason = entity.IgnoredReason,
        CreatedTime = entity.CreatedTime
    };
}
```

- [ ] **Step 7: Add command contract and implementation**

In `backend/src/modules/XiHan.BasicApp.AI/Application/Contracts/AiTaskContracts.cs`, add to `IAiTaskAppService`:

```csharp
Task<AiTaskRunGuidanceDto> AppendRunGuidanceAsync(AiTaskAppendGuidanceDto input, CancellationToken cancellationToken = default);
```

In `backend/src/modules/XiHan.BasicApp.AI/Application/AppServices/AiTaskAppService.cs`, inject `IAiTaskRunRepository`, `IAiTaskRunGuidanceRepository`, and `IAiTaskRunEventService`.

Constructor tail after Task 2:

```csharp
IAiTaskRunQueue runQueue,
IAiTaskRunRepository runRepository,
IAiTaskRunGuidanceRepository guidanceRepository,
IAiTaskRunEventService runEventService)
```

Assign these fields:

```csharp
_runRepository = runRepository;
_guidanceRepository = guidanceRepository;
_runEventService = runEventService;
```

Then add:

```csharp
[PermissionAuthorize(AiTaskPermissionCodes.Execute)]
[UnitOfWork(true)]
public async Task<AiTaskRunGuidanceDto> AppendRunGuidanceAsync(AiTaskAppendGuidanceDto input, CancellationToken cancellationToken = default)
{
    ArgumentNullException.ThrowIfNull(input);
    if (input.RunId <= 0)
    {
        throw new ArgumentOutOfRangeException(nameof(input), "AI 任务运行记录主键必须大于 0。");
    }

    var content = input.Content.Trim();
    if (string.IsNullOrWhiteSpace(content))
    {
        throw new InvalidOperationException("AI 任务运行引导不能为空。");
    }

    var existing = !string.IsNullOrWhiteSpace(input.ClientRequestId)
        ? await _guidanceRepository.GetByClientRequestIdAsync(input.RunId, input.ClientRequestId.Trim(), cancellationToken)
        : null;
    if (existing is not null)
    {
        return AiTaskApplicationMapper.ToRunGuidanceDto(existing);
    }

    var run = await _runRepository.GetByIdAsync(input.RunId, cancellationToken)
        ?? throw new InvalidOperationException("AI 任务运行记录不存在。");
    if (run.RunStatus is not AiTaskRunStatus.Queued and not AiTaskRunStatus.Running)
    {
        throw new InvalidOperationException("AI 任务运行已结束，不能追加引导。");
    }

    var guidance = await _guidanceRepository.AddAsync(new SysAiTaskRunGuidance
    {
        RunId = input.RunId,
        Content = content,
        Status = AiTaskRunGuidanceStatus.Pending,
        ClientRequestId = input.ClientRequestId
    }, cancellationToken);

    await _runEventService.AppendAsync(
        input.RunId,
        AiTaskRunEventType.GuidanceReceived,
        AiTaskRunEventRole.User,
        content,
        null,
        cancellationToken);

    return AiTaskApplicationMapper.ToRunGuidanceDto(guidance);
}
```

- [ ] **Step 8: Add schema and registration**

Add guidance table SQL to `AiTaskSchemaSeeder`:

```sql
CREATE TABLE IF NOT EXISTS "sys_ai_task_run_guidance" (
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
    "content" text NOT NULL,
    "status" int4 NOT NULL DEFAULT 0,
    "client_request_id" varchar(100) NULL,
    "applied_time" timestamptz NULL,
    "ignored_reason" varchar(500) NULL,
    CONSTRAINT "pk_sys_ai_task_run_guidance" PRIMARY KEY ("basic_id")
);
CREATE INDEX IF NOT EXISTS "ix_sys_ai_task_run_guidance_tenant_run_created"
    ON "sys_ai_task_run_guidance" ("tenant_id", "run_id", "created_time");
CREATE INDEX IF NOT EXISTS "ix_sys_ai_task_run_guidance_tenant_run_client"
    ON "sys_ai_task_run_guidance" ("tenant_id", "run_id", "client_request_id");
```

Register in `AddAiTaskServices`:

```csharp
services.AddScoped<IAiTaskRunGuidanceRepository, AiTaskRunGuidanceRepository>();
```

- [ ] **Step 9: Add in-memory guidance repo**

Append to `AiTaskTestDoubles.cs`:

```csharp
internal sealed class InMemoryAiTaskRunGuidanceRepository : IAiTaskRunGuidanceRepository
{
    public List<SysAiTaskRunGuidance> Guidance { get; } = [];

    public Task<SysAiTaskRunGuidance> AddAsync(SysAiTaskRunGuidance entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (entity.BasicId <= 0)
        {
            entity = new SysAiTaskRunGuidance(Guidance.Count + 1)
            {
                RunId = entity.RunId,
                Content = entity.Content.Trim(),
                Status = entity.Status,
                ClientRequestId = string.IsNullOrWhiteSpace(entity.ClientRequestId) ? null : entity.ClientRequestId.Trim(),
                CreatedTime = DateTimeOffset.Now
            };
        }

        Guidance.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<SysAiTaskRunGuidance?> GetByClientRequestIdAsync(long runId, string clientRequestId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Guidance.FirstOrDefault(g => g.RunId == runId && g.ClientRequestId == clientRequestId.Trim()));
    }

    public Task<IReadOnlyList<SysAiTaskRunGuidance>> GetByRunIdAsync(long runId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<SysAiTaskRunGuidance>>(Guidance.Where(g => g.RunId == runId).OrderBy(g => g.CreatedTime).ToList());
    }

    public Task<IReadOnlyList<SysAiTaskRunGuidance>> GetPendingByRunIdAsync(long runId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<SysAiTaskRunGuidance>>(Guidance.Where(g => g.RunId == runId && g.Status == AiTaskRunGuidanceStatus.Pending).ToList());
    }

    public Task MarkAppliedAsync(IReadOnlyList<long> ids, DateTimeOffset appliedTime, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (var item in Guidance.Where(g => ids.Contains(g.BasicId) && g.Status == AiTaskRunGuidanceStatus.Pending))
        {
            item.Status = AiTaskRunGuidanceStatus.Applied;
            item.AppliedTime = appliedTime;
        }

        return Task.CompletedTask;
    }

    public Task MarkIgnoredAsync(IReadOnlyList<long> ids, string reason, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (var item in Guidance.Where(g => ids.Contains(g.BasicId) && g.Status == AiTaskRunGuidanceStatus.Pending))
        {
            item.Status = AiTaskRunGuidanceStatus.Ignored;
            item.IgnoredReason = reason;
        }

        return Task.CompletedTask;
    }
}
```

- [ ] **Step 10: Run tests**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore --filter "AiTaskRunGuidanceTests|AiTaskRunEventTests"
```

Expected: PASS.

- [ ] **Step 11: Commit**

```bash
git add backend/src/modules/XiHan.BasicApp.AI/Domain/Entities/SysAiTaskRunGuidance.cs \
  backend/src/modules/XiHan.BasicApp.AI/Domain/Repositories/IAiTaskRunGuidanceRepository.cs \
  backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Repositories/AiTaskRunGuidanceRepository.cs \
  backend/src/modules/XiHan.BasicApp.AI/Domain/Enums/AiTaskEnums.cs \
  backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Seeders/System/AiTaskSchemaSeeder.cs \
  backend/src/modules/XiHan.BasicApp.AI/Application/Dtos/AiTaskDtos.cs \
  backend/src/modules/XiHan.BasicApp.AI/Application/Contracts/AiTaskContracts.cs \
  backend/src/modules/XiHan.BasicApp.AI/Application/AppServices/AiTaskAppService.cs \
  backend/src/modules/XiHan.BasicApp.AI/Application/Mappers/AiTaskApplicationMapper.cs \
  backend/src/modules/XiHan.BasicApp.AI/Extensions/ServiceCollectionExtensions.cs \
  backend/test/XiHan.BasicApp.AI.Tests/AiTaskTestDoubles.cs \
  backend/test/XiHan.BasicApp.AI.Tests/AiTaskRunGuidanceTests.cs
git commit -m "feat: add ai task run guidance"
```

---

### Task 3: Event Service, Query API, And Live SignalR Publishing

**Files:**
- Create: `backend/src/modules/XiHan.BasicApp.AI/Application/Services/IAiTaskRunEventService.cs`
- Create: `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskRunEventService.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/Dtos/AiTaskDtos.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/Contracts/AiTaskContracts.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/QueryServices/AiTaskQueryService.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/Mappers/AiTaskApplicationMapper.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Extensions/ServiceCollectionExtensions.cs`
- Modify: `backend/test/XiHan.BasicApp.AI.Tests/AiTaskTestDoubles.cs`
- Modify: `backend/test/XiHan.BasicApp.AI.Tests/AiTaskRunHistoryQueryTests.cs`

**Interfaces:**
- Produces: `IAiTaskRunEventService.AppendAsync`, `AiTaskRunEventDto`, `IAiTaskQueryService.GetRunEventsAsync`.
- Consumes: `IAiTaskRunEventRepository`, `IRealtimeNotificationService<BasicAppNotificationHub>`.

- [ ] **Step 1: Add failing query tests**

In `backend/test/XiHan.BasicApp.AI.Tests/AiTaskRunHistoryQueryTests.cs`, add:

```csharp
[Fact]
public async Task GetRunEventsAsync_returns_events_after_sequence()
{
    var runRepository = new InMemoryAiTaskRunRepository();
    runRepository.Runs.Add(new SysAiTaskRun(42)
    {
        AiTaskId = 7,
        AiTaskCode = "daily",
        RunStatus = AiTaskRunStatus.Running
    });
    var eventRepository = new InMemoryAiTaskRunEventRepository();
    await eventRepository.AddAsync(new SysAiTaskRunEvent { RunId = 42, EventType = AiTaskRunEventType.RunQueued, Role = AiTaskRunEventRole.System });
    await eventRepository.AddAsync(new SysAiTaskRunEvent { RunId = 42, EventType = AiTaskRunEventType.RunStarted, Role = AiTaskRunEventRole.System });

    var service = CreateQueryService(runRepository: runRepository, runEventRepository: eventRepository);

    var result = await service.GetRunEventsAsync(42, afterSequence: 1);

    Assert.Single(result);
    Assert.Equal(2, result[0].Sequence);
    Assert.Equal(AiTaskRunEventType.RunStarted, result[0].EventType);
}
```

Add this helper inside the existing `AiTaskRunHistoryQueryTests` class:

```csharp
private static AiTaskQueryService CreateQueryService(
    InMemoryAiTaskRunRepository? runRepository = null,
    InMemoryAiTaskRunEventRepository? runEventRepository = null,
    InMemoryAiTaskRunGuidanceRepository? guidanceRepository = null)
{
    return new AiTaskQueryService(
        new InMemoryAiTaskRepository(),
        new InMemoryAiTaskToolPolicyRepository([]),
        new InMemoryAiToolRepository(new SysAiTool(11)
        {
            ToolCode = "knowledge",
            ToolName = "Knowledge",
            Status = EnableStatus.Enabled
        }),
        runRepository ?? new InMemoryAiTaskRunRepository(),
        runEventRepository ?? new InMemoryAiTaskRunEventRepository(),
        guidanceRepository ?? new InMemoryAiTaskRunGuidanceRepository());
}
```

- [ ] **Step 2: Run failing query tests**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore --filter GetRunEventsAsync
```

Expected: FAIL because `GetRunEventsAsync` and `AiTaskRunEventDto` do not exist.

- [ ] **Step 3: Add event DTO and mapper**

In `AiTaskDtos.cs`, add:

```csharp
public sealed class AiTaskRunEventDto : BasicAppDto
{
    public long RunId { get; set; }
    public long Sequence { get; set; }
    public AiTaskRunEventType EventType { get; set; }
    public AiTaskRunEventRole Role { get; set; }
    public string? Content { get; set; }
    public string? PayloadJson { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
}
```

In `AiTaskApplicationMapper.cs`, add:

```csharp
public static AiTaskRunEventDto ToRunEventDto(SysAiTaskRunEvent entity)
{
    ArgumentNullException.ThrowIfNull(entity);
    return new AiTaskRunEventDto
    {
        BasicId = entity.BasicId,
        RunId = entity.RunId,
        Sequence = entity.Sequence,
        EventType = entity.EventType,
        Role = entity.Role,
        Content = entity.Content,
        PayloadJson = entity.PayloadJson,
        CreatedTime = entity.CreatedTime
    };
}
```

- [ ] **Step 4: Add event service**

Create `backend/src/modules/XiHan.BasicApp.AI/Application/Services/IAiTaskRunEventService.cs`:

```csharp
using XiHan.BasicApp.AI.Application.Dtos;
using XiHan.BasicApp.AI.Domain.Enums;

namespace XiHan.BasicApp.AI.Application.Services;

public interface IAiTaskRunEventService
{
    Task<AiTaskRunEventDto> AppendAsync(
        long runId,
        AiTaskRunEventType eventType,
        AiTaskRunEventRole role,
        string? content,
        string? payloadJson,
        CancellationToken cancellationToken = default);
}
```

Create `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskRunEventService.cs`:

```csharp
using Microsoft.Extensions.Logging;
using XiHan.BasicApp.AI.Application.Dtos;
using XiHan.BasicApp.AI.Application.Mappers;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.AI.Domain.Repositories;
using XiHan.BasicApp.Saas.Hubs;
using XiHan.Framework.Web.RealTime.Services;

namespace XiHan.BasicApp.AI.Application.Services;

public sealed class AiTaskRunEventService : IAiTaskRunEventService
{
    public const string ClientMethod = "AiTaskRunEventReceived";

    private readonly IAiTaskRunEventRepository _eventRepository;
    private readonly IRealtimeNotificationService<BasicAppNotificationHub> _realtimeNotificationService;
    private readonly ILogger<AiTaskRunEventService> _logger;

    public AiTaskRunEventService(
        IAiTaskRunEventRepository eventRepository,
        IRealtimeNotificationService<BasicAppNotificationHub> realtimeNotificationService,
        ILogger<AiTaskRunEventService> logger)
    {
        _eventRepository = eventRepository;
        _realtimeNotificationService = realtimeNotificationService;
        _logger = logger;
    }

    public async Task<AiTaskRunEventDto> AppendAsync(
        long runId,
        AiTaskRunEventType eventType,
        AiTaskRunEventRole role,
        string? content,
        string? payloadJson,
        CancellationToken cancellationToken = default)
    {
        var entity = await _eventRepository.AddAsync(new SysAiTaskRunEvent
        {
            RunId = runId,
            EventType = eventType,
            Role = role,
            Content = string.IsNullOrWhiteSpace(content) ? null : content.Trim(),
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? null : payloadJson.Trim()
        }, cancellationToken);

        var dto = AiTaskApplicationMapper.ToRunEventDto(entity);
        try
        {
            await _realtimeNotificationService.SendToAllAsync(ClientMethod, dto);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "推送 AI 任务运行事件失败，RunId={RunId}, Sequence={Sequence}", runId, dto.Sequence);
        }

        return dto;
    }
}
```

This matches the existing `NotificationAppService` usage of `SendToAllAsync(method, payload)`.

- [ ] **Step 5: Add query API**

In `IAiTaskQueryService`, add:

```csharp
Task<IReadOnlyList<AiTaskRunEventDto>> GetRunEventsAsync(long runId, long afterSequence = 0, CancellationToken cancellationToken = default);
```

In `AiTaskQueryService`, inject `IAiTaskRunEventRepository` and `IAiTaskRunGuidanceRepository`, then add:

```csharp
[PermissionAuthorize(AiTaskPermissionCodes.Read)]
public async Task<IReadOnlyList<AiTaskRunEventDto>> GetRunEventsAsync(long runId, long afterSequence = 0, CancellationToken cancellationToken = default)
{
    if (runId <= 0)
    {
        throw new ArgumentOutOfRangeException(nameof(runId), "AI 任务运行记录主键必须大于 0。");
    }

    cancellationToken.ThrowIfCancellationRequested();
    var events = await _runEventRepository.GetByRunIdAsync(runId, afterSequence, cancellationToken);
    return events.Select(AiTaskApplicationMapper.ToRunEventDto).ToList();
}
```

Update `GetRunDetailAsync` so it loads events and guidance:

```csharp
var detail = AiTaskApplicationMapper.ToRunDetailDto(run);
detail.Events = (await _runEventRepository.GetByRunIdAsync(id, 0, cancellationToken))
    .Select(AiTaskApplicationMapper.ToRunEventDto)
    .ToList();
detail.Guidance = (await _guidanceRepository.GetByRunIdAsync(id, cancellationToken))
    .Select(AiTaskApplicationMapper.ToRunGuidanceDto)
    .ToList();
return detail;
```

- [ ] **Step 6: Register event service**

In `AddAiTaskServices`, add:

```csharp
services.AddScoped<IAiTaskRunEventService, AiTaskRunEventService>();
```

- [ ] **Step 7: Add fake event service**

Append to `AiTaskTestDoubles.cs`:

```csharp
internal sealed class FakeAiTaskRunEventService : IAiTaskRunEventService
{
    public List<AiTaskRunEventDto> Events { get; } = [];

    public Task<AiTaskRunEventDto> AppendAsync(long runId, AiTaskRunEventType eventType, AiTaskRunEventRole role, string? content, string? payloadJson, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var dto = new AiTaskRunEventDto
        {
            BasicId = Events.Count + 1,
            RunId = runId,
            Sequence = Events.Count(e => e.RunId == runId) + 1,
            EventType = eventType,
            Role = role,
            Content = content,
            PayloadJson = payloadJson,
            CreatedTime = DateTimeOffset.Now
        };
        Events.Add(dto);
        return Task.FromResult(dto);
    }
}
```

- [ ] **Step 8: Run tests**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore --filter "AiTaskRunEventTests|GetRunEventsAsync"
```

Expected: PASS.

- [ ] **Step 9: Commit**

```bash
git add backend/src/modules/XiHan.BasicApp.AI/Application/Services/IAiTaskRunEventService.cs \
  backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskRunEventService.cs \
  backend/src/modules/XiHan.BasicApp.AI/Application/Dtos/AiTaskDtos.cs \
  backend/src/modules/XiHan.BasicApp.AI/Application/Contracts/AiTaskContracts.cs \
  backend/src/modules/XiHan.BasicApp.AI/Application/QueryServices/AiTaskQueryService.cs \
  backend/src/modules/XiHan.BasicApp.AI/Application/Mappers/AiTaskApplicationMapper.cs \
  backend/src/modules/XiHan.BasicApp.AI/Extensions/ServiceCollectionExtensions.cs \
  backend/test/XiHan.BasicApp.AI.Tests/AiTaskTestDoubles.cs \
  backend/test/XiHan.BasicApp.AI.Tests/AiTaskRunHistoryQueryTests.cs
git commit -m "feat: publish ai task run events"
```

---

### Task 4: Runner Configuration And PlainChat Agent Runner

**Files:**
- Create: `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Configuration/AiTaskRuntimeOptions.cs`
- Create: `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskRuntimeModels.cs`
- Create: `backend/src/modules/XiHan.BasicApp.AI/Application/Services/IAiTaskAgentRunner.cs`
- Create: `backend/src/modules/XiHan.BasicApp.AI/Application/Services/PlainChatAiTaskRunner.cs`
- Create: `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskRunnerResolver.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Domain/Enums/AiTaskEnums.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Domain/Entities/SysAiTaskRun.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Seeders/System/AiTaskSchemaSeeder.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/Dtos/AiTaskDtos.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/Mappers/AiTaskApplicationMapper.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Extensions/ServiceCollectionExtensions.cs`
- Create: `backend/test/XiHan.BasicApp.AI.Tests/AiTaskAgentRunnerTests.cs`

**Interfaces:**
- Produces: `AiTaskRunnerKind`, `IAiTaskAgentRunner.RunAsync`, `AiTaskRunnerResolver.Resolve`.
- Consumes: current `IAiTaskChatService.CompleteAsync`.

- [ ] **Step 1: Add failing runner tests**

Create `backend/test/XiHan.BasicApp.AI.Tests/AiTaskAgentRunnerTests.cs`:

```csharp
using Microsoft.Extensions.Options;
using XiHan.BasicApp.AI.Application.Services;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.AI.Infrastructure.Configuration;
using Xunit;

namespace XiHan.BasicApp.AI.Tests;

public sealed class AiTaskAgentRunnerTests
{
    [Fact]
    public async Task PlainChatAiTaskRunner_merges_pending_guidance_before_chat_call()
    {
        var chat = new FakeAiTaskChatService("done");
        var eventService = new FakeAiTaskRunEventService();
        var runner = new PlainChatAiTaskRunner(chat, eventService);
        var context = new AiTaskRunContext(
            new SysAiTask(7) { AiTaskCode = "daily", AiTaskName = "Daily" },
            new SysAiTaskRun(42) { AiTaskId = 7, AiTaskCode = "daily" },
            "Base prompt",
            [
                new SysAiTaskRunGuidance(1) { RunId = 42, Content = "Focus on overdue items." }
            ]);

        var result = await runner.RunAsync(context);

        Assert.Equal("done", result.ResultText);
        Assert.Contains("Base prompt", chat.LastPrompt);
        Assert.Contains("Additional run guidance", chat.LastPrompt);
        Assert.Contains("Focus on overdue items.", chat.LastPrompt);
        Assert.Contains(eventService.Events, e => e.EventType == AiTaskRunEventType.GuidanceApplied);
    }

    [Fact]
    public void AiTaskRunnerResolver_returns_plain_chat_runner_by_default()
    {
        var plain = new PlainChatAiTaskRunner(new FakeAiTaskChatService("done"), new FakeAiTaskRunEventService());
        var resolver = new AiTaskRunnerResolver(
            Options.Create(new AiTaskRuntimeOptions { DefaultRunnerKind = AiTaskRunnerKind.PlainChat }),
            [plain]);

        var result = resolver.Resolve();

        Assert.Same(plain, result);
    }
}
```

- [ ] **Step 2: Run runner tests and verify failure**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore --filter AiTaskAgentRunnerTests
```

Expected: FAIL because runner types do not exist.

- [ ] **Step 3: Add runner kind enum and run metadata fields**

Append to `AiTaskEnums.cs`:

```csharp
/// <summary>
/// AI 任务运行器类型
/// </summary>
public enum AiTaskRunnerKind
{
    [Description("普通聊天")]
    PlainChat = 0,

    [Description("Microsoft Agent Framework")]
    MicrosoftAgentFramework = 1,

    [Description("Semantic Kernel Agent")]
    SemanticKernelAgent = 2
}
```

In `SysAiTaskRun.cs`, add:

```csharp
[SugarColumn(ColumnName = "Runner_Kind", ColumnDescription = "运行器类型", Length = 100, IsNullable = true)]
public virtual string? RunnerKind { get; set; }

[SugarColumn(ColumnName = "Runner_Version", ColumnDescription = "运行器版本", Length = 100, IsNullable = true)]
public virtual string? RunnerVersion { get; set; }

[SugarColumn(ColumnName = "Agent_Session_Id", ColumnDescription = "Agent会话标识", Length = 200, IsNullable = true)]
public virtual string? AgentSessionId { get; set; }
```

Add corresponding `ALTER TABLE` statements to `AiTaskSchemaSeeder`:

```sql
ALTER TABLE IF EXISTS "sys_ai_task_run" ADD COLUMN IF NOT EXISTS "runner_kind" varchar(100) NULL;
ALTER TABLE IF EXISTS "sys_ai_task_run" ADD COLUMN IF NOT EXISTS "runner_version" varchar(100) NULL;
ALTER TABLE IF EXISTS "sys_ai_task_run" ADD COLUMN IF NOT EXISTS "agent_session_id" varchar(200) NULL;
```

- [ ] **Step 4: Add runtime options**

Create `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Configuration/AiTaskRuntimeOptions.cs`:

```csharp
using XiHan.BasicApp.AI.Domain.Enums;

namespace XiHan.BasicApp.AI.Infrastructure.Configuration;

public sealed class AiTaskRuntimeOptions
{
    public const string SectionName = "XiHan:AI:TaskRuntime";

    public AiTaskRunnerKind DefaultRunnerKind { get; set; } = AiTaskRunnerKind.PlainChat;

    public bool AllowTaskOverride { get; set; } = true;
}
```

- [ ] **Step 5: Add runner models and interface**

Create `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskRuntimeModels.cs`:

```csharp
using XiHan.BasicApp.AI.Domain.Entities;

namespace XiHan.BasicApp.AI.Application.Services;

public sealed record AiTaskRunContext(
    SysAiTask Task,
    SysAiTaskRun Run,
    string Prompt,
    IReadOnlyList<SysAiTaskRunGuidance> PendingGuidance);

public sealed record AiTaskRunnerResult(
    string? ResultText,
    string? AgentSessionId = null);
```

Create `backend/src/modules/XiHan.BasicApp.AI/Application/Services/IAiTaskAgentRunner.cs`:

```csharp
using XiHan.BasicApp.AI.Domain.Enums;

namespace XiHan.BasicApp.AI.Application.Services;

public interface IAiTaskAgentRunner
{
    AiTaskRunnerKind Kind { get; }

    string Version { get; }

    Task<AiTaskRunnerResult> RunAsync(AiTaskRunContext context, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 6: Add PlainChat runner**

Create `backend/src/modules/XiHan.BasicApp.AI/Application/Services/PlainChatAiTaskRunner.cs`:

```csharp
using System.Text;
using XiHan.BasicApp.AI.Domain.Enums;

namespace XiHan.BasicApp.AI.Application.Services;

public sealed class PlainChatAiTaskRunner : IAiTaskAgentRunner
{
    private readonly IAiTaskChatService _chatService;
    private readonly IAiTaskRunEventService _eventService;

    public PlainChatAiTaskRunner(IAiTaskChatService chatService, IAiTaskRunEventService eventService)
    {
        _chatService = chatService;
        _eventService = eventService;
    }

    public AiTaskRunnerKind Kind => AiTaskRunnerKind.PlainChat;

    public string Version => "plain-chat-v1";

    public async Task<AiTaskRunnerResult> RunAsync(AiTaskRunContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var prompt = BuildPrompt(context.Prompt, context.PendingGuidance);
        if (context.PendingGuidance.Count > 0)
        {
            await _eventService.AppendAsync(
                context.Run.BasicId,
                AiTaskRunEventType.GuidanceApplied,
                AiTaskRunEventRole.System,
                $"已应用 {context.PendingGuidance.Count} 条运行引导。",
                null,
                cancellationToken);
        }

        var result = await _chatService.CompleteAsync(context.Task, prompt, cancellationToken);
        if (!string.IsNullOrWhiteSpace(result))
        {
            await _eventService.AppendAsync(
                context.Run.BasicId,
                AiTaskRunEventType.AgentMessageDelta,
                AiTaskRunEventRole.Assistant,
                result,
                null,
                cancellationToken);
        }

        return new AiTaskRunnerResult(result);
    }

    private static string BuildPrompt(string prompt, IReadOnlyList<Domain.Entities.SysAiTaskRunGuidance> pendingGuidance)
    {
        if (pendingGuidance.Count == 0)
        {
            return prompt;
        }

        var builder = new StringBuilder(prompt.Trim());
        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine("Additional run guidance:");
        foreach (var guidance in pendingGuidance)
        {
            builder.AppendLine($"- {guidance.Content.Trim()}");
        }

        return builder.ToString();
    }
}
```

- [ ] **Step 7: Add runner resolver**

Create `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskRunnerResolver.cs`:

```csharp
using Microsoft.Extensions.Options;
using XiHan.BasicApp.AI.Infrastructure.Configuration;

namespace XiHan.BasicApp.AI.Application.Services;

public sealed class AiTaskRunnerResolver
{
    private readonly AiTaskRuntimeOptions _options;
    private readonly IReadOnlyList<IAiTaskAgentRunner> _runners;

    public AiTaskRunnerResolver(IOptions<AiTaskRuntimeOptions> options, IEnumerable<IAiTaskAgentRunner> runners)
    {
        _options = options.Value;
        _runners = runners.ToList();
    }

    public IAiTaskAgentRunner Resolve()
    {
        var runner = _runners.FirstOrDefault(r => r.Kind == _options.DefaultRunnerKind);
        if (runner is null)
        {
            throw new InvalidOperationException($"AI 任务运行器未注册：{_options.DefaultRunnerKind}");
        }

        return runner;
    }
}
```

- [ ] **Step 8: Register options and runner**

In `AddAiTaskServices`, add:

```csharp
services.AddOptions<AiTaskRuntimeOptions>().BindConfiguration(AiTaskRuntimeOptions.SectionName);
services.AddScoped<IAiTaskAgentRunner, PlainChatAiTaskRunner>();
services.AddScoped<AiTaskRunnerResolver>();
```

- [ ] **Step 9: Map run metadata fields**

In `AiTaskApplicationMapper.ToRunDetailDto`, map:

```csharp
RunnerKind = entity.RunnerKind,
RunnerVersion = entity.RunnerVersion,
AgentSessionId = entity.AgentSessionId,
```

- [ ] **Step 10: Run tests**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore --filter AiTaskAgentRunnerTests
```

Expected: PASS.

- [ ] **Step 11: Commit**

```bash
git add backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Configuration/AiTaskRuntimeOptions.cs \
  backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskRuntimeModels.cs \
  backend/src/modules/XiHan.BasicApp.AI/Application/Services/IAiTaskAgentRunner.cs \
  backend/src/modules/XiHan.BasicApp.AI/Application/Services/PlainChatAiTaskRunner.cs \
  backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskRunnerResolver.cs \
  backend/src/modules/XiHan.BasicApp.AI/Domain/Enums/AiTaskEnums.cs \
  backend/src/modules/XiHan.BasicApp.AI/Domain/Entities/SysAiTaskRun.cs \
  backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Seeders/System/AiTaskSchemaSeeder.cs \
  backend/src/modules/XiHan.BasicApp.AI/Application/Dtos/AiTaskDtos.cs \
  backend/src/modules/XiHan.BasicApp.AI/Application/Mappers/AiTaskApplicationMapper.cs \
  backend/src/modules/XiHan.BasicApp.AI/Extensions/ServiceCollectionExtensions.cs \
  backend/test/XiHan.BasicApp.AI.Tests/AiTaskAgentRunnerTests.cs
git commit -m "feat: add ai task runner abstraction"
```

---

### Task 5: Wire Executor Events, Guidance Consumption, And Recovery Events

**Files:**
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskExecutor.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskRunRecoveryService.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Domain/Repositories/IAiTaskRunRepository.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Repositories/AiTaskRunRepository.cs`
- Modify: `backend/test/XiHan.BasicApp.AI.Tests/AiTaskJobExecutorTests.cs`
- Modify: `backend/test/XiHan.BasicApp.AI.Tests/AiTaskRunRecoveryTests.cs`
- Modify: `backend/test/XiHan.BasicApp.AI.Tests/AiTaskTestDoubles.cs`

**Interfaces:**
- Consumes: `AiTaskRunnerResolver.Resolve`, `IAiTaskRunEventService.AppendAsync`, `IAiTaskRunGuidanceRepository.GetPendingByRunIdAsync`, `MarkAppliedAsync`, `MarkIgnoredAsync`.
- Produces: execution event stream for queued/start/prompt/success/failure/requeue.

- [ ] **Step 1: Add failing executor event tests**

In `AiTaskJobExecutorTests.cs`, add:

```csharp
[Fact]
public async Task ExecuteRunAsync_emits_auditable_run_events()
{
    var task = new SysAiTask(7)
    {
        AiTaskCode = "morning-news",
        AiTaskName = "Morning News",
        PromptMode = AiTaskPromptMode.Inline,
        PromptText = "Write a short brief.",
        Status = EnableStatus.Enabled
    };
    var repository = new InMemoryAiTaskRepository(task);
    var runRepository = new InMemoryAiTaskRunRepository();
    var eventService = new FakeAiTaskRunEventService();
    var guidanceRepository = new InMemoryAiTaskRunGuidanceRepository();
    var chat = new FakeAiTaskChatService("Brief result");
    var runner = new PlainChatAiTaskRunner(chat, eventService);
    var resolver = new AiTaskRunnerResolver(
        Microsoft.Extensions.Options.Options.Create(new AiTaskRuntimeOptions()),
        [runner]);
    var executor = new AiTaskExecutor(repository, runRepository, resolver, new AiTaskPromptRenderer(new FakeAiPromptStore()), eventService, guidanceRepository);

    var result = await executor.ExecuteAsync(7);

    Assert.True(result.Succeeded);
    Assert.Contains(eventService.Events, e => e.EventType == AiTaskRunEventType.RunQueued);
    Assert.Contains(eventService.Events, e => e.EventType == AiTaskRunEventType.RunStarted);
    Assert.Contains(eventService.Events, e => e.EventType == AiTaskRunEventType.PromptRendered);
    Assert.Contains(eventService.Events, e => e.EventType == AiTaskRunEventType.RunSucceeded);
}

[Fact]
public async Task ExecuteRunAsync_marks_pending_guidance_applied_after_runner_consumes_it()
{
    var task = new SysAiTask(7)
    {
        AiTaskCode = "daily",
        AiTaskName = "Daily",
        PromptMode = AiTaskPromptMode.Inline,
        PromptText = "Base prompt.",
        Status = EnableStatus.Enabled
    };
    var repository = new InMemoryAiTaskRepository(task);
    var runRepository = new InMemoryAiTaskRunRepository();
    var eventService = new FakeAiTaskRunEventService();
    var guidanceRepository = new InMemoryAiTaskRunGuidanceRepository();
    runRepository.Runs.Add(new SysAiTaskRun(42) { AiTaskId = 7, AiTaskCode = "daily", RunStatus = AiTaskRunStatus.Queued });
    await guidanceRepository.AddAsync(new SysAiTaskRunGuidance { RunId = 42, Content = "Focus on risk." });
    var chat = new FakeAiTaskChatService("done");
    var runner = new PlainChatAiTaskRunner(chat, eventService);
    var resolver = new AiTaskRunnerResolver(Microsoft.Extensions.Options.Options.Create(new AiTaskRuntimeOptions()), [runner]);
    var executor = new AiTaskExecutor(repository, runRepository, resolver, new AiTaskPromptRenderer(new FakeAiPromptStore()), eventService, guidanceRepository);

    await executor.ExecuteRunAsync(42);

    Assert.All(guidanceRepository.Guidance, item => Assert.Equal(AiTaskRunGuidanceStatus.Applied, item.Status));
    Assert.Contains("Focus on risk.", chat.LastPrompt);
}
```

Update existing executor test construction to use the new constructor signature.

- [ ] **Step 2: Run executor tests and verify failure**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore --filter AiTaskJobExecutorTests
```

Expected: FAIL because `AiTaskExecutor` still depends on `IAiTaskChatService`.

- [ ] **Step 3: Update repository completion to persist runner metadata**

In `IAiTaskRunRepository.CompleteRunningAsync`, add parameters:

```csharp
string? runnerKind,
string? runnerVersion,
string? agentSessionId,
```

Update `AiTaskRunRepository.CompleteRunningAsync` to set:

```csharp
.SetColumns(run => run.RunnerKind == runnerKind)
.SetColumns(run => run.RunnerVersion == runnerVersion)
.SetColumns(run => run.AgentSessionId == agentSessionId)
```

Update `InMemoryAiTaskRunRepository.CompleteRunningAsync` to assign the same fields.

- [ ] **Step 4: Refactor `AiTaskExecutor` constructor**

Change fields and constructor in `AiTaskExecutor.cs` from chat service to:

```csharp
private readonly AiTaskRunnerResolver _runnerResolver;
private readonly IAiTaskRunEventService _eventService;
private readonly IAiTaskRunGuidanceRepository _guidanceRepository;
```

Constructor:

```csharp
public AiTaskExecutor(
    IAiTaskRepository taskRepository,
    IAiTaskRunRepository runRepository,
    AiTaskRunnerResolver runnerResolver,
    AiTaskPromptRenderer promptRenderer,
    IAiTaskRunEventService eventService,
    IAiTaskRunGuidanceRepository guidanceRepository)
```

- [ ] **Step 5: Emit queue/start/prompt/success/failure events**

In `StartRunAsync`, after run creation:

```csharp
await _eventService.AppendAsync(run.BasicId, AiTaskRunEventType.RunQueued, AiTaskRunEventRole.System, "AI 任务运行已排队。", null, cancellationToken);
return run;
```

In `ExecuteStartedRunAsync`, after lease validation and before prompt rendering:

```csharp
await _eventService.AppendAsync(run.BasicId, AiTaskRunEventType.RunStarted, AiTaskRunEventRole.System, "AI 任务开始执行。", null, cancellationToken);
```

After prompt rendering:

```csharp
await _eventService.AppendAsync(run.BasicId, AiTaskRunEventType.PromptRendered, AiTaskRunEventRole.System, "AI 任务提示词已渲染。", null, executionToken);
```

After successful completion:

```csharp
await _eventService.AppendAsync(run.BasicId, AiTaskRunEventType.RunSucceeded, AiTaskRunEventRole.System, "AI 任务执行成功。", null, cancellationToken);
```

In timeout and general exception failure branches:

```csharp
await _eventService.AppendAsync(run.BasicId, AiTaskRunEventType.RunFailed, AiTaskRunEventRole.System, errorMessage, null, CancellationToken.None);
```

- [ ] **Step 6: Use runner and mark guidance applied/ignored**

Replace direct `_chatService.CompleteAsync` call with:

```csharp
var pendingGuidance = await _guidanceRepository.GetPendingByRunIdAsync(run.BasicId, executionToken);
var runner = _runnerResolver.Resolve();
var runnerResult = await runner.RunAsync(new AiTaskRunContext(task, run, prompt, pendingGuidance), executionToken);
if (pendingGuidance.Count > 0)
{
    await _guidanceRepository.MarkAppliedAsync(pendingGuidance.Select(g => g.BasicId).ToList(), DateTimeOffset.Now, cancellationToken);
}

var resultText = runnerResult.ResultText;
```

Pass runner metadata into `CompleteRunningAsync`:

```csharp
runner.Kind.ToString(),
runner.Version,
runnerResult.AgentSessionId,
```

After success/failure, mark any still-pending guidance ignored:

```csharp
var lateGuidance = await _guidanceRepository.GetPendingByRunIdAsync(run.BasicId, CancellationToken.None);
await _guidanceRepository.MarkIgnoredAsync(lateGuidance.Select(g => g.BasicId).ToList(), "运行已结束，未被执行器消费。", CancellationToken.None);
```

- [ ] **Step 7: Emit recovery events**

In `AiTaskRunRecoveryService.RequeueAsync` branch, after requeue:

```csharp
await _eventService.AppendAsync(run.BasicId, AiTaskRunEventType.RunRequeued, AiTaskRunEventRole.System, message, null, cancellationToken);
```

In recovery fail branch, after fail:

```csharp
await _eventService.AppendAsync(run.BasicId, AiTaskRunEventType.RunFailed, AiTaskRunEventRole.System, message, null, cancellationToken);
```

Update constructor injection and tests accordingly.

- [ ] **Step 8: Run focused backend tests**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore --filter "AiTaskJobExecutorTests|AiTaskRunRecoveryTests|AiTaskAgentRunnerTests|AiTaskRunGuidanceTests"
```

Expected: PASS.

- [ ] **Step 9: Commit**

```bash
git add backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskExecutor.cs \
  backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskRunRecoveryService.cs \
  backend/src/modules/XiHan.BasicApp.AI/Domain/Repositories/IAiTaskRunRepository.cs \
  backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Repositories/AiTaskRunRepository.cs \
  backend/test/XiHan.BasicApp.AI.Tests/AiTaskJobExecutorTests.cs \
  backend/test/XiHan.BasicApp.AI.Tests/AiTaskRunRecoveryTests.cs \
  backend/test/XiHan.BasicApp.AI.Tests/AiTaskTestDoubles.cs
git commit -m "feat: wire ai task runner events"
```

---

### Task 6: Frontend API Types And Run History Live Preview UI

**Files:**
- Modify: `frontend/src/api/modules/ai/task.enums.ts`
- Modify: `frontend/src/api/modules/ai/task.types.ts`
- Modify: `frontend/src/api/modules/ai/task.ts`
- Modify: `frontend/src/views/develop/ai-task/index.vue`
- Modify: `frontend/packages/locales/langs/zh-CN/develop.ts`
- Modify: `frontend/packages/locales/langs/en-US/develop.ts`

**Interfaces:**
- Consumes backend Dynamic API:
  - `AiTaskQuery.RunEvents(runId, afterSequence)`
  - `AiTask.AppendRunGuidance(input)`
  - SignalR method `AiTaskRunEventReceived`.
- Produces live event timeline and guidance append UI in existing AI Task history drawer.

- [ ] **Step 1: Add frontend enum/type definitions**

In `frontend/src/api/modules/ai/task.enums.ts`, add:

```ts
export enum AiTaskRunEventType {
  RunQueued = 'RunQueued',
  RunStarted = 'RunStarted',
  PromptRendered = 'PromptRendered',
  AgentStepStarted = 'AgentStepStarted',
  AgentMessageDelta = 'AgentMessageDelta',
  ToolCallStarted = 'ToolCallStarted',
  ToolCallFinished = 'ToolCallFinished',
  GuidanceReceived = 'GuidanceReceived',
  GuidanceApplied = 'GuidanceApplied',
  GuidanceIgnored = 'GuidanceIgnored',
  RunSucceeded = 'RunSucceeded',
  RunFailed = 'RunFailed',
  RunCanceled = 'RunCanceled',
  RunRequeued = 'RunRequeued',
}

export enum AiTaskRunEventRole {
  System = 'System',
  User = 'User',
  Assistant = 'Assistant',
  Tool = 'Tool',
}

export enum AiTaskRunGuidanceStatus {
  Pending = 'Pending',
  Applied = 'Applied',
  Ignored = 'Ignored',
}

export enum AiTaskRunnerKind {
  PlainChat = 'PlainChat',
  MicrosoftAgentFramework = 'MicrosoftAgentFramework',
  SemanticKernelAgent = 'SemanticKernelAgent',
}
```

In `frontend/src/api/modules/ai/task.types.ts`, update imports and add:

```ts
export interface AiTaskAppendGuidanceDto {
  runId: ApiId
  content: string
  clientRequestId?: string | null
}

export interface AiTaskRunEventDto extends BasicDto {
  runId: ApiId
  sequence: number
  eventType: AiTaskRunEventType
  role: AiTaskRunEventRole
  content?: string | null
  payloadJson?: string | null
  createdTime: DateTimeString
}

export interface AiTaskRunGuidanceDto extends BasicDto {
  runId: ApiId
  content: string
  status: AiTaskRunGuidanceStatus
  clientRequestId?: string | null
  appliedTime?: DateTimeString | null
  ignoredReason?: string | null
  createdTime: DateTimeString
}
```

Extend `AiTaskRunDetailDto`:

```ts
runnerKind?: AiTaskRunnerKind | string | null
runnerVersion?: string | null
agentSessionId?: string | null
events: AiTaskRunEventDto[]
guidance: AiTaskRunGuidanceDto[]
```

- [ ] **Step 2: Add API facade methods**

In `frontend/src/api/modules/ai/task.ts`, import new DTOs and add:

```ts
appendRunGuidance(input: AiTaskAppendGuidanceDto) {
  return command.post<AiTaskRunGuidanceDto, AiTaskAppendGuidanceDto>('AppendRunGuidance', input)
},
runEvents(runId: ApiId, afterSequence = 0) {
  return query.get<AiTaskRunEventDto[]>(`RunEvents/${formatDynamicApiRouteValue(runId)}?afterSequence=${afterSequence}`)
},
```

- [ ] **Step 3: Add UI state in AI Task page**

In `frontend/src/views/develop/ai-task/index.vue`, import `useSignalR` from `~/composables` and event/guidance types. Add state:

```ts
const runGuidanceText = ref('')
const runGuidanceSubmitting = ref(false)
const runEventRows = computed(() => runHistoryDetail.value?.events ?? [])
const canAppendRunGuidance = computed(() =>
  runHistoryDetail.value?.runStatus === AiTaskRunStatus.Queued
  || runHistoryDetail.value?.runStatus === AiTaskRunStatus.Running,
)
const signalR = useSignalR()
```

Add helper:

```ts
function mergeRunEvents(events: AiTaskRunEventDto[]) {
  if (!runHistoryDetail.value || !events.length) {
    return
  }

  const existing = new Map(runHistoryDetail.value.events.map(event => [event.sequence, event]))
  for (const event of events) {
    if (String(event.runId) === String(runHistoryDetail.value.basicId)) {
      existing.set(event.sequence, event)
    }
  }
  runHistoryDetail.value.events = [...existing.values()].sort((a, b) => a.sequence - b.sequence)
}

async function refreshRunEventsAfterLastSequence() {
  if (!runHistoryDetail.value) {
    return
  }
  const lastSequence = runHistoryDetail.value.events.at(-1)?.sequence ?? 0
  const events = await aiTaskApi.runEvents(runHistoryDetail.value.basicId, lastSequence)
  mergeRunEvents(events)
}
```

Add SignalR handler inside script setup:

```ts
function handleAiTaskRunEvent(payload: unknown) {
  const event = payload as AiTaskRunEventDto
  mergeRunEvents([event])
  if (runHistoryDetail.value && String(event.runId) === String(runHistoryDetail.value.basicId)) {
    if ([AiTaskRunEventType.RunSucceeded, AiTaskRunEventType.RunFailed, AiTaskRunEventType.RunCanceled, AiTaskRunEventType.RunRequeued].includes(event.eventType)) {
      void selectRunHistory(runHistoryDetail.value)
    }
  }
}

onMounted(async () => {
  signalR.on('AiTaskRunEventReceived', handleAiTaskRunEvent)
  await signalR.start()
})

onBeforeUnmount(() => {
  signalR.off('AiTaskRunEventReceived', handleAiTaskRunEvent)
})
```

If `onMounted`/`onBeforeUnmount` are not imported yet, add them to the Vue import list.

- [ ] **Step 4: Add guidance submit function**

In `index.vue`, add:

```ts
async function appendRunGuidance() {
  if (!runHistoryDetail.value || !runGuidanceText.value.trim()) {
    return
  }

  runGuidanceSubmitting.value = true
  try {
    const guidance = await aiTaskApi.appendRunGuidance({
      runId: runHistoryDetail.value.basicId,
      content: runGuidanceText.value.trim(),
      clientRequestId: `${runHistoryDetail.value.basicId}-${Date.now()}`,
    })
    runHistoryDetail.value.guidance = [...runHistoryDetail.value.guidance, guidance]
    runGuidanceText.value = ''
    await refreshRunEventsAfterLastSequence()
    message.success(t('develop.ai_task.run_guidance_submitted'))
  }
  catch {
    message.error(t('develop.ai_task.run_guidance_failed'))
  }
  finally {
    runGuidanceSubmitting.value = false
  }
}
```

- [ ] **Step 5: Update history drawer template**

Inside the run detail block, after metadata and before prompt snapshot, add:

```vue
<section class="ai-task-run-block">
  <div class="ai-task-section-title">
    {{ t('develop.ai_task.run_detail_events') }}
  </div>
  <NTimeline v-if="runEventRows.length">
    <NTimelineItem
      v-for="event in runEventRows"
      :key="event.sequence"
      :time="formatDateTime(event.createdTime)"
      :title="t(`develop.ai_task.run_event_${event.eventType}`)"
      :type="event.eventType === AiTaskRunEventType.RunFailed ? 'error' : event.eventType === AiTaskRunEventType.RunSucceeded ? 'success' : 'info'"
    >
      <pre v-if="event.content">{{ event.content }}</pre>
    </NTimelineItem>
  </NTimeline>
  <NEmpty v-else size="small" :description="t('develop.ai_task.run_events_empty')" />
</section>

<section class="ai-task-run-block">
  <div class="ai-task-section-title">
    {{ t('develop.ai_task.run_guidance_title') }}
  </div>
  <NSpace vertical>
    <NInput
      v-model:value="runGuidanceText"
      type="textarea"
      :disabled="!canAppendRunGuidance"
      :placeholder="canAppendRunGuidance ? t('develop.ai_task.run_guidance_placeholder') : t('develop.ai_task.run_guidance_disabled')"
    />
    <NSpace justify="end">
      <NButton
        type="primary"
        :disabled="!canAppendRunGuidance || !runGuidanceText.trim()"
        :loading="runGuidanceSubmitting"
        @click="appendRunGuidance"
      >
        {{ t('develop.ai_task.run_guidance_submit') }}
      </NButton>
    </NSpace>
  </NSpace>
</section>
```

- [ ] **Step 6: Add locale keys**

In `frontend/packages/locales/langs/zh-CN/develop.ts`, under `ai_task`, add:

```ts
run_detail_events: '运行记录',
run_events_empty: '暂无运行事件',
run_guidance_title: '运行引导',
run_guidance_placeholder: '追加本次运行的补充提示词，执行器会在下一个检查点尝试应用',
run_guidance_disabled: '运行已结束，不能追加引导',
run_guidance_submit: '追加引导',
run_guidance_submitted: '运行引导已提交',
run_guidance_failed: '提交运行引导失败',
run_event_RunQueued: '已排队',
run_event_RunStarted: '开始执行',
run_event_PromptRendered: '提示词已渲染',
run_event_AgentStepStarted: 'Agent 步骤开始',
run_event_AgentMessageDelta: 'AI 输出',
run_event_ToolCallStarted: '工具调用开始',
run_event_ToolCallFinished: '工具调用完成',
run_event_GuidanceReceived: '收到运行引导',
run_event_GuidanceApplied: '运行引导已应用',
run_event_GuidanceIgnored: '运行引导已忽略',
run_event_RunSucceeded: '执行成功',
run_event_RunFailed: '执行失败',
run_event_RunCanceled: '执行取消',
run_event_RunRequeued: '已重新排队',
```

In `frontend/packages/locales/langs/en-US/develop.ts`, add the corresponding English values:

```ts
run_detail_events: 'Run Events',
run_events_empty: 'No run events',
run_guidance_title: 'Run Guidance',
run_guidance_placeholder: 'Append guidance for this run. The executor will try to apply it at the next checkpoint.',
run_guidance_disabled: 'The run has ended. Guidance cannot be appended.',
run_guidance_submit: 'Append Guidance',
run_guidance_submitted: 'Run guidance submitted',
run_guidance_failed: 'Failed to submit run guidance',
run_event_RunQueued: 'Queued',
run_event_RunStarted: 'Started',
run_event_PromptRendered: 'Prompt rendered',
run_event_AgentStepStarted: 'Agent step started',
run_event_AgentMessageDelta: 'AI output',
run_event_ToolCallStarted: 'Tool call started',
run_event_ToolCallFinished: 'Tool call finished',
run_event_GuidanceReceived: 'Guidance received',
run_event_GuidanceApplied: 'Guidance applied',
run_event_GuidanceIgnored: 'Guidance ignored',
run_event_RunSucceeded: 'Succeeded',
run_event_RunFailed: 'Failed',
run_event_RunCanceled: 'Canceled',
run_event_RunRequeued: 'Requeued',
```

- [ ] **Step 7: Run frontend type check**

Run:

```bash
cd frontend && pnpm type-check
```

Expected: PASS.

- [ ] **Step 8: Commit**

```bash
git add frontend/src/api/modules/ai/task.enums.ts \
  frontend/src/api/modules/ai/task.types.ts \
  frontend/src/api/modules/ai/task.ts \
  frontend/src/views/develop/ai-task/index.vue \
  frontend/packages/locales/langs/zh-CN/develop.ts \
  frontend/packages/locales/langs/en-US/develop.ts
git commit -m "feat: add ai task live run preview"
```

---

### Task 7: Full Verification And Runtime Smoke Test

**Files:**
- Modify only if earlier tasks reveal compile or type issues.

**Interfaces:**
- Consumes all prior tasks.
- Produces verified end-to-end behavior.

- [ ] **Step 1: Run AI backend tests**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore
```

Expected: PASS for all AI tests.

- [ ] **Step 2: Run backend build**

Run:

```bash
dotnet build backend/XiHan.BasicApp.slnx --no-restore
```

Expected: PASS. Existing package vulnerability warnings may remain, but there must be 0 errors.

- [ ] **Step 3: Run frontend type check**

Run:

```bash
cd frontend && pnpm type-check
```

Expected: PASS.

- [ ] **Step 4: Run whitespace check**

Run:

```bash
git diff --check
```

Expected: no output.

- [ ] **Step 5: Runtime smoke test**

Start or reuse the existing backend/frontend services. Then:

1. Open AI Task page.
2. Start an AI Task with the existing Run action.
3. Open `更多 -> 历史 -> 详情`.
4. Confirm event timeline shows at least `RunQueued`, `RunStarted`, `PromptRendered`, and a terminal event.
5. While a run is `Queued` or `Running`, append guidance and confirm it appears in the timeline as `GuidanceReceived`.
6. Refresh the browser while the drawer is open, reopen the same run, and confirm events are restored from the API.
7. Open the existing AI chat feature and confirm its normal conversation flow still works.

- [ ] **Step 6: Commit verification-only fixes if needed**

If verification required code fixes:

```bash
git add <only-files-fixed-for-verification>
git commit -m "fix: stabilize ai task runtime foundation"
```

If no fixes were needed, do not create an empty commit.

---

## Self-Review Notes

- Spec coverage: AI Task automation-first guidance is covered by Tasks 2, 5, and 6. Persisted event source of truth is covered by Tasks 1 and 3. Existing AI chat isolation is covered by Task 4 by wrapping only AI Task execution and not touching chat UI/session storage. Runner configurability is covered by Task 4 without adding Microsoft Agent Framework SDK. Frontend live preview is covered by Task 6.
- Scope check: Microsoft Agent Framework adapter and future `WaitingForUserInput` behavior are intentionally out of scope and require a later plan.
- Type consistency: backend enum names match frontend enum names; event client method is consistently `AiTaskRunEventReceived`; guidance command is consistently `AppendRunGuidance`.
