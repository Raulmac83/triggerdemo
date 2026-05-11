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
  createCustomer,
  fetchCustomer,
  updateCustomer,
  type CustomerInput,
} from '../api/client';

interface CustomerFormProps {
  mode: 'create' | 'edit';
}

export default function CustomerForm({ mode }: CustomerFormProps) {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const customerId = id ? Number(id) : undefined;

  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [isActive, setIsActive] = useState(true);

  const [loading, setLoading] = useState(mode === 'edit');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (mode !== 'edit' || !customerId) return;
    let cancelled = false;
    fetchCustomer(customerId)
      .then((c) => {
        if (cancelled) return;
        setName(c.name);
        setEmail(c.email ?? '');
        setPhone(c.phone ?? '');
        setIsActive(c.isActive);
      })
      .catch((e) => !cancelled && setError(e instanceof Error ? e.message : 'Failed to load'))
      .finally(() => !cancelled && setLoading(false));
    return () => {
      cancelled = true;
    };
  }, [mode, customerId]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    const payload: CustomerInput = {
      name: name.trim(),
      email: email.trim() ? email.trim() : null,
      phone: phone.trim() ? phone.trim() : null,
      isActive,
    };
    try {
      if (mode === 'create') {
        await createCustomer(payload);
      } else if (customerId) {
        await updateCustomer(customerId, payload);
      }
      navigate('/customers');
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
        {mode === 'create' ? 'New Customer' : 'Edit Customer'}
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
                label="Email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                type="email"
                fullWidth
                slotProps={{ htmlInput: { maxLength: 256 } }}
              />
              <TextField
                label="Phone"
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
                fullWidth
                slotProps={{ htmlInput: { maxLength: 50 } }}
              />
              <FormControlLabel
                control={<Switch checked={isActive} onChange={(_, v) => setIsActive(v)} />}
                label="Active"
              />
              <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
                <Button onClick={() => navigate('/customers')} disabled={submitting}>
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
