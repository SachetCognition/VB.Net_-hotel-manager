import React, { useEffect, useState } from 'react';
import {
  Box, Typography, Button, TextField, Dialog, DialogTitle, DialogContent,
  DialogActions, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Paper, IconButton, Alert, CircularProgress, Grid,
  FormControl, InputLabel, Select, MenuItem, Chip, Card, CardContent, Divider,
} from '@mui/material';
import { Add, Delete, Search, Calculate } from '@mui/icons-material';
import api from '../../api/client';
import { CheckInRecord, CreateCheckInRequest, Guest, Room, Currency, ExtraBed, TaxCalculationResult } from '../../types';

const CheckInPage: React.FC = () => {
  const [records, setRecords] = useState<CheckInRecord[]>([]);
  const [guests, setGuests] = useState<Guest[]>([]);
  const [rooms, setRooms] = useState<Room[]>([]);
  const [currencies, setCurrencies] = useState<Currency[]>([]);
  const [extraBeds, setExtraBeds] = useState<ExtraBed[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [taxResult, setTaxResult] = useState<TaxCalculationResult | null>(null);
  const [form, setForm] = useState<CreateCheckInRequest>({
    guestID: '', roomNo: '', roomCharges: 0, dateIN: '', dateOUT: '',
    noOfAdults: 1, noOfKids: 0, guestName: '', address: '', city: '',
    contactNo: '', idType: '', idNumber: '', otherCharges: 0, discountPer: 0,
    serviceTaxPer: 0, luxuryTaxPer: 0, totalPaid: 0, extraBed: 'None', currency: '', notes: '',
  });

  const fetchData = async () => {
    try {
      setLoading(true);
      const [checkins, guestsRes, roomsRes, currRes, bedRes] = await Promise.all([
        api.get<CheckInRecord[]>('/CheckIn'),
        api.get<Guest[]>('/Guests'),
        api.get<Room[]>('/CheckIn/room-available'),
        api.get<Currency[]>('/Currency'),
        api.get<ExtraBed[]>('/HotelInfo/extra-bed'),
      ]);
      setRecords(checkins.data);
      setGuests(guestsRes.data);
      setRooms(Array.isArray(roomsRes.data) ? roomsRes.data : []);
      setCurrencies(currRes.data);
      setExtraBeds(Array.isArray(bedRes.data) ? bedRes.data : []);
    } catch {
      setError('Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const handleGuestSelect = (guestID: string) => {
    const guest = guests.find((g) => g.guestID === guestID);
    if (guest) {
      setForm({
        ...form, guestID, guestName: guest.guestName, address: guest.address,
        city: guest.city, contactNo: guest.contactNo, idType: guest.idType, idNumber: guest.idNumber,
      });
    }
  };

  const handleRoomSelect = (roomNo: string) => {
    const room = rooms.find((r) => r.roomNo === roomNo);
    setForm({ ...form, roomNo, roomCharges: room?.roomCharges || 0 });
  };

  const handleCalculateTax = async () => {
    try {
      const res = await api.post<TaxCalculationResult>('/CheckIn/calculate-tax', {
        roomCharges: form.roomCharges, dateIN: form.dateIN, dateOUT: form.dateOUT,
        otherCharges: form.otherCharges, discountPer: form.discountPer,
        serviceTaxPer: form.serviceTaxPer, luxuryTaxPer: form.luxuryTaxPer, totalPaid: form.totalPaid,
      });
      setTaxResult(res.data);
    } catch {
      setError('Failed to calculate tax');
    }
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      await api.post('/CheckIn', form);
      setDialogOpen(false);
      fetchData();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to check in');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Delete this check-in record?')) return;
    try {
      await api.delete(`/CheckIn/${id}`);
      fetchData();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to delete');
    }
  };

  const openNewDialog = () => {
    setForm({
      guestID: '', roomNo: '', roomCharges: 0, dateIN: '', dateOUT: '',
      noOfAdults: 1, noOfKids: 0, guestName: '', address: '', city: '',
      contactNo: '', idType: '', idNumber: '', otherCharges: 0, discountPer: 0,
      serviceTaxPer: 0, luxuryTaxPer: 0, totalPaid: 0, extraBed: 'None', currency: '', notes: '',
    });
    setTaxResult(null);
    setDialogOpen(true);
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight={700} color="#1a237e">Check-In</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={openNewDialog}>New Check-In</Button>
      </Box>

      {error && <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>{error}</Alert>}

      {loading ? <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}><CircularProgress /></Box> : (
        <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
          <Table size="small">
            <TableHead>
              <TableRow sx={{ bgcolor: '#f5f5f5' }}>
                <TableCell><strong>ID</strong></TableCell>
                <TableCell><strong>Guest</strong></TableCell>
                <TableCell><strong>Room</strong></TableCell>
                <TableCell><strong>Check-In</strong></TableCell>
                <TableCell><strong>Check-Out</strong></TableCell>
                <TableCell><strong>Grand Total</strong></TableCell>
                <TableCell><strong>Balance</strong></TableCell>
                <TableCell><strong>Status</strong></TableCell>
                <TableCell align="right"><strong>Actions</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {records.map((r) => (
                <TableRow key={r.id} hover>
                  <TableCell>{r.id}</TableCell>
                  <TableCell>{r.guestName}</TableCell>
                  <TableCell><Chip label={r.roomNo} size="small" color="primary" /></TableCell>
                  <TableCell>{new Date(r.dateIN).toLocaleDateString()}</TableCell>
                  <TableCell>{new Date(r.dateOUT).toLocaleDateString()}</TableCell>
                  <TableCell>${r.grandTotal.toFixed(2)}</TableCell>
                  <TableCell>${r.balance.toFixed(2)}</TableCell>
                  <TableCell><Chip label={r.status} size="small" color={r.status === 'Checked-In' ? 'success' : 'default'} /></TableCell>
                  <TableCell align="right">
                    <IconButton size="small" color="error" onClick={() => handleDelete(r.id)}><Delete fontSize="small" /></IconButton>
                  </TableCell>
                </TableRow>
              ))}
              {records.length === 0 && <TableRow><TableCell colSpan={9} align="center">No check-in records</TableCell></TableRow>}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="lg" fullWidth>
        <DialogTitle>New Check-In</DialogTitle>
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
            <Grid size={{ xs: 12, sm: 3 }}>
              <TextField fullWidth label="Check-In Date" type="date" value={form.dateIN} onChange={(e) => setForm({ ...form, dateIN: e.target.value })} InputLabelProps={{ shrink: true }} />
            </Grid>
            <Grid size={{ xs: 12, sm: 3 }}>
              <TextField fullWidth label="Check-Out Date" type="date" value={form.dateOUT} onChange={(e) => setForm({ ...form, dateOUT: e.target.value })} InputLabelProps={{ shrink: true }} />
            </Grid>
            <Grid size={{ xs: 6, sm: 3 }}>
              <TextField fullWidth label="Adults" type="number" value={form.noOfAdults} onChange={(e) => setForm({ ...form, noOfAdults: parseInt(e.target.value) || 0 })} />
            </Grid>
            <Grid size={{ xs: 6, sm: 3 }}>
              <TextField fullWidth label="Kids" type="number" value={form.noOfKids} onChange={(e) => setForm({ ...form, noOfKids: parseInt(e.target.value) || 0 })} />
            </Grid>

            <Grid size={12}><Divider sx={{ my: 1 }}><Typography variant="body2" color="text.secondary">Pricing</Typography></Divider></Grid>

            <Grid size={{ xs: 12, sm: 3 }}>
              <TextField fullWidth label="Room Charges" type="number" value={form.roomCharges} onChange={(e) => setForm({ ...form, roomCharges: parseFloat(e.target.value) || 0 })} />
            </Grid>
            <Grid size={{ xs: 12, sm: 3 }}>
              <TextField fullWidth label="Other Charges" type="number" value={form.otherCharges} onChange={(e) => setForm({ ...form, otherCharges: parseFloat(e.target.value) || 0 })} />
            </Grid>
            <Grid size={{ xs: 12, sm: 3 }}>
              <TextField fullWidth label="Discount %" type="number" value={form.discountPer} onChange={(e) => setForm({ ...form, discountPer: parseFloat(e.target.value) || 0 })} />
            </Grid>
            <Grid size={{ xs: 12, sm: 3 }}>
              <TextField fullWidth label="Service Tax %" type="number" value={form.serviceTaxPer} onChange={(e) => setForm({ ...form, serviceTaxPer: parseFloat(e.target.value) || 0 })} />
            </Grid>
            <Grid size={{ xs: 12, sm: 3 }}>
              <TextField fullWidth label="Luxury Tax %" type="number" value={form.luxuryTaxPer} onChange={(e) => setForm({ ...form, luxuryTaxPer: parseFloat(e.target.value) || 0 })} />
            </Grid>
            <Grid size={{ xs: 12, sm: 3 }}>
              <TextField fullWidth label="Total Paid" type="number" value={form.totalPaid} onChange={(e) => setForm({ ...form, totalPaid: parseFloat(e.target.value) || 0 })} />
            </Grid>
            <Grid size={{ xs: 12, sm: 3 }}>
              <FormControl fullWidth>
                <InputLabel>Extra Bed</InputLabel>
                <Select value={form.extraBed} label="Extra Bed" onChange={(e) => setForm({ ...form, extraBed: e.target.value })}>
                  <MenuItem value="None">None</MenuItem>
                  {extraBeds.map((b) => <MenuItem key={b.id} value={b.bedType}>{b.bedType} (${b.charges})</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 3 }}>
              <FormControl fullWidth>
                <InputLabel>Currency</InputLabel>
                <Select value={form.currency} label="Currency" onChange={(e) => setForm({ ...form, currency: e.target.value })}>
                  {currencies.map((c) => <MenuItem key={c.id} value={c.currencyName}>{c.currencyName}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>

            <Grid size={12}>
              <Button variant="outlined" startIcon={<Calculate />} onClick={handleCalculateTax}>Calculate Tax</Button>
            </Grid>

            {taxResult && (
              <Grid size={12}>
                <Card variant="outlined" sx={{ bgcolor: '#f5f5f5' }}>
                  <CardContent>
                    <Grid container spacing={2}>
                      <Grid size={{ xs: 6, sm: 3 }}><Typography variant="body2">Days: <strong>{taxResult.noOfDays}</strong></Typography></Grid>
                      <Grid size={{ xs: 6, sm: 3 }}><Typography variant="body2">Room Total: <strong>${taxResult.totalRoomCharges.toFixed(2)}</strong></Typography></Grid>
                      <Grid size={{ xs: 6, sm: 3 }}><Typography variant="body2">Discount: <strong>${taxResult.discount.toFixed(2)}</strong></Typography></Grid>
                      <Grid size={{ xs: 6, sm: 3 }}><Typography variant="body2">Sub Total: <strong>${taxResult.subTotal.toFixed(2)}</strong></Typography></Grid>
                      <Grid size={{ xs: 6, sm: 3 }}><Typography variant="body2">Service Tax: <strong>${taxResult.serviceTaxAmount.toFixed(2)}</strong></Typography></Grid>
                      <Grid size={{ xs: 6, sm: 3 }}><Typography variant="body2">Luxury Tax: <strong>${taxResult.luxuryTaxAmount.toFixed(2)}</strong></Typography></Grid>
                      <Grid size={{ xs: 6, sm: 3 }}><Typography variant="body2" color="primary"><strong>Grand Total: ${taxResult.grandTotal.toFixed(2)}</strong></Typography></Grid>
                      <Grid size={{ xs: 6, sm: 3 }}><Typography variant="body2" color="error"><strong>Balance: ${taxResult.balance.toFixed(2)}</strong></Typography></Grid>
                    </Grid>
                  </CardContent>
                </Card>
              </Grid>
            )}

            <Grid size={12}>
              <TextField fullWidth label="Notes" value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} multiline rows={2} />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSave} disabled={saving}>
            {saving ? <CircularProgress size={20} /> : 'Check In'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default CheckInPage;
