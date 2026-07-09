# AI Task Safe Activation Design

## Background

XiHan.BasicApp already has several AI module capabilities:

- AI provider management through `SysAiProvider` and the XiHan AI provider resolver.
- Prompt management through `SysAiPrompt` and the `IAiPromptStore` database implementation.
- Knowledge retrieval and Q&A through `KnowledgeQueryAppService`.
- AI scheduled task management through `SysAiTask`, backing `SysTask` scheduler sync, and `AiTaskExecutor`.

The current AI Task implementation can create tasks, sync them into the existing scheduler, and run inline prompts through `IXiHanAiService.ChatAsync`. It is not yet a complete operational loop because prompt-library tasks still fail at render time, run history is persisted but not exposed for review, and task execution errors are only surfaced through the immediate run result or scheduler logs.

## Goal

Make AI Task usable as an independent scheduled AI automation feature without changing the behavior of the existing AI Q&A capability.

The first implementation slice should support:

- Inline prompt tasks.
- Prompt-library tasks that read enabled prompts from `SysAiPrompt` by `PromptCode` and optional `PromptVersion`.
- Manual run and scheduled run using the existing backing `SysTask` scheduler bridge.
- Run history query and frontend viewing for `SysAiTaskRun`.
- Clear runtime errors when provider, prompt, schedule, or execution fails.
- Focused regression protection proving existing AI knowledge Q&A still behaves as before.

## Protected Existing AI Q&A Boundary

The existing AI Q&A capability is a protected boundary for this work. It includes:

- `KnowledgeQueryAppService.QueryAsync`
- `KnowledgeQueryDto`
- `KnowledgeQueryResultDto`
- `knowledgeApi.query`
- `/develop/knowledge` query playground behavior
- The current retrieval-to-answer flow:
  - `IKnowledgeRetriever.RetrieveAsync`
  - `IRagPromptAugmenter.Augment`
  - `IXiHanAiService.ChatAsync`
  - returning `Answer` plus `Citations`

This work must not change the request or response shape, routing, permissions, page behavior, retrieval filter semantics, prompt augmentation behavior, or citation output of that existing AI Q&A flow.

## Allowed AI Task Scope

Changes should stay inside AI Task ownership:

- `AiTaskPromptRenderer`
- `AiTaskExecutor`
- `IAiTaskChatService`
- `XiHanAiTaskChatService`
- `SysAiTask`
- `SysAiTaskRun`
- AI Task app/query services, DTOs, mappers, repositories, permissions, seeders
- `/develop/ai-task`
- `frontend/src/api/modules/ai/task*`

AI Task may read shared AI infrastructure:

- `SysAiProvider`
- `IAiProviderRepository`
- `IXiHanAiService`
- `SysAiPrompt`
- `IAiPromptStore`

AI Task must not modify shared infrastructure behavior for the sake of task execution. If shared framework behavior has to change, that change must be designed separately and regression-tested against existing AI Q&A.

## Design Approach

### Prompt Rendering

`AiTaskPromptRenderer` should become an asynchronous renderer that supports both prompt modes:

- `Inline`: return `SysAiTask.PromptText` after existing validation.
- `PromptStore`: resolve `PromptCode` and `PromptVersion` through `IAiPromptStore.GetAsync`.

If the prompt cannot be found or is disabled, execution should fail with a clear task-facing error. The renderer should not call `KnowledgeQueryAppService` and should not reuse the RAG prompt augmenter.

Prompt variables are out of scope for the first slice because the current AI Task schema no longer includes `InputVariablesJson`. The first slice should render the stored prompt content as-is.

### Task Execution

`AiTaskExecutor` remains the single orchestration point for task runs:

1. Load `SysAiTask`.
2. Validate enabled status.
3. Render prompt using `AiTaskPromptRenderer`.
4. Create `SysAiTaskRun` with `Running` status and prompt snapshot.
5. Execute through `IAiTaskChatService`.
6. Update the run with success result or failure error.

`IAiTaskChatService` remains the AI Task-specific adapter over `IXiHanAiService`. It may use the selected task provider, but it must not change provider resolver defaults or global AI service behavior.

### Run History

Expose run history from `SysAiTaskRun` through AI Task-owned query contracts:

- page/list by `AiTaskId`
- detail by run id
- fields for status, started time, ended time, duration, prompt snapshot, result text, and error message

The frontend AI Task page should add a row action or drawer/modal for run history. This UI belongs to `/develop/ai-task`; it should not add behavior to `/develop/knowledge`.

### Scheduler Boundary

AI Task should continue to reuse the existing SaaS scheduler bridge:

- `SysAiTask` owns the user-facing definition.
- A backing `SysTask` is synchronized for timing only.
- `DynamicJobWorker` invokes `AiTaskJobExecutor.ExecuteAsync`.
- Direct mutation of AI-managed backing `SysTask` rows remains blocked by the existing guard.

Do not introduce a second scheduler or make users configure `TaskClass`, `TaskMethod`, or reflection details.

### Skill Boundary

Skill execution is not part of this first slice. The current task-skill binding should remain metadata only until a separate design explicitly introduces task-scoped skill execution.

If skill execution is added later, it must be implemented inside AI Task execution and must not change `KnowledgeQueryAppService` behavior. Existing read-only skills such as `knowledge_retrieve` can be considered later, but only with task-owned trace/audit and explicit regression checks.

## Considered Options

### Option A: Finish AI Task Plain Chat Loop First

Complete prompt rendering, run history, and error visibility while keeping execution as plain chat. This is the recommended option because it makes the current feature usable with the smallest blast radius.

### Option B: Immediately Add Agent/Skill Execution

Use the framework agent/tool abstractions now. This is attractive long term, but it would reintroduce tool policy, trace, limits, permissions, and approval questions right after the schema was deliberately simplified.

### Option C: Reuse Existing Knowledge Q&A Flow For Tasks

Call `KnowledgeQueryAppService.QueryAsync` from AI Task. This is not recommended because it would couple scheduled task behavior to an interactive Q&A workflow and increase the chance of breaking the existing knowledge playground.

## Recommendation

Implement Option A first.

This gives AI Task a complete and safe operational loop without affecting the existing AI Q&A feature. Once that loop is stable, a separate design can introduce task-scoped read-only skills or RAG-enhanced task modes.

## Error Handling

AI Task errors should be captured in `SysAiTaskRun.ErrorMessage` and shown in the AI Task run history UI. Expected first-slice error cases include:

- AI task does not exist.
- AI task is disabled.
- Prompt is empty.
- PromptStore reference is missing, disabled, or invalid.
- Provider is missing or disabled.
- AI provider call fails or times out.
- Scheduled invocation fails.

The error handling must not rely on the existing knowledge playground UI.

## Testing

Backend tests should cover:

- Inline prompt rendering still works.
- PromptStore rendering resolves an enabled prompt.
- PromptStore rendering fails clearly when the prompt is missing.
- `AiTaskExecutor` records success and failure runs.
- Run history query returns the expected task runs.
- Existing `KnowledgeQueryAppService.QueryAsync` request/response behavior is unchanged.

Frontend verification should cover:

- `/develop/ai-task` can create or edit tasks with inline and prompt-library prompts.
- manual run shows success or failure.
- run history drawer/modal displays run status, result, prompt snapshot, and error message.
- `/develop/knowledge` query playground still sends the same query request and displays answer/citations.

## Out Of Scope

- Agent tool calling for AI Task.
- Skill execution inside scheduled tasks.
- Approval workflows for AI tools.
- Token/cost accounting.
- Streaming task output.
- Sending AI Task results into existing chat, messages, or notifications.
- Changing the existing AI knowledge Q&A contract or UI.
