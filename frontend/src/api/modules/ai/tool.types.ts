import type { BasicDto, DateTimeString, PageRequest } from '../../types'
import type { EnableStatus } from '../shared'
import type { AiToolRiskLevel, AiToolSafetyLevel, AiToolType } from './tool.enums'

export { EnableStatus } from '../shared'

export interface AiToolPageQueryDto extends PageRequest {
  keyword?: string | null
  toolType?: AiToolType | null
  riskLevel?: AiToolRiskLevel | null
  status?: EnableStatus | null
}

export interface AiToolListItemDto extends BasicDto {
  toolCode: string
  toolName: string
  toolType: AiToolType
  sourceKey: string
  category?: string | null
  description?: string | null
  riskLevel: AiToolRiskLevel
  safetyLevel: AiToolSafetyLevel
  requiresApproval: boolean
  defaultMaxCalls: number
  status: EnableStatus
  createdTime: DateTimeString
  modifiedTime?: DateTimeString | null
}

export interface AiToolDetailDto extends AiToolListItemDto {
  inputSchemaJson?: string | null
  outputSchemaJson?: string | null
  remark?: string | null
}

export interface AiToolSelectItemDto extends BasicDto {
  toolCode: string
  toolName: string
  toolType: AiToolType
  riskLevel: AiToolRiskLevel
  safetyLevel: AiToolSafetyLevel
  requiresApproval: boolean
  defaultMaxCalls: number
}

export interface AiToolCreateDto {
  toolCode: string
  toolName: string
  toolType: AiToolType
  sourceKey: string
  category?: string | null
  description?: string | null
  inputSchemaJson?: string | null
  outputSchemaJson?: string | null
  riskLevel: AiToolRiskLevel
  safetyLevel: AiToolSafetyLevel
  requiresApproval: boolean
  defaultMaxCalls: number
  status: EnableStatus
  remark?: string | null
}

export interface AiToolUpdateDto extends BasicDto {
  toolName: string
  category?: string | null
  description?: string | null
  inputSchemaJson?: string | null
  outputSchemaJson?: string | null
  riskLevel: AiToolRiskLevel
  safetyLevel: AiToolSafetyLevel
  requiresApproval: boolean
  defaultMaxCalls: number
  remark?: string | null
}

export interface AiToolStatusUpdateDto extends BasicDto {
  status: EnableStatus
  remark?: string | null
}
