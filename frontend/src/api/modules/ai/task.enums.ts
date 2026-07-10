/** AI 任务枚举（与后端字符串枚举 wire value 一致）。 */

export enum AiTaskPromptMode {
  Inline = 'Inline',
  PromptStore = 'PromptStore',
}

export enum AiTaskTriggerType {
  Immediate = 'Immediate',
  Schedule = 'Schedule',
  Recurring = 'Recurring',
  Cron = 'Cron',
}

export enum AiTaskRunStatus {
  Running = 'Running',
  Success = 'Success',
  Failed = 'Failed',
  Canceled = 'Canceled',
  Queued = 'Queued',
}

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

export const AI_TASK_PROMPT_MODE_OPTIONS = [
  { label: '内联提示词', value: AiTaskPromptMode.Inline },
  { label: '提示词库', value: AiTaskPromptMode.PromptStore },
]

export const AI_TASK_TRIGGER_TYPE_OPTIONS = [
  { label: '立即执行', value: AiTaskTriggerType.Immediate },
  { label: '一次性定时', value: AiTaskTriggerType.Schedule },
  { label: '循环执行', value: AiTaskTriggerType.Recurring },
  { label: 'Cron 表达式', value: AiTaskTriggerType.Cron },
]

export const AI_TASK_SCHEDULE_TRIGGER_TYPE_OPTIONS = AI_TASK_TRIGGER_TYPE_OPTIONS
  .filter(option => option.value !== AiTaskTriggerType.Immediate)

export const AI_TASK_RUN_STATUS_OPTIONS = [
  { label: '排队中', value: AiTaskRunStatus.Queued },
  { label: '执行中', value: AiTaskRunStatus.Running },
  { label: '成功', value: AiTaskRunStatus.Success },
  { label: '失败', value: AiTaskRunStatus.Failed },
  { label: '已取消', value: AiTaskRunStatus.Canceled },
]
