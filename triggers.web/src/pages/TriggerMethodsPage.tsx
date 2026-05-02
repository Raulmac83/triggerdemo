import { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Card,
  CardActionArea,
  CardContent,
  Chip,
  CircularProgress,
  Grid,
  Stack,
  Typography,
} from '@mui/material';
import OpenInNewIcon from '@mui/icons-material/OpenInNew';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import { Link as RouterLink } from 'react-router-dom';
import {
  fetchActiveMethod,
  fetchTriggerMethods,
  setActiveMethod,
  type TriggerMethodInfo,
} from '../api/client';

export default function TriggerMethodsPage() {
  const [methods, setMethods] = useState<TriggerMethodInfo[]>([]);
  const [active, setActive] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [updating, setUpdating] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([fetchTriggerMethods(), fetchActiveMethod()])
      .then(([m, a]) => {
        setMethods(m);
        setActive(a);
      })
      .catch((e) => setError(e instanceof Error ? e.message : 'Failed to load'))
      .finally(() => setLoading(false));
  }, []);

  async function handleSelect(id: string) {
    if (id === active) return;
    setUpdating(id);
    setError(null);
    try {
      const next = await setActiveMethod(id);
      setActive(next);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to set method');
    } finally {
      setUpdating(null);
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
        Trigger Method
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
        Choose which library handles trigger-table change events. The active method writes a
        <code> Notification </code>row whenever a Trigger record is created, updated, or deleted.
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Grid container spacing={2}>
        {methods.map((m) => {
          const isActive = active === m.id;
          const isUpdating = updating === m.id;
          return (
            <Grid key={m.id} size={{ xs: 12, sm: 6 }}>
              <Card
                variant="outlined"
                sx={{
                  borderWidth: isActive ? 2 : 1,
                  borderColor: isActive ? 'primary.main' : 'divider',
                  height: '100%',
                  display: 'flex',
                  flexDirection: 'column',
                }}
              >
                <CardActionArea onClick={() => handleSelect(m.id)} disabled={isUpdating} sx={{ flexGrow: 1 }}>
                  <CardContent>
                    <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
                      <Box>
                        <Typography variant="overline" color="text.secondary">
                          {m.library}
                        </Typography>
                        <Typography variant="h6" sx={{ fontWeight: 600 }}>
                          {m.name}
                        </Typography>
                      </Box>
                      {isActive && <Chip color="primary" size="small" icon={<CheckCircleIcon />} label="Active" />}
                      {isUpdating && <CircularProgress size={20} />}
                    </Stack>
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }}>
                      {m.tagline}
                    </Typography>
                    <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap', gap: 0.5 }}>
                      {m.highlights.map((h) => (
                        <Chip key={h} label={h} size="small" variant="outlined" />
                      ))}
                    </Stack>
                  </CardContent>
                </CardActionArea>
                <Box sx={{ p: 1, pt: 0 }}>
                  <Button
                    size="small"
                    component={RouterLink}
                    to={`/docs/${m.id}`}
                    endIcon={<OpenInNewIcon fontSize="small" />}
                  >
                    View documentation
                  </Button>
                </Box>
              </Card>
            </Grid>
          );
        })}
      </Grid>
    </Box>
  );
}
