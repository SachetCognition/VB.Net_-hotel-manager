import React, { useEffect, useState } from 'react';
import {
  Box, Typography, Button, TextField, Dialog, DialogTitle, DialogContent,
  DialogActions, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Paper, IconButton, Alert, CircularProgress, Grid,
  FormControl, InputLabel, Select, MenuItem, Chip, Card, CardContent,
} from '@mui/material';
import { Add, Delete } from '@mui/icons-material';
import api from '../../api/client';
import { CheckOutRecord, CheckInRecord, CheckOutRequest, Currency } from '../../types';

const CheckOutPage: React.FC = () => {
  const [records, setRecords] = useState<CheckOutRecord[]>([]);
  const [checkins, setCheckins] = useState<CheckInRecord[]>([]);
  const [currencies, setCurrencies] = useState<Currency[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [selectedCheckIn, setSelectedCheckIn] = useState<CheckInRecord | null>(null);
  const [form, setForm] = useState<CheckOutRequest>({ checkInId: 0, totalPaid: 0, currency: '' });

  const fetchData = async () => {
    try {
      setLoading(true);
      const [coRes, ciRes, currRes] = await Promise.all([
        api.get<CheckOutRecord[]>('/CheckOut'),
        api.get<CheckInRecord[]>('/CheckIn'),
        api.get<Currency[]>('/Currency'),
      ]);
      setRecords(coRes.data);
      setCheckins(ciRes.data.filter((ci) => ci.status === 'Checked-In'));
      setCurrencies(currRes.data);
    } catch {
      setError('Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const handleCheckInSelect = (id: number) => {
    const ci = checkins.find((c) => c.id === id);
    setSelectedCheckIn(ci || null);
    setForm({ checkInId: id, totalPaid: ci?.balance || 0, currency: ci?.currency || '' });
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      await api.post('/CheckOut', form);
      setDialogOpen(false);
      fetchData();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to check out');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Delete this check-out record?')) return;
    try {
      await api.delete(`/CheckOut/${id}`);
      fetchData();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to delete');
    }
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight={700} color="#1a237e">Check-Out</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => {
          setSelectedCheckIn(null);
          setForm({ checkInId: 0, totalPaid: 0, currency: '' });
          setDialogOpen(true);
        }}>New Check-Out</Button>
      </Box>

      {error && <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>{error}</Alert>}

      {loading ? <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}><CircularProgress /></Box> : (
        <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
          <Table size="small">
            <TableHead>
              <TableRow sx={{ bgcolor: '#f5f5f5' }}>
                <TableCell><strong>Bill No</strong></TableCell>
                <TableCell><strong>Room</strong></TableCell>
                <TableCell><strong>Guest</strong></TableCell>
                <TableCell><strong>Check-In</strong></TableCell>
                <TableCell><strong>Check-Out</strong></TableCell>
                <TableCell><strong>Grand Total</strong></TableCell>
                <TableCell><strong>Paid</strong></TableCell>
                <TableCell><strong>Balance</strong></TableCell>
                <TableCell align="right"><strong>Actions</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {records.map((r) => (
                <TableRow key={r.id} hover>
                  <TableCell>{r.billNo}</TableCell>
                  <TableCell><Chip label={r.roomNo} size="small" color="primary" /></TableCell>
                  <TableCell>{r.guestID}</TableCell>
                  <TableCell>{new Date(r.dateIN).toLocaleDateString()}</TableCell>
                  <TableCell>{new Date(r.checkOutDate).toLocaleDateString()}</TableCell>
                  <TableCell>${r.grandTotal.toFixed(2)}</TableCell>
                  <TableCell>${r.totalPaid.toFixed(2)}</TableCell>
                  <TableCell>${r.balance.toFixed(2)}</TableCell>
                  <TableCell align="right">
                    <IconButton size="small" color="error" onClick={() => handleDelete(r.id)}><Delete fontSize="small" /></IconButton>
                  </TableCell>
                </TableRow>
              ))}
              {records.length === 0 && <TableRow><TableCell colSpan={9} align="center">No check-out records</TableCell></TableRow>}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>New Check-Out</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 0.5 }}>
            <Grid size={12}>
              <FormControl fullWidth>
                <InputLabel>Select Checked-In Room</InputLabel>
                <Select value={form.checkInId || ''} label="Select Checked-In Room" onChange={(e) => handleCheckInSelect(Number(e.target.value))}>
                  {checkins.map((ci) => (
                    <MenuItem key={ci.id} value={ci.id}>
                      Room {ci.roomNo} - {ci.guestName} (Balance: ${ci.balance.toFixed(2)})
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            {selectedCheckIn && (
              <Grid size={12}>
                <Card variant="outlined" sx={{ bgcolor: '#f5f5f5' }}>
                  <CardContent>
                    <Typography variant="h6" gutterBottom>Bill Summary</Typography>
                    <Grid container spacing={2}>
                      <Grid size={{ xs: 6, sm: 4 }}><Typography variant="body2">Room: <strong>{selectedCheckIn.roomNo}</strong></Typography></Grid>
                      <Grid size={{ xs: 6, sm: 4 }}><Typography variant="body2">Guest: <strong>{selectedCheckIn.guestName}</strong></Typography></Grid>
                      <Grid size={{ xs: 6, sm: 4 }}><Typography variant="body2">Days: <strong>{selectedCheckIn.noOfDays}</strong></Typography></Grid>
                      <Grid size={{ xs: 6, sm: 4 }}><Typography variant="body2">Room Charges: <strong>${selectedCheckIn.totalRoomCharges.toFixed(2)}</strong></Typography></Grid>
                      <Grid size={{ xs: 6, sm: 4 }}><Typography variant="body2">Service Tax: <strong>${selectedCheckIn.serviceTaxAmount.toFixed(2)}</strong></Typography></Grid>
                      <Grid size={{ xs: 6, sm: 4 }}><Typography variant="body2">Luxury Tax: <strong>${selectedCheckIn.luxuryTaxAmount.toFixed(2)}</strong></Typography></Grid>
                      <Grid size={{ xs: 6, sm: 4 }}><Typography variant="body2" color="primary"><strong>Grand Total: ${selectedCheckIn.grandTotal.toFixed(2)}</strong></Typography></Grid>
                      <Grid size={{ xs: 6, sm: 4 }}><Typography variant="body2">Already Paid: <strong>${selectedCheckIn.totalPaid.toFixed(2)}</strong></Typography></Grid>
                      <Grid size={{ xs: 6, sm: 4 }}><Typography variant="body2" color="error"><strong>Balance: ${selectedCheckIn.balance.toFixed(2)}</strong></Typography></Grid>
                    </Grid>
                  </CardContent>
                </Card>
              </Grid>
            )}

            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField fullWidth label="Amount Paid" type="number" value={form.totalPaid} onChange={(e) => setForm({ ...form, totalPaid: parseFloat(e.target.value) || 0 })} />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth>
                <InputLabel>Currency</InputLabel>
                <Select value={form.currency} label="Currency" onChange={(e) => setForm({ ...form, currency: e.target.value })}>
                  {currencies.map((c) => <MenuItem key={c.id} value={c.currencyName}>{c.currencyName}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSave} disabled={saving}>
            {saving ? <CircularProgress size={20} /> : 'Check Out'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default CheckOutPage;
