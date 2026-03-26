import React, { useEffect, useState } from 'react';
import {
  Box, Typography, Button, TextField, Dialog, DialogTitle, DialogContent,
  DialogActions, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Paper, IconButton, Alert, CircularProgress, Grid,
  FormControl, InputLabel, Select, MenuItem, Tabs, Tab,
} from '@mui/material';
import { Add, Edit, Delete } from '@mui/icons-material';
import api from '../../api/client';
import { Hall, Garden, HallGardenReservation, HallOrGardenReservation, Guest, Currency, CreateHallGardenReservationRequest, CreateHallOrGardenReservationRequest } from '../../types';

const HallGardenPage: React.FC = () => {
  const [tab, setTab] = useState(0);
  const [halls, setHalls] = useState<Hall[]>([]);
  const [gardens, setGardens] = useState<Garden[]>([]);
  const [hgReservations, setHgReservations] = useState<HallGardenReservation[]>([]);
  const [hoReservations, setHoReservations] = useState<HallOrGardenReservation[]>([]);
  const [guests, setGuests] = useState<Guest[]>([]);
  const [currencies, setCurrencies] = useState<Currency[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const [hallDialogOpen, setHallDialogOpen] = useState(false);
  const [hallForm, setHallForm] = useState({ hallName: '', charges: 0, description: '' });
  const [editingHallId, setEditingHallId] = useState<number | null>(null);

  const [gardenDialogOpen, setGardenDialogOpen] = useState(false);
  const [gardenForm, setGardenForm] = useState({ gardenName: '', charges: 0, description: '' });
  const [editingGardenId, setEditingGardenId] = useState<number | null>(null);

  const [hgDialogOpen, setHgDialogOpen] = useState(false);
  const [hgForm, setHgForm] = useState<CreateHallGardenReservationRequest>({
    guestID: '', guestName: '', hallName: '', gardenName: '', dateIN: '', dateOUT: '',
    noOfDaysHall: 1, noOfDaysGarden: 1, rateHall: 0, rateGarden: 0, otherCharges: 0,
    discountPer: 0, serviceTaxPer: 0, luxuryTaxPer: 0, totalPaid: 0, currency: '', notes: '',
  });

  const [hoDialogOpen, setHoDialogOpen] = useState(false);
  const [hoForm, setHoForm] = useState<CreateHallOrGardenReservationRequest>({
    guestID: '', guestName: '', venueName: '', venueType: 'Hall', dateIN: '', dateOUT: '',
    noOfDays: 1, rate: 0, otherCharges: 0, discountPer: 0, serviceTaxPer: 0,
    luxuryTaxPer: 0, totalPaid: 0, currency: '', notes: '',
  });

  const fetchData = async () => {
    try {
      setLoading(true);
      const [hRes, gRes, hgRes, hoRes, guRes, cRes] = await Promise.all([
        api.get<Hall[]>('/HallGarden/halls'),
        api.get<Garden[]>('/HallGarden/gardens'),
        api.get<HallGardenReservation[]>('/HallGarden/reservations/hall-and-garden'),
        api.get<HallOrGardenReservation[]>('/HallGarden/reservations/hall-or-garden'),
        api.get<Guest[]>('/Guests'),
        api.get<Currency[]>('/Currency'),
      ]);
      setHalls(hRes.data); setGardens(gRes.data);
      setHgReservations(hgRes.data); setHoReservations(hoRes.data);
      setGuests(guRes.data); setCurrencies(cRes.data);
    } catch {
      setError('Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const saveHall = async () => {
    setSaving(true);
    try {
      if (editingHallId) await api.put(`/HallGarden/halls/${editingHallId}`, hallForm);
      else await api.post('/HallGarden/halls', hallForm);
      setHallDialogOpen(false); setEditingHallId(null); fetchData();
    } catch { setError('Failed to save hall'); }
    finally { setSaving(false); }
  };

  const saveGarden = async () => {
    setSaving(true);
    try {
      if (editingGardenId) await api.put(`/HallGarden/gardens/${editingGardenId}`, gardenForm);
      else await api.post('/HallGarden/gardens', gardenForm);
      setGardenDialogOpen(false); setEditingGardenId(null); fetchData();
    } catch { setError('Failed to save garden'); }
    finally { setSaving(false); }
  };

  const saveHgReservation = async () => {
    setSaving(true);
    try {
      await api.post('/HallGarden/reservations/hall-and-garden', hgForm);
      setHgDialogOpen(false); fetchData();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to save reservation');
    } finally { setSaving(false); }
  };

  const saveHoReservation = async () => {
    setSaving(true);
    try {
      await api.post('/HallGarden/reservations/hall-or-garden', hoForm);
      setHoDialogOpen(false); fetchData();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to save reservation');
    } finally { setSaving(false); }
  };

  const deleteHall = async (id: number) => {
    if (!window.confirm('Delete?')) return;
    try { await api.delete(`/HallGarden/halls/${id}`); fetchData(); } catch { setError('Failed to delete'); }
  };

  const deleteGarden = async (id: number) => {
    if (!window.confirm('Delete?')) return;
    try { await api.delete(`/HallGarden/gardens/${id}`); fetchData(); } catch { setError('Failed to delete'); }
  };

  if (loading) return <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}><CircularProgress /></Box>;

  return (
    <Box>
      <Typography variant="h4" fontWeight={700} color="#1a237e" gutterBottom>Hall & Garden Management</Typography>
      {error && <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>{error}</Alert>}

      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 2 }}>
        <Tab label="Halls" /><Tab label="Gardens" /><Tab label="Hall+Garden Reservations" /><Tab label="Hall/Garden Reservations" />
      </Tabs>

      {tab === 0 && (
        <>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
            <Button variant="contained" startIcon={<Add />} onClick={() => { setEditingHallId(null); setHallForm({ hallName: '', charges: 0, description: '' }); setHallDialogOpen(true); }}>Add Hall</Button>
          </Box>
          <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
            <Table size="small">
              <TableHead><TableRow sx={{ bgcolor: '#f5f5f5' }}>
                <TableCell><strong>ID</strong></TableCell><TableCell><strong>Name</strong></TableCell><TableCell><strong>Charges</strong></TableCell><TableCell><strong>Description</strong></TableCell><TableCell align="right"><strong>Actions</strong></TableCell>
              </TableRow></TableHead>
              <TableBody>
                {halls.map((h) => (
                  <TableRow key={h.id} hover>
                    <TableCell>{h.id}</TableCell><TableCell>{h.hallName}</TableCell><TableCell>${h.charges.toFixed(2)}</TableCell><TableCell>{h.description}</TableCell>
                    <TableCell align="right">
                      <IconButton size="small" onClick={() => { setEditingHallId(h.id); setHallForm({ hallName: h.hallName, charges: h.charges, description: h.description }); setHallDialogOpen(true); }}><Edit fontSize="small" /></IconButton>
                      <IconButton size="small" color="error" onClick={() => deleteHall(h.id)}><Delete fontSize="small" /></IconButton>
                    </TableCell>
                  </TableRow>
                ))}
                {halls.length === 0 && <TableRow><TableCell colSpan={5} align="center">No halls</TableCell></TableRow>}
              </TableBody>
            </Table>
          </TableContainer>
        </>
      )}

      {tab === 1 && (
        <>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
            <Button variant="contained" startIcon={<Add />} onClick={() => { setEditingGardenId(null); setGardenForm({ gardenName: '', charges: 0, description: '' }); setGardenDialogOpen(true); }}>Add Garden</Button>
          </Box>
          <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
            <Table size="small">
              <TableHead><TableRow sx={{ bgcolor: '#f5f5f5' }}>
                <TableCell><strong>ID</strong></TableCell><TableCell><strong>Name</strong></TableCell><TableCell><strong>Charges</strong></TableCell><TableCell><strong>Description</strong></TableCell><TableCell align="right"><strong>Actions</strong></TableCell>
              </TableRow></TableHead>
              <TableBody>
                {gardens.map((g) => (
                  <TableRow key={g.id} hover>
                    <TableCell>{g.id}</TableCell><TableCell>{g.gardenName}</TableCell><TableCell>${g.charges.toFixed(2)}</TableCell><TableCell>{g.description}</TableCell>
                    <TableCell align="right">
                      <IconButton size="small" onClick={() => { setEditingGardenId(g.id); setGardenForm({ gardenName: g.gardenName, charges: g.charges, description: g.description }); setGardenDialogOpen(true); }}><Edit fontSize="small" /></IconButton>
                      <IconButton size="small" color="error" onClick={() => deleteGarden(g.id)}><Delete fontSize="small" /></IconButton>
                    </TableCell>
                  </TableRow>
                ))}
                {gardens.length === 0 && <TableRow><TableCell colSpan={5} align="center">No gardens</TableCell></TableRow>}
              </TableBody>
            </Table>
          </TableContainer>
        </>
      )}

      {tab === 2 && (
        <>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
            <Button variant="contained" startIcon={<Add />} onClick={() => {
              setHgForm({ guestID: '', guestName: '', hallName: '', gardenName: '', dateIN: '', dateOUT: '', noOfDaysHall: 1, noOfDaysGarden: 1, rateHall: 0, rateGarden: 0, otherCharges: 0, discountPer: 0, serviceTaxPer: 0, luxuryTaxPer: 0, totalPaid: 0, currency: '', notes: '' });
              setHgDialogOpen(true);
            }}>New Reservation</Button>
          </Box>
          <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
            <Table size="small">
              <TableHead><TableRow sx={{ bgcolor: '#f5f5f5' }}>
                <TableCell><strong>ID</strong></TableCell><TableCell><strong>Guest</strong></TableCell><TableCell><strong>Hall</strong></TableCell><TableCell><strong>Garden</strong></TableCell><TableCell><strong>Dates</strong></TableCell><TableCell><strong>Grand Total</strong></TableCell><TableCell><strong>Status</strong></TableCell>
              </TableRow></TableHead>
              <TableBody>
                {hgReservations.map((r) => (
                  <TableRow key={r.id} hover>
                    <TableCell>{r.id}</TableCell><TableCell>{r.guestName}</TableCell><TableCell>{r.hallName}</TableCell><TableCell>{r.gardenName}</TableCell>
                    <TableCell>{new Date(r.dateIN).toLocaleDateString()} - {new Date(r.dateOUT).toLocaleDateString()}</TableCell>
                    <TableCell>${r.grandTotal.toFixed(2)}</TableCell><TableCell>{r.status}</TableCell>
                  </TableRow>
                ))}
                {hgReservations.length === 0 && <TableRow><TableCell colSpan={7} align="center">No reservations</TableCell></TableRow>}
              </TableBody>
            </Table>
          </TableContainer>
        </>
      )}

      {tab === 3 && (
        <>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
            <Button variant="contained" startIcon={<Add />} onClick={() => {
              setHoForm({ guestID: '', guestName: '', venueName: '', venueType: 'Hall', dateIN: '', dateOUT: '', noOfDays: 1, rate: 0, otherCharges: 0, discountPer: 0, serviceTaxPer: 0, luxuryTaxPer: 0, totalPaid: 0, currency: '', notes: '' });
              setHoDialogOpen(true);
            }}>New Reservation</Button>
          </Box>
          <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
            <Table size="small">
              <TableHead><TableRow sx={{ bgcolor: '#f5f5f5' }}>
                <TableCell><strong>ID</strong></TableCell><TableCell><strong>Guest</strong></TableCell><TableCell><strong>Venue</strong></TableCell><TableCell><strong>Type</strong></TableCell><TableCell><strong>Dates</strong></TableCell><TableCell><strong>Grand Total</strong></TableCell><TableCell><strong>Status</strong></TableCell>
              </TableRow></TableHead>
              <TableBody>
                {hoReservations.map((r) => (
                  <TableRow key={r.id} hover>
                    <TableCell>{r.id}</TableCell><TableCell>{r.guestName}</TableCell><TableCell>{r.venueName}</TableCell><TableCell>{r.venueType}</TableCell>
                    <TableCell>{new Date(r.dateIN).toLocaleDateString()} - {new Date(r.dateOUT).toLocaleDateString()}</TableCell>
                    <TableCell>${r.grandTotal.toFixed(2)}</TableCell><TableCell>{r.status}</TableCell>
                  </TableRow>
                ))}
                {hoReservations.length === 0 && <TableRow><TableCell colSpan={7} align="center">No reservations</TableCell></TableRow>}
              </TableBody>
            </Table>
          </TableContainer>
        </>
      )}

      {/* Hall Dialog */}
      <Dialog open={hallDialogOpen} onClose={() => setHallDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingHallId ? 'Edit Hall' : 'Add Hall'}</DialogTitle>
        <DialogContent>
          <TextField fullWidth label="Hall Name" value={hallForm.hallName} onChange={(e) => setHallForm({ ...hallForm, hallName: e.target.value })} margin="normal" />
          <TextField fullWidth label="Charges" type="number" value={hallForm.charges} onChange={(e) => setHallForm({ ...hallForm, charges: parseFloat(e.target.value) || 0 })} margin="normal" />
          <TextField fullWidth label="Description" value={hallForm.description} onChange={(e) => setHallForm({ ...hallForm, description: e.target.value })} margin="normal" multiline rows={2} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setHallDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={saveHall} disabled={saving}>{saving ? <CircularProgress size={20} /> : 'Save'}</Button>
        </DialogActions>
      </Dialog>

      {/* Garden Dialog */}
      <Dialog open={gardenDialogOpen} onClose={() => setGardenDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingGardenId ? 'Edit Garden' : 'Add Garden'}</DialogTitle>
        <DialogContent>
          <TextField fullWidth label="Garden Name" value={gardenForm.gardenName} onChange={(e) => setGardenForm({ ...gardenForm, gardenName: e.target.value })} margin="normal" />
          <TextField fullWidth label="Charges" type="number" value={gardenForm.charges} onChange={(e) => setGardenForm({ ...gardenForm, charges: parseFloat(e.target.value) || 0 })} margin="normal" />
          <TextField fullWidth label="Description" value={gardenForm.description} onChange={(e) => setGardenForm({ ...gardenForm, description: e.target.value })} margin="normal" multiline rows={2} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setGardenDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={saveGarden} disabled={saving}>{saving ? <CircularProgress size={20} /> : 'Save'}</Button>
        </DialogActions>
      </Dialog>

      {/* Hall+Garden Reservation Dialog */}
      <Dialog open={hgDialogOpen} onClose={() => setHgDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>Hall & Garden Reservation</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 0.5 }}>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth><InputLabel>Guest</InputLabel>
                <Select value={hgForm.guestID} label="Guest" onChange={(e) => { const g = guests.find((g) => g.guestID === e.target.value); setHgForm({ ...hgForm, guestID: e.target.value, guestName: g?.guestName || '' }); }}>
                  {guests.map((g) => <MenuItem key={g.guestID} value={g.guestID}>{g.guestName}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 3 }}>
              <FormControl fullWidth><InputLabel>Hall</InputLabel>
                <Select value={hgForm.hallName} label="Hall" onChange={(e) => { const h = halls.find((h) => h.hallName === e.target.value); setHgForm({ ...hgForm, hallName: e.target.value, rateHall: h?.charges || 0 }); }}>
                  {halls.map((h) => <MenuItem key={h.id} value={h.hallName}>{h.hallName}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 3 }}>
              <FormControl fullWidth><InputLabel>Garden</InputLabel>
                <Select value={hgForm.gardenName} label="Garden" onChange={(e) => { const g = gardens.find((g) => g.gardenName === e.target.value); setHgForm({ ...hgForm, gardenName: e.target.value, rateGarden: g?.charges || 0 }); }}>
                  {gardens.map((g) => <MenuItem key={g.id} value={g.gardenName}>{g.gardenName}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="Check-In" type="date" value={hgForm.dateIN} onChange={(e) => setHgForm({ ...hgForm, dateIN: e.target.value })} InputLabelProps={{ shrink: true }} /></Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="Check-Out" type="date" value={hgForm.dateOUT} onChange={(e) => setHgForm({ ...hgForm, dateOUT: e.target.value })} InputLabelProps={{ shrink: true }} /></Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="Hall Days" type="number" value={hgForm.noOfDaysHall} onChange={(e) => setHgForm({ ...hgForm, noOfDaysHall: parseInt(e.target.value) || 0 })} /></Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="Garden Days" type="number" value={hgForm.noOfDaysGarden} onChange={(e) => setHgForm({ ...hgForm, noOfDaysGarden: parseInt(e.target.value) || 0 })} /></Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="Other Charges" type="number" value={hgForm.otherCharges} onChange={(e) => setHgForm({ ...hgForm, otherCharges: parseFloat(e.target.value) || 0 })} /></Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="Discount %" type="number" value={hgForm.discountPer} onChange={(e) => setHgForm({ ...hgForm, discountPer: parseFloat(e.target.value) || 0 })} /></Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="Service Tax %" type="number" value={hgForm.serviceTaxPer} onChange={(e) => setHgForm({ ...hgForm, serviceTaxPer: parseFloat(e.target.value) || 0 })} /></Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="Luxury Tax %" type="number" value={hgForm.luxuryTaxPer} onChange={(e) => setHgForm({ ...hgForm, luxuryTaxPer: parseFloat(e.target.value) || 0 })} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="Total Paid" type="number" value={hgForm.totalPaid} onChange={(e) => setHgForm({ ...hgForm, totalPaid: parseFloat(e.target.value) || 0 })} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth><InputLabel>Currency</InputLabel>
                <Select value={hgForm.currency} label="Currency" onChange={(e) => setHgForm({ ...hgForm, currency: e.target.value })}>
                  {currencies.map((c) => <MenuItem key={c.id} value={c.currencyName}>{c.currencyName}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={12}><TextField fullWidth label="Notes" value={hgForm.notes} onChange={(e) => setHgForm({ ...hgForm, notes: e.target.value })} multiline rows={2} /></Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setHgDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={saveHgReservation} disabled={saving}>{saving ? <CircularProgress size={20} /> : 'Save'}</Button>
        </DialogActions>
      </Dialog>

      {/* Hall/Garden Reservation Dialog */}
      <Dialog open={hoDialogOpen} onClose={() => setHoDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>Hall or Garden Reservation</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 0.5 }}>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth><InputLabel>Guest</InputLabel>
                <Select value={hoForm.guestID} label="Guest" onChange={(e) => { const g = guests.find((g) => g.guestID === e.target.value); setHoForm({ ...hoForm, guestID: e.target.value, guestName: g?.guestName || '' }); }}>
                  {guests.map((g) => <MenuItem key={g.guestID} value={g.guestID}>{g.guestName}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 3 }}>
              <FormControl fullWidth><InputLabel>Venue Type</InputLabel>
                <Select value={hoForm.venueType} label="Venue Type" onChange={(e) => setHoForm({ ...hoForm, venueType: e.target.value, venueName: '' })}>
                  <MenuItem value="Hall">Hall</MenuItem><MenuItem value="Garden">Garden</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 3 }}>
              <FormControl fullWidth><InputLabel>Venue</InputLabel>
                <Select value={hoForm.venueName} label="Venue" onChange={(e) => {
                  const name = e.target.value;
                  const venue = hoForm.venueType === 'Hall' ? halls.find((h) => h.hallName === name) : gardens.find((g) => g.gardenName === name);
                  setHoForm({ ...hoForm, venueName: name, rate: venue?.charges || 0 });
                }}>
                  {(hoForm.venueType === 'Hall' ? halls.map((h) => h.hallName) : gardens.map((g) => g.gardenName)).map((n) => <MenuItem key={n} value={n}>{n}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="Check-In" type="date" value={hoForm.dateIN} onChange={(e) => setHoForm({ ...hoForm, dateIN: e.target.value })} InputLabelProps={{ shrink: true }} /></Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="Check-Out" type="date" value={hoForm.dateOUT} onChange={(e) => setHoForm({ ...hoForm, dateOUT: e.target.value })} InputLabelProps={{ shrink: true }} /></Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="No of Days" type="number" value={hoForm.noOfDays} onChange={(e) => setHoForm({ ...hoForm, noOfDays: parseInt(e.target.value) || 0 })} /></Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="Rate" type="number" value={hoForm.rate} onChange={(e) => setHoForm({ ...hoForm, rate: parseFloat(e.target.value) || 0 })} /></Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="Other Charges" type="number" value={hoForm.otherCharges} onChange={(e) => setHoForm({ ...hoForm, otherCharges: parseFloat(e.target.value) || 0 })} /></Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="Discount %" type="number" value={hoForm.discountPer} onChange={(e) => setHoForm({ ...hoForm, discountPer: parseFloat(e.target.value) || 0 })} /></Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="Service Tax %" type="number" value={hoForm.serviceTaxPer} onChange={(e) => setHoForm({ ...hoForm, serviceTaxPer: parseFloat(e.target.value) || 0 })} /></Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="Luxury Tax %" type="number" value={hoForm.luxuryTaxPer} onChange={(e) => setHoForm({ ...hoForm, luxuryTaxPer: parseFloat(e.target.value) || 0 })} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="Total Paid" type="number" value={hoForm.totalPaid} onChange={(e) => setHoForm({ ...hoForm, totalPaid: parseFloat(e.target.value) || 0 })} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth><InputLabel>Currency</InputLabel>
                <Select value={hoForm.currency} label="Currency" onChange={(e) => setHoForm({ ...hoForm, currency: e.target.value })}>
                  {currencies.map((c) => <MenuItem key={c.id} value={c.currencyName}>{c.currencyName}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={12}><TextField fullWidth label="Notes" value={hoForm.notes} onChange={(e) => setHoForm({ ...hoForm, notes: e.target.value })} multiline rows={2} /></Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setHoDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={saveHoReservation} disabled={saving}>{saving ? <CircularProgress size={20} /> : 'Save'}</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default HallGardenPage;
