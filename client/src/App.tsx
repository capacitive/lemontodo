import { useState } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'sonner';
import { Header } from './components/Layout/Header';
import { BoardView } from './components/Board/BoardView';
import { ListView } from './components/List/ListView';
import { ArchiveView } from './components/Archive/ArchiveView';
import { useSignalR } from './hooks/useSignalR';
import type { ViewMode } from './types';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { refetchOnWindowFocus: false, retry: 1 },
  },
});

function AppContent() {
  const [view, setView] = useState<ViewMode>('board');
  useSignalR();

  return (
    <div style={{ minHeight: '100vh', background: '#fafafa', fontFamily: 'system-ui, sans-serif' }}>
      <Header view={view} onViewChange={setView} />
      {view === 'board' && <BoardView />}
      {view === 'list' && <ListView />}
      {view === 'archive' && <ArchiveView />}
    </div>
  );
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AppContent />
      <Toaster position="bottom-right" richColors />
    </QueryClientProvider>
  );
}
