import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { archiveApi } from '../api/taskApi';

export function useArchiveSearch(query: string, page: number, pageSize = 20) {
  return useQuery({
    queryKey: ['archive', query, page, pageSize],
    queryFn: () => archiveApi.search(query || undefined, page, pageSize),
  });
}

export function useRestoreTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => archiveApi.restore(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['archive'] });
      qc.invalidateQueries({ queryKey: ['tasks'] });
    },
  });
}

export function useDeleteArchiveTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => archiveApi.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['archive'] });
    },
  });
}

export function usePurgeAllArchive() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: () => archiveApi.purgeAll(),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['archive'] });
    },
  });
}
