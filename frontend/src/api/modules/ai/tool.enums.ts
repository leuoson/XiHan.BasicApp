export enum AiToolType {
  BuiltInSkill = 'BuiltInSkill',
}

export enum AiToolRiskLevel {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
}

export enum AiToolSafetyLevel {
  ReadOnly = 'ReadOnly',
  Write = 'Write',
  ExternalNetwork = 'ExternalNetwork',
  SecretAccess = 'SecretAccess',
}

export enum AiTaskToolAccessMode {
  Deny = 'Deny',
  Allow = 'Allow',
  RequireApproval = 'RequireApproval',
}

export const AI_TOOL_TYPE_OPTIONS = [
  { label: '内置技能', value: AiToolType.BuiltInSkill },
]

export const AI_TOOL_RISK_LEVEL_OPTIONS = [
  { label: '低', value: AiToolRiskLevel.Low },
  { label: '中', value: AiToolRiskLevel.Medium },
  { label: '高', value: AiToolRiskLevel.High },
]

export const AI_TOOL_SAFETY_LEVEL_OPTIONS = [
  { label: '只读', value: AiToolSafetyLevel.ReadOnly },
  { label: '写入', value: AiToolSafetyLevel.Write },
  { label: '外部网络', value: AiToolSafetyLevel.ExternalNetwork },
  { label: '密钥访问', value: AiToolSafetyLevel.SecretAccess },
]

export const AI_TASK_TOOL_ACCESS_MODE_OPTIONS = [
  { label: '允许', value: AiTaskToolAccessMode.Allow },
  { label: '需要确认', value: AiTaskToolAccessMode.RequireApproval },
  { label: '禁止', value: AiTaskToolAccessMode.Deny },
]
