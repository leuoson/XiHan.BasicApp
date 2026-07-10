import type { ApiId, BasicDto, DateTimeString } from '../../types'
import type { EnableStatus } from '../shared'
import type {
  AiTaskPromptMode,
  AiTaskRunEventRole,
  AiTaskRunEventType,
  AiTaskRunGuidanceStatus,
  AiTaskRunnerKind,
  AiTaskRunStatus,
  AiTaskTriggerType,
} from './task.enums'
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

export interface AiTaskAppendGuidanceDto {
  runId: ApiId
  content: string
  clientRequestId?: string | null
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
  runId?: ApiId | null
  runStatus?: AiTaskRunStatus | null
  succeeded: boolean
  resultText?: string | null
  errorMessage?: string | null
}

export interface AiTaskRunListItemDto extends BasicDto {
  aiTaskId: ApiId
  aiTaskCode: string
  startedTime: DateTimeString
  endedTime?: DateTimeString | null
  runStatus: AiTaskRunStatus
  attemptCount: number
  durationMilliseconds?: number | null
  errorMessage?: string | null
  createdTime: DateTimeString
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

export interface AiTaskRunDetailDto extends AiTaskRunListItemDto {
  leaseOwner?: string | null
  leaseExpiresAt?: DateTimeString | null
  lastHeartbeatTime?: DateTimeString | null
  runnerKind?: AiTaskRunnerKind | string | null
  runnerVersion?: string | null
  agentSessionId?: string | null
  promptSnapshot?: string | null
  resultText?: string | null
  events: AiTaskRunEventDto[]
  guidance: AiTaskRunGuidanceDto[]
}
