import React, { useEffect, useState } from 'react';
import {
  Box, Typography, Button, TextField, Dialog, DialogTitle, DialogContent,
  DialogActions, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Paper, IconButton, Alert, CircularProgress, InputAdornment,
  FormControl, InputLabel, Select, MenuItem, Chip, Grid,
} from '@mui/material';
import { Add, Edit, Delete, Search } from '@mui/icons-material';
import api from '../../api/client';
import { Reservation, CreateReservationRequest, Guest, Room, Currency } from '../../types';

const ReservationsPage: React.FC = () => {
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [guests, setGuests] = useState<Guest[]>([]);
  const [rooms, setRooms] = useState<Room[]>([]);
  const [currencies, setCurrencies] = useState<Currency[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<CreateReservationRequest>({
    guestID: '', guestName: '', roomNo: '', roomType: '', roomCharges: 0,
    dateIN: '', dateOUT: '', noOfAdults: 1, noOfKids: 0, currency: '', notes: '',
  });
  const [saving, setSaving] = useState(false);

  const fetchData = async () => {
    try {
      setLoading(true);
      const [resRes, guestRes, roomRes, currRes] = await Promise.all([
        api.get<Reservation[]>('/Reservations'),
        api.get<Guest[]>('/Guests'),
        api.get<Room[]>('/Rooms/available'),
        api.get<Currency[]>('/Currency'),
      ]);
      setReservations(resRes.data);
      setGuests(guestRes.data);
      setRooms(roomRes.data);
      setCurrencies(currRes.data);
    } catch {
      setError('Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const handleGuestSelect = (guestID: string) => {
    const guest = guests.find((g) => g.guestID === guestID);
    setForm({ ...form, guestID, guestName: guest?.guestName || '' });
  };

  const handleRoomSelect = (roomNo: string) => {
    const room = rooms.find((r) => r.roomNo === roomNo);
    setForm({ ...form, roomNo, roomType: room?.roomType || '', roomCharges: room?.roomCharges || 0 });
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      if (editingId) {
        await api.put(`/Reservations/${editingId}`, form);
      } else {
        await api.post('/Reservations', form);
      }
      setDialogOpen(false);
      setEditingId(null);
      fetchData();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to save reservation');
    } finally {
      setSaving(false);
    }
  };

  const handleEdit = (r: Reservation) => {
    setEditingId(r.id);
    setForm({
      guestID: r.guestID, guestName: r.guestName, roomNo: r.roomNo, roomType: r.roomType,
      roomCharges: r.roomCharges, dateIN: r.dateIN.split('T')[0], dateOUT: r.dateOUT.split('T')[0],
      noOfAdults: r.noOfAdults, noOfKids: r.noOfKids, currency: r.currency, notes: r.notes,
    });
    setDialogOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Delete this reservation?')) return;
    try {
      await api.delete(`/Reservations/${id}`);
      fetchData();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to delete');
    }
  };

  const getStatusColor = (status: string): 'success' | 'warning' | 'error' | 'default' => {
    const map: Record<string, 'success' | 'warning' | 'error' | 'default'> = {
      'Reserved': 'warning', 'Confirmed': 'success', 'Cancelled': 'error',
    };
    return map[status] || 'default';
  };

  const filtered = reservations.filter((r) =>
    r.guestName.toLowerCase().includes(search.toLowerCase()) ||
    r.roomNo.toLowerCase().includes(search.toLowerCase()) ||
    r.status.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight={700} color="#1a237e">Reservations</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => {
          setEditingId(null);
          setForm({ guestID: '', guestName: '', roomNo: '', roomType: '', roomCharges: 0, dateIN: '', dateOUT: '', noOfAdults: 1, noOfKids: 0, currency: '', notes: '' });
          setDialogOpen(true);
        }}>New Reservation</Button>
      </Box>

      {error && <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>{error}</Alert>}

      <TextField fullWidth placeholder="Search reservations..." value={search} onChange={(e) => setSearch(e.target.value)}
        InputProps={{ startAdornment: <InputAdornment position="start"><Search /></InputAdornment> }} sx={{ mb: 2 }} />

      {loading ? <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}><CircularProgress /></Box> : (
        <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
          <Table>
            <TableHead>
              <TableRow sx={{ bgcolor: '#f5f5f5' }}>
                <TableCell><strong>ID</strong></TableCell>
                <TableCell><strong>Guest</strong></TableCell>
                <TableCell><strong>Room</strong></TableCell>
                <TableCell><strong>Type</strong></TableCell>
                <TableCell><strong>Check-In</strong></TableCell>
                <TableCell><strong>Check-Out</strong></TableCell>
                <TableCell><strong>Status</strong></TableCell>
                <TableCell align="right"><strong>Actions</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filtered.map((r) => (
                <TableRow key={r.id} hover>
                  <TableCell>{r.id}</TableCell>
                  <TableCell>{r.guestName}</TableCell>
                  <TableCell><Chip label={r.roomNo} size="small" color="primary" /></TableCell>
                  <TableCell>{r.roomType}</TableCell>
                  <TableCell>{new Date(r.dateIN).toLocaleDateString()}</TableCell>
                  <TableCell>{new Date(r.dateOUT).toLocaleDateString()}</TableCell>
                  <TableCell><Chip label={r.status} size="small" color={getStatusColor(r.status)} /></TableCell>
                  <TableCell align="right">
                    <IconButton size="small" onClick={() => handleEdit(r)}><Edit fontSize="small" /></IconButton>
                    <IconButton size="small" color="error" onClick={() => handleDelete(r.id)}><Delete fontSize="small" /></IconButton>
                  </TableCell>
                </TableRow>
              ))}
              {filtered.length === 0 && <TableRow><TableCell colSpan={8} align="center">No reservations found</TableCell></TableRow>}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>{editingId ? 'Edit Reservation' : 'New Reservation'}</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 0.5 }}>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth>
                <InputLabel>Guest</InputLabel>
                <Select value={form.guestID} label="Guest" onChange={(e) => handleGuestSelect(e.target.value)}>
                  {guests.map((g) => <MenuItem key={g.guestID} value={g.guestID}>{g.guestName} ({g.guestID})</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth>
                <InputLabel>Room</InputLabel>
                <Select value={form.roomNo} label="Room" onChange={(e) => handleRoomSelect(e.target.value)}>
                  {rooms.map((r) => <MenuItem key={r.roomNo} value={r.roomNo}>{r.roomNo} - {r.roomType} (${r.roomCharges})</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField fullWidth label="Check-In Date" type="date" value={form.dateIN} onChange={(e) => setForm({ ...form, dateIN: e.target.value })} InputLabelProps={{ shrink: true }} />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField fullWidth label="Check-Out Date" type="date" value={form.dateOUT} onChange={(e) => setForm({ ...form, dateOUT: e.target.value })} InputLabelProps={{ shrink: true }} />
            </Grid>
            <Grid size={{ xs: 6, sm: 3 }}>
              <TextField fullWidth label="Adults" type="number" value={form.noOfAdults} onChange={(e) => setForm({ ...form, noOfAdults: parseInt(e.target.value) || 0 })} />
            </Grid>
            <Grid size={{ xs: 6, sm: 3 }}>
              <TextField fullWidth label="Kids" type="number" value={form.noOfKids} onChange={(e) => setForm({ ...form, noOfKids: parseInt(e.target.value) || 0 })} />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth>
                <InputLabel>Currency</InputLabel>
                <Select value={form.currency} label="Currency" onChange={(e) => setForm({ ...form, currency: e.target.value })}>
                  {currencies.map((c) => <MenuItem key={c.id} value={c.currencyName}>{c.currencyName} ({c.symbol})</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12 }}>
              <TextField fullWidth label="Notes" value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} multiline rows={2} />
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

export default ReservationsPage;
