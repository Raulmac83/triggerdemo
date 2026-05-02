import { useCallback, useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Chip,
  IconButton,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material';
import RefreshIcon from '@mui/icons-material/Refresh';
import DeleteSweepIcon from '@mui/icons-material/DeleteSweep';
import { DataGrid, type GridColDef } from '@mui/x-data-grid';
import {
  clearNotifications,
  fetchNotifications,
  type NotificationRow,
} from '../api/client';
import ConfirmDialog from '../components/ConfirmDialog';

const methodColor: Record<string, 'primary' | 'secondary' | 'success' | 'warning' | 'default'> = {
  Interceptor: 'primary',
  EFCoreTriggered: 'secondary',
  DomainEvents: 'success',
  AuditNet: 'warning',
};

export default function NotificationsPage() {
  const [rows, setRows] = useState<NotificationRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [clearing, setClearing] = useState(false);

  const load = useCallback(() => {
    setLoading(true);
    fetchNotifications(200)
      .then(setRows)
      .catch((e) => setError(e instanceof Error ? e.message : 'Failed to load'))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    load();
    const interval = window.setInterval(load, 5000);
    return () => window.clearInterval(interval);
  }, [load]);

  async function handleClearAll() {
    setClearing(true);
    try {
      await clearNotifications();
      setConfirmOpen(false);
      load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Clear failed');
    } finally {
      setClearing(false);
    }
  }

  const columns: GridColDef<NotificationRow>[] = [
    { field: 'id', headerName: 'ID', width: 80 },
    {
      field: 'triggerMethod',
      headerName: 'Method',
      width: 170,
      renderCell: ({ value }) =>
        value ? (
          <Chip size="small" color={methodColor[value as string] ?? 'default'} label={value} />
        ) : (
          <Chip size="small" label="—" />
        ),
      sortable: false,
    },
    { field: 'type', headerName: 'Type', width: 160 },
    { field: 'title', headerName: 'Title', flex: 1, minWidth: 240 },
    { field: 'entityType', headerName: 'Entity', width: 110 },
    { field: 'entityId', headerName: 'Entity Id', width: 100 },
    {
      field: 'createdAt',
      headerName: 'Created',
      width: 180,
      valueFormatter: (value) => (value ? new Date(value as string).toLocaleString() : ''),
    },
  ];

  return (
    <Box>
      <Stack direction="row" sx={{ mb: 3, justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h4" sx={{ fontWeight: 700 }}>
            Notifications
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Auto-refreshes every 5s. Each row shows which trigger method produced it.
          </Typography>
        </Box>
        <Stack direction="row" spacing={1}>
          <Tooltip title="Reload">
            <IconButton onClick={load} disabled={loading}>
              <RefreshIcon />
            </IconButton>
          </Tooltip>
          <Button
            variant="outlined"
            color="error"
            startIcon={<DeleteSweepIcon />}
            onClick={() => setConfirmOpen(true)}
            disabled={rows.length === 0}
          >
            Clear all
          </Button>
        </Stack>
      </Stack>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Box sx={{ height: 600, width: '100%' }}>
        <DataGrid<NotificationRow>
          rows={rows}
          columns={columns}
          loading={loading}
          getRowId={(r) => r.id}
          disableRowSelectionOnClick
          initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          pageSizeOptions={[25, 50, 100]}
          sx={{ bgcolor: 'background.paper' }}
        />
      </Box>

      <ConfirmDialog
        open={confirmOpen}
        title="Clear all notifications?"
        message={`This deletes all ${rows.length} notification(s). This action cannot be undone.`}
        confirmLabel="Clear all"
        destructive
        loading={clearing}
        onConfirm={handleClearAll}
        onCancel={() => !clearing && setConfirmOpen(false)}
      />
    </Box>
  );
}
