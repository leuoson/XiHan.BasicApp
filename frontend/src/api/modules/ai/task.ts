import type { ApiId } from '../../types'
import type {
  AiTaskCreateDto,
  AiTaskAppendGuidanceDto,
  AiTaskDetailDto,
  AiTaskExecutionResultDto,
  AiTaskListItemDto,
  AiTaskRunDetailDto,
  AiTaskRunDto,
  AiTaskRunEventDto,
  AiTaskRunGuidanceDto,
  AiTaskRunListItemDto,
  AiTaskStatusUpdateDto,
  AiTaskUpdateDto,
} from './task.types'
import { createDynamicApiClient, formatDynamicApiRouteValue } from '../../base'

const command = createDynamicApiClient('AiTask')
const query = createDynamicApiClient('AiTaskQuery')

export const aiTaskApi = {
  create(input: AiTaskCreateDto) {
    return command.post<AiTaskDetailDto, AiTaskCreateDto>('Create', input)
  },
  update(input: AiTaskUpdateDto) {
    return command.put<AiTaskDetailDto, AiTaskUpdateDto>('Update', input)
  },
  updateStatus(input: AiTaskStatusUpdateDto) {
    return command.put<AiTaskDetailDto, AiTaskStatusUpdateDto>('Status', input)
  },
  delete(id: ApiId) {
    return command.delete(`Delete/${formatDynamicApiRouteValue(id)}`)
  },
  run(id: ApiId) {
    return command.post<AiTaskExecutionResultDto, AiTaskRunDto>('Run', { basicId: id })
  },
  appendRunGuidance(input: AiTaskAppendGuidanceDto) {
    return command.post<AiTaskRunGuidanceDto, AiTaskAppendGuidanceDto>('AppendRunGuidance', input)
  },
  list() {
    return query.get<AiTaskListItemDto[]>('List')
  },
  detail(id: ApiId) {
    return query.get<AiTaskDetailDto | null>(`Detail/${formatDynamicApiRouteValue(id)}`)
  },
  runList(taskId: ApiId) {
    return query.get<AiTaskRunListItemDto[]>(`RunList/${formatDynamicApiRouteValue(taskId)}`)
  },
  runDetail(runId: ApiId) {
    return query.get<AiTaskRunDetailDto | null>(`RunDetail/${formatDynamicApiRouteValue(runId)}`)
  },
  runEvents(runId: ApiId, afterSequence = 0) {
    return query.get<AiTaskRunEventDto[]>(`RunEvents/${formatDynamicApiRouteValue(runId)}?afterSequence=${afterSequence}`)
  },
}
