export interface Feedback {
  id: string;
  comment: string;
  date: string;
  createdAt: string;
}

export interface FeedbackRequest {
  comment: string;
}

export interface FeedbackResponse {
  success: boolean;
  id?: string;
  createdAt?: string;
  error?: string;
  feedbacks?: Feedback[];
}
