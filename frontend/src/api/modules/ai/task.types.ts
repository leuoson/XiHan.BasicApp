import type { ApiId, BasicDto, DateTimeString } from '../../types'
import type { EnableStatus } from '../shared'
import type { AiTaskPromptMode, AiTaskTriggerType } from './task.enums'
import type { AiToolRiskLevel, AiToolSafetyLevel, AiToolType } from './tool.enums'

export { EnableStatus } from '../shared'

export interface AiTaskCreateDto {
  aiTaskCode: string
  aiTaskName: string
  description?: string | null
  category?: string | null
  promptMode: AiTaskPromptMode
  promptText?: string | null
  promptCode?: string | null
  promptVersion?: string | null
  providerId?: ApiId | null
  triggerType: AiTaskTriggerType
  cronExpression?: string | null
  startTime?: DateTimeString | null
  endTime?: DateTimeString | null
  intervalSeconds?: number | null
  repeatCount: number
  timeoutSeconds: number
  priority: number
  allowConcurrent: boolean
  maxRetryCount: number
  sort: number
  status: EnableStatus
  toolPolicies: AiTaskToolPolicyInputDto[]
  remark?: string | null
}

export interface AiTaskUpdateDto extends BasicDto, Omit<AiTaskCreateDto, 'aiTaskCode' | 'status'> {
}

export interface AiTaskStatusUpdateDto extends BasicDto {
  status: EnableStatus
  remark?: string | null
}

export interface AiTaskRunDto extends BasicDto {
}

export interface AiTaskToolPolicyInputDto {
  toolId: ApiId
  remark?: string | null
}

export interface AiTaskToolPolicyDto extends BasicDto {
  toolId: ApiId
  toolCode: string
  toolName: string
  toolType: AiToolType
  riskLevel: AiToolRiskLevel
  safetyLevel: AiToolSafetyLevel
  isAvailable: boolean
  remark?: string | null
}

export interface AiTaskListItemDto extends BasicDto {
  aiTaskCode: string
  aiTaskName: string
  description?: string | null
  category?: string | null
  promptMode: AiTaskPromptMode
  providerId?: ApiId | null
  backingTaskId?: ApiId | null
  triggerType: AiTaskTriggerType
  cronExpression?: string | null
  intervalSeconds?: number | null
  repeatCount: number
  timeoutSeconds: number
  priority: number
  allowConcurrent: boolean
  maxRetryCount: number
  sort: number
  capabilityCount: number
  status: EnableStatus
  createdTime: DateTimeString
  modifiedTime?: DateTimeString | null
}

export interface AiTaskDetailDto extends AiTaskListItemDto {
  promptText?: string | null
  promptCode?: string | null
  promptVersion?: string | null
  startTime?: DateTimeString | null
  endTime?: DateTimeString | null
  toolPolicies: AiTaskToolPolicyDto[]
  remark?: string | null
}

export interface AiTaskExecutionResultDto {
  succeeded: boolean
  resultText?: string | null
  errorMessage?: string | null
}
