export interface SearchResponse {
  score: number;
  postId?: number;
  postAttributeId?: number;
  commentId?: number;
  commentAttributeId?: number;
}