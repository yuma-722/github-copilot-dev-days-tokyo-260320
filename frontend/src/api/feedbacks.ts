import type { FeedbackResponse } from '../types/feedback';

export async function postFeedback(comment: string): Promise<FeedbackResponse> {
  const res = await fetch('/api/feedbacks', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ comment }),
  });
  return res.json();
}

export async function getFeedbacks(): Promise<FeedbackResponse> {
  const res = await fetch('/api/feedbacks');
  return res.json();
}
