import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { startConnection, onTaskClosed, onTaskRestored, onTaskUpdated } from '../api/signalr';

export function useSignalR() {
  const qc = useQueryClient();

  useEffect(() => {
    startConnection();

    const unsubs = [
      onTaskClosed((taskId) => {
        qc.invalidateQueries({ queryKey: ['tasks'] });
        qc.invalidateQueries({ queryKey: ['archive'] });
        toast.info(`Task ${taskId.slice(0, 8)}... archived`);
      }),
      onTaskRestored((taskId) => {
        qc.invalidateQueries({ queryKey: ['tasks'] });
        qc.invalidateQueries({ queryKey: ['archive'] });
        toast.info(`Task ${taskId.slice(0, 8)}... restored`);
      }),
      onTaskUpdated(() => {
        qc.invalidateQueries({ queryKey: ['tasks'] });
      }),
    ];

    return () => unsubs.forEach((unsub) => unsub());
  }, [qc]);
}
