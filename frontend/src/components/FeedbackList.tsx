import { useEffect, useState } from 'react';
import { getFeedbacks } from '../api/feedbacks';
import type { Feedback } from '../types/feedback';

function timeAgo(dateStr: string): string {
  const diff = Date.now() - new Date(dateStr).getTime();
  const seconds = Math.floor(diff / 1000);
  if (seconds < 60) return `${seconds}秒前`;
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}分前`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}時間前`;
  const days = Math.floor(hours / 24);
  return `${days}日前`;
}

interface Props {
  refreshKey: number;
}

export default function FeedbackList({ refreshKey }: Props) {
  const [feedbacks, setFeedbacks] = useState<Feedback[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setLoading(true);
    setError(null);
    getFeedbacks()
      .then((res) => {
        if (res.success && res.feedbacks) {
          setFeedbacks(res.feedbacks);
        } else {
          setError(res.error ?? 'データの取得に失敗しました');
        }
      })
      .catch(() => setError('ネットワークエラーが発生しました'))
      .finally(() => setLoading(false));
  }, [refreshKey]);

  if (loading) {
    return (
      <div className="flex justify-center py-12">
        <div className="h-6 w-6 animate-spin rounded-full border-2 border-[#30363d] border-t-[#8b5cf6]" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="rounded-lg border border-red-500/30 bg-red-500/10 p-4 text-sm text-red-400">
        {error}
      </div>
    );
  }

  if (feedbacks.length === 0) {
    return (
      <div className="rounded-lg border border-[#30363d] bg-[#161b22] p-8 text-center text-sm text-[#7d8590]">
        まだフィードバックはありません。最初の投稿をしてみましょう！
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {feedbacks.map((fb) => (
        <div
          key={fb.id}
          className="rounded-lg border border-[#30363d] bg-[#161b22] p-4"
        >
          <p className="whitespace-pre-wrap text-sm text-[#e6edf3]">{fb.comment}</p>
          <p className="mt-2 text-xs text-[#7d8590]">{timeAgo(fb.createdAt)}</p>
        </div>
      ))}
    </div>
  );
}
