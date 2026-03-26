import React, { useEffect, useState } from 'react';
import {
  Box, Typography, Button, TextField, Dialog, DialogTitle, DialogContent,
  DialogActions, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Paper, IconButton, Alert, CircularProgress, InputAdornment,
  FormControl, InputLabel, Select, MenuItem, Chip, Tabs, Tab,
} from '@mui/material';
import { Add, Edit, Delete, Search } from '@mui/icons-material';
import api from '../../api/client';
import { Room, CreateRoomRequest, UpdateRoomRequest } from '../../types';

const ROOM_TYPES = ['AC', 'Non-AC', 'Deluxe', 'Suite'];

const RoomsPage: React.FC = () => {
  const [rooms, setRooms] = useState<Room[]>([]);
  const [availableRooms, setAvailableRooms] = useState<Room[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<CreateRoomRequest>({ roomNo: '', roomType: 'AC', roomCharges: 0 });
  const [saving, setSaving] = useState(false);
  const [tab, setTab] = useState(0);

  const fetchRooms = async () => {
    try {
      setLoading(true);
      const [allRes, availRes] = await Promise.all([
        api.get<Room[]>('/Rooms'),
        api.get<Room[]>('/Rooms/available'),
      ]);
      setRooms(allRes.data);
      setAvailableRooms(availRes.data);
    } catch {
      setError('Failed to load rooms');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchRooms(); }, []);

  const handleSave = async () => {
    setSaving(true);
    try {
      if (editingId) {
        const updateData: UpdateRoomRequest = { roomType: form.roomType, roomCharges: form.roomCharges };
        await api.put(`/Rooms/${editingId}`, updateData);
      } else {
        await api.post('/Rooms', form);
      }
      setDialogOpen(false);
      setForm({ roomNo: '', roomType: 'AC', roomCharges: 0 });
      setEditingId(null);
      fetchRooms();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to save room');
    } finally {
      setSaving(false);
    }
  };

  const handleEdit = (room: Room) => {
    setEditingId(room.roomNo);
    setForm({ roomNo: room.roomNo, roomType: room.roomType, roomCharges: room.roomCharges });
    setDialogOpen(true);
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Are you sure you want to delete this room?')) return;
    try {
      await api.delete(`/Rooms/${id}`);
      fetchRooms();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to delete room');
    }
  };

  const displayRooms = tab === 0 ? rooms : availableRooms;
  const filtered = displayRooms.filter((r) =>
    r.roomNo.toLowerCase().includes(search.toLowerCase()) ||
    r.roomType.toLowerCase().includes(search.toLowerCase())
  );

  const getRoomTypeColor = (type: string): 'primary' | 'success' | 'warning' | 'error' => {
    const colors: Record<string, 'primary' | 'success' | 'warning' | 'error'> = {
      'AC': 'primary', 'Non-AC': 'success', 'Deluxe': 'warning', 'Suite': 'error',
    };
    return colors[type] || 'primary';
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight={700} color="#1a237e">Room Management</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => { setEditingId(null); setForm({ roomNo: '', roomType: 'AC', roomCharges: 0 }); setDialogOpen(true); }}>
          Add Room
        </Button>
      </Box>

      {error && <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>{error}</Alert>}

      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 2 }}>
        <Tab label={`All Rooms (${rooms.length})`} />
        <Tab label={`Available (${availableRooms.length})`} />
      </Tabs>

      <TextField
        fullWidth placeholder="Search rooms..."
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
                <TableCell><strong>Room No</strong></TableCell>
                <TableCell><strong>Room Type</strong></TableCell>
                <TableCell><strong>Charges</strong></TableCell>
                <TableCell align="right"><strong>Actions</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filtered.map((room) => (
                <TableRow key={room.roomNo} hover>
                  <TableCell><strong>{room.roomNo}</strong></TableCell>
                  <TableCell><Chip label={room.roomType} size="small" color={getRoomTypeColor(room.roomType)} /></TableCell>
                  <TableCell>${room.roomCharges.toFixed(2)}</TableCell>
                  <TableCell align="right">
                    <IconButton size="small" onClick={() => handleEdit(room)}><Edit fontSize="small" /></IconButton>
                    <IconButton size="small" color="error" onClick={() => handleDelete(room.roomNo)}><Delete fontSize="small" /></IconButton>
                  </TableCell>
                </TableRow>
              ))}
              {filtered.length === 0 && (
                <TableRow><TableCell colSpan={4} align="center">No rooms found</TableCell></TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingId ? 'Edit Room' : 'Add Room'}</DialogTitle>
        <DialogContent>
          <TextField fullWidth label="Room No" value={form.roomNo} onChange={(e) => setForm({ ...form, roomNo: e.target.value })} margin="normal" required disabled={!!editingId} />
          <FormControl fullWidth margin="normal">
            <InputLabel>Room Type</InputLabel>
            <Select value={form.roomType} label="Room Type" onChange={(e) => setForm({ ...form, roomType: e.target.value })}>
              {ROOM_TYPES.map((t) => <MenuItem key={t} value={t}>{t}</MenuItem>)}
            </Select>
          </FormControl>
          <TextField fullWidth label="Room Charges" type="number" value={form.roomCharges} onChange={(e) => setForm({ ...form, roomCharges: parseFloat(e.target.value) || 0 })} margin="normal" required />
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

export default RoomsPage;
