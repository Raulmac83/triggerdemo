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
  createProduct,
  fetchProduct,
  updateProduct,
  type ProductInput,
} from '../api/client';

interface ProductFormProps {
  mode: 'create' | 'edit';
}

export default function ProductForm({ mode }: ProductFormProps) {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const productId = id ? Number(id) : undefined;

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [sku, setSku] = useState('');
  const [price, setPrice] = useState('0');
  const [isActive, setIsActive] = useState(true);

  const [loading, setLoading] = useState(mode === 'edit');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (mode !== 'edit' || !productId) return;
    let cancelled = false;
    fetchProduct(productId)
      .then((p) => {
        if (cancelled) return;
        setName(p.name);
        setDescription(p.description ?? '');
        setSku(p.sku ?? '');
        setPrice(String(p.price));
        setIsActive(p.isActive);
      })
      .catch((e) => !cancelled && setError(e instanceof Error ? e.message : 'Failed to load'))
      .finally(() => !cancelled && setLoading(false));
    return () => {
      cancelled = true;
    };
  }, [mode, productId]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    const parsedPrice = Number(price);
    if (Number.isNaN(parsedPrice)) {
      setError('Price must be a number.');
      setSubmitting(false);
      return;
    }
    const payload: ProductInput = {
      name: name.trim(),
      description: description.trim() ? description.trim() : null,
      sku: sku.trim() ? sku.trim() : null,
      price: parsedPrice,
      isActive,
    };
    try {
      if (mode === 'create') {
        await createProduct(payload);
      } else if (productId) {
        await updateProduct(productId, payload);
      }
      navigate('/products');
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
        {mode === 'create' ? 'New Product' : 'Edit Product'}
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
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField
                  label="SKU"
                  value={sku}
                  onChange={(e) => setSku(e.target.value)}
                  fullWidth
                  slotProps={{ htmlInput: { maxLength: 50 } }}
                />
                <TextField
                  label="Price"
                  value={price}
                  onChange={(e) => setPrice(e.target.value)}
                  type="number"
                  inputProps={{ step: '0.01', min: 0 }}
                  fullWidth
                  required
                />
              </Stack>
              <FormControlLabel
                control={<Switch checked={isActive} onChange={(_, v) => setIsActive(v)} />}
                label="Active"
              />
              <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
                <Button onClick={() => navigate('/products')} disabled={submitting}>
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
