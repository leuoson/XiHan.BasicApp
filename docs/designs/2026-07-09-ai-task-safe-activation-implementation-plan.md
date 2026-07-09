# AI Task Safe Activation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make AI Task usable for inline and prompt-library scheduled chat runs while preserving the existing AI knowledge Q&A contract.

**Architecture:** Keep AI Task execution inside the AI module's existing application-service/domain-service/repository shape. AI Task reads shared provider and prompt-store infrastructure but does not call or change `KnowledgeQueryAppService`, RAG retrieval, RAG prompt augmentation, or `/develop/knowledge`.

**Tech Stack:** .NET 10, XiHan.Framework Dynamic API, SqlSugar repositories, xUnit, Vue 3, TypeScript, Naive UI, vue-i18n.

## Global Constraints

- Existing AI Q&A boundary is protected: do not change `KnowledgeQueryAppService.QueryAsync`, `KnowledgeQueryDto`, `KnowledgeQueryResultDto`, `knowledgeApi.query`, or `/develop/knowledge`.
- AI Task may read `IAiPromptStore`, `SysAiPrompt`, `IXiHanAiService`, and `SysAiProvider`, but must not change their shared behavior for task execution.
- Skill execution and agent tool calling remain out of scope; `SysAiTaskToolPolicy` continues to be metadata only.
- Use TDD for backend behavior changes: write the failing xUnit test, run it, implement, rerun.
- Keep frontend calls behind `frontend/src/api/modules/ai/task.ts` and DTOs in `task.types.ts`.
- Add user-facing frontend text through `frontend/packages/locales/langs/zh-CN/develop.ts` and `frontend/packages/locales/langs/en-US/develop.ts`.

---

## File Structure

- Modify `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskPromptRenderer.cs`: make prompt rendering async and resolve `PromptStore` through `IAiPromptStore`.
- Modify `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskExecutor.cs`: create/update `SysAiTaskRun` consistently for render and chat failures.
- Modify `backend/src/modules/XiHan.BasicApp.AI/Domain/Repositories/IAiTaskRunRepository.cs`: add run-history read methods.
- Modify `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Repositories/AiTaskRunRepository.cs`: implement run-history read methods with `CreateQueryable()`.
- Modify `backend/src/modules/XiHan.BasicApp.AI/Application/Dtos/AiTaskDtos.cs`: add `AiTaskRunListItemDto` and `AiTaskRunDetailDto`.
- Modify `backend/src/modules/XiHan.BasicApp.AI/Application/Mappers/AiTaskApplicationMapper.cs`: map `SysAiTaskRun` to run-history DTOs.
- Modify `backend/src/modules/XiHan.BasicApp.AI/Application/Contracts/AiTaskContracts.cs`: expose run-history query methods on `IAiTaskQueryService`.
- Modify `backend/src/modules/XiHan.BasicApp.AI/Application/QueryServices/AiTaskQueryService.cs`: add run-history list/detail methods protected by `AiTaskPermissionCodes.Read`.
- Modify `backend/test/XiHan.BasicApp.AI.Tests/AiTaskTestDoubles.cs`: extend test doubles for prompt store, captured chat prompts, and run-history queries.
- Create `backend/test/XiHan.BasicApp.AI.Tests/AiTaskPromptRendererTests.cs`: renderer tests.
- Modify `backend/test/XiHan.BasicApp.AI.Tests/AiTaskJobExecutorTests.cs`: add success/failure run persistence tests.
- Create `backend/test/XiHan.BasicApp.AI.Tests/AiTaskRunHistoryQueryTests.cs`: query service tests.
- Create `backend/test/XiHan.BasicApp.AI.Tests/KnowledgeQueryBoundaryTests.cs`: regression test for existing knowledge Q&A response shape.
- Modify `frontend/src/api/modules/ai/task.enums.ts`: add `AiTaskRunStatus` and display options.
- Modify `frontend/src/api/modules/ai/task.types.ts`: add run-history DTO types.
- Modify `frontend/src/api/modules/ai/task.ts`: add `runList(taskId)` and `runDetail(runId)`.
- Modify `frontend/src/views/develop/ai-task/index.vue`: add run-history row action and drawer.
- Modify `frontend/packages/locales/langs/zh-CN/develop.ts`: add run-history Chinese labels.
- Modify `frontend/packages/locales/langs/en-US/develop.ts`: add run-history English labels.

## Task 1: PromptStore Rendering

**Files:**
- Create: `backend/test/XiHan.BasicApp.AI.Tests/AiTaskPromptRendererTests.cs`
- Modify: `backend/test/XiHan.BasicApp.AI.Tests/AiTaskTestDoubles.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskPromptRenderer.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskExecutor.cs`

**Interfaces:**
- Consumes: `IAiPromptStore.GetAsync(string name, string? version, CancellationToken cancellationToken)`
- Produces: `AiTaskPromptRenderer.RenderAsync(SysAiTask task, CancellationToken cancellationToken = default): Task<string>`

- [ ] **Step 1: Add failing renderer tests**

Add `FakeAiPromptStore` to `AiTaskTestDoubles.cs`:

```csharp
internal sealed class FakeAiPromptStore : IAiPromptStore
{
    private readonly Dictionary<string, AiPromptTemplate> _prompts = new(StringComparer.OrdinalIgnoreCase);

    public void Add(string name, string content, string? version = null)
    {
        _prompts[$"{name.Trim()}::{version?.Trim()}"] = new AiPromptTemplate
        {
            Name = name.Trim(),
            Content = content,
            Version = version
        };
    }

    public Task<AiPromptTemplate?> GetAsync(string name, string? version = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _prompts.TryGetValue($"{name.Trim()}::{version?.Trim()}", out var prompt);
        return Task.FromResult(prompt);
    }

    public Task<IReadOnlyList<AiPromptTemplate>> ListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<AiPromptTemplate>>(_prompts.Values.ToList());
    }
}
```

Create `AiTaskPromptRendererTests.cs`:

```csharp
using XiHan.BasicApp.AI.Application.Services;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using Xunit;

namespace XiHan.BasicApp.AI.Tests;

public sealed class AiTaskPromptRendererTests
{
    [Fact]
    public async Task RenderAsync_returns_inline_prompt()
    {
        var renderer = new AiTaskPromptRenderer(new FakeAiPromptStore());
        var task = new SysAiTask(1)
        {
            PromptMode = AiTaskPromptMode.Inline,
            PromptText = "Summarize today's orders."
        };

        var prompt = await renderer.RenderAsync(task);

        Assert.Equal("Summarize today's orders.", prompt);
    }

    [Fact]
    public async Task RenderAsync_resolves_prompt_store_content()
    {
        var store = new FakeAiPromptStore();
        store.Add("daily-summary", "Write the daily summary.", "v1");
        var renderer = new AiTaskPromptRenderer(store);
        var task = new SysAiTask(1)
        {
            PromptMode = AiTaskPromptMode.PromptStore,
            PromptCode = "daily-summary",
            PromptVersion = "v1"
        };

        var prompt = await renderer.RenderAsync(task);

        Assert.Equal("Write the daily summary.", prompt);
    }

    [Fact]
    public async Task RenderAsync_throws_clear_message_when_prompt_store_prompt_is_missing()
    {
        var renderer = new AiTaskPromptRenderer(new FakeAiPromptStore());
        var task = new SysAiTask(1)
        {
            PromptMode = AiTaskPromptMode.PromptStore,
            PromptCode = "missing-prompt"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => renderer.RenderAsync(task));

        Assert.Equal("AI 任务引用的提示词不存在或已禁用。", ex.Message);
    }
}
```

- [ ] **Step 2: Run tests to verify RED**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore --filter AiTaskPromptRendererTests
```

Expected: fail to compile because `AiTaskPromptRenderer` does not accept `IAiPromptStore` and does not expose `RenderAsync`.

- [ ] **Step 3: Implement async renderer**

Change `AiTaskPromptRenderer` to:

```csharp
public sealed class AiTaskPromptRenderer
{
    private readonly IAiPromptStore _promptStore;

    public AiTaskPromptRenderer(IAiPromptStore promptStore)
    {
        _promptStore = promptStore;
    }

    public async Task<string> RenderAsync(SysAiTask task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        cancellationToken.ThrowIfCancellationRequested();

        return task.PromptMode switch
        {
            AiTaskPromptMode.Inline when !string.IsNullOrWhiteSpace(task.PromptText) => task.PromptText,
            AiTaskPromptMode.PromptStore => await RenderPromptStoreAsync(task, cancellationToken),
            _ => throw new InvalidOperationException("AI 任务提示词不能为空。")
        };
    }

    private async Task<string> RenderPromptStoreAsync(SysAiTask task, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(task.PromptCode))
        {
            throw new InvalidOperationException("AI 任务提示词编码不能为空。");
        }

        var prompt = await _promptStore.GetAsync(task.PromptCode.Trim(), task.PromptVersion?.Trim(), cancellationToken);
        if (prompt is null || string.IsNullOrWhiteSpace(prompt.Content))
        {
            throw new InvalidOperationException("AI 任务引用的提示词不存在或已禁用。");
        }

        return prompt.Content;
    }
}
```

Update `AiTaskExecutor` to call `await _promptRenderer.RenderAsync(task, cancellationToken)`.

- [ ] **Step 4: Run tests to verify GREEN**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore --filter "AiTaskPromptRendererTests|AiTaskJobExecutorTests"
```

Expected: all selected tests pass.

## Task 2: Run Records For Render And Chat Failures

**Files:**
- Modify: `backend/test/XiHan.BasicApp.AI.Tests/AiTaskTestDoubles.cs`
- Modify: `backend/test/XiHan.BasicApp.AI.Tests/AiTaskJobExecutorTests.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/Services/AiTaskExecutor.cs`

**Interfaces:**
- Consumes: `AiTaskPromptRenderer.RenderAsync(...)`
- Produces: every started task execution creates exactly one `SysAiTaskRun` that ends in `Success` or `Failed`.

- [ ] **Step 1: Add failing executor tests**

Extend `FakeAiTaskChatService`:

```csharp
internal sealed class FakeAiTaskChatService : IAiTaskChatService
{
    private readonly string? _resultText;
    private readonly Exception? _exception;

    public string? LastPrompt { get; private set; }

    public FakeAiTaskChatService(string? resultText, Exception? exception = null)
    {
        _resultText = resultText;
        _exception = exception;
    }

    public Task<string?> CompleteAsync(SysAiTask task, string prompt, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LastPrompt = prompt;
        return _exception is null ? Task.FromResult(_resultText) : Task.FromException<string?>(_exception);
    }
}
```

Add tests to `AiTaskJobExecutorTests.cs`:

```csharp
[Fact]
public async Task ExecuteAsync_records_success_run_for_prompt_store_task()
{
    var store = new FakeAiPromptStore();
    store.Add("daily-summary", "Write the daily summary.", "v1");
    var task = new SysAiTask(7)
    {
        AiTaskCode = "morning-news",
        AiTaskName = "Morning News",
        PromptMode = AiTaskPromptMode.PromptStore,
        PromptCode = "daily-summary",
        PromptVersion = "v1",
        Status = EnableStatus.Enabled
    };
    var repository = new InMemoryAiTaskRepository(task);
    var runRepository = new InMemoryAiTaskRunRepository();
    var chat = new FakeAiTaskChatService("Brief result");
    var executor = new AiTaskExecutor(repository, runRepository, chat, new AiTaskPromptRenderer(store));

    var result = await executor.ExecuteAsync(7);

    Assert.True(result.Succeeded);
    Assert.Equal("Write the daily summary.", chat.LastPrompt);
    Assert.Single(runRepository.Runs);
    Assert.Equal(AiTaskRunStatus.Success, runRepository.Runs[0].RunStatus);
    Assert.Equal("Write the daily summary.", runRepository.Runs[0].PromptSnapshot);
}

[Fact]
public async Task ExecuteAsync_records_failed_run_when_prompt_store_prompt_is_missing()
{
    var task = new SysAiTask(7)
    {
        AiTaskCode = "morning-news",
        AiTaskName = "Morning News",
        PromptMode = AiTaskPromptMode.PromptStore,
        PromptCode = "missing-prompt",
        Status = EnableStatus.Enabled
    };
    var repository = new InMemoryAiTaskRepository(task);
    var runRepository = new InMemoryAiTaskRunRepository();
    var chat = new FakeAiTaskChatService("Brief result");
    var executor = new AiTaskExecutor(repository, runRepository, chat, new AiTaskPromptRenderer(new FakeAiPromptStore()));

    var result = await executor.ExecuteAsync(7);

    Assert.False(result.Succeeded);
    Assert.Single(runRepository.Runs);
    Assert.Equal(AiTaskRunStatus.Failed, runRepository.Runs[0].RunStatus);
    Assert.Equal("AI 任务引用的提示词不存在或已禁用。", runRepository.Runs[0].ErrorMessage);
    Assert.Null(chat.LastPrompt);
}
```

- [ ] **Step 2: Run tests to verify RED**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore --filter AiTaskJobExecutorTests
```

Expected: prompt-store success may fail until Task 1 is complete; missing-prompt failure should fail because the run is not recorded when rendering throws before run creation.

- [ ] **Step 3: Implement run creation before rendering**

In `AiTaskExecutor.ExecuteAsync`, after loading and validating the task, create a `Running` run with `AiTaskId`, `AiTaskCode`, and `StartedTime`. Move prompt rendering inside the `try`; when rendering succeeds, assign `run.PromptSnapshot = prompt` before chat. In `catch`, update the same run as `Failed`.

- [ ] **Step 4: Run tests to verify GREEN**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore --filter "AiTaskPromptRendererTests|AiTaskJobExecutorTests"
```

Expected: all selected tests pass.

## Task 3: Run History Backend API

**Files:**
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Domain/Repositories/IAiTaskRunRepository.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Infrastructure/Repositories/AiTaskRunRepository.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/Dtos/AiTaskDtos.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/Mappers/AiTaskApplicationMapper.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/Contracts/AiTaskContracts.cs`
- Modify: `backend/src/modules/XiHan.BasicApp.AI/Application/QueryServices/AiTaskQueryService.cs`
- Modify: `backend/test/XiHan.BasicApp.AI.Tests/AiTaskTestDoubles.cs`
- Create: `backend/test/XiHan.BasicApp.AI.Tests/AiTaskRunHistoryQueryTests.cs`

**Interfaces:**
- Produces repository methods:
  - `Task<SysAiTaskRun?> GetByIdAsync(long id, CancellationToken cancellationToken = default)`
  - `Task<IReadOnlyList<SysAiTaskRun>> GetByTaskIdAsync(long aiTaskId, CancellationToken cancellationToken = default)`
- Produces query-service methods:
  - `Task<IReadOnlyList<AiTaskRunListItemDto>> GetRunListAsync(long aiTaskId, CancellationToken cancellationToken = default)`
  - `Task<AiTaskRunDetailDto?> GetRunDetailAsync(long id, CancellationToken cancellationToken = default)`
- Produces Dynamic API routes consumed by frontend as `AiTaskQuery/RunList/{aiTaskId}` and `AiTaskQuery/RunDetail/{runId}`.

- [ ] **Step 1: Add failing query tests**

Extend `InMemoryAiTaskRunRepository` with the two read methods in the interface, then create `AiTaskRunHistoryQueryTests.cs`:

```csharp
using XiHan.BasicApp.AI.Application.QueryServices;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Saas.Domain.Enums;
using Xunit;

namespace XiHan.BasicApp.AI.Tests;

public sealed class AiTaskRunHistoryQueryTests
{
    [Fact]
    public async Task GetRunListAsync_returns_task_runs_ordered_newest_first()
    {
        var runRepository = new InMemoryAiTaskRunRepository();
        runRepository.Runs.AddRange(
        [
            new SysAiTaskRun(1) { AiTaskId = 7, AiTaskCode = "morning-news", StartedTime = DateTimeOffset.Parse("2026-07-09T08:00:00+08:00"), RunStatus = AiTaskRunStatus.Success },
            new SysAiTaskRun(2) { AiTaskId = 8, AiTaskCode = "other", StartedTime = DateTimeOffset.Parse("2026-07-09T09:00:00+08:00"), RunStatus = AiTaskRunStatus.Success },
            new SysAiTaskRun(3) { AiTaskId = 7, AiTaskCode = "morning-news", StartedTime = DateTimeOffset.Parse("2026-07-09T10:00:00+08:00"), RunStatus = AiTaskRunStatus.Failed, ErrorMessage = "failed" }
        ]);
        var service = new AiTaskQueryService(
            new InMemoryAiTaskRepository(),
            new InMemoryAiTaskToolPolicyRepository([]),
            new InMemoryAiToolRepository(new SysAiTool(11) { ToolCode = "knowledge", ToolName = "Knowledge", Status = EnableStatus.Enabled }),
            runRepository);

        var runs = await service.GetRunListAsync(7);

        Assert.Equal([3L, 1L], runs.Select(run => run.BasicId).ToArray());
        Assert.Equal("failed", runs[0].ErrorMessage);
    }

    [Fact]
    public async Task GetRunDetailAsync_returns_prompt_snapshot_and_result()
    {
        var runRepository = new InMemoryAiTaskRunRepository();
        runRepository.Runs.Add(new SysAiTaskRun(3)
        {
            AiTaskId = 7,
            AiTaskCode = "morning-news",
            StartedTime = DateTimeOffset.Parse("2026-07-09T10:00:00+08:00"),
            EndedTime = DateTimeOffset.Parse("2026-07-09T10:00:01+08:00"),
            DurationMilliseconds = 1000,
            RunStatus = AiTaskRunStatus.Success,
            PromptSnapshot = "Write the daily summary.",
            ResultText = "Brief result"
        });
        var service = new AiTaskQueryService(
            new InMemoryAiTaskRepository(),
            new InMemoryAiTaskToolPolicyRepository([]),
            new InMemoryAiToolRepository(new SysAiTool(11) { ToolCode = "knowledge", ToolName = "Knowledge", Status = EnableStatus.Enabled }),
            runRepository);

        var detail = await service.GetRunDetailAsync(3);

        Assert.NotNull(detail);
        Assert.Equal("Write the daily summary.", detail.PromptSnapshot);
        Assert.Equal("Brief result", detail.ResultText);
    }
}
```

- [ ] **Step 2: Run tests to verify RED**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore --filter AiTaskRunHistoryQueryTests
```

Expected: fail to compile because run-history DTOs, repository read methods, and query service methods do not exist.

- [ ] **Step 3: Implement backend run history**

Add DTOs:

```csharp
public class AiTaskRunListItemDto : BasicAppDto
{
    public long AiTaskId { get; set; }
    public string AiTaskCode { get; set; } = string.Empty;
    public DateTimeOffset StartedTime { get; set; }
    public DateTimeOffset? EndedTime { get; set; }
    public AiTaskRunStatus RunStatus { get; set; }
    public long? DurationMilliseconds { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
}

public sealed class AiTaskRunDetailDto : AiTaskRunListItemDto
{
    public string? PromptSnapshot { get; set; }
    public string? ResultText { get; set; }
}
```

Map all fields in `AiTaskApplicationMapper.ToRunListItemDto(SysAiTaskRun entity)` and `ToRunDetailDto(SysAiTaskRun entity)`.

Implement repository queries:

```csharp
public async Task<SysAiTaskRun?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
{
    if (id <= 0)
    {
        throw new ArgumentOutOfRangeException(nameof(id), "AI 任务运行记录主键必须大于 0。");
    }

    cancellationToken.ThrowIfCancellationRequested();
    return await CreateQueryable().Where(run => run.BasicId == id).FirstAsync(cancellationToken);
}

public async Task<IReadOnlyList<SysAiTaskRun>> GetByTaskIdAsync(long aiTaskId, CancellationToken cancellationToken = default)
{
    if (aiTaskId <= 0)
    {
        throw new ArgumentOutOfRangeException(nameof(aiTaskId), "AI 任务主键必须大于 0。");
    }

    cancellationToken.ThrowIfCancellationRequested();
    return await CreateQueryable()
        .Where(run => run.AiTaskId == aiTaskId)
        .OrderByDescending(run => run.StartedTime)
        .ToListAsync(cancellationToken);
}
```

Inject `IAiTaskRunRepository` into `AiTaskQueryService` and expose the two query-service methods.

- [ ] **Step 4: Run tests to verify GREEN**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore --filter "AiTaskRunHistoryQueryTests|AiTaskJobExecutorTests"
```

Expected: all selected tests pass.

## Task 4: Knowledge Q&A Boundary Regression

**Files:**
- Create: `backend/test/XiHan.BasicApp.AI.Tests/KnowledgeQueryBoundaryTests.cs`

**Interfaces:**
- Consumes existing `KnowledgeQueryAppService.QueryAsync(KnowledgeQueryDto input, CancellationToken cancellationToken = default)`.
- Produces no production-code changes unless the test reveals an accidental regression.

- [ ] **Step 1: Reflect dependency interfaces if needed**

If local source for `IKnowledgeRetriever`, `IRagPromptAugmenter`, `IXiHanAiService`, or `ICurrentTenant` is not searchable, inspect the compiled assemblies with a short reflection snippet before writing fakes:

```bash
dotnet script # or a temporary C# scratch command if available
```

Use this only to identify method signatures; do not change production code.

- [ ] **Step 2: Add failing/protective regression test**

Create a test that constructs `KnowledgeQueryAppService` with fakes returning one retrieved chunk and a chat response text. Assert:

```csharp
Assert.Equal("AI answer", result.Answer);
Assert.Single(result.Citations);
Assert.Equal("Knowledge title", result.Citations[0].Title);
```

Also assert that the fake augmenter and fake AI service were called exactly once when `Answer = true`.

- [ ] **Step 3: Run test to verify current behavior**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore --filter KnowledgeQueryBoundaryTests
```

Expected: pass without production changes. If it fails, stop and inspect whether the test fake mismatches the existing contract or whether the boundary has already regressed.

## Task 5: Frontend Run History

**Files:**
- Modify: `frontend/src/api/modules/ai/task.enums.ts`
- Modify: `frontend/src/api/modules/ai/task.types.ts`
- Modify: `frontend/src/api/modules/ai/task.ts`
- Modify: `frontend/src/views/develop/ai-task/index.vue`
- Modify: `frontend/packages/locales/langs/zh-CN/develop.ts`
- Modify: `frontend/packages/locales/langs/en-US/develop.ts`

**Interfaces:**
- Consumes:
  - `aiTaskApi.runList(taskId): Promise<AiTaskRunListItemDto[]>`
  - `aiTaskApi.runDetail(runId): Promise<AiTaskRunDetailDto | null>`
- Produces a `/develop/ai-task` row action that opens a run-history drawer and displays run status, start/end time, duration, prompt snapshot, result text, and error message.

- [ ] **Step 1: Add frontend API/types**

Add enum:

```ts
export enum AiTaskRunStatus {
  Running = 'Running',
  Success = 'Success',
  Failed = 'Failed',
  Canceled = 'Canceled',
}
```

Add DTOs:

```ts
export interface AiTaskRunListItemDto extends BasicDto {
  aiTaskId: ApiId
  aiTaskCode: string
  startedTime: DateTimeString
  endedTime?: DateTimeString | null
  runStatus: AiTaskRunStatus
  durationMilliseconds?: number | null
  errorMessage?: string | null
  createdTime: DateTimeString
}

export interface AiTaskRunDetailDto extends AiTaskRunListItemDto {
  promptSnapshot?: string | null
  resultText?: string | null
}
```

Add API methods:

```ts
runList(taskId: ApiId) {
  return query.get<AiTaskRunListItemDto[]>(`RunList/${formatDynamicApiRouteValue(taskId)}`)
},
runDetail(runId: ApiId) {
  return query.get<AiTaskRunDetailDto | null>(`RunDetail/${formatDynamicApiRouteValue(runId)}`)
},
```

- [ ] **Step 2: Add run-history drawer UI**

In `frontend/src/views/develop/ai-task/index.vue`, add a row action:

```ts
{ key: 'history', title: t('develop.ai_task.action_history'), scope: 'row', icon: 'lucide:list-clock' },
```

Add state:

```ts
const runHistoryVisible = ref(false)
const runHistoryLoading = ref(false)
const runHistoryRows = ref<AiTaskRunListItemDto[]>([])
const runHistoryDetail = ref<AiTaskRunDetailDto | null>(null)
const runHistoryTask = ref<AiTaskListItemDto | null>(null)
```

Add handlers:

```ts
async function openRunHistory(row: AiTaskListItemDto) {
  runHistoryTask.value = row
  runHistoryVisible.value = true
  await loadRunHistory(row.basicId)
}

async function loadRunHistory(taskId: ApiId) {
  runHistoryLoading.value = true
  try {
    const rows = await aiTaskApi.runList(taskId)
    runHistoryRows.value = rows
    runHistoryDetail.value = rows[0] ? await aiTaskApi.runDetail(rows[0].basicId) : null
  }
  catch {
    message.error(t('develop.ai_task.load_run_history_failed'))
  }
  finally {
    runHistoryLoading.value = false
  }
}
```

Add `NDrawer`/`NDrawerContent` with a compact `NDataTable` and read-only text blocks for `promptSnapshot`, `resultText`, and `errorMessage`.

- [ ] **Step 3: Add locale keys**

Add keys under `ai_task` in both locale files:

```ts
action_history: '运行历史',
load_run_history_failed: '加载运行历史失败',
run_history_title: '运行历史',
run_history_empty: '暂无运行记录',
run_col_status: '状态',
run_col_started: '开始时间',
run_col_duration: '耗时',
run_detail_prompt: '提示词快照',
run_detail_result: '结果',
run_detail_error: '错误',
```

Use English equivalents in `en-US/develop.ts`.

- [ ] **Step 4: Run frontend type check**

Run:

```bash
cd frontend && pnpm type-check
```

Expected: pass.

## Task 6: Full Verification And Commit

**Files:**
- All modified files from Tasks 1-5.

**Interfaces:**
- Produces one verified implementation commit after all tests pass.

- [ ] **Step 1: Run focused backend tests**

Run:

```bash
dotnet test backend/test/XiHan.BasicApp.AI.Tests/XiHan.BasicApp.AI.Tests.csproj --no-restore
```

Expected: all AI tests pass.

- [ ] **Step 2: Run backend build**

Run:

```bash
dotnet build backend/XiHan.BasicApp.slnx
```

Expected: build succeeds. Existing `SQLitePCLRaw.lib.e_sqlite3` NU1903 warnings are acceptable if unchanged.

- [ ] **Step 3: Run frontend type check**

Run:

```bash
cd frontend && pnpm type-check
```

Expected: type check succeeds.

- [ ] **Step 4: Confirm protected boundary**

Run:

```bash
git diff -- backend/src/modules/XiHan.BasicApp.AI/Application/AppServices/KnowledgeQueryAppService.cs backend/src/modules/XiHan.BasicApp.AI/Application/Dtos/KnowledgeDtos.cs frontend/src/api/modules/ai/knowledge.ts frontend/src/views/develop/knowledge/index.vue
```

Expected: no diff.

- [ ] **Step 5: Commit**

Run:

```bash
git status --short
git add docs/designs/2026-07-09-ai-task-safe-activation-implementation-plan.md backend/src/modules/XiHan.BasicApp.AI backend/test/XiHan.BasicApp.AI.Tests frontend/src/api/modules/ai frontend/src/views/develop/ai-task/index.vue frontend/packages/locales/langs/zh-CN/develop.ts frontend/packages/locales/langs/en-US/develop.ts
git commit -m "feat: activate ai task prompt runs"
```
