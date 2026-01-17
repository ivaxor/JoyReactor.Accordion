export interface CrawlerTaskResponse {
  id: string,
  tag: ParsedTagThinResponse,
  postLineType: number  
  pageCurrent: number,
  pageLast?: number,
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