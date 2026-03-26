import React, { useEffect, useState } from 'react';
import {
  Box, Typography, Button, TextField, Dialog, DialogTitle, DialogContent,
  DialogActions, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Paper, IconButton, Alert, CircularProgress, Grid,
  FormControl, InputLabel, Select, MenuItem,
} from '@mui/material';
import { Add, Delete } from '@mui/icons-material';
import api from '../../api/client';
import { Transaction, CreateTransactionRequest, Guest, Currency } from '../../types';

const TRANSACTION_TYPES = ['Payment', 'Refund', 'Deposit', 'Charge', 'Other'];

const TransactionsPage: React.FC = () => {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [guests, setGuests] = useState<Guest[]>([]);
  const [currencies, setCurrencies] = useState<Currency[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState<CreateTransactionRequest>({
    guestID: '', guestName: '', transactionType: '', amount: 0, description: '', currency: '', notes: '',
  });

  const fetchData = async () => {
    try {
      setLoading(true);
      const [tRes, gRes, cRes] = await Promise.all([
        api.get<Transaction[]>('/Transactions'),
        api.get<Guest[]>('/Guests'),
        api.get<Currency[]>('/Currency'),
      ]);
      setTransactions(tRes.data);
      setGuests(gRes.data);
      setCurrencies(cRes.data);
    } catch {
      setError('Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const handleSave = async () => {
    setSaving(true);
    try {
      await api.post('/Transactions', form);
      setDialogOpen(false);
      fetchData();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to save');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Delete this transaction?')) return;
    try {
      await api.delete(`/Transactions/${id}`);
      fetchData();
    } catch {
      setError('Failed to delete');
    }
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight={700} color="#1a237e">Transactions</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => {
          setForm({ guestID: '', guestName: '', transactionType: '', amount: 0, description: '', currency: '', notes: '' });
          setDialogOpen(true);
        }}>Add Transaction</Button>
      </Box>

      {error && <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>{error}</Alert>}

      {loading ? <CircularProgress /> : (
        <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
          <Table size="small">
            <TableHead><TableRow sx={{ bgcolor: '#f5f5f5' }}>
              <TableCell><strong>ID</strong></TableCell><TableCell><strong>Guest</strong></TableCell>
              <TableCell><strong>Type</strong></TableCell><TableCell><strong>Amount</strong></TableCell>
              <TableCell><strong>Date</strong></TableCell><TableCell><strong>Description</strong></TableCell>
              <TableCell><strong>Currency</strong></TableCell><TableCell align="right"><strong>Actions</strong></TableCell>
            </TableRow></TableHead>
            <TableBody>
              {transactions.map((t) => (
                <TableRow key={t.id} hover>
                  <TableCell>{t.id}</TableCell><TableCell>{t.guestName}</TableCell>
                  <TableCell>{t.transactionType}</TableCell><TableCell>${t.amount.toFixed(2)}</TableCell>
                  <TableCell>{new Date(t.transactionDate).toLocaleDateString()}</TableCell>
                  <TableCell>{t.description}</TableCell><TableCell>{t.currency}</TableCell>
                  <TableCell align="right"><IconButton size="small" color="error" onClick={() => handleDelete(t.id)}><Delete fontSize="small" /></IconButton></TableCell>
                </TableRow>
              ))}
              {transactions.length === 0 && <TableRow><TableCell colSpan={8} align="center">No transactions</TableCell></TableRow>}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Add Transaction</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 0.5 }}>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth><InputLabel>Guest</InputLabel>
                <Select value={form.guestID} label="Guest" onChange={(e) => {
                  const g = guests.find((g) => g.guestID === e.target.value);
                  setForm({ ...form, guestID: e.target.value, guestName: g?.guestName || '' });
                }}>
                  {guests.map((g) => <MenuItem key={g.guestID} value={g.guestID}>{g.guestName}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth><InputLabel>Type</InputLabel>
                <Select value={form.transactionType} label="Type" onChange={(e) => setForm({ ...form, transactionType: e.target.value })}>
                  {TRANSACTION_TYPES.map((t) => <MenuItem key={t} value={t}>{t}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField fullWidth label="Amount" type="number" value={form.amount} onChange={(e) => setForm({ ...form, amount: parseFloat(e.target.value) || 0 })} />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth><InputLabel>Currency</InputLabel>
                <Select value={form.currency} label="Currency" onChange={(e) => setForm({ ...form, currency: e.target.value })}>
                  {currencies.map((c) => <MenuItem key={c.id} value={c.currencyName}>{c.currencyName}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={12}><TextField fullWidth label="Description" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} /></Grid>
            <Grid size={12}><TextField fullWidth label="Notes" value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} multiline rows={2} /></Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSave} disabled={saving}>{saving ? <CircularProgress size={20} /> : 'Save'}</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default TransactionsPage;
