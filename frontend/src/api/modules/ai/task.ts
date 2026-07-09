import type { ApiId } from '../../types'
import type {
  AiTaskCreateDto,
  AiTaskDetailDto,
  AiTaskExecutionResultDto,
  AiTaskListItemDto,
  AiTaskRunDto,
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
  list() {
    return query.get<AiTaskListItemDto[]>('List')
  },
  detail(id: ApiId) {
    return query.get<AiTaskDetailDto | null>(`Detail/${formatDynamicApiRouteValue(id)}`)
  },
}
