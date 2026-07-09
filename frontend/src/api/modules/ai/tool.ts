import type { ApiId, PageResult } from '../../types'
import type {
  AiToolDetailDto,
  AiToolCreateDto,
  AiToolListItemDto,
  AiToolPageQueryDto,
  AiToolSelectItemDto,
  AiToolStatusUpdateDto,
  AiToolUpdateDto,
} from './tool.types'
import { createDynamicApiClient, formatDynamicApiRouteValue } from '../../base'

const command = createDynamicApiClient('AiTool')
const query = createDynamicApiClient('AiToolQuery')

export const aiToolApi = {
  create(input: AiToolCreateDto) {
    return command.post<AiToolDetailDto, AiToolCreateDto>('Create', input)
  },
  update(input: AiToolUpdateDto) {
    return command.put<AiToolDetailDto, AiToolUpdateDto>('Update', input)
  },
  updateStatus(input: AiToolStatusUpdateDto) {
    return command.put<AiToolDetailDto, AiToolStatusUpdateDto>('Status', input)
  },
  page(input: AiToolPageQueryDto) {
    return query.post<PageResult<AiToolListItemDto>>('Page', input)
  },
  select() {
    return query.get<AiToolSelectItemDto[]>('Select')
  },
  detail(id: ApiId) {
    return query.get<AiToolDetailDto | null>(`Detail/${formatDynamicApiRouteValue(id)}`)
  },
}
