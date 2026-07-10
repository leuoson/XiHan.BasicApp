<script setup lang="ts">
import type {
  AiPromptListItemDto,
  AiProviderListItemDto,
  AiTaskCreateDto,
  AiTaskDetailDto,
  AiTaskListItemDto,
  AiTaskRunDetailDto,
  AiTaskRunEventDto,
  AiTaskRunListItemDto,
  AiTaskToolPolicyInputDto,
  AiTaskUpdateDto,
  AiToolSelectItemDto,
} from '@/api'
import type { DataTableColumns, SelectOption } from 'naive-ui'
import type { ListFieldSchema, PageSchema, SchemaActionPayload } from '~/components'
import type { PageResult } from '~/types/contracts'
import { Icon } from '@iconify/vue'
import {
  NButton,
  NDataTable,
  NDatePicker,
  NDrawer,
  NDrawerContent,
  NForm,
  NFormItem,
  NIcon,
  NInput,
  NInputNumber,
  NModal,
  NSelect,
  NSpace,
  NSwitch,
  NTag,
  NTimeline,
  NTimelineItem,
  NTooltip,
  useDialog,
  useMessage,
} from 'naive-ui'
import { computed, h, onMounted, onUnmounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  AI_TASK_PROMPT_MODE_OPTIONS,
  AiTaskRunEventRole,
  AiTaskRunEventType,
  AiTaskRunGuidanceStatus,
  AI_TASK_RUN_STATUS_OPTIONS,
  AI_TASK_SCHEDULE_TRIGGER_TYPE_OPTIONS,
  AI_TASK_TRIGGER_TYPE_OPTIONS,
  AI_TOOL_RISK_LEVEL_OPTIONS,
  AI_TOOL_SAFETY_LEVEL_OPTIONS,
  aiPromptApi,
  aiProviderApi,
  aiTaskApi,
  aiToolApi,
  AiTaskPromptMode,
  AiTaskRunStatus,
  EnableStatus,
  AiTaskTriggerType,
  createPageRequest,
} from '@/api'
import { SchemaPage } from '~/components'
import { useSignalR } from '~/composables'
import { STATUS_OPTIONS } from '~/constants'
import { useEnumOptions } from '~/hooks'
import { getOptionLabel } from '~/utils'

defineOptions({ name: 'DevelopAiTaskPage' })

interface TaskFormModel {
  basicId?: string
  aiTaskCode: string
  aiTaskName: string
  description?: string | null
  category?: string | null
  promptMode: AiTaskPromptMode
  promptText?: string | null
  promptCode?: string | null
  promptVersion?: string | null
  providerId?: string | null
  triggerType: AiTaskTriggerType
  cronExpression?: string | null
  startTime?: number | null
  endTime?: number | null
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

const { t } = useI18n()
const message = useMessage()
const dialog = useDialog()
const signalR = useSignalR()
const statusEnumOptions = useEnumOptions('EnableStatus', STATUS_OPTIONS)
const AI_TASK_RUN_EVENT_RECEIVED = 'AiTaskRunEventReceived'
const RUN_EVENT_POLLING_INTERVAL_MS = 2000

const lookupLoading = ref(false)
const submitLoading = ref(false)
const providers = ref<AiProviderListItemDto[]>([])
const prompts = ref<AiPromptListItemDto[]>([])
const tools = ref<AiToolSelectItemDto[]>([])
const schemaPageRef = ref<{ reload: () => Promise<void> } | null>(null)
const modalVisible = ref(false)
const skillPickerVisible = ref(false)
const runHistoryVisible = ref(false)
const runHistoryLoading = ref(false)
const editingStatus = ref<EnableStatus | null>(null)
const skillPickerSelection = ref<string[]>([])
const skillPickerKeyword = ref('')
const form = ref<TaskFormModel>(createDefaultForm())
const runHistoryRows = ref<AiTaskRunListItemDto[]>([])
const runHistoryDetail = ref<AiTaskRunDetailDto | null>(null)
const runHistoryTask = ref<AiTaskListItemDto | null>(null)
const runGuidanceInput = ref('')
const runGuidanceSubmitting = ref(false)
let runEventPollingTimer: ReturnType<typeof setInterval> | null = null

const modalTitle = computed(() => (form.value.basicId ? t('develop.ai_task.modal_edit_title') : t('develop.ai_task.modal_add_title')))
const runHistoryTitle = computed(() => runHistoryTask.value
  ? `${t('develop.ai_task.run_history_title')} - ${runHistoryTask.value.aiTaskName}`
  : t('develop.ai_task.run_history_title'))
const canAppendRunGuidance = computed(() => !!runHistoryDetail.value && isRunActive(runHistoryDetail.value.runStatus))

function reload() {
  void schemaPageRef.value?.reload()
}

const providerSelectOptions = computed<SelectOption[]>(() => {
  const options = providers.value.map(provider => ({
    label: [
      provider.configName,
      provider.provider || provider.model ? `(${[provider.provider, provider.model].filter(Boolean).join(' / ')})` : '',
      provider.isDefault ? t('common.statuses.default_tag') : '',
    ].filter(Boolean).join(' '),
    value: provider.basicId,
  }))

  const currentProviderId = form.value.providerId
  if (currentProviderId && !options.some(option => option.value === currentProviderId)) {
    options.unshift({
      label: t('develop.ai_task.option_current_value', { value: currentProviderId }),
      value: currentProviderId,
    })
  }

  return options
})

const promptSelectOptions = computed<SelectOption[]>(() => {
  const options = prompts.value.map(prompt => ({
    label: [
      prompt.promptName,
      `(${prompt.promptCode})`,
      prompt.version ? `v${prompt.version}` : '',
      prompt.category ? `[${prompt.category}]` : '',
    ].filter(Boolean).join(' '),
    value: prompt.promptCode,
  }))

  const currentPromptCode = form.value.promptCode
  if (currentPromptCode && !options.some(option => option.value === currentPromptCode)) {
    options.unshift({
      label: t('develop.ai_task.option_current_value', { value: currentPromptCode }),
      value: currentPromptCode,
    })
  }

  return options
})

const toolById = computed(() => new Map(tools.value.map(tool => [tool.basicId, tool])))
const filteredSkillRows = computed(() => {
  const keyword = skillPickerKeyword.value.trim().toLowerCase()
  if (!keyword) {
    return tools.value
  }

  return tools.value.filter(tool =>
    [tool.toolName, tool.toolCode].some(value => value.toLowerCase().includes(keyword)))
})

const fields = computed<ListFieldSchema[]>(() => [
  { key: 'keyword', title: t('develop.ai_task.col_name'), dataType: 'string', visible: false, searchable: true, searchPlaceholder: t('develop.ai_task.search_placeholder'), order: 0 },
  { title: t('develop.ai_task.col_name'), key: 'aiTaskName', dataType: 'string', minWidth: 180, fixed: 'left', sortable: true, order: 1 },
  { title: t('develop.ai_task.col_code'), key: 'aiTaskCode', dataType: 'string', minWidth: 150, sortable: true, order: 2 },
  { title: t('develop.ai_task.col_category'), key: 'category', dataType: 'string', width: 110, order: 3 },
  {
    title: t('develop.ai_task.col_trigger'),
    key: 'triggerType',
    dataType: 'enum',
    options: AI_TASK_TRIGGER_TYPE_OPTIONS,
    width: 140,
    order: 4,
    render(row) {
      const r = row as unknown as AiTaskListItemDto
      const label = getOptionLabel(AI_TASK_TRIGGER_TYPE_OPTIONS, r.triggerType)
      return h('span', {}, r.triggerType === AiTaskTriggerType.Cron && r.cronExpression ? `${label}: ${r.cronExpression}` : label)
    },
  },
  {
    title: t('develop.ai_task.col_capabilities'),
    key: 'capabilityCount',
    dataType: 'number',
    width: 120,
    order: 5,
    render(row) {
      const r = row as unknown as AiTaskListItemDto
      return h(NTag, { size: 'small', round: true, bordered: false, type: r.capabilityCount > 0 ? 'info' : 'default' }, () =>
        r.capabilityCount > 0
          ? t('develop.ai_task.capability_count', { count: r.capabilityCount })
          : t('develop.ai_task.capability_none'))
    },
  },
  {
    title: t('common.fields.status'),
    key: 'status',
    dataType: 'enum',
    dictionaryCode: 'EnableStatus',
    searchable: true,
    searchMultiple: true,
    sortable: true,
    options: STATUS_OPTIONS,
    searchPlaceholder: t('common.fields.status'),
    width: 90,
    order: 6,
    render(row) {
      const r = row as unknown as AiTaskListItemDto
      return h(NTag, { size: 'small', round: true, bordered: false, type: r.status === EnableStatus.Enabled ? 'success' : 'error' }, () => getOptionLabel(statusEnumOptions.value, r.status))
    },
  },
  {
    title: t('develop.ai_task.col_backing_task'),
    key: 'backingTaskId',
    dataType: 'string',
    width: 120,
    order: 7,
    render(row) {
      const r = row as unknown as AiTaskListItemDto
      return r.backingTaskId
        ? h(NTag, { size: 'small', round: true, bordered: false, type: 'info' }, () => t('develop.ai_task.tag_synced'))
        : h(NTag, { size: 'small', round: true, bordered: false, type: 'warning' }, () => t('develop.ai_task.tag_unsynced'))
    },
  },
])

const schema = computed<PageSchema>(() => ({
  pageCode: 'develop.ai.task',
  pageName: t('develop.ai_task.page_name'),
  rowKey: 'basicId',
  scrollX: 1220,
  fields: fields.value,
  resource: {
    page: async (params) => {
      const all = await aiTaskApi.list()
      const filtered = filterTaskRows(all, params.filters)
      const sorted = sortTaskRows(filtered, params.sorts ?? [])
      return toTaskPageResult(sorted, params.page, params.pageSize) as unknown as PageResult<Record<string, unknown>>
    },
  },
  actions: [
    { key: 'create', title: t('develop.ai_task.add'), scope: 'page', type: 'primary', icon: 'lucide:plus' },
    { key: 'run', title: t('develop.ai_task.action_run'), scope: 'row', type: 'info', icon: 'lucide:play' },
    { key: 'history', title: t('develop.ai_task.action_history'), scope: 'row', icon: 'lucide:list-clock' },
    { key: 'enable', title: t('common.actions.enable'), scope: 'row', icon: 'lucide:power', visible: row => (row as unknown as AiTaskListItemDto).status !== EnableStatus.Enabled },
    { key: 'disable', title: t('common.actions.disable'), scope: 'row', icon: 'lucide:power-off', visible: row => (row as unknown as AiTaskListItemDto).status === EnableStatus.Enabled },
    { key: 'edit', title: t('common.actions.edit'), scope: 'row', icon: 'lucide:pencil' },
    { key: 'delete', title: t('common.actions.delete'), scope: 'row', type: 'error', icon: 'lucide:trash-2' },
  ],
}))

function filterTaskRows(rows: AiTaskListItemDto[], filters: Record<string, unknown>) {
  const keyword = typeof filters.keyword === 'string' ? filters.keyword.trim().toLowerCase() : ''
  const rawStatus = filters.status
  const statuses = Array.isArray(rawStatus) ? rawStatus : rawStatus !== undefined && rawStatus !== null && rawStatus !== '' ? [rawStatus] : []

  return rows.filter((row) => {
    const keywordMatched = !keyword || [row.aiTaskName, row.aiTaskCode].some(value => value?.toLowerCase().includes(keyword))
    const statusMatched = statuses.length === 0 || statuses.includes(row.status)
    return keywordMatched && statusMatched
  })
}

function sortTaskRows(rows: AiTaskListItemDto[], sorts: Array<{ field: string, order?: string }>) {
  const activeSort = sorts[0]
  if (!activeSort?.field || !activeSort.order) {
    return rows
  }

  const factor = activeSort.order === 'descend' ? -1 : 1
  return [...rows].sort((left, right) => {
    const a = left[activeSort.field as keyof AiTaskListItemDto]
    const b = right[activeSort.field as keyof AiTaskListItemDto]
    return String(a ?? '').localeCompare(String(b ?? ''), undefined, { numeric: true }) * factor
  })
}

function toTaskPageResult(rows: AiTaskListItemDto[], page: number, pageSize: number): PageResult<AiTaskListItemDto> {
  const start = (page - 1) * pageSize
  const items = rows.slice(start, start + pageSize)
  const totalPages = Math.max(1, Math.ceil(rows.length / pageSize))
  return {
    items,
    page: {
      currentPageCount: items.length,
      endRecord: start + items.length,
      hasNext: page < totalPages,
      hasPrevious: page > 1,
      isFirstPage: page <= 1,
      isLastPage: page >= totalPages,
      pageIndex: page,
      pageSize,
      startRecord: items.length ? start + 1 : 0,
      totalCount: rows.length,
      totalPages,
    },
  }
}

const toolPolicyColumns = computed<DataTableColumns<AiTaskToolPolicyInputDto>>(() => [
  {
    title: t('develop.ai_task.form_skills'),
    key: 'toolId',
    minWidth: 180,
    render(row) {
      const tool = toolById.value.get(row.toolId)
      return h('div', { class: 'policy-tool' }, [
        h('span', { class: 'policy-tool__name' }, tool?.toolName ?? row.toolId),
      ])
    },
  },
  {
    title: t('develop.ai_task.policy_risk'),
    key: 'riskLevel',
    width: 90,
    render(row) {
      const tool = toolById.value.get(row.toolId)
      return tool
        ? h(NTag, { size: 'small', round: true, bordered: false }, () => getOptionLabel(AI_TOOL_RISK_LEVEL_OPTIONS, tool.riskLevel))
        : '-'
    },
  },
  {
    title: t('develop.ai_task.policy_safety'),
    key: 'safetyLevel',
    width: 120,
    render(row) {
      const tool = toolById.value.get(row.toolId)
      return tool
        ? h(NTag, { size: 'small', round: true, bordered: false, type: 'info' }, () => getOptionLabel(AI_TOOL_SAFETY_LEVEL_OPTIONS, tool.safetyLevel))
        : '-'
    },
  },
  {
    title: t('common.fields.actions'),
    key: 'actions',
    width: 90,
    render(_, index) {
      return h(NButton, { size: 'small', type: 'error', onClick: () => removeToolPolicy(index) }, () => t('common.actions.delete'))
    },
  },
])
const skillPickerColumns = computed<DataTableColumns<AiToolSelectItemDto>>(() => [
  { type: 'selection', width: 48 },
  {
    title: t('develop.ai_task.form_skills'),
    key: 'toolName',
    minWidth: 220,
    render(row) {
      return h('div', { class: 'policy-tool' }, [
        h('span', { class: 'policy-tool__name' }, `${row.toolName} (${row.toolCode})`),
      ])
    },
  },
  {
    title: t('develop.ai_task.policy_risk'),
    key: 'riskLevel',
    width: 90,
    render(row) {
      return h(NTag, { size: 'small', round: true, bordered: false }, () => getOptionLabel(AI_TOOL_RISK_LEVEL_OPTIONS, row.riskLevel))
    },
  },
  {
    title: t('develop.ai_task.policy_safety'),
    key: 'safetyLevel',
    width: 120,
    render(row) {
      return h(NTag, { size: 'small', round: true, bordered: false, type: 'info' }, () => getOptionLabel(AI_TOOL_SAFETY_LEVEL_OPTIONS, row.safetyLevel))
    },
  },
])
const runHistoryColumns = computed<DataTableColumns<AiTaskRunListItemDto>>(() => [
  {
    title: t('develop.ai_task.run_col_status'),
    key: 'runStatus',
    width: 100,
    render(row) {
      return h(NTag, { size: 'small', round: true, bordered: false, type: getRunStatusTagType(row.runStatus) }, () =>
        getOptionLabel(AI_TASK_RUN_STATUS_OPTIONS, row.runStatus))
    },
  },
  {
    title: t('develop.ai_task.run_col_started'),
    key: 'startedTime',
    minWidth: 170,
    render(row) {
      return formatDateTime(row.startedTime)
    },
  },
  {
    title: t('develop.ai_task.run_col_duration'),
    key: 'durationMilliseconds',
    width: 100,
    render(row) {
      return formatDuration(row.durationMilliseconds)
    },
  },
  {
    title: t('common.fields.actions'),
    key: 'actions',
    width: 90,
    render(row) {
      return h(NButton, { size: 'small', onClick: () => selectRunHistory(row) }, () => t('common.actions.detail'))
    },
  },
])

function createDefaultForm(): TaskFormModel {
  return {
    aiTaskCode: '',
    aiTaskName: '',
    description: null,
    category: null,
    promptMode: AiTaskPromptMode.Inline,
    promptText: '',
    promptCode: null,
    promptVersion: null,
    providerId: null,
    triggerType: AiTaskTriggerType.Cron,
    cronExpression: '0 0 9 * * ?',
    startTime: null,
    endTime: null,
    intervalSeconds: null,
    repeatCount: -1,
    timeoutSeconds: 300,
    priority: 0,
    allowConcurrent: false,
    maxRetryCount: 3,
    sort: 100,
    status: EnableStatus.Enabled,
    toolPolicies: [],
    remark: null,
  }
}

async function loadLookups() {
  lookupLoading.value = true
  try {
    const [providerPage, promptPage, toolItems] = await Promise.all([
      aiProviderApi.page({
        ...createPageRequest({ page: { pageIndex: 1, pageSize: 200 } }),
        isEnabled: true,
        status: EnableStatus.Enabled,
      }),
      aiPromptApi.page({
        ...createPageRequest({ page: { pageIndex: 1, pageSize: 200 } }),
        isEnabled: true,
        status: EnableStatus.Enabled,
      }),
      aiToolApi.select(),
    ])
    providers.value = providerPage.items
    prompts.value = promptPage.items
    tools.value = toolItems
  }
  catch {
    message.error(t('develop.ai_task.load_options_failed'))
  }
  finally {
    lookupLoading.value = false
  }
}

function ensureLookupsLoaded() {
  if (providers.value.length === 0 || prompts.value.length === 0 || tools.value.length === 0) {
    void loadLookups()
  }
}

function handleAdd() {
  editingStatus.value = null
  skillPickerSelection.value = []
  form.value = createDefaultForm()
  ensureLookupsLoaded()
  modalVisible.value = true
}

async function handleEdit(row: AiTaskListItemDto) {
  try {
    const detail = await aiTaskApi.detail(row.basicId)
    if (!detail) {
      message.error(t('develop.ai_task.not_found'))
      return
    }
    editingStatus.value = detail.status
    skillPickerSelection.value = []
    form.value = detailToForm(detail)
    ensureLookupsLoaded()
    modalVisible.value = true
  }
  catch {
    message.error(t('develop.ai_task.load_detail_failed'))
  }
}

function detailToForm(detail: AiTaskDetailDto): TaskFormModel {
  return {
    basicId: detail.basicId,
    aiTaskCode: detail.aiTaskCode,
    aiTaskName: detail.aiTaskName,
    description: detail.description ?? null,
    category: detail.category ?? null,
    promptMode: detail.promptMode,
    promptText: detail.promptText ?? '',
    promptCode: detail.promptCode ?? null,
    promptVersion: detail.promptVersion ?? null,
    providerId: detail.providerId ?? null,
    triggerType: detail.triggerType,
    cronExpression: detail.cronExpression ?? null,
    startTime: detail.startTime ? new Date(detail.startTime).getTime() : null,
    endTime: detail.endTime ? new Date(detail.endTime).getTime() : null,
    intervalSeconds: detail.intervalSeconds ?? null,
    repeatCount: detail.repeatCount,
    timeoutSeconds: detail.timeoutSeconds,
    priority: detail.priority,
    allowConcurrent: detail.allowConcurrent,
    maxRetryCount: detail.maxRetryCount,
    sort: detail.sort,
    status: detail.status,
    toolPolicies: detail.toolPolicies.map(policy => ({
      toolId: policy.toolId,
      remark: policy.remark ?? null,
    })),
    remark: detail.remark ?? null,
  }
}

function validateForm() {
  if (!form.value.aiTaskName.trim()) {
    message.warning(t('develop.ai_task.validate_name'))
    return false
  }
  if (!form.value.basicId && !form.value.aiTaskCode.trim()) {
    message.warning(t('develop.ai_task.validate_code'))
    return false
  }
  if (form.value.promptMode === AiTaskPromptMode.Inline && !form.value.promptText?.trim()) {
    message.warning(t('develop.ai_task.validate_prompt'))
    return false
  }
  if (form.value.promptMode === AiTaskPromptMode.PromptStore && !form.value.promptCode?.trim()) {
    message.warning(t('develop.ai_task.validate_prompt_code'))
    return false
  }
  if (form.value.triggerType === AiTaskTriggerType.Cron && !form.value.cronExpression?.trim()) {
    message.warning(t('develop.ai_task.validate_cron'))
    return false
  }
  if (form.value.triggerType === AiTaskTriggerType.Schedule && !form.value.startTime) {
    message.warning(t('develop.ai_task.validate_start_time'))
    return false
  }
  if (form.value.triggerType === AiTaskTriggerType.Recurring && (!form.value.intervalSeconds || form.value.intervalSeconds <= 0)) {
    message.warning(t('develop.ai_task.validate_interval'))
    return false
  }
  if (form.value.startTime && form.value.endTime && form.value.endTime <= form.value.startTime) {
    message.warning(t('develop.ai_task.validate_time_range'))
    return false
  }
  return true
}

function handlePromptCodeUpdate(value: string | null) {
  form.value.promptCode = value
  const prompt = prompts.value.find(item => item.promptCode === value)
  form.value.promptVersion = prompt?.version ?? null
}

function removeToolPolicy(index: number) {
  form.value.toolPolicies.splice(index, 1)
}

function openSkillPicker() {
  ensureLookupsLoaded()
  skillPickerSelection.value = form.value.toolPolicies.map(policy => String(policy.toolId))
  skillPickerKeyword.value = ''
  skillPickerVisible.value = true
}

function confirmSkillPicker() {
  const existingById = new Map(form.value.toolPolicies.map(policy => [String(policy.toolId), policy]))
  form.value.toolPolicies = skillPickerSelection.value.map((toolId) => {
    const existing = existingById.get(String(toolId))
    return {
      toolId,
      remark: existing?.remark ?? null,
    }
  })
  skillPickerVisible.value = false
}

function onAction(payload: SchemaActionPayload) {
  const row = payload.row as unknown as AiTaskListItemDto | undefined
  switch (payload.key) {
    case 'create':
      handleAdd()
      break
    case 'run':
      if (row) {
        void handleRun(row)
      }
      break
    case 'history':
      if (row) {
        void openRunHistory(row)
      }
      break
    case 'enable':
    case 'disable':
      if (row) {
        void handleToggleStatus(row)
      }
      break
    case 'edit':
      if (row) {
        void handleEdit(row)
      }
      break
    case 'delete':
      if (row) {
        handleDelete(row)
      }
      break
  }
}

async function submitForm() {
  if (!validateForm()) {
    return
  }

  submitLoading.value = true
  try {
    if (form.value.basicId) {
      const payload: AiTaskUpdateDto = {
        ...formToPayload(),
        basicId: form.value.basicId,
      }
      const updated = await aiTaskApi.update(payload)
      if (editingStatus.value !== null && form.value.status !== editingStatus.value) {
        await aiTaskApi.updateStatus({ basicId: updated.basicId, status: form.value.status, remark: t('develop.ai_task.update_status_remark') })
      }
      message.success(t('common.messages.save_success'))
    }
    else {
      const payload: AiTaskCreateDto = {
        ...formToPayload(),
        aiTaskCode: form.value.aiTaskCode.trim(),
        status: form.value.status,
      }
      await aiTaskApi.create(payload)
      message.success(t('common.messages.create_success'))
    }
    modalVisible.value = false
    reload()
  }
  catch {
    message.error(t('common.messages.save_failed'))
  }
  finally {
    submitLoading.value = false
  }
}

function formToPayload() {
  return {
    aiTaskName: form.value.aiTaskName.trim(),
    description: form.value.description?.trim() || null,
    category: form.value.category?.trim() || null,
    promptMode: form.value.promptMode,
    promptText: form.value.promptText || null,
    promptCode: form.value.promptCode?.trim() || null,
    promptVersion: form.value.promptVersion?.trim() || null,
    providerId: form.value.providerId || null,
    triggerType: form.value.triggerType,
    cronExpression: form.value.triggerType === AiTaskTriggerType.Cron ? form.value.cronExpression?.trim() || null : null,
    startTime: form.value.startTime ? new Date(form.value.startTime).toISOString() : null,
    endTime: form.value.endTime ? new Date(form.value.endTime).toISOString() : null,
    intervalSeconds: form.value.triggerType === AiTaskTriggerType.Recurring ? form.value.intervalSeconds ?? null : null,
    repeatCount: form.value.triggerType === AiTaskTriggerType.Schedule ? 1 : form.value.repeatCount,
    timeoutSeconds: form.value.timeoutSeconds,
    priority: form.value.priority,
    allowConcurrent: form.value.allowConcurrent,
    maxRetryCount: form.value.maxRetryCount,
    sort: form.value.sort,
    toolPolicies: form.value.toolPolicies.map(policy => ({
      toolId: policy.toolId,
      remark: policy.remark?.trim() || null,
    })),
    remark: form.value.remark?.trim() || null,
  }
}

async function handleRun(row: AiTaskListItemDto) {
  const reset = message.loading(t('develop.ai_task.running'), { duration: 0 })
  try {
    const result = await aiTaskApi.run(row.basicId)
    reset.destroy()
    if (result.succeeded) {
      message.success(t('develop.ai_task.run_success'))
    }
    else {
      message.error(result.errorMessage || t('develop.ai_task.run_failed'))
    }
    if (runHistoryVisible.value && runHistoryTask.value?.basicId === row.basicId) {
      await loadRunHistory(row.basicId, result.runId ?? undefined)
    }
  }
  catch {
    reset.destroy()
    message.error(t('develop.ai_task.run_failed'))
  }
}

async function openRunHistory(row: AiTaskListItemDto) {
  runHistoryTask.value = row
  runHistoryVisible.value = true
  await loadRunHistory(row.basicId)
}

async function loadRunHistory(taskId: AiTaskListItemDto['basicId'], selectedRunId?: AiTaskRunListItemDto['basicId']) {
  runHistoryLoading.value = true
  try {
    const rows = await aiTaskApi.runList(taskId)
    runHistoryRows.value = rows
    const selected = rows.find(row => String(row.basicId) === String(selectedRunId))
      ?? rows.find(row => row.basicId === runHistoryDetail.value?.basicId)
      ?? rows[0]
    runHistoryDetail.value = selected ? normalizeRunDetail(await aiTaskApi.runDetail(selected.basicId)) : null
    runGuidanceInput.value = ''
  }
  catch {
    message.error(t('develop.ai_task.load_run_history_failed'))
  }
  finally {
    runHistoryLoading.value = false
  }
}

async function selectRunHistory(row: AiTaskRunListItemDto) {
  runHistoryLoading.value = true
  try {
    runHistoryDetail.value = normalizeRunDetail(await aiTaskApi.runDetail(row.basicId))
    runGuidanceInput.value = ''
  }
  catch {
    message.error(t('develop.ai_task.load_run_history_failed'))
  }
  finally {
    runHistoryLoading.value = false
  }
}

function normalizeRunDetail(detail: AiTaskRunDetailDto | null): AiTaskRunDetailDto | null {
  if (!detail) {
    return null
  }

  return {
    ...detail,
    events: [...(detail.events ?? [])].sort(compareRunEvents),
    guidance: [...(detail.guidance ?? [])].sort((a, b) => String(a.createdTime).localeCompare(String(b.createdTime))),
  }
}

function compareRunEvents(left: AiTaskRunEventDto, right: AiTaskRunEventDto) {
  return left.sequence - right.sequence || String(left.createdTime).localeCompare(String(right.createdTime))
}

function mergeRunEvents(events: AiTaskRunEventDto[]) {
  const detail = runHistoryDetail.value
  if (!detail || events.length === 0) {
    return
  }

  const byKey = new Map<string, AiTaskRunEventDto>()
  for (const event of detail.events ?? []) {
    byKey.set(getRunEventKey(event), event)
  }
  for (const event of events) {
    if (String(event.runId) === String(detail.basicId)) {
      byKey.set(getRunEventKey(event), event)
    }
  }
  detail.events = [...byKey.values()].sort(compareRunEvents)
}

function mergeRunGuidance(guidance: NonNullable<AiTaskRunDetailDto['guidance']>[number]) {
  const detail = runHistoryDetail.value
  if (!detail || String(guidance.runId) !== String(detail.basicId)) {
    return
  }

  const byId = new Map(detail.guidance.map(item => [String(item.basicId), item]))
  byId.set(String(guidance.basicId), guidance)
  detail.guidance = [...byId.values()].sort((a, b) => String(a.createdTime).localeCompare(String(b.createdTime)))
}

function getRunEventKey(event: AiTaskRunEventDto) {
  return `${event.runId}:${event.sequence}:${event.basicId}`
}

function getLastRunEventSequence() {
  const events = runHistoryDetail.value?.events ?? []
  return events.reduce((max, event) => Math.max(max, event.sequence), 0)
}

async function loadRunEventsOnce() {
  const detail = runHistoryDetail.value
  if (!detail) {
    return
  }

  try {
    const events = await aiTaskApi.runEvents(detail.basicId, getLastRunEventSequence())
    mergeRunEvents(events)
    if (events.some(event => isTerminalRunEvent(event.eventType))) {
      await refreshSelectedRunDetail(false)
    }
  }
  catch {
    // 实时预览失败不阻断页面；用户手动刷新详情仍可恢复。
  }
}

async function refreshSelectedRunDetail(showError = true) {
  const detail = runHistoryDetail.value
  if (!detail) {
    return
  }

  try {
    const updated = await aiTaskApi.runDetail(detail.basicId)
    runHistoryDetail.value = normalizeRunDetail(updated)
    if (runHistoryTask.value) {
      runHistoryRows.value = await aiTaskApi.runList(runHistoryTask.value.basicId)
    }
  }
  catch {
    if (showError) {
      message.error(t('develop.ai_task.load_run_history_failed'))
    }
  }
}

async function appendRunGuidance() {
  const detail = runHistoryDetail.value
  const content = runGuidanceInput.value.trim()
  if (!detail || !content) {
    return
  }

  runGuidanceSubmitting.value = true
  try {
    const guidance = await aiTaskApi.appendRunGuidance({
      runId: detail.basicId,
      content,
      clientRequestId: createClientRequestId(),
    })
    mergeRunGuidance(guidance)
    runGuidanceInput.value = ''
    await loadRunEventsOnce()
    message.success(t('develop.ai_task.run_guidance_submit_success'))
  }
  catch {
    message.error(t('develop.ai_task.run_guidance_submit_failed'))
  }
  finally {
    runGuidanceSubmitting.value = false
  }
}

function createClientRequestId() {
  return globalThis.crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random().toString(16).slice(2)}`
}

function isRunActive(status: AiTaskRunStatus) {
  return status === AiTaskRunStatus.Queued || status === AiTaskRunStatus.Running
}

function isTerminalRunEvent(eventType: AiTaskRunEventType) {
  return eventType === AiTaskRunEventType.RunSucceeded
    || eventType === AiTaskRunEventType.RunFailed
    || eventType === AiTaskRunEventType.RunCanceled
}

function handleAiTaskRunEventReceived(payload: unknown) {
  const event = payload as AiTaskRunEventDto | null
  if (!event?.runId || !runHistoryDetail.value || String(event.runId) !== String(runHistoryDetail.value.basicId)) {
    return
  }

  mergeRunEvents([event])
  if (isTerminalRunEvent(event.eventType)) {
    void refreshSelectedRunDetail(false)
  }
}

function syncRunEventPolling() {
  stopRunEventPolling()
  const detail = runHistoryDetail.value
  if (!runHistoryVisible.value || !detail || !isRunActive(detail.runStatus)) {
    return
  }

  runEventPollingTimer = setInterval(() => {
    void loadRunEventsOnce()
  }, RUN_EVENT_POLLING_INTERVAL_MS)
}

function stopRunEventPolling() {
  if (runEventPollingTimer) {
    clearInterval(runEventPollingTimer)
    runEventPollingTimer = null
  }
}

function getRunStatusTagType(status: AiTaskRunStatus) {
  switch (status) {
    case AiTaskRunStatus.Success:
      return 'success'
    case AiTaskRunStatus.Failed:
      return 'error'
    case AiTaskRunStatus.Canceled:
      return 'warning'
    default:
      return 'info'
  }
}

function getRunEventLabel(eventType: AiTaskRunEventType) {
  return t(`develop.ai_task.run_event_${eventType}`)
}

function getRunEventRoleLabel(role: AiTaskRunEventRole) {
  return t(`develop.ai_task.run_event_role_${role}`)
}

function getRunGuidanceStatusLabel(status: AiTaskRunGuidanceStatus) {
  return t(`develop.ai_task.run_guidance_status_${status}`)
}

function getRunEventTimelineType(eventType: AiTaskRunEventType) {
  if (eventType === AiTaskRunEventType.RunSucceeded || eventType === AiTaskRunEventType.GuidanceApplied) {
    return 'success'
  }
  if (eventType === AiTaskRunEventType.RunFailed || eventType === AiTaskRunEventType.GuidanceIgnored) {
    return 'error'
  }
  if (eventType === AiTaskRunEventType.GuidanceReceived || eventType === AiTaskRunEventType.RunRequeued) {
    return 'warning'
  }
  return 'info'
}

function getRunGuidanceTagType(status: AiTaskRunGuidanceStatus) {
  if (status === AiTaskRunGuidanceStatus.Applied) {
    return 'success'
  }
  if (status === AiTaskRunGuidanceStatus.Ignored) {
    return 'warning'
  }
  return 'info'
}

function formatDateTime(value?: string | null) {
  if (!value) {
    return '-'
  }

  return new Date(value).toLocaleString()
}

function formatDuration(value?: number | null) {
  if (value === undefined || value === null) {
    return '-'
  }

  if (value < 1000) {
    return `${value}ms`
  }

  return `${(value / 1000).toFixed(1)}s`
}

async function handleToggleStatus(row: AiTaskListItemDto) {
  try {
    await aiTaskApi.updateStatus({
      basicId: row.basicId,
      status: row.status === EnableStatus.Enabled ? EnableStatus.Disabled : EnableStatus.Enabled,
      remark: t('develop.ai_task.update_status_remark'),
    })
    message.success(t('common.messages.save_success'))
    reload()
  }
  catch {
    message.error(t('common.messages.save_failed'))
  }
}

function handleDelete(row: AiTaskListItemDto) {
  dialog.warning({
    title: t('common.actions.delete'),
    content: t('develop.ai_task.confirm_delete'),
    positiveText: t('common.actions.confirm'),
    negativeText: t('common.actions.cancel'),
    onPositiveClick: async () => {
      try {
        await aiTaskApi.delete(row.basicId)
        message.success(t('common.messages.delete_success'))
        reload()
      }
      catch {
        message.error(t('common.messages.delete_failed'))
      }
    },
  })
}

watch(
  [runHistoryVisible, () => runHistoryDetail.value?.basicId, () => runHistoryDetail.value?.runStatus],
  () => syncRunEventPolling(),
)

onMounted(() => {
  void loadLookups()
  signalR.on(AI_TASK_RUN_EVENT_RECEIVED, handleAiTaskRunEventReceived)
  void signalR.start()
})

onUnmounted(() => {
  signalR.off(AI_TASK_RUN_EVENT_RECEIVED, handleAiTaskRunEventReceived)
  stopRunEventPolling()
})
</script>

<template>
  <SchemaPage ref="schemaPageRef" :schema="schema" @action="onAction">
    <NModal
      v-model:show="modalVisible"
      :auto-focus="false"
      :bordered="false"
      :content-style="{ maxHeight: 'calc(100vh - 180px)', overflowY: 'auto', padding: '16px 20px' }"
      preset="card"
      :title="modalTitle"
      style="width: 860px; max-width: 92vw"
    >
      <NForm label-placement="top" :model="form" class="ai-task-form">
        <section class="ai-task-form-section">
          <div class="ai-task-section-title">
            {{ t('develop.ai_task.section_basic') }}
          </div>
          <div class="ai-task-form-grid">
            <NFormItem :label="t('develop.ai_task.form_code')">
              <NInput v-model:value="form.aiTaskCode" :disabled="!!form.basicId" :placeholder="t('develop.ai_task.form_code_placeholder')" />
            </NFormItem>
            <NFormItem :label="t('develop.ai_task.form_name')">
              <NInput v-model:value="form.aiTaskName" />
            </NFormItem>
            <NFormItem :label="t('develop.ai_task.form_category')">
              <NInput v-model:value="form.category" />
            </NFormItem>
            <NFormItem :label="t('common.fields.status')">
              <NSelect v-model:value="form.status" :options="STATUS_OPTIONS" />
            </NFormItem>
            <NFormItem :label="t('develop.ai_task.form_provider')">
              <NSelect
                v-model:value="form.providerId"
                :options="providerSelectOptions"
                :loading="lookupLoading"
                filterable
                clearable
                :placeholder="t('develop.ai_task.form_provider_placeholder')"
              />
            </NFormItem>
          </div>
        </section>

        <section class="ai-task-form-section">
          <div class="ai-task-section-title">
            {{ t('develop.ai_task.section_schedule') }}
          </div>
          <div class="ai-task-form-grid">
            <NFormItem :label="t('develop.ai_task.form_trigger_type')">
              <NSelect v-model:value="form.triggerType" :options="AI_TASK_SCHEDULE_TRIGGER_TYPE_OPTIONS" />
            </NFormItem>
            <NFormItem v-if="form.triggerType === AiTaskTriggerType.Schedule" :label="t('develop.ai_task.form_start_time')">
              <NDatePicker v-model:value="form.startTime" clearable type="datetime" class="full-input" />
            </NFormItem>
            <NFormItem v-if="form.triggerType === AiTaskTriggerType.Cron" :label="t('develop.ai_task.form_cron')">
              <NInput v-model:value="form.cronExpression" />
            </NFormItem>
            <NFormItem v-if="form.triggerType === AiTaskTriggerType.Recurring" :label="t('develop.ai_task.form_interval')">
              <NInputNumber v-model:value="form.intervalSeconds" :min="1" class="full-input" />
            </NFormItem>
            <NFormItem v-if="form.triggerType !== AiTaskTriggerType.Schedule" :label="t('develop.ai_task.form_start_time')">
              <NDatePicker v-model:value="form.startTime" clearable type="datetime" class="full-input" />
            </NFormItem>
            <NFormItem>
              <template #label>
                <span class="ai-task-form-label-help">
                  <span>{{ t('develop.ai_task.form_end_time') }}</span>
                  <NTooltip>
                    <template #trigger>
                      <NIcon :size="14" class="ai-task-form-help-icon">
                        <Icon icon="lucide:circle-help" />
                      </NIcon>
                    </template>
                    {{ t('develop.ai_task.form_end_time_help') }}
                  </NTooltip>
                </span>
              </template>
              <NDatePicker v-model:value="form.endTime" clearable type="datetime" class="full-input" />
            </NFormItem>
            <NFormItem v-if="form.triggerType !== AiTaskTriggerType.Schedule" :label="t('develop.ai_task.form_repeat_count')">
              <NInputNumber v-model:value="form.repeatCount" :min="-1" class="full-input" />
            </NFormItem>
          </div>
        </section>

        <section class="ai-task-form-section">
          <div class="ai-task-section-title">
            {{ t('develop.ai_task.section_advanced') }}
          </div>
          <div class="ai-task-form-grid ai-task-form-grid--compact">
            <NFormItem>
              <template #label>
                <span class="ai-task-form-label-help">
                  <span>{{ t('develop.ai_task.form_timeout') }}</span>
                  <NTooltip>
                    <template #trigger>
                      <NIcon :size="14" class="ai-task-form-help-icon">
                        <Icon icon="lucide:circle-help" />
                      </NIcon>
                    </template>
                    {{ t('develop.ai_task.form_timeout_help') }}
                  </NTooltip>
                </span>
              </template>
              <NInputNumber v-model:value="form.timeoutSeconds" :min="1" class="full-input" />
            </NFormItem>
            <NFormItem :label="t('develop.ai_task.form_retry')">
              <NInputNumber v-model:value="form.maxRetryCount" :min="0" class="full-input" />
            </NFormItem>
            <NFormItem :label="t('develop.ai_task.form_priority')">
              <NInputNumber v-model:value="form.priority" class="full-input" />
            </NFormItem>
            <NFormItem :label="t('develop.ai_task.form_sort')">
              <NInputNumber v-model:value="form.sort" class="full-input" />
            </NFormItem>
            <NFormItem :label="t('develop.ai_task.form_concurrent')">
              <NSwitch v-model:value="form.allowConcurrent" />
            </NFormItem>
          </div>
        </section>

        <section class="ai-task-form-section">
          <div class="ai-task-section-title">
            {{ t('develop.ai_task.section_prompt') }}
          </div>
          <div class="ai-task-form-grid">
            <NFormItem :label="t('develop.ai_task.form_prompt_mode')">
              <NSelect v-model:value="form.promptMode" :options="AI_TASK_PROMPT_MODE_OPTIONS" />
            </NFormItem>
          </div>
          <NFormItem v-if="form.promptMode === AiTaskPromptMode.Inline" :label="t('develop.ai_task.form_prompt')">
            <NInput v-model:value="form.promptText" type="textarea" :autosize="{ minRows: 5, maxRows: 10 }" :placeholder="t('develop.ai_task.form_prompt_placeholder')" />
          </NFormItem>
          <div v-else class="ai-task-form-grid">
            <NFormItem :label="t('develop.ai_task.form_prompt_code')">
              <NSelect
                :value="form.promptCode"
                :options="promptSelectOptions"
                :loading="lookupLoading"
                filterable
                clearable
                :placeholder="t('develop.ai_task.form_prompt_code_placeholder')"
                @update:value="handlePromptCodeUpdate"
              />
            </NFormItem>
            <NFormItem :label="t('develop.ai_task.form_prompt_version')">
              <NInput v-model:value="form.promptVersion" readonly />
            </NFormItem>
          </div>
        </section>

        <section class="ai-task-form-section">
          <div class="ai-task-section-title">
            {{ t('develop.ai_task.section_skills') }}
          </div>
          <NFormItem :label="t('develop.ai_task.form_capabilities')">
            <NSpace vertical class="full-input">
              <NButton @click="openSkillPicker">
                {{ t('develop.ai_task.action_select_skills') }}
              </NButton>
              <NDataTable
                :columns="toolPolicyColumns"
                :data="form.toolPolicies"
                :pagination="false"
                size="small"
              />
            </NSpace>
          </NFormItem>
          <NFormItem :label="t('develop.ai_task.form_remark')">
            <NInput v-model:value="form.remark" type="textarea" :autosize="{ minRows: 2, maxRows: 4 }" />
          </NFormItem>
        </section>
      </NForm>

      <template #footer>
        <NSpace justify="end">
          <NButton @click="modalVisible = false">
            {{ t('common.actions.cancel') }}
          </NButton>
          <NButton type="primary" :loading="submitLoading" @click="submitForm">
            {{ t('common.actions.save') }}
          </NButton>
        </NSpace>
      </template>
    </NModal>

    <NModal
      v-model:show="skillPickerVisible"
      :auto-focus="false"
      :bordered="false"
      preset="card"
      :title="t('develop.ai_task.skill_picker_title')"
      style="width: 620px; max-width: 92vw"
    >
      <NSpace vertical>
        <NInput
          v-model:value="skillPickerKeyword"
          clearable
          :placeholder="t('develop.ai_task.skill_picker_search_placeholder')"
        />
        <NDataTable
          :checked-row-keys="skillPickerSelection"
          :columns="skillPickerColumns"
          :data="filteredSkillRows"
          :loading="lookupLoading"
          :pagination="{ pageSize: 8 }"
          :row-key="(row: AiToolSelectItemDto) => row.basicId"
          size="small"
          @update:checked-row-keys="keys => skillPickerSelection = keys.map(String)"
        />
      </NSpace>

      <template #footer>
        <NSpace justify="end">
          <NButton @click="skillPickerVisible = false">
            {{ t('common.actions.cancel') }}
          </NButton>
          <NButton type="primary" @click="confirmSkillPicker">
            {{ t('common.actions.confirm') }}
          </NButton>
        </NSpace>
      </template>
    </NModal>

    <NDrawer
      v-model:show="runHistoryVisible"
      :width="920"
      placement="right"
    >
      <NDrawerContent :title="runHistoryTitle" closable>
        <NSpace vertical :size="16">
          <NDataTable
            :columns="runHistoryColumns"
            :data="runHistoryRows"
            :loading="runHistoryLoading"
            :pagination="{ pageSize: 8 }"
            :row-key="(row: AiTaskRunListItemDto) => row.basicId"
            size="small"
          />

          <div v-if="runHistoryDetail" class="ai-task-run-detail">
            <div class="ai-task-run-meta">
              <div class="ai-task-run-meta__item">
                <span>{{ t('common.fields.status') }}</span>
                <NTag size="small" round :bordered="false" :type="getRunStatusTagType(runHistoryDetail.runStatus)">
                  {{ getOptionLabel(AI_TASK_RUN_STATUS_OPTIONS, runHistoryDetail.runStatus) }}
                </NTag>
              </div>
              <div class="ai-task-run-meta__item">
                <span>{{ t('develop.ai_task.run_col_started') }}</span>
                <strong>{{ formatDateTime(runHistoryDetail.startedTime) }}</strong>
              </div>
              <div class="ai-task-run-meta__item">
                <span>{{ t('develop.ai_task.run_col_ended') }}</span>
                <strong>{{ formatDateTime(runHistoryDetail.endedTime) }}</strong>
              </div>
              <div class="ai-task-run-meta__item">
                <span>{{ t('develop.ai_task.run_col_duration') }}</span>
                <strong>{{ formatDuration(runHistoryDetail.durationMilliseconds) }}</strong>
              </div>
              <div class="ai-task-run-meta__item">
                <span>{{ t('develop.ai_task.run_detail_runner') }}</span>
                <strong>{{ runHistoryDetail.runnerKind || '-' }}</strong>
              </div>
              <div class="ai-task-run-meta__item">
                <span>{{ t('develop.ai_task.run_detail_agent_session') }}</span>
                <strong>{{ runHistoryDetail.agentSessionId || '-' }}</strong>
              </div>
            </div>

            <section class="ai-task-run-block">
              <div class="ai-task-section-title ai-task-section-title--row">
                <span>{{ t('develop.ai_task.run_detail_events') }}</span>
                <NButton size="tiny" quaternary :loading="runHistoryLoading" @click="refreshSelectedRunDetail()">
                  {{ t('common.actions.refresh') }}
                </NButton>
              </div>
              <NTimeline v-if="runHistoryDetail.events?.length" class="ai-task-run-events">
                <NTimelineItem
                  v-for="event in runHistoryDetail.events"
                  :key="`${event.runId}-${event.sequence}-${event.basicId}`"
                  :time="formatDateTime(event.createdTime)"
                  :type="getRunEventTimelineType(event.eventType)"
                >
                  <template #header>
                    <div class="ai-task-run-event-header">
                      <span>{{ getRunEventLabel(event.eventType) }}</span>
                      <NTag size="small" round :bordered="false">
                        {{ getRunEventRoleLabel(event.role) }}
                      </NTag>
                    </div>
                  </template>
                  <pre v-if="event.content" class="ai-task-run-event-content">{{ event.content }}</pre>
                </NTimelineItem>
              </NTimeline>
              <div v-else class="ai-task-run-empty">
                {{ t('develop.ai_task.run_events_empty') }}
              </div>
            </section>

            <section v-if="canAppendRunGuidance" class="ai-task-run-block">
              <div class="ai-task-section-title">
                {{ t('develop.ai_task.run_guidance_title') }}
              </div>
              <NSpace vertical class="ai-task-run-guidance-form">
                <NInput
                  v-model:value="runGuidanceInput"
                  type="textarea"
                  :autosize="{ minRows: 2, maxRows: 5 }"
                  :maxlength="4000"
                  show-count
                  :placeholder="t('develop.ai_task.run_guidance_placeholder')"
                />
                <NSpace justify="end">
                  <NButton
                    type="primary"
                    :disabled="!runGuidanceInput.trim()"
                    :loading="runGuidanceSubmitting"
                    @click="appendRunGuidance"
                  >
                    {{ t('develop.ai_task.run_guidance_submit') }}
                  </NButton>
                </NSpace>
              </NSpace>
            </section>

            <section v-if="runHistoryDetail.guidance?.length" class="ai-task-run-block">
              <div class="ai-task-section-title">
                {{ t('develop.ai_task.run_guidance_history') }}
              </div>
              <div class="ai-task-run-guidance-list">
                <div v-for="item in runHistoryDetail.guidance" :key="item.basicId" class="ai-task-run-guidance-item">
                  <div class="ai-task-run-guidance-item__meta">
                    <span>{{ formatDateTime(item.createdTime) }}</span>
                    <NTag size="small" round :bordered="false" :type="getRunGuidanceTagType(item.status)">
                      {{ getRunGuidanceStatusLabel(item.status) }}
                    </NTag>
                  </div>
                  <pre>{{ item.content }}</pre>
                  <div v-if="item.ignoredReason" class="ai-task-run-guidance-item__reason">
                    {{ item.ignoredReason }}
                  </div>
                </div>
              </div>
            </section>

            <section class="ai-task-run-block">
              <div class="ai-task-section-title">
                {{ t('develop.ai_task.run_detail_prompt') }}
              </div>
              <pre>{{ runHistoryDetail.promptSnapshot || '-' }}</pre>
            </section>

            <section class="ai-task-run-block">
              <div class="ai-task-section-title">
                {{ t('develop.ai_task.run_detail_result') }}
              </div>
              <pre>{{ runHistoryDetail.resultText || '-' }}</pre>
            </section>

            <section v-if="runHistoryDetail.errorMessage" class="ai-task-run-block">
              <div class="ai-task-section-title">
                {{ t('develop.ai_task.run_detail_error') }}
              </div>
              <pre>{{ runHistoryDetail.errorMessage }}</pre>
            </section>
          </div>

          <div v-else class="ai-task-run-empty">
            {{ t('develop.ai_task.run_history_empty') }}
          </div>
        </NSpace>
      </NDrawerContent>
    </NDrawer>
  </SchemaPage>
</template>

<style scoped>
.ai-task-form {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.ai-task-form-section {
  min-width: 0;
}

.ai-task-form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  column-gap: 16px;
}

.ai-task-form-grid--compact {
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.ai-task-section-title {
  margin: 0 0 12px;
  padding-bottom: 6px;
  border-bottom: 1px solid var(--n-border-color);
  color: var(--n-text-color-2);
  font-size: 13px;
  font-weight: 600;
}

.ai-task-section-title--row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.full-input {
  width: 100%;
}

.ai-task-form-label-help {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  min-width: 0;
}

.ai-task-form-help-icon {
  color: var(--n-text-color-3);
  cursor: help;
}

.ai-task-form-help-icon:hover {
  color: var(--n-text-color-2);
}

.policy-tool {
  display: flex;
  align-items: center;
  gap: 6px;
  min-width: 0;
}

.policy-tool__name {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.ai-task-run-detail {
  display: flex;
  flex-direction: column;
  gap: 14px;
  min-width: 0;
}

.ai-task-run-meta {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px 14px;
}

.ai-task-run-meta__item {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.ai-task-run-meta__item span {
  color: var(--n-text-color-3);
  font-size: 12px;
}

.ai-task-run-meta__item strong {
  color: var(--n-text-color);
  font-size: 13px;
  font-weight: 500;
}

.ai-task-run-block {
  min-width: 0;
}

.ai-task-run-events {
  padding: 2px 0 0 2px;
}

.ai-task-run-event-header {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  min-width: 0;
  font-size: 13px;
  font-weight: 500;
}

.ai-task-run-event-content {
  max-height: 160px;
  margin: 6px 0 0;
  padding: 8px 10px;
  overflow: auto;
  border: 1px solid var(--n-border-color);
  border-radius: 6px;
  background: var(--n-color);
  color: var(--n-text-color);
  font-family: var(--n-font-family-mono);
  font-size: 12px;
  line-height: 1.55;
  white-space: pre-wrap;
  word-break: break-word;
}

.ai-task-run-block pre {
  max-height: 220px;
  margin: 0;
  padding: 10px 12px;
  overflow: auto;
  border: 1px solid var(--n-border-color);
  border-radius: 6px;
  background: var(--n-color);
  color: var(--n-text-color);
  font-family: var(--n-font-family-mono);
  font-size: 12px;
  line-height: 1.6;
  white-space: pre-wrap;
  word-break: break-word;
}

.ai-task-run-guidance-form {
  width: 100%;
}

.ai-task-run-guidance-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.ai-task-run-guidance-item {
  min-width: 0;
  padding: 10px 12px;
  border: 1px solid var(--n-border-color);
  border-radius: 6px;
}

.ai-task-run-guidance-item__meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 8px;
  color: var(--n-text-color-3);
  font-size: 12px;
}

.ai-task-run-guidance-item pre {
  max-height: 160px;
  margin: 0;
  padding: 0;
  overflow: auto;
  border: 0;
  background: transparent;
}

.ai-task-run-guidance-item__reason {
  margin-top: 8px;
  color: var(--n-text-color-3);
  font-size: 12px;
}

.ai-task-run-empty {
  padding: 18px 0;
  color: var(--n-text-color-3);
  text-align: center;
}

@media (max-width: 760px) {
  .ai-task-run-meta,
  .ai-task-form-grid--compact,
  .ai-task-form-grid {
    grid-template-columns: 1fr;
  }
}
</style>
