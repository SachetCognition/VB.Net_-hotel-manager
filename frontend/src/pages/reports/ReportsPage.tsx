import React, { useState } from 'react';
import {
  Box, Typography, Button, Grid, Card, CardContent, CardActions,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Paper, Alert, CircularProgress, Dialog, DialogTitle, DialogContent, DialogActions,
} from '@mui/material';
import {
  People, Person, Login, Logout, BookOnline, Receipt, EventNote,
  AttachMoney, Inventory2, Assessment,
} from '@mui/icons-material';
import api from '../../api/client';

interface ReportConfig {
  title: string;
  icon: React.ReactElement;
  endpoint: string;
}

const reports: ReportConfig[] = [
  { title: 'Guest Report', icon: <People />, endpoint: '/Reports/guests' },
  { title: 'Employee Report', icon: <Person />, endpoint: '/Reports/employees' },
  { title: 'Check-In Report', icon: <Login />, endpoint: '/Reports/checkin' },
  { title: 'Check-Out Report', icon: <Logout />, endpoint: '/Reports/checkout' },
  { title: 'Reservation Report', icon: <BookOnline />, endpoint: '/Reports/reservations' },
  { title: 'Transaction Report', icon: <Receipt />, endpoint: '/Reports/transactions' },
  { title: 'Attendance Report', icon: <EventNote />, endpoint: '/Reports/attendance' },
  { title: 'Overtime Report', icon: <EventNote />, endpoint: '/Reports/overtime' },
  { title: 'Advance Payment Report', icon: <AttachMoney />, endpoint: '/Reports/advance-payment' },
  { title: 'Employee Payment Report', icon: <AttachMoney />, endpoint: '/Reports/employee-payment' },
  { title: 'Salary Slips', icon: <AttachMoney />, endpoint: '/Reports/salary-slips' },
  { title: 'Purchased Inventory', icon: <Inventory2 />, endpoint: '/Reports/purchased-inventory' },
];

const ReportsPage: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [reportData, setReportData] = useState<Record<string, unknown>[] | null>(null);
  const [reportTitle, setReportTitle] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);

  const fetchReport = async (report: ReportConfig) => {
    setLoading(true);
    setError('');
    try {
      const res = await api.get(report.endpoint);
      const data = Array.isArray(res.data) ? res.data : [res.data];
      setReportData(data);
      setReportTitle(report.title);
      setDialogOpen(true);
    } catch {
      setError(`Failed to load ${report.title}`);
    } finally {
      setLoading(false);
    }
  };

  const getColumns = (): string[] => {
    if (!reportData || reportData.length === 0) return [];
    return Object.keys(reportData[0]);
  };

  const formatValue = (val: unknown): string => {
    if (val === null || val === undefined) return '-';
    if (typeof val === 'number') return val.toFixed(2);
    if (typeof val === 'string' && val.match(/^\d{4}-\d{2}-\d{2}/)) {
      return new Date(val).toLocaleDateString();
    }
    return String(val);
  };

  return (
    <Box>
      <Typography variant="h4" fontWeight={700} color="#1a237e" gutterBottom>Reports</Typography>

      {error && <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>{error}</Alert>}
      {loading && <Box sx={{ display: 'flex', justifyContent: 'center', mb: 2 }}><CircularProgress /></Box>}

      <Grid container spacing={3}>
        {reports.map((report) => (
          <Grid size={{ xs: 12, sm: 6, md: 4 }} key={report.endpoint}>
            <Card sx={{ borderRadius: 2, '&:hover': { boxShadow: 4 }, transition: 'box-shadow 0.2s' }}>
              <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Box sx={{ p: 1.5, borderRadius: 2, bgcolor: '#e8eaf6', color: '#1a237e' }}>
                  {report.icon}
                </Box>
                <Typography variant="h6" fontSize={16} fontWeight={600}>{report.title}</Typography>
              </CardContent>
              <CardActions>
                <Button size="small" onClick={() => fetchReport(report)} disabled={loading}>
                  View Report
                </Button>
              </CardActions>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="lg" fullWidth>
        <DialogTitle>{reportTitle}</DialogTitle>
        <DialogContent>
          {reportData && reportData.length > 0 ? (
            <TableContainer component={Paper} sx={{ maxHeight: 500 }}>
              <Table size="small" stickyHeader>
                <TableHead>
                  <TableRow>
                    {getColumns().map((col) => (
                      <TableCell key={col} sx={{ fontWeight: 700, bgcolor: '#f5f5f5' }}>
                        {col.replace(/([A-Z])/g, ' $1').replace(/^./, (s) => s.toUpperCase())}
                      </TableCell>
                    ))}
                  </TableRow>
                </TableHead>
                <TableBody>
                  {reportData.map((row, idx) => (
                    <TableRow key={idx} hover>
                      {getColumns().map((col) => (
                        <TableCell key={col}>{formatValue(row[col])}</TableCell>
                      ))}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          ) : (
            <Typography color="text.secondary" align="center" sx={{ py: 4 }}>No data available</Typography>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => window.print()}>Print</Button>
          <Button onClick={() => setDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ReportsPage;
