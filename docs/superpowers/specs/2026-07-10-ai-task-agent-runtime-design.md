# AI Task Agent Runtime Design

Date: 2026-07-10
Status: Approved design for implementation planning

## Context

The current AI Task feature has task configuration, prompt/provider selection, async run records, and recovery-oriented execution state. The execution path is still effectively a single AI chat call: render prompt, call the AI service, store final text.

That is enough for simple scheduled prompts, but it does not satisfy the next target:

- show real-time run progress and final output in the task history detail page;
- persist an auditable execution record, not only the final AI response;
- allow a user to append extra prompt guidance while the task is running;
- prepare the runtime so a real agent can execute multi-step work with tools;
- keep existing AI conversation/chat capability isolated and unaffected.

## Product Boundary

AI Task and AI Agent share runtime infrastructure, but they do not have the same interaction policy.

AI Task is automation-first:

- it should run without waiting for human interaction by default;
- extra user input is accepted as guidance, not as a required reply;
- the task may apply guidance at safe checkpoints while it is still running;
- completed, failed, or canceled runs must not be silently changed by late guidance;
- operators should be able to audit what happened after the fact.

Future AI Agent is collaboration-first:

- it may actively ask the user for confirmation, missing context, or branch selection;
- it may enter a waiting state such as `WaitingForUserInput`;
- it can reuse the same event/message/tool-call persistence model;
- it can use Microsoft Agent Framework as a configured runtime option.

Existing AI conversation/chat must remain separate:

- no endpoint, DTO, storage table, or UI route used by the existing AI chat experience should be changed for this feature unless explicitly required later;
- AI Task should not reinterpret existing AI chat history as task run history;
- shared low-level provider clients are acceptable, but orchestration, state, and history belong to the AI Task runtime.

## Runtime Strategy

Introduce an AI Task runtime abstraction:

```csharp
public interface IAiTaskAgentRunner
{
    Task RunAsync(AiTaskRunContext context, CancellationToken cancellationToken);
}
```

The AI Task executor depends on this abstraction instead of calling a chat service directly.

Supported runner kinds:

- `PlainChat`: compatibility runner that preserves current one-shot chat behavior.
- `MicrosoftAgentFramework`: preferred long-term runner for agent sessions, streaming updates, tool use, checkpoints, and later human-in-the-loop workflows.
- `SemanticKernelAgent`: optional fallback if Microsoft Agent Framework cannot be adopted immediately or if existing Microsoft.Extensions.AI/Semantic Kernel integrations make it cheaper.

The selected runner is configurable:

```text
AiTaskRuntime:
  DefaultRunnerKind: PlainChat | MicrosoftAgentFramework | SemanticKernelAgent
  AllowTaskOverride: true
```

Each run snapshots its runtime choice:

- `RunnerKind`
- `RunnerVersion`
- `ProviderId`
- `PromptSnapshot`
- `ToolPolicySnapshot`
- optional `AgentSessionId`

This makes later diagnosis possible even after configuration changes.

## Real-Time Run Preview

Run preview must be built on persisted events first and SignalR second.

Persisted events are the source of truth. SignalR is only a delivery optimization for live pages.

Add an AI Task run event model:

```text
SysAiTaskRunEvent
- Id
- RunId
- Sequence
- EventType
- Role
- Content
- PayloadJson
- CreatedTime
```

`Sequence` is monotonic per run and allows reliable resume after disconnect.

Initial event types:

- `RunQueued`
- `RunStarted`
- `PromptRendered`
- `AgentStepStarted`
- `AgentMessageDelta`
- `ToolCallStarted`
- `ToolCallFinished`
- `GuidanceReceived`
- `GuidanceApplied`
- `GuidanceIgnored`
- `RunSucceeded`
- `RunFailed`
- `RunCanceled`

The UI reads existing events on page load and subscribes to live updates while the detail drawer/page stays open.

Suggested query shape:

```http
GET /api/AiTaskQuery/RunEvents/{runId}?afterSequence=0
```

Suggested push event:

```text
AiTaskRunEventReceived
```

The history detail page should show:

- current run status;
- ordered run events;
- rendered prompt snapshot if the user has permission;
- tool call summaries;
- final result;
- guidance records and whether each one was applied.

The UI must not display or imply access to hidden model chain-of-thought. It should show auditable execution records, step summaries, tool calls, streamed model-visible responses, and system-generated status events.

## Additional Prompt Guidance During a Run

For AI Task, additional user input is named guidance, not user reply.

Suggested command:

```http
POST /api/AiTaskRun/{runId}/Guidance
```

Request:

```json
{
  "content": "When summarizing, focus on overdue high-risk items.",
  "clientRequestId": "optional-idempotency-key"
}
```

Behavior:

- `Queued` or `Running`: accept and persist guidance.
- `Success`, `Failed`, `Canceled`: reject with a clear state error or persist as `GuidanceIgnored`; the initial implementation should reject to avoid confusing operators.
- duplicate `clientRequestId`: return the previous guidance result.
- every accepted guidance produces `GuidanceReceived`.
- when the runner consumes it at a checkpoint, produce `GuidanceApplied`.
- if the run completes before consuming it, mark it `GuidanceIgnored`.

The runner checks for pending guidance at controlled checkpoints:

- before the first model call;
- between agent steps;
- before a tool call if the runner supports interruption;
- before final summarization.

The runner is not required to interrupt an in-flight provider request. This keeps implementation and recovery semantics predictable.

## Data Model Additions

Add a guidance table:

```text
SysAiTaskRunGuidance
- Id
- RunId
- Content
- Status: Pending | Applied | Ignored
- ClientRequestId
- CreatedBy
- CreatedTime
- AppliedTime
- IgnoredReason
```

Add optional run-level fields:

```text
SysAiTaskRun
- RunnerKind
- RunnerVersion
- AgentSessionId
```

Keep final result fields on `SysAiTaskRun` for list/detail compatibility. The event table is additional detail, not a replacement for `ResultText`.

## Recovery Semantics

The existing async run recovery model remains responsible for queued/running task recovery.

Event and guidance persistence must follow these rules:

- event writes are append-only;
- completion/failure events are emitted only after the run status transition succeeds, or in the same transaction if supported;
- recovery can continue a run from persisted status and events;
- guidance accepted before a crash remains pending and can be applied after retry;
- duplicate events from retry should be minimized with run step ids or idempotent event creation where practical.

If a recovered run cannot resume an existing provider/agent session, it may start a new session using the prompt snapshot plus prior applied guidance. The run event stream must make that visible.

## Permissions And Isolation

Use AI Task-specific permissions for new APIs:

- view run events;
- append run guidance;
- view prompt snapshot and tool payload details if those are considered sensitive.

Do not reuse permissions from existing AI chat unless the project later decides both surfaces share an authorization policy.

Existing AI chat remains unaffected because:

- AI Task gets its own event and guidance tables;
- AI Task uses `IAiTaskAgentRunner`, not the existing chat UI/session abstraction;
- provider clients can be shared only below the orchestration layer;
- no existing AI chat API contract is changed in this design.

## Microsoft Agent Framework Adoption

Microsoft Agent Framework should be treated as a runtime adapter, not the domain model.

The project-owned domain model remains:

- run;
- run event;
- guidance;
- tool policy snapshot;
- final result.

The adapter maps framework concepts into this model:

- streaming updates become `AgentMessageDelta` or step events;
- tool calls become `ToolCallStarted` and `ToolCallFinished`;
- framework session id becomes `AgentSessionId`;
- framework checkpoint or workflow state is stored only if needed for resume.

This avoids locking AI Task storage and APIs to one external SDK.

## Implementation Phases

Phase 1: Observability and guidance with current plain-chat runner.

- add run event persistence;
- emit queue/start/prompt/final/failure events;
- add guidance API and persistence;
- allow pending guidance to be merged before the plain chat call starts;
- show run events and guidance state in the history detail UI.

Phase 2: Agent runner abstraction.

- introduce `IAiTaskAgentRunner`;
- move current chat execution behind `PlainChatAiTaskRunner`;
- snapshot `RunnerKind` and runtime metadata on each run;
- keep `/api/AiTask/Run` behavior unchanged from the caller's perspective.

Phase 3: Microsoft Agent Framework adapter.

- add configurable runner implementation;
- map streaming, tool calls, and agent steps to run events;
- apply guidance at agent checkpoints;
- preserve recovery behavior and fall back clearly when sessions cannot resume.

Phase 4: Future interactive AI Agent mode.

- add waiting input state and user-reply semantics;
- reuse event/message/guidance storage where compatible;
- expose a separate product surface from AI Task.

## Testing Requirements

Backend tests:

- run event sequence is monotonic per run;
- accepted guidance is persisted only for active runs;
- duplicate `clientRequestId` is idempotent;
- late guidance is rejected for completed runs;
- plain-chat runner consumes pre-start guidance;
- run recovery preserves pending guidance;
- existing AI chat-related tests still pass.

Frontend tests or focused verification:

- history detail loads persisted events;
- live SignalR events append without duplicating after reconnect;
- guidance input is enabled only for active runs;
- final result display remains compatible with existing run detail fields.

Manual verification:

- start a run and observe queued/running/success events;
- append guidance while queued or running and verify whether it is applied or rejected based on timing;
- refresh the detail page during a run and confirm events are reconstructed from persistence;
- confirm existing AI chat flow still works through its original route/API.

## Implementation Planning Decisions

These should be resolved during implementation planning, not by changing the product boundary:

- whether `RunEvents` belongs under `AiTaskQuery` or a new `AiTaskRunQuery` service;
- whether SignalR uses the existing notification hub or a dedicated AI Task hub;
- whether event payloads are plain JSON text or typed DTO payloads per event type;
- whether Phase 1 guidance should be merged into prompt text or passed as structured messages to the provider, while preserving the confirmed guidance semantics.
