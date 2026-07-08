# AI Scheduled Task Design

## Background

The project already has a general scheduled task subsystem in the SaaS module. `SysTask` stores schedule metadata, `TaskSchedulerSyncService` registers enabled rows with the XiHan scheduled job framework, and `DynamicJobWorker` executes the configured task by resolving `TaskClass` and invoking `TaskMethod` through reflection.

This is useful as an internal scheduler, but it is not the right user-facing model for AI automation. A user should not configure backend class names, method names, or raw reflection parameters to run an AI task. The user-facing concept should be an AI scheduled task: write a prompt, choose a provider, choose the tools the agent may use, configure a trigger time, and let the system execute it automatically.

## Goal

Add a first-class AI scheduled task capability that lets users create, manage, and run timed AI agent work such as "collect and summarize morning news every day at 09:00". The feature should reuse the existing `SysTask` scheduler as the timing and execution trigger substrate, while presenting a safer and more human AI task configuration model.

## Non-Goals

- Do not expose `TaskClass`, `TaskMethod`, or reflection concepts in the AI task UI.
- Do not build a second independent scheduler.
- Do not let AI grant itself tool access or bypass user permissions.
- Do not implement every possible output action in the first slice. Message push, webhook calls, and business-table writes should be modeled as controlled actions/tools, not as arbitrary code execution.

## Current State

`SysTask` is developer-oriented:

- `TaskClass` is required on create and persisted as a non-null column.
- `TaskMethod` and `TaskParams` are passed to `DynamicJobWorker`.
- The scheduler registers every enabled task as a `JobInfo` whose `JobType` is `DynamicJobWorker`.
- `DynamicJobWorker` loads `SysTask` by `TaskCode`, checks status, resolves the configured class, creates an instance through DI or `Activator`, deserializes JSON parameters, and invokes the method.

The existing flow should be reused for time calculation, retries, concurrency, timeout, execution status, and task logs. The AI feature should not ask users to configure that low-level execution model directly.

## Product Model

Introduce AI Task as the user-facing feature.

An AI task should contain:

- Basic information: code, name, description, category, status.
- Prompt source: inline prompt text or reference to an existing `SysAiPrompt` by `PromptCode` and optional version.
- Provider: selected `SysAiProvider.ConfigCode`, with null meaning default provider.
- Schedule: trigger type, Cron expression, start/end time, interval, repeat count, timeout, retry, concurrency, and priority.
- Agent mode: plain chat, agent with tools, or RAG-enhanced agent.
- Tool policy: the explicit tools this task may use, with per-tool limits and permission requirements.
- Execution identity: the user identity and tenant context under which scheduled runs execute.
- Output policy: how execution results are stored in the first slice and which controlled actions may receive the result in future slices.
- Backing scheduler link: `SysTaskId` for the internal scheduler row.

The AI task page should be the main management surface. Backing `SysTask` rows should use `TaskGroup = "ai-task"` and be shown as system-managed rows on the existing system task page, with direct edit/delete actions disabled for those rows.

## Data Design

### SysAiTask

Stores the AI automation definition.

Key fields:

- `AiTaskCode`: tenant-unique code.
- `AiTaskName`: display name.
- `Description`, `Category`, `Remark`.
- `PromptMode`: inline or prompt-library.
- `PromptText`: inline prompt body when `PromptMode` is inline.
- `PromptCode`, `PromptVersion`: prompt-library reference.
- `ProviderCode`: AI provider config code; null means default provider.
- `AgentMode`: plain chat, agent with tools, RAG agent.
- `Schedule*`: schedule fields mirrored to the backing `SysTask`.
- `RunAsUserId`: user identity for scheduled execution.
- `BackingSysTaskId`: linked `SysTask.BasicId`.
- `Status`: enabled or disabled.

### SysAiTool

Stores the tool catalog exposed to AI tasks.

Key fields:

- `ToolCode`: stable unique tool id.
- `ToolName`, `Description`, `Category`.
- `ToolSource`: built-in skill, MCP tool, RAG, internal API, or output action.
- `RequiredPermissionCode`: permission needed to grant/use this tool.
- `RiskLevel`: low, medium, high.
- `InputSchemaJson`: optional schema shown to users and used for validation.
- `DefaultMaxCalls`: safe default call limit.
- `Status`.

The first implementation can seed built-in tools from known AI skills and add a manual catalog table. A future tool-catalog synchronization slice can import metadata from `IAiSkillRegistry` and MCP tool metadata.

### SysAiTaskToolPolicy

Stores the tools allowed for one AI task.

Key fields:

- `AiTaskId`.
- `ToolCode`.
- `IsEnabled`.
- `MaxCalls`.
- `AllowedArgumentsJson` or `ArgumentPolicyJson`.
- `Remark`.

The executor must enforce this policy. Prompt text alone must never authorize tool use.

### SysAiTaskRun

Stores AI task execution history.

Key fields:

- `AiTaskId`.
- `SysTaskLogId` or `BatchNumber` to correlate with scheduler logs.
- `RunStatus`: pending, running, success, failed, canceled.
- `TriggerMode`: scheduled or manual.
- `PromptSnapshot`: rendered prompt or reference snapshot.
- `ProviderSnapshot`: provider code and model at execution time.
- `ToolPolicySnapshot`: allowed tools at execution time.
- `ResultText`: AI final output.
- `ToolTraceJson`: summarized tool calls and results.
- `TokenUsageJson`, `CostJson`: optional telemetry.
- `ErrorMessage`, `ErrorStackTrace`.
- `StartedTime`, `EndedTime`, `ExecutionMilliseconds`.

## Execution Flow

1. User creates or updates an AI task through `AiTaskAppService`.
2. The app service validates prompt/provider/schedule/tool policy and writes `SysAiTask`.
3. The app service creates or updates a backing `SysTask` with:
   - `TaskClass = AiTaskJobExecutor`
   - `TaskMethod = ExecuteAsync`
   - `TaskParams = {"aiTaskId": <id>}`
   - schedule, retry, timeout, concurrency, priority, and status mirrored from `SysAiTask`
4. Existing scheduler sync registers the backing `SysTask`.
5. On trigger, `DynamicJobWorker` invokes `AiTaskJobExecutor.ExecuteAsync`.
6. `AiTaskJobExecutor` loads `SysAiTask`, reconstructs tenant/user execution context, snapshots prompt/provider/tool policy, creates a run record, and executes the AI task.
7. If no tools are enabled, execution can call `IXiHanAiService.ChatAsync`.
8. If tools are enabled, execution creates an agent through the XiHan AI agent factory and injects only tools allowed by `SysAiTaskToolPolicy`.
9. The executor writes result or failure to `SysAiTaskRun`; scheduler infrastructure writes `SysTaskLog`.
10. Future output actions such as "send to my messages" can be modeled as controlled tools or post-run actions.

## Tool And Permission Policy

Tool access has three gates:

1. The tool is enabled in `SysAiTool`.
2. The AI task policy explicitly allows the tool.
3. The task's execution identity has the tool's required permission.

If any gate fails, the tool is unavailable to the agent. If permissions are revoked after task creation, future runs should fail closed or run without the revoked tool, depending on the task's strictness setting.

The AI task creation assistant may suggest tools, schedules, and prompt text, but it must only choose from tools the current user is already allowed to use. It may create a disabled draft or a pending-confirmation draft, but it must not enable high-risk tools automatically.

## User Experience

Add an AI task management page under the AI/development area, separate from the low-level system task page.

Primary workflows:

- Create AI task from form:
  - name, description, provider, prompt, schedule, tool policy, status.
- Create AI task with assistant:
  - user describes intent in natural language.
  - AI proposes structured draft: prompt, Cron, provider, tools.
  - user reviews and confirms.
- Test run:
  - execute once immediately, show result and tool trace.
- Run history:
  - list AI task runs, status, result, errors, tool traces.
- Tool policy management:
  - list available tools with permission/risk indicators.
  - allow only authorized tools.

The existing system task page remains useful for platform administrators and diagnostics, but normal users should manage AI tasks from the AI task page.

## First Implementation Slice

Build the smallest complete loop:

- `SysAiTask`, `SysAiTaskToolPolicy`, and `SysAiTaskRun`.
- AI task CRUD/query services.
- Backing `SysTask` create/update/delete/status sync.
- `AiTaskJobExecutor` that supports:
  - inline prompt or prompt-library prompt.
  - selected provider or default provider.
  - plain chat execution.
  - tool policy storage and validation, with agent/tool execution prepared behind an interface.
- AI task page with prompt/provider/schedule/tool-policy fields.
- Manual "run now" and run history.

Tool execution can be introduced behind an `IAiTaskToolProvider` abstraction so the data model and UI are ready for tool policies from the first slice, even if only a small built-in tool set is enabled initially.

## Decisions For The First Implementation

- Backing `SysTask` rows use `TaskGroup = "ai-task"` and are displayed as read-only system-managed rows in the system task page.
- The first seeded tool catalog should include the existing knowledge retrieval skill and a disabled placeholder catalog entry for web search, because web search requires a real tool/provider integration before it can run safely.
- High-risk tools cannot be enabled automatically by the assistant-created draft flow. They require explicit user confirmation and the matching permission code.
- The first output action is run history only. "Send result to my messages" should be implemented in a future output-action slice as a controlled tool/action, not as arbitrary executor code.

