import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Paper,
  Stack,
  Tab,
  Tabs,
  Typography,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { fetchMethodDocs, fetchTriggerMethods, type TriggerMethodInfo } from '../api/client';

export default function DocsPage() {
  const { id } = useParams<{ id?: string }>();
  const navigate = useNavigate();

  const [methods, setMethods] = useState<TriggerMethodInfo[]>([]);
  const [docs, setDocs] = useState<string>('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchTriggerMethods()
      .then((m) => {
        setMethods(m);
        if (m.length === 0) setLoading(false);
      })
      .catch((e) => {
        setError(e instanceof Error ? e.message : 'Failed to load');
        setLoading(false);
      });
  }, []);

  const activeId = id ?? methods[0]?.id;

  useEffect(() => {
    if (!activeId) return;
    if (!id && methods.length > 0) {
      navigate(`/docs/${methods[0].id}`, { replace: true });
      return;
    }
    setLoading(true);
    setError(null);
    fetchMethodDocs(activeId)
      .then(setDocs)
      .catch((e) => setError(e instanceof Error ? e.message : 'Failed to load docs'))
      .finally(() => setLoading(false));
  }, [activeId, id, methods, navigate]);

  return (
    <Box>
      <Stack direction="row" spacing={1} sx={{ mb: 2, alignItems: 'center' }}>
        <Button
          size="small"
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/methods')}
        >
          Back to methods
        </Button>
      </Stack>
      <Typography variant="h4" sx={{ fontWeight: 700, mb: 2 }}>
        Documentation
      </Typography>

      <Paper variant="outlined" sx={{ mb: 2 }}>
        <Tabs
          value={activeId ?? false}
          onChange={(_, v: string) => navigate(`/docs/${v}`)}
          variant="scrollable"
          scrollButtons="auto"
        >
          {methods.map((m) => (
            <Tab key={m.id} value={m.id} label={m.name} />
          ))}
        </Tabs>
      </Paper>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Paper sx={{ p: 4 }}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Box
            sx={{
              '& h1': { fontSize: '2rem', mt: 0, mb: 2, fontWeight: 700 },
              '& h2': { fontSize: '1.5rem', mt: 4, mb: 1.5, fontWeight: 600 },
              '& h3': { fontSize: '1.15rem', mt: 3, mb: 1, fontWeight: 600 },
              '& p': { lineHeight: 1.7, mb: 1.5 },
              '& code': {
                bgcolor: 'grey.100',
                px: 0.75,
                py: 0.25,
                borderRadius: 0.5,
                fontFamily: 'ui-monospace, SFMono-Regular, monospace',
                fontSize: '0.9em',
              },
              '& pre': {
                bgcolor: 'grey.900',
                color: 'grey.50',
                p: 2,
                borderRadius: 1,
                overflowX: 'auto',
              },
              '& pre code': { bgcolor: 'transparent', color: 'inherit', p: 0 },
              '& table': { borderCollapse: 'collapse', mb: 2, width: '100%' },
              '& th, & td': { border: '1px solid', borderColor: 'divider', px: 1.5, py: 1, textAlign: 'left' },
              '& th': { bgcolor: 'grey.100' },
              '& blockquote': {
                borderLeft: '4px solid',
                borderColor: 'primary.main',
                pl: 2,
                color: 'text.secondary',
                fontStyle: 'italic',
                my: 2,
              },
              '& ul, & ol': { pl: 3 },
              '& li': { mb: 0.5 },
              '& a': { color: 'primary.main' },
            }}
          >
            <ReactMarkdown remarkPlugins={[remarkGfm]}>{docs}</ReactMarkdown>
          </Box>
        )}
      </Paper>
    </Box>
  );
}
