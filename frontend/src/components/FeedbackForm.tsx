import { useState } from 'react';
import { postFeedback } from '../api/feedbacks';

const MAX_LENGTH = 2000;

interface Props {
  onSubmitted: () => void;
}

export default function FeedbackForm({ onSubmitted }: Props) {
  const [comment, setComment] = useState('');
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const canSubmit = comment.trim().length > 0 && comment.length <= MAX_LENGTH && !loading;

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!canSubmit) return;

    setLoading(true);
    setMessage(null);

    try {
      const res = await postFeedback(comment.trim());
      if (res.success) {
        setMessage({ type: 'success', text: 'フィードバックを送信しました！' });
        setComment('');
        onSubmitted();
      } else {
        setMessage({ type: 'error', text: res.error ?? '送信に失敗しました' });
      }
    } catch {
      setMessage({ type: 'error', text: 'ネットワークエラーが発生しました' });
    } finally {
      setLoading(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="rounded-lg border border-[#30363d] bg-[#161b22] p-5">
      <label htmlFor="comment" className="mb-2 block text-sm font-medium text-[#e6edf3]">
        フィードバックを入力
      </label>
      <textarea
        id="comment"
        value={comment}
        onChange={(e) => setComment(e.target.value)}
        maxLength={MAX_LENGTH}
        rows={4}
        placeholder="イベントの感想やご意見をお聞かせください..."
        className="w-full resize-none rounded-md border border-[#30363d] bg-[#0d1117] px-3 py-2 text-sm text-[#e6edf3] placeholder-[#7d8590] focus:border-[#8b5cf6] focus:outline-none focus:ring-1 focus:ring-[#8b5cf6]"
      />
      <div className="mt-2 flex items-center justify-between">
        <span className={`text-xs ${comment.length > MAX_LENGTH ? 'text-red-400' : 'text-[#7d8590]'}`}>
          {comment.length} / {MAX_LENGTH}
        </span>
        <button
          type="submit"
          disabled={!canSubmit}
          className="rounded-md bg-[#8b5cf6] px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-[#7c3aed] disabled:cursor-not-allowed disabled:opacity-50"
        >
          {loading ? '送信中...' : '送信する'}
        </button>
      </div>
      {message && (
        <p className={`mt-3 text-sm ${message.type === 'success' ? 'text-green-400' : 'text-red-400'}`}>
          {message.text}
        </p>
      )}
    </form>
  );
}
