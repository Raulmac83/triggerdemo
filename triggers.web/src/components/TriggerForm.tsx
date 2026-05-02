import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  FormControlLabel,
  Stack,
  Switch,
  TextField,
  Typography,
} from '@mui/material';
import {
  createTrigger,
  fetchTrigger,
  updateTrigger,
  type TriggerInput,
} from '../api/client';

interface TriggerFormProps {
  mode: 'create' | 'edit';
}

export default function TriggerForm({ mode }: TriggerFormProps) {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const triggerId = id ? Number(id) : undefined;

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [isEnabled, setIsEnabled] = useState(false);

  const [loading, setLoading] = useState(mode === 'edit');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (mode !== 'edit' || !triggerId) return;
    let cancelled = false;
    fetchTrigger(triggerId)
      .then((t) => {
        if (cancelled) return;
        setName(t.name);
        setDescription(t.description ?? '');
        setIsEnabled(t.isEnabled);
      })
      .catch((e) => !cancelled && setError(e instanceof Error ? e.message : 'Failed to load'))
      .finally(() => !cancelled && setLoading(false));
    return () => {
      cancelled = true;
    };
  }, [mode, triggerId]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    const payload: TriggerInput = {
      name: name.trim(),
      description: description.trim() ? description.trim() : null,
      isEnabled,
    };
    try {
      if (mode === 'create') {
        await createTrigger(payload);
      } else if (triggerId) {
        await updateTrigger(triggerId, payload);
      }
      navigate('/triggers');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Save failed');
    } finally {
      setSubmitting(false);
    }
  }

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 6 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" sx={{ fontWeight: 700 }} gutterBottom>
        {mode === 'create' ? 'New Trigger' : 'Edit Trigger'}
      </Typography>

      <Card sx={{ maxWidth: 640 }}>
        <CardContent>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}
          <Box component="form" onSubmit={handleSubmit}>
            <Stack spacing={2}>
              <TextField
                label="Name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
                fullWidth
                autoFocus
                slotProps={{ htmlInput: { maxLength: 200 } }}
              />
              <TextField
                label="Description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                fullWidth
                multiline
                minRows={3}
                slotProps={{ htmlInput: { maxLength: 1000 } }}
              />
              <FormControlLabel
                control={<Switch checked={isEnabled} onChange={(_, v) => setIsEnabled(v)} />}
                label="Enabled"
              />
              <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
                <Button onClick={() => navigate('/triggers')} disabled={submitting}>
                  Cancel
                </Button>
                <Button
                  type="submit"
                  variant="contained"
                  disabled={submitting || !name.trim()}
                  startIcon={submitting ? <CircularProgress size={16} color="inherit" /> : null}
                >
                  {mode === 'create' ? 'Create' : 'Save changes'}
                </Button>
              </Stack>
            </Stack>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
}
