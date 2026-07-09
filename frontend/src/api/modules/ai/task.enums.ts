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
