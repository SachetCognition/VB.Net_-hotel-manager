import React, { useEffect, useState } from 'react';
import {
  Box, Grid, Card, CardContent, Typography, CircularProgress, Alert,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, Chip,
} from '@mui/material';
import { People, MeetingRoom, BookOnline, Login } from '@mui/icons-material';
import api from '../../api/client';
import { DashboardData } from '../../types';

const DashboardPage: React.FC = () => {
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchData = async () => {
      try {
        const res = await api.get<DashboardData>('/HotelInfo/dashboard');
        setData(res.data);
      } catch {
        setError('Failed to load dashboard data');
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  if (loading) return <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}><CircularProgress /></Box>;
  if (error) return <Alert severity="error">{error}</Alert>;

  const checkInCount = data?.currentCheckIns?.length || 0;
  const reservationCount = data?.currentReservations?.length || 0;

  return (
    <Box>
      <Typography variant="h4" fontWeight={700} gutterBottom color="#1a237e">
        Dashboard
      </Typography>

      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card sx={{ background: 'linear-gradient(135deg, #1a237e, #3949ab)', color: '#fff', borderRadius: 2 }}>
            <CardContent>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Box>
                  <Typography variant="h3" fontWeight={700}>{checkInCount}</Typography>
                  <Typography variant="body2" sx={{ opacity: 0.85 }}>Current Check-Ins</Typography>
                </Box>
                <Login sx={{ fontSize: 48, opacity: 0.7 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card sx={{ background: 'linear-gradient(135deg, #e65100, #f57c00)', color: '#fff', borderRadius: 2 }}>
            <CardContent>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Box>
                  <Typography variant="h3" fontWeight={700}>{reservationCount}</Typography>
                  <Typography variant="body2" sx={{ opacity: 0.85 }}>Active Reservations</Typography>
                </Box>
                <BookOnline sx={{ fontSize: 48, opacity: 0.7 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 6 }}>
          <Paper sx={{ p: 2, borderRadius: 2 }}>
            <Typography variant="h6" fontWeight={600} gutterBottom>
              Current Check-Ins
            </Typography>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell><strong>Room</strong></TableCell>
                    <TableCell><strong>Guest</strong></TableCell>
                    <TableCell><strong>Check-In</strong></TableCell>
                    <TableCell><strong>Check-Out</strong></TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {data?.currentCheckIns?.length ? data.currentCheckIns.map((ci, idx) => (
                    <TableRow key={idx}>
                      <TableCell><Chip label={ci.roomNo} size="small" color="primary" /></TableCell>
                      <TableCell>{ci.guestName}</TableCell>
                      <TableCell>{new Date(ci.dateIN).toLocaleDateString()}</TableCell>
                      <TableCell>{new Date(ci.dateOUT).toLocaleDateString()}</TableCell>
                    </TableRow>
                  )) : (
                    <TableRow>
                      <TableCell colSpan={4} align="center">No current check-ins</TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </Paper>
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <Paper sx={{ p: 2, borderRadius: 2 }}>
            <Typography variant="h6" fontWeight={600} gutterBottom>
              Active Reservations
            </Typography>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell><strong>Room</strong></TableCell>
                    <TableCell><strong>Guest</strong></TableCell>
                    <TableCell><strong>From</strong></TableCell>
                    <TableCell><strong>To</strong></TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {data?.currentReservations?.length ? data.currentReservations.map((r, idx) => (
                    <TableRow key={idx}>
                      <TableCell><Chip label={r.roomNo} size="small" color="warning" /></TableCell>
                      <TableCell>{r.guestName}</TableCell>
                      <TableCell>{new Date(r.dateIN).toLocaleDateString()}</TableCell>
                      <TableCell>{new Date(r.dateOUT).toLocaleDateString()}</TableCell>
                    </TableRow>
                  )) : (
                    <TableRow>
                      <TableCell colSpan={4} align="center">No active reservations</TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default DashboardPage;
