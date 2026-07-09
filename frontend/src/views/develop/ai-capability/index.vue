<script setup lang="ts">
import type {
  AiToolCreateDto,
  AiToolDetailDto,
  AiToolListItemDto,
  AiToolUpdateDto,
} from '@/api'
import type { ListFieldSchema, PageSchema, SchemaActionPayload } from '~/components'
import type { PageResult } from '~/types/contracts'
import {
  NButton,
  NForm,
  NFormItem,
  NInput,
  NInputNumber,
  NModal,
  NSelect,
  NSpace,
  NSwitch,
  NTag,
  useMessage,
} from 'naive-ui'
import { computed, h, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  AI_TOOL_RISK_LEVEL_OPTIONS,
  AI_TOOL_SAFETY_LEVEL_OPTIONS,
  AI_TOOL_TYPE_OPTIONS,
  aiToolApi,
  AiToolType,
  AiToolRiskLevel,
  AiToolSafetyLevel,
  EnableStatus,
  createPageRequest,
  querySortsFromSchema,
} from '@/api'
import { SchemaPage } from '~/components'
import { STATUS_OPTIONS } from '~/constants'
import { useEnumOptions } from '~/hooks'
import { getOptionLabel } from '~/utils'

defineOptions({ name: 'DevelopAiCapabilityPage' })

interface CapabilityFormModel {
  basicId?: string
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

const { t } = useI18n()
const message = useMessage()
const statusEnumOptions = useEnumOptions('EnableStatus', STATUS_OPTIONS)

const schemaPageRef = ref<{ reload: () => Promise<void> } | null>(null)
const modalVisible = ref(false)
const submitLoading = ref(false)
const editingStatus = ref<EnableStatus | null>(null)
const form = ref<CapabilityFormModel>(createDefaultForm())

const modalTitle = computed(() => (form.value.basicId ? t('develop.ai_capability.modal_edit_title') : t('develop.ai_capability.modal_add_title')))

function reload() {
  void schemaPageRef.value?.reload()
}

const fields = computed<ListFieldSchema[]>(() => [
  { key: 'keyword', title: t('develop.ai_capability.col_name'), dataType: 'string', visible: false, searchable: true, searchPlaceholder: t('develop.ai_capability.search_placeholder'), order: 0 },
  { key: 'toolName', title: t('develop.ai_capability.col_name'), dataType: 'string', minWidth: 180, fixed: 'left', sortable: true, order: 1 },
  { key: 'toolCode', title: t('develop.ai_capability.col_code'), dataType: 'string', minWidth: 150, sortable: true, order: 2 },
  {
    key: 'toolType',
    title: t('develop.ai_capability.col_type'),
    dataType: 'enum',
    options: AI_TOOL_TYPE_OPTIONS,
    width: 120,
    order: 3,
    render: row => getOptionLabel(AI_TOOL_TYPE_OPTIONS, (row as unknown as AiToolListItemDto).toolType),
  },
  {
    key: 'riskLevel',
    title: t('develop.ai_capability.col_risk'),
    dataType: 'enum',
    options: AI_TOOL_RISK_LEVEL_OPTIONS,
    width: 100,
    order: 4,
    render(row) {
      const r = row as unknown as AiToolListItemDto
      const type = r.riskLevel === AiToolRiskLevel.High ? 'error' : r.riskLevel === AiToolRiskLevel.Medium ? 'warning' : 'success'
      return h(NTag, { size: 'small', round: true, bordered: false, type }, () => getOptionLabel(AI_TOOL_RISK_LEVEL_OPTIONS, r.riskLevel))
    },
  },
  {
    key: 'safetyLevel',
    title: t('develop.ai_capability.col_safety'),
    dataType: 'enum',
    options: AI_TOOL_SAFETY_LEVEL_OPTIONS,
    width: 120,
    order: 5,
    render: row => getOptionLabel(AI_TOOL_SAFETY_LEVEL_OPTIONS, (row as unknown as AiToolListItemDto).safetyLevel),
  },
  {
    key: 'status',
    title: t('common.fields.status'),
    dataType: 'enum',
    options: STATUS_OPTIONS,
    width: 90,
    order: 6,
    render(row) {
      const r = row as unknown as AiToolListItemDto
      return h(NTag, { size: 'small', round: true, bordered: false, type: r.status === EnableStatus.Enabled ? 'success' : 'error' }, () => getOptionLabel(statusEnumOptions.value, r.status))
    },
  },
])

const schema = computed<PageSchema>(() => ({
  pageCode: 'develop.ai.capability',
  pageName: t('develop.ai_capability.page_name'),
  rowKey: 'basicId',
  scrollX: 980,
  fields: fields.value,
  resource: {
    page: (params) => {
      const f = params.filters
      return aiToolApi.page({
        ...createPageRequest({
          page: { pageIndex: params.page, pageSize: params.pageSize },
          conditions: { sorts: querySortsFromSchema(params.sorts), filters: params.conditionFilters ?? [] },
        }),
        keyword: (f.keyword as string | undefined)?.trim() || undefined,
      }) as unknown as Promise<PageResult<Record<string, unknown>>>
    },
  },
  actions: [
    { key: 'create', title: t('develop.ai_capability.add'), scope: 'page', type: 'primary', icon: 'lucide:plus' },
    { key: 'edit', title: t('common.actions.edit'), scope: 'row', icon: 'lucide:pencil' },
  ],
}))

function createDefaultForm(): CapabilityFormModel {
  return {
    toolCode: '',
    toolName: '',
    toolType: AiToolType.BuiltInSkill,
    sourceKey: '',
    category: null,
    description: null,
    inputSchemaJson: null,
    outputSchemaJson: null,
    riskLevel: AiToolRiskLevel.Low,
    safetyLevel: AiToolSafetyLevel.ReadOnly,
    requiresApproval: false,
    defaultMaxCalls: 5,
    status: EnableStatus.Enabled,
    remark: null,
  }
}

function onAction(payload: SchemaActionPayload) {
  const row = payload.row as unknown as AiToolListItemDto | undefined
  switch (payload.key) {
    case 'create':
      handleAdd()
      break
    case 'edit':
      if (row) {
        void handleEdit(row)
      }
      break
  }
}

function handleAdd() {
  editingStatus.value = null
  form.value = createDefaultForm()
  modalVisible.value = true
}

async function handleEdit(row: AiToolListItemDto) {
  try {
    const detail = await aiToolApi.detail(row.basicId)
    if (!detail) {
      message.error(t('develop.ai_capability.not_found'))
      return
    }

    editingStatus.value = detail.status
    form.value = detailToForm(detail)
    modalVisible.value = true
  }
  catch {
    message.error(t('develop.ai_capability.load_detail_failed'))
  }
}

function detailToForm(detail: AiToolDetailDto): CapabilityFormModel {
  return {
    basicId: detail.basicId,
    toolCode: detail.toolCode,
    toolName: detail.toolName,
    toolType: detail.toolType,
    sourceKey: detail.sourceKey,
    category: detail.category ?? null,
    description: detail.description ?? null,
    inputSchemaJson: detail.inputSchemaJson ?? null,
    outputSchemaJson: detail.outputSchemaJson ?? null,
    riskLevel: detail.riskLevel,
    safetyLevel: detail.safetyLevel,
    requiresApproval: detail.requiresApproval,
    defaultMaxCalls: detail.defaultMaxCalls,
    status: detail.status,
    remark: detail.remark ?? null,
  }
}

function validateForm() {
  if (!form.value.toolName.trim()) {
    message.warning(t('develop.ai_capability.validate_name'))
    return false
  }
  if (!form.value.basicId && !form.value.toolCode.trim()) {
    message.warning(t('develop.ai_capability.validate_code'))
    return false
  }
  if (!form.value.basicId && !form.value.sourceKey.trim()) {
    message.warning(t('develop.ai_capability.validate_source_key'))
    return false
  }
  return true
}

async function handleSubmit() {
  if (!validateForm()) {
    return
  }

  submitLoading.value = true
  try {
    if (form.value.basicId) {
      const updateInput: AiToolUpdateDto = {
        basicId: form.value.basicId,
        toolName: form.value.toolName.trim(),
        category: form.value.category?.trim() || null,
        description: form.value.description?.trim() || null,
        inputSchemaJson: form.value.inputSchemaJson?.trim() || null,
        outputSchemaJson: form.value.outputSchemaJson?.trim() || null,
        riskLevel: form.value.riskLevel,
        safetyLevel: form.value.safetyLevel,
        requiresApproval: form.value.requiresApproval,
        defaultMaxCalls: form.value.defaultMaxCalls,
        remark: form.value.remark?.trim() || null,
      }
      await aiToolApi.update(updateInput)
    }
    else {
      const createInput: AiToolCreateDto = {
        toolCode: form.value.toolCode.trim(),
        toolName: form.value.toolName.trim(),
        toolType: AiToolType.BuiltInSkill,
        sourceKey: form.value.sourceKey.trim(),
        category: form.value.category?.trim() || null,
        description: form.value.description?.trim() || null,
        inputSchemaJson: form.value.inputSchemaJson?.trim() || null,
        outputSchemaJson: form.value.outputSchemaJson?.trim() || null,
        riskLevel: form.value.riskLevel,
        safetyLevel: form.value.safetyLevel,
        requiresApproval: form.value.requiresApproval,
        defaultMaxCalls: form.value.defaultMaxCalls,
        status: form.value.status,
        remark: form.value.remark?.trim() || null,
      }
      await aiToolApi.create(createInput)
    }
    if (form.value.basicId && editingStatus.value !== form.value.status) {
      await aiToolApi.updateStatus({
        basicId: form.value.basicId,
        status: form.value.status,
        remark: t('develop.ai_capability.update_status_remark'),
      })
    }
    message.success(t('common.messages.save_success'))
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
</script>

<template>
  <SchemaPage ref="schemaPageRef" :schema="schema" @action="onAction">
    <NModal
      v-model:show="modalVisible"
      :auto-focus="false"
      :bordered="false"
      :title="modalTitle"
      preset="card"
      style="width: 760px; max-width: 92vw"
    >
      <NForm :model="form" class="xh-edit-form-grid" label-placement="top">
        <div class="xh-form-section-title">
          {{ t('develop.ai_capability.section_basic') }}
        </div>
        <NFormItem :label="t('develop.ai_capability.form_code')">
          <NInput v-model:value="form.toolCode" :readonly="!!form.basicId" :placeholder="t('develop.ai_capability.form_code_placeholder')" />
        </NFormItem>
        <NFormItem :label="t('develop.ai_capability.form_source_key')">
          <NInput v-model:value="form.sourceKey" :readonly="!!form.basicId" :placeholder="t('develop.ai_capability.form_source_key_placeholder')" />
        </NFormItem>
        <NFormItem :label="t('develop.ai_capability.form_name')">
          <NInput v-model:value="form.toolName" clearable />
        </NFormItem>
        <NFormItem :label="t('develop.ai_capability.form_category')">
          <NInput v-model:value="form.category" clearable />
        </NFormItem>
        <NFormItem :label="t('develop.ai_capability.form_type')">
          <NSelect v-model:value="form.toolType" :options="AI_TOOL_TYPE_OPTIONS" disabled />
        </NFormItem>
        <div class="xh-form-section-title">
          {{ t('develop.ai_capability.section_policy') }}
        </div>
        <NFormItem :label="t('develop.ai_capability.form_risk')">
          <NSelect v-model:value="form.riskLevel" :options="AI_TOOL_RISK_LEVEL_OPTIONS" />
        </NFormItem>
        <NFormItem :label="t('develop.ai_capability.form_safety')">
          <NSelect v-model:value="form.safetyLevel" :options="AI_TOOL_SAFETY_LEVEL_OPTIONS" />
        </NFormItem>
        <NFormItem :label="t('develop.ai_capability.form_default_max_calls')">
          <NInputNumber v-model:value="form.defaultMaxCalls" :min="0" style="width: 100%" />
        </NFormItem>
        <NFormItem :label="t('develop.ai_capability.form_requires_approval')">
          <NSwitch v-model:value="form.requiresApproval" />
        </NFormItem>
        <NFormItem :label="t('common.fields.status')">
          <NSelect v-model:value="form.status" :options="statusEnumOptions" />
        </NFormItem>
        <NFormItem class="xh-form-full" :label="t('develop.ai_capability.form_description')">
          <NInput v-model:value="form.description" clearable type="textarea" :autosize="{ minRows: 2, maxRows: 4 }" />
        </NFormItem>
        <div class="xh-form-section-title">
          {{ t('develop.ai_capability.section_schema') }}
        </div>
        <NFormItem class="xh-form-full" :label="t('develop.ai_capability.form_input_schema')">
          <NInput v-model:value="form.inputSchemaJson" clearable type="textarea" :autosize="{ minRows: 3, maxRows: 8 }" />
        </NFormItem>
        <NFormItem class="xh-form-full" :label="t('develop.ai_capability.form_output_schema')">
          <NInput v-model:value="form.outputSchemaJson" clearable type="textarea" :autosize="{ minRows: 3, maxRows: 8 }" />
        </NFormItem>
        <NFormItem class="xh-form-full" :label="t('develop.ai_capability.form_remark')">
          <NInput v-model:value="form.remark" clearable type="textarea" :autosize="{ minRows: 2, maxRows: 4 }" />
        </NFormItem>
      </NForm>

      <template #footer>
        <NSpace justify="end">
          <NButton @click="modalVisible = false">
            {{ t('common.actions.cancel') }}
          </NButton>
          <NButton :loading="submitLoading" type="primary" @click="handleSubmit">
            {{ t('common.actions.save') }}
          </NButton>
        </NSpace>
      </template>
    </NModal>
  </SchemaPage>
</template>

<style scoped>
.xh-form-section-title {
  grid-column: 1 / -1;
  margin: 4px 0 2px;
  padding-bottom: 6px;
  border-bottom: 1px solid var(--n-border-color);
  color: var(--n-text-color-2);
  font-size: 13px;
  font-weight: 600;
}
</style>
