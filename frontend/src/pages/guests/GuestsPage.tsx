import React, { useEffect, useState } from 'react';
import {
  Box, Typography, Button, TextField, Dialog, DialogTitle, DialogContent,
  DialogActions, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Paper, IconButton, Alert, CircularProgress, InputAdornment,
  FormControl, InputLabel, Select, MenuItem,
} from '@mui/material';
import { Add, Edit, Delete, Search, FileDownload } from '@mui/icons-material';
import api from '../../api/client';
import { Guest, CreateGuestRequest } from '../../types';

const ID_TYPES = ['Passport', 'Driver License', 'National ID', 'Aadhar Card', 'Other'];

const emptyGuest: CreateGuestRequest = {
  guestName: '', address: '', city: '', contactNo: '', idType: '', idNumber: '', notes: '',
};

const GuestsPage: React.FC = () => {
  const [guests, setGuests] = useState<Guest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<CreateGuestRequest>(emptyGuest);
  const [saving, setSaving] = useState(false);

  const fetchGuests = async () => {
    try {
      setLoading(true);
      const res = await api.get<Guest[]>('/Guests');
      setGuests(res.data);
    } catch {
      setError('Failed to load guests');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchGuests(); }, []);

  const handleSave = async () => {
    setSaving(true);
    try {
      if (editingId) {
        await api.put(`/Guests/${editingId}`, form);
      } else {
        await api.post('/Guests', form);
      }
      setDialogOpen(false);
      setForm(emptyGuest);
      setEditingId(null);
      fetchGuests();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to save guest');
    } finally {
      setSaving(false);
    }
  };

  const handleEdit = (guest: Guest) => {
    setEditingId(guest.guestID);
    setForm({
      guestName: guest.guestName, address: guest.address, city: guest.city,
      contactNo: guest.contactNo, idType: guest.idType, idNumber: guest.idNumber, notes: guest.notes,
    });
    setDialogOpen(true);
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Are you sure you want to delete this guest?')) return;
    try {
      await api.delete(`/Guests/${id}`);
      fetchGuests();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to delete guest');
    }
  };

  const handleExport = async () => {
    try {
      const res = await api.get('/Guests/export', { responseType: 'blob' });
      const url = window.URL.createObjectURL(new Blob([res.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', 'guests.xlsx');
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch {
      setError('Failed to export guests');
    }
  };

  const filtered = guests.filter((g) =>
    g.guestName.toLowerCase().includes(search.toLowerCase()) ||
    g.guestID.toLowerCase().includes(search.toLowerCase()) ||
    g.city.toLowerCase().includes(search.toLowerCase()) ||
    g.contactNo.includes(search)
  );

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight={700} color="#1a237e">Guest Management</Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button variant="outlined" startIcon={<FileDownload />} onClick={handleExport}>Export Excel</Button>
          <Button variant="contained" startIcon={<Add />} onClick={() => { setEditingId(null); setForm(emptyGuest); setDialogOpen(true); }}>
            Add Guest
          </Button>
        </Box>
      </Box>

      {error && <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>{error}</Alert>}

      <TextField
        fullWidth placeholder="Search guests by name, ID, city, or contact..."
        value={search} onChange={(e) => setSearch(e.target.value)}
        InputProps={{ startAdornment: <InputAdornment position="start"><Search /></InputAdornment> }}
        sx={{ mb: 2 }}
      />

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}><CircularProgress /></Box>
      ) : (
        <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
          <Table>
            <TableHead>
              <TableRow sx={{ bgcolor: '#f5f5f5' }}>
                <TableCell><strong>Guest ID</strong></TableCell>
                <TableCell><strong>Name</strong></TableCell>
                <TableCell><strong>City</strong></TableCell>
                <TableCell><strong>Contact</strong></TableCell>
                <TableCell><strong>ID Type</strong></TableCell>
                <TableCell><strong>ID Number</strong></TableCell>
                <TableCell align="right"><strong>Actions</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filtered.map((guest) => (
                <TableRow key={guest.guestID} hover>
                  <TableCell>{guest.guestID}</TableCell>
                  <TableCell>{guest.guestName}</TableCell>
                  <TableCell>{guest.city}</TableCell>
                  <TableCell>{guest.contactNo}</TableCell>
                  <TableCell>{guest.idType}</TableCell>
                  <TableCell>{guest.idNumber}</TableCell>
                  <TableCell align="right">
                    <IconButton size="small" onClick={() => handleEdit(guest)}><Edit fontSize="small" /></IconButton>
                    <IconButton size="small" color="error" onClick={() => handleDelete(guest.guestID)}><Delete fontSize="small" /></IconButton>
                  </TableCell>
                </TableRow>
              ))}
              {filtered.length === 0 && (
                <TableRow><TableCell colSpan={7} align="center">No guests found</TableCell></TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingId ? 'Edit Guest' : 'Add Guest'}</DialogTitle>
        <DialogContent>
          <TextField fullWidth label="Guest Name" value={form.guestName} onChange={(e) => setForm({ ...form, guestName: e.target.value })} margin="normal" required />
          <TextField fullWidth label="Address" value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} margin="normal" multiline rows={2} />
          <TextField fullWidth label="City" value={form.city} onChange={(e) => setForm({ ...form, city: e.target.value })} margin="normal" />
          <TextField fullWidth label="Contact No" value={form.contactNo} onChange={(e) => setForm({ ...form, contactNo: e.target.value })} margin="normal" />
          <FormControl fullWidth margin="normal">
            <InputLabel>ID Type</InputLabel>
            <Select value={form.idType} label="ID Type" onChange={(e) => setForm({ ...form, idType: e.target.value })}>
              {ID_TYPES.map((t) => <MenuItem key={t} value={t}>{t}</MenuItem>)}
            </Select>
          </FormControl>
          <TextField fullWidth label="ID Number" value={form.idNumber} onChange={(e) => setForm({ ...form, idNumber: e.target.value })} margin="normal" />
          <TextField fullWidth label="Notes" value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} margin="normal" multiline rows={2} />
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

export default GuestsPage;
