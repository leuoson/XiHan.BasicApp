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
  status: EnableStatus
  createdTime: DateTimeString
  modifiedTime?: DateTimeString | null
}

export interface AiToolDetailDto extends AiToolListItemDto {
  remark?: string | null
}

export interface AiToolSelectItemDto extends BasicDto {
  toolCode: string
  toolName: string
  toolType: AiToolType
  riskLevel: AiToolRiskLevel
  safetyLevel: AiToolSafetyLevel
}

export interface AiToolCreateDto {
  toolCode: string
  toolName: string
  toolType: AiToolType
  sourceKey: string
  category?: string | null
  description?: string | null
  riskLevel: AiToolRiskLevel
  safetyLevel: AiToolSafetyLevel
  status: EnableStatus
  remark?: string | null
}

export interface AiToolUpdateDto extends BasicDto {
  toolName: string
  category?: string | null
  description?: string | null
  riskLevel: AiToolRiskLevel
  safetyLevel: AiToolSafetyLevel
  remark?: string | null
}

export interface AiToolStatusUpdateDto extends BasicDto {
  status: EnableStatus
  remark?: string | null
}
