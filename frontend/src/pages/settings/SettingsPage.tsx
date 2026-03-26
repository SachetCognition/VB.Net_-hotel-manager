import React, { useEffect, useState } from 'react';
import {
  Box, Typography, Button, TextField, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, Paper, IconButton, Alert,
  CircularProgress, Grid, Tabs, Tab, Dialog, DialogTitle, DialogContent, DialogActions,
} from '@mui/material';
import { Add, Delete, Save } from '@mui/icons-material';
import api from '../../api/client';
import { HotelInfo, HotelInfoRequest, TaxInfo, TaxInfoRequest, ExtraBed, ExtraBedRequest, Currency, CurrencyRequest } from '../../types';

const SettingsPage: React.FC = () => {
  const [tab, setTab] = useState(0);
  const [hotelInfo, setHotelInfo] = useState<HotelInfo | null>(null);
  const [taxes, setTaxes] = useState<TaxInfo[]>([]);
  const [extraBeds, setExtraBeds] = useState<ExtraBed[]>([]);
  const [currencies, setCurrencies] = useState<Currency[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);
  const [hotelForm, setHotelForm] = useState<HotelInfoRequest>({
    hotelName: '', address: '', city: '', state: '', zipCode: '', phone: '', email: '', website: '', tin: '', serviceTaxNo: '',
  });

  const [taxDialogOpen, setTaxDialogOpen] = useState(false);
  const [taxForm, setTaxForm] = useState<TaxInfoRequest>({ taxName: '', taxPercentage: 0, description: '' });
  const [bedDialogOpen, setBedDialogOpen] = useState(false);
  const [bedForm, setBedForm] = useState<ExtraBedRequest>({ bedType: '', charges: 0 });
  const [currDialogOpen, setCurrDialogOpen] = useState(false);
  const [currForm, setCurrForm] = useState<CurrencyRequest>({ currencyName: '', symbol: '' });

  const fetchData = async () => {
    try {
      setLoading(true);
      const [hiRes, taxRes, bedRes, currRes] = await Promise.all([
        api.get<HotelInfo>('/HotelInfo').catch(() => ({ data: null })),
        api.get<TaxInfo[]>('/HotelInfo/tax'),
        api.get<ExtraBed[]>('/HotelInfo/extra-bed'),
        api.get<Currency[]>('/Currency'),
      ]);
      if (hiRes.data) {
        setHotelInfo(hiRes.data);
        setHotelForm({
          hotelName: hiRes.data.hotelName || '', address: hiRes.data.address || '',
          city: hiRes.data.city || '', state: hiRes.data.state || '',
          zipCode: hiRes.data.zipCode || '', phone: hiRes.data.phone || '',
          email: hiRes.data.email || '', website: hiRes.data.website || '',
          tin: hiRes.data.tin || '', serviceTaxNo: hiRes.data.serviceTaxNo || '',
        });
      }
      setTaxes(taxRes.data);
      setExtraBeds(Array.isArray(bedRes.data) ? bedRes.data : []);
      setCurrencies(currRes.data);
    } catch {
      setError('Failed to load settings');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const saveHotelInfo = async () => {
    setSaving(true);
    try {
      await api.put('/HotelInfo', hotelForm);
      setError('');
      fetchData();
    } catch {
      setError('Failed to save hotel info');
    } finally {
      setSaving(false);
    }
  };

  const saveTax = async () => {
    try {
      await api.post('/HotelInfo/tax', taxForm);
      setTaxDialogOpen(false);
      fetchData();
    } catch {
      setError('Failed to save tax');
    }
  };

  const saveBed = async () => {
    try {
      await api.post('/HotelInfo/extra-bed', bedForm);
      setBedDialogOpen(false);
      fetchData();
    } catch {
      setError('Failed to save extra bed');
    }
  };

  const saveCurrency = async () => {
    try {
      await api.post('/Currency', currForm);
      setCurrDialogOpen(false);
      fetchData();
    } catch {
      setError('Failed to save currency');
    }
  };

  const deleteCurrency = async (id: number) => {
    if (!window.confirm('Delete this currency?')) return;
    try {
      await api.delete(`/Currency/${id}`);
      fetchData();
    } catch {
      setError('Failed to delete');
    }
  };

  if (loading) return <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}><CircularProgress /></Box>;

  return (
    <Box>
      <Typography variant="h4" fontWeight={700} color="#1a237e" gutterBottom>Hotel Settings</Typography>

      {error && <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>{error}</Alert>}

      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 2 }}>
        <Tab label="Hotel Profile" />
        <Tab label="Tax Information" />
        <Tab label="Extra Beds" />
        <Tab label="Currencies" />
      </Tabs>

      {tab === 0 && (
        <Paper sx={{ p: 3, borderRadius: 2 }}>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="Hotel Name" value={hotelForm.hotelName} onChange={(e) => setHotelForm({ ...hotelForm, hotelName: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="Phone" value={hotelForm.phone} onChange={(e) => setHotelForm({ ...hotelForm, phone: e.target.value })} /></Grid>
            <Grid size={12}><TextField fullWidth label="Address" value={hotelForm.address} onChange={(e) => setHotelForm({ ...hotelForm, address: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, sm: 4 }}><TextField fullWidth label="City" value={hotelForm.city} onChange={(e) => setHotelForm({ ...hotelForm, city: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, sm: 4 }}><TextField fullWidth label="State" value={hotelForm.state} onChange={(e) => setHotelForm({ ...hotelForm, state: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, sm: 4 }}><TextField fullWidth label="Zip Code" value={hotelForm.zipCode} onChange={(e) => setHotelForm({ ...hotelForm, zipCode: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="Email" value={hotelForm.email} onChange={(e) => setHotelForm({ ...hotelForm, email: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="Website" value={hotelForm.website} onChange={(e) => setHotelForm({ ...hotelForm, website: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="TIN" value={hotelForm.tin} onChange={(e) => setHotelForm({ ...hotelForm, tin: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="Service Tax No" value={hotelForm.serviceTaxNo} onChange={(e) => setHotelForm({ ...hotelForm, serviceTaxNo: e.target.value })} /></Grid>
            <Grid size={12}>
              <Button variant="contained" startIcon={<Save />} onClick={saveHotelInfo} disabled={saving}>
                {saving ? <CircularProgress size={20} /> : 'Save Hotel Info'}
              </Button>
            </Grid>
          </Grid>
        </Paper>
      )}

      {tab === 1 && (
        <>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
            <Button variant="contained" startIcon={<Add />} onClick={() => { setTaxForm({ taxName: '', taxPercentage: 0, description: '' }); setTaxDialogOpen(true); }}>Add Tax</Button>
          </Box>
          <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
            <Table size="small">
              <TableHead><TableRow sx={{ bgcolor: '#f5f5f5' }}>
                <TableCell><strong>ID</strong></TableCell><TableCell><strong>Tax Name</strong></TableCell><TableCell><strong>Percentage</strong></TableCell><TableCell><strong>Description</strong></TableCell>
              </TableRow></TableHead>
              <TableBody>
                {taxes.map((t) => (
                  <TableRow key={t.id} hover>
                    <TableCell>{t.id}</TableCell><TableCell>{t.taxName}</TableCell><TableCell>{t.taxPercentage}%</TableCell><TableCell>{t.description}</TableCell>
                  </TableRow>
                ))}
                {taxes.length === 0 && <TableRow><TableCell colSpan={4} align="center">No tax records</TableCell></TableRow>}
              </TableBody>
            </Table>
          </TableContainer>
        </>
      )}

      {tab === 2 && (
        <>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
            <Button variant="contained" startIcon={<Add />} onClick={() => { setBedForm({ bedType: '', charges: 0 }); setBedDialogOpen(true); }}>Add Extra Bed</Button>
          </Box>
          <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
            <Table size="small">
              <TableHead><TableRow sx={{ bgcolor: '#f5f5f5' }}>
                <TableCell><strong>ID</strong></TableCell><TableCell><strong>Bed Type</strong></TableCell><TableCell><strong>Charges</strong></TableCell>
              </TableRow></TableHead>
              <TableBody>
                {extraBeds.map((b) => (
                  <TableRow key={b.id} hover>
                    <TableCell>{b.id}</TableCell><TableCell>{b.bedType}</TableCell><TableCell>${b.charges.toFixed(2)}</TableCell>
                  </TableRow>
                ))}
                {extraBeds.length === 0 && <TableRow><TableCell colSpan={3} align="center">No extra beds configured</TableCell></TableRow>}
              </TableBody>
            </Table>
          </TableContainer>
        </>
      )}

      {tab === 3 && (
        <>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
            <Button variant="contained" startIcon={<Add />} onClick={() => { setCurrForm({ currencyName: '', symbol: '' }); setCurrDialogOpen(true); }}>Add Currency</Button>
          </Box>
          <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
            <Table size="small">
              <TableHead><TableRow sx={{ bgcolor: '#f5f5f5' }}>
                <TableCell><strong>ID</strong></TableCell><TableCell><strong>Currency</strong></TableCell><TableCell><strong>Symbol</strong></TableCell><TableCell align="right"><strong>Actions</strong></TableCell>
              </TableRow></TableHead>
              <TableBody>
                {currencies.map((c) => (
                  <TableRow key={c.id} hover>
                    <TableCell>{c.id}</TableCell><TableCell>{c.currencyName}</TableCell><TableCell>{c.symbol}</TableCell>
                    <TableCell align="right"><IconButton size="small" color="error" onClick={() => deleteCurrency(c.id)}><Delete fontSize="small" /></IconButton></TableCell>
                  </TableRow>
                ))}
                {currencies.length === 0 && <TableRow><TableCell colSpan={4} align="center">No currencies</TableCell></TableRow>}
              </TableBody>
            </Table>
          </TableContainer>
        </>
      )}

      <Dialog open={taxDialogOpen} onClose={() => setTaxDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Add Tax</DialogTitle>
        <DialogContent>
          <TextField fullWidth label="Tax Name" value={taxForm.taxName} onChange={(e) => setTaxForm({ ...taxForm, taxName: e.target.value })} margin="normal" />
          <TextField fullWidth label="Percentage" type="number" value={taxForm.taxPercentage} onChange={(e) => setTaxForm({ ...taxForm, taxPercentage: parseFloat(e.target.value) || 0 })} margin="normal" />
          <TextField fullWidth label="Description" value={taxForm.description} onChange={(e) => setTaxForm({ ...taxForm, description: e.target.value })} margin="normal" />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setTaxDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={saveTax}>Save</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={bedDialogOpen} onClose={() => setBedDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Add Extra Bed</DialogTitle>
        <DialogContent>
          <TextField fullWidth label="Bed Type" value={bedForm.bedType} onChange={(e) => setBedForm({ ...bedForm, bedType: e.target.value })} margin="normal" />
          <TextField fullWidth label="Charges" type="number" value={bedForm.charges} onChange={(e) => setBedForm({ ...bedForm, charges: parseFloat(e.target.value) || 0 })} margin="normal" />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setBedDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={saveBed}>Save</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={currDialogOpen} onClose={() => setCurrDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Add Currency</DialogTitle>
        <DialogContent>
          <TextField fullWidth label="Currency Name" value={currForm.currencyName} onChange={(e) => setCurrForm({ ...currForm, currencyName: e.target.value })} margin="normal" />
          <TextField fullWidth label="Symbol" value={currForm.symbol} onChange={(e) => setCurrForm({ ...currForm, symbol: e.target.value })} margin="normal" />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCurrDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={saveCurrency}>Save</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default SettingsPage;
