import React, { useEffect, useState } from 'react';
import {
  Box, Typography, Button, TextField, Dialog, DialogTitle, DialogContent,
  DialogActions, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Paper, IconButton, Alert, CircularProgress, Grid, Chip,
} from '@mui/material';
import { Add, Edit, Delete } from '@mui/icons-material';
import api from '../../api/client';
import { Schedule, ScheduleRequest } from '../../types';

const emptyForm: ScheduleRequest = {
  subject: '', location: '', startDate: '', endDate: '', description: '', label: '', status: '',
};

const SchedulePage: React.FC = () => {
  const [schedules, setSchedules] = useState<Schedule[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<ScheduleRequest>(emptyForm);
  const [saving, setSaving] = useState(false);

  const fetchData = async () => {
    try {
      setLoading(true);
      const res = await api.get<Schedule[]>('/Schedule');
      setSchedules(res.data);
    } catch {
      setError('Failed to load schedules');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const handleSave = async () => {
    setSaving(true);
    try {
      if (editingId) {
        await api.put(`/Schedule/${editingId}`, form);
      } else {
        await api.post('/Schedule', form);
      }
      setDialogOpen(false);
      setEditingId(null);
      fetchData();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to save schedule');
    } finally {
      setSaving(false);
    }
  };

  const handleEdit = (s: Schedule) => {
    setEditingId(s.id);
    setForm({
      subject: s.subject, location: s.location, startDate: s.startDate.split('T')[0],
      endDate: s.endDate.split('T')[0], description: s.description, label: s.label, status: s.status,
    });
    setDialogOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Delete this schedule?')) return;
    try {
      await api.delete(`/Schedule/${id}`);
      fetchData();
    } catch {
      setError('Failed to delete');
    }
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight={700} color="#1a237e">Schedule Management</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => { setEditingId(null); setForm(emptyForm); setDialogOpen(true); }}>Add Schedule</Button>
      </Box>

      {error && <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>{error}</Alert>}

      {loading ? <CircularProgress /> : (
        <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
          <Table size="small">
            <TableHead>
              <TableRow sx={{ bgcolor: '#f5f5f5' }}>
                <TableCell><strong>ID</strong></TableCell>
                <TableCell><strong>Subject</strong></TableCell>
                <TableCell><strong>Location</strong></TableCell>
                <TableCell><strong>Start</strong></TableCell>
                <TableCell><strong>End</strong></TableCell>
                <TableCell><strong>Status</strong></TableCell>
                <TableCell><strong>Label</strong></TableCell>
                <TableCell align="right"><strong>Actions</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {schedules.map((s) => (
                <TableRow key={s.id} hover>
                  <TableCell>{s.id}</TableCell>
                  <TableCell>{s.subject}</TableCell>
                  <TableCell>{s.location}</TableCell>
                  <TableCell>{new Date(s.startDate).toLocaleDateString()}</TableCell>
                  <TableCell>{new Date(s.endDate).toLocaleDateString()}</TableCell>
                  <TableCell>{s.status && <Chip label={s.status} size="small" />}</TableCell>
                  <TableCell>{s.label}</TableCell>
                  <TableCell align="right">
                    <IconButton size="small" onClick={() => handleEdit(s)}><Edit fontSize="small" /></IconButton>
                    <IconButton size="small" color="error" onClick={() => handleDelete(s.id)}><Delete fontSize="small" /></IconButton>
                  </TableCell>
                </TableRow>
              ))}
              {schedules.length === 0 && <TableRow><TableCell colSpan={8} align="center">No schedules</TableCell></TableRow>}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingId ? 'Edit Schedule' : 'Add Schedule'}</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 0.5 }}>
            <Grid size={12}><TextField fullWidth label="Subject" value={form.subject} onChange={(e) => setForm({ ...form, subject: e.target.value })} required /></Grid>
            <Grid size={12}><TextField fullWidth label="Description" value={form.description || ''} onChange={(e) => setForm({ ...form, description: e.target.value })} multiline rows={2} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="Start Date" type="date" value={form.startDate} onChange={(e) => setForm({ ...form, startDate: e.target.value })} InputLabelProps={{ shrink: true }} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="End Date" type="date" value={form.endDate} onChange={(e) => setForm({ ...form, endDate: e.target.value })} InputLabelProps={{ shrink: true }} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="Location" value={form.location || ''} onChange={(e) => setForm({ ...form, location: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, sm: 3 }}><TextField fullWidth label="Status" value={form.status || ''} onChange={(e) => setForm({ ...form, status: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, sm: 3 }}><TextField fullWidth label="Label" value={form.label || ''} onChange={(e) => setForm({ ...form, label: e.target.value })} /></Grid>
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

export default SchedulePage;
