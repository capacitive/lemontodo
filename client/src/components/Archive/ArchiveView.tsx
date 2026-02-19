import { useState } from 'react';
import { useArchiveSearch, useRestoreTask, useDeleteArchiveTask, usePurgeAllArchive } from '../../hooks/useArchive';
import { toast } from 'sonner';

export function ArchiveView() {
  const [query, setQuery] = useState('');
  const [page, setPage] = useState(1);
  const { data, isLoading, error } = useArchiveSearch(query, page);
  const restoreTask = useRestoreTask();
  const deleteTask = useDeleteArchiveTask();
  const purgeAll = usePurgeAllArchive();

  const handleDelete = (id: string) => {
    deleteTask.mutate(id, {
      onSuccess: () => toast.success('Task deleted'),
      onError: () => toast.error('Failed to delete task'),
    });
  };

  const handlePurgeAll = () => {
    if (!confirm('Are you sure you want to permanently delete all archived tasks?')) return;
    purgeAll.mutate(undefined, {
      onSuccess: () => toast.success('All archived tasks deleted'),
      onError: () => toast.error('Failed to purge archive'),
    });
  };

  const handleRestore = (id: string) => {
    restoreTask.mutate(id, {
      onSuccess: () => toast.success('Task restored'),
      onError: () => toast.error('Failed to restore task'),
    });
  };

  const totalPages = data ? Math.ceil(data.totalCount / data.pageSize) : 0;

  return (
    <div style={{ padding: 24, maxWidth: 700, margin: '0 auto' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <h2 style={{ fontSize: '1.2rem', color: '#374151', margin: 0 }}>Archive</h2>
        {data && data.totalCount > 0 && (
          <button
            onClick={handlePurgeAll}
            disabled={purgeAll.isPending}
            style={{
              padding: '6px 14px', borderRadius: 6, border: 'none',
              background: '#fee2e2', color: '#dc2626', cursor: 'pointer',
              fontWeight: 500, fontSize: '0.8rem',
            }}
          >
            Purge All
          </button>
        )}
      </div>

      <input
        type="text"
        placeholder="Search archived tasks..."
        value={query}
        onChange={(e) => { setQuery(e.target.value); setPage(1); }}
        style={{
          width: '100%', padding: '10px 14px', borderRadius: 8,
          border: '1px solid #d4d4d8', fontSize: '0.9rem',
          marginBottom: 16, boxSizing: 'border-box',
        }}
      />

      {isLoading && <div style={{ textAlign: 'center', color: '#9ca3af' }}>Loading...</div>}
      {error && <div style={{ color: '#dc2626' }}>Error loading archive</div>}

      {data && (
        <>
          <div style={{ fontSize: '0.85rem', color: '#6b7280', marginBottom: 12 }}>
            {data.totalCount} result{data.totalCount !== 1 ? 's' : ''}
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
            {[...data.items].sort((a, b) => a.completionDate.localeCompare(b.completionDate)).map((task) => (
              <div
                key={task.id}
                style={{
                  padding: 12, borderRadius: 8, background: '#fff',
                  border: '1px solid #e5e7eb', display: 'flex',
                  justifyContent: 'space-between', alignItems: 'center',
                }}
              >
                <div>
                  <strong style={{ fontSize: '0.95rem' }}>{task.name}</strong>
                  {task.description && (
                    <p style={{ margin: '4px 0 0', fontSize: '0.8rem', color: '#6b7280' }}>
                      {task.description}
                    </p>
                  )}
                  <div style={{ display: 'flex', gap: 12, fontSize: '0.75rem', color: '#9ca3af', marginTop: 4 }}>
                    <span>Due: {task.completionDate}</span>
                    <span>Closed: {task.closedAt ? new Date(task.closedAt).toLocaleDateString() : 'N/A'}</span>
                  </div>
                </div>
                <div style={{ display: 'flex', gap: 6, flexShrink: 0 }}>
                  <button
                    onClick={() => handleRestore(task.id)}
                    disabled={restoreTask.isPending}
                    style={{
                      padding: '6px 14px', borderRadius: 6, border: 'none',
                      background: '#dbeafe', color: '#2563eb', cursor: 'pointer',
                      fontWeight: 500, fontSize: '0.8rem',
                    }}
                  >
                    Restore
                  </button>
                  <button
                    onClick={() => handleDelete(task.id)}
                    disabled={deleteTask.isPending}
                    style={{
                      padding: '6px 14px', borderRadius: 6, border: 'none',
                      background: '#fee2e2', color: '#dc2626', cursor: 'pointer',
                      fontWeight: 500, fontSize: '0.8rem',
                    }}
                  >
                    Delete
                  </button>
                </div>
              </div>
            ))}

            {data.items.length === 0 && (
              <div style={{ color: '#9ca3af', textAlign: 'center', padding: 20 }}>
                No archived tasks found
              </div>
            )}
          </div>

          {totalPages > 1 && (
            <div style={{ display: 'flex', justifyContent: 'center', gap: 8, marginTop: 16 }}>
              <button
                disabled={page <= 1}
                onClick={() => setPage((p) => p - 1)}
                style={pageBtnStyle}
              >
                Prev
              </button>
              <span style={{ fontSize: '0.85rem', color: '#6b7280', alignSelf: 'center' }}>
                {page} / {totalPages}
              </span>
              <button
                disabled={page >= totalPages}
                onClick={() => setPage((p) => p + 1)}
                style={pageBtnStyle}
              >
                Next
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}

const pageBtnStyle: React.CSSProperties = {
  padding: '6px 14px', borderRadius: 6, border: '1px solid #d4d4d8',
  background: '#fff', cursor: 'pointer', fontSize: '0.85rem',
};
