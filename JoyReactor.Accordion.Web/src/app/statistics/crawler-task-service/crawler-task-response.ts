export interface CrawlerTaskResponse {
  id: string,
  tag: ParsedTagThinResponse,
  postLineType: number
  pageFrom?: number
  pageTo?: number,
  pageCurrent?: number,
  isIndefinite: boolean
  isCompleted: boolean,
  startedAt?: Date,
  finishedAt?: Date,
  createdAt: Date,
  updatedAt: Date,
}

export interface ParsedTagThinResponse {
  id: string,
  numberId: number,
  name: string,
}