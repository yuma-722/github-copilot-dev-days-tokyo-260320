import { useState, useCallback } from 'react';
import Header from './components/Header';
import FeedbackForm from './components/FeedbackForm';
import FeedbackList from './components/FeedbackList';

export default function App() {
  const [refreshKey, setRefreshKey] = useState(0);

  const handleSubmitted = useCallback(() => {
    setRefreshKey((k) => k + 1);
  }, []);

  return (
    <div className="flex min-h-screen flex-col bg-[#0d1117]">
      <Header />
      <main className="mx-auto w-full max-w-2xl flex-1 px-4 py-8">
        <div className="space-y-8">
          <FeedbackForm onSubmitted={handleSubmitted} />
          <section>
            <h2 className="mb-4 text-lg font-semibold text-[#e6edf3]">
              みんなのフィードバック
            </h2>
            <FeedbackList refreshKey={refreshKey} />
          </section>
        </div>
      </main>
    </div>
  );
}
