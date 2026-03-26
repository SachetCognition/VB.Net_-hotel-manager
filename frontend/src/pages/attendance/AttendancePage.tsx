import React, { useEffect, useState } from 'react';
import {
  Box, Typography, Button, TextField, Dialog, DialogTitle, DialogContent,
  DialogActions, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Paper, IconButton, Alert, CircularProgress, Grid,
  FormControl, InputLabel, Select, MenuItem, Chip,
} from '@mui/material';
import { Add, Delete } from '@mui/icons-material';
import api from '../../api/client';
import { Attendance, AttendanceRequest, Employee } from '../../types';

const STATUSES = ['Present', 'Absent', 'Half Day', 'Leave'];

const AttendancePage: React.FC = () => {
  const [records, setRecords] = useState<Attendance[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState<AttendanceRequest>({
    employeeID: '', employeeName: '', workingDate: '', status: 'Present', overtime: '0', department: '',
  });

  const fetchData = async () => {
    try {
      setLoading(true);
      const [attRes, empRes] = await Promise.all([
        api.get<Attendance[]>('/Attendance'),
        api.get<Employee[]>('/Employees'),
      ]);
      setRecords(attRes.data);
      setEmployees(empRes.data);
    } catch {
      setError('Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const handleEmployeeSelect = (employeeID: string) => {
    const emp = employees.find((e) => e.employeeID === employeeID);
    setForm({ ...form, employeeID, employeeName: emp?.employeeName || '', department: emp?.department || '' });
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      await api.post('/Attendance', form);
      setDialogOpen(false);
      fetchData();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to save attendance');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Delete this record?')) return;
    try {
      await api.delete(`/Attendance/${id}`);
      fetchData();
    } catch {
      setError('Failed to delete');
    }
  };

  const getStatusColor = (status: string): 'success' | 'error' | 'warning' | 'default' => {
    const map: Record<string, 'success' | 'error' | 'warning' | 'default'> = {
      'Present': 'success', 'Absent': 'error', 'Half Day': 'warning', 'Leave': 'default',
    };
    return map[status] || 'default';
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight={700} color="#1a237e">Attendance</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => {
          setForm({ employeeID: '', employeeName: '', workingDate: '', status: 'Present', overtime: '0', department: '' });
          setDialogOpen(true);
        }}>Record Attendance</Button>
      </Box>

      {error && <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>{error}</Alert>}

      {loading ? <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}><CircularProgress /></Box> : (
        <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
          <Table size="small">
            <TableHead>
              <TableRow sx={{ bgcolor: '#f5f5f5' }}>
                <TableCell><strong>ID</strong></TableCell>
                <TableCell><strong>Employee</strong></TableCell>
                <TableCell><strong>Department</strong></TableCell>
                <TableCell><strong>Date</strong></TableCell>
                <TableCell><strong>Status</strong></TableCell>
                <TableCell><strong>Overtime</strong></TableCell>
                <TableCell align="right"><strong>Actions</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {records.map((r) => (
                <TableRow key={r.id} hover>
                  <TableCell>{r.id}</TableCell>
                  <TableCell>{r.employeeName}</TableCell>
                  <TableCell>{r.department}</TableCell>
                  <TableCell>{new Date(r.workingDate).toLocaleDateString()}</TableCell>
                  <TableCell><Chip label={r.status} size="small" color={getStatusColor(r.status)} /></TableCell>
                  <TableCell>{r.overtime}</TableCell>
                  <TableCell align="right">
                    <IconButton size="small" color="error" onClick={() => handleDelete(r.id)}><Delete fontSize="small" /></IconButton>
                  </TableCell>
                </TableRow>
              ))}
              {records.length === 0 && <TableRow><TableCell colSpan={7} align="center">No records</TableCell></TableRow>}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Record Attendance</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 0.5 }}>
            <Grid size={12}>
              <FormControl fullWidth>
                <InputLabel>Employee</InputLabel>
                <Select value={form.employeeID} label="Employee" onChange={(e) => handleEmployeeSelect(e.target.value)}>
                  {employees.map((emp) => <MenuItem key={emp.employeeID} value={emp.employeeID}>{emp.employeeName} ({emp.employeeID})</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField fullWidth label="Date" type="date" value={form.workingDate} onChange={(e) => setForm({ ...form, workingDate: e.target.value })} InputLabelProps={{ shrink: true }} />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth>
                <InputLabel>Status</InputLabel>
                <Select value={form.status} label="Status" onChange={(e) => setForm({ ...form, status: e.target.value })}>
                  {STATUSES.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={12}>
              <TextField fullWidth label="Overtime (hours)" value={form.overtime} onChange={(e) => setForm({ ...form, overtime: e.target.value })} />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSave} disabled={saving}>
            {saving ? <CircularProgress size={20} /> : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default AttendancePage;
