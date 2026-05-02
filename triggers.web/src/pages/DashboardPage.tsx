import { Box, Card, CardContent, Grid, Typography } from '@mui/material';

const stats = [
  { label: 'Total Triggers', value: '—' },
  { label: 'Enabled', value: '—' },
  { label: 'Fired Today', value: '—' },
  { label: 'Errors', value: '—' },
];

export default function DashboardPage() {
  return (
    <Box>
      <Typography variant="h4" sx={{ fontWeight: 700 }} gutterBottom>
        Dashboard
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
        Overview of your trigger activity.
      </Typography>
      <Grid container spacing={3}>
        {stats.map((s) => (
          <Grid key={s.label} size={{ xs: 12, sm: 6, md: 3 }}>
            <Card>
              <CardContent>
                <Typography variant="overline" color="text.secondary">
                  {s.label}
                </Typography>
                <Typography variant="h4" sx={{ fontWeight: 700 }}>
                  {s.value}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
}
