import React, { useEffect, useState } from 'react';
import {
  Box, Typography, Button, TextField, Dialog, DialogTitle, DialogContent,
  DialogActions, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Paper, IconButton, Alert, CircularProgress, Grid,
  FormControl, InputLabel, Select, MenuItem, Tabs, Tab,
} from '@mui/material';
import { Add, Edit, Delete } from '@mui/icons-material';
import api from '../../api/client';
import { Dish, Beer, Liquor, LiquorMaster, Purchase, Stock, CreateDishRequest, CreateBeerRequest, CreateLiquorRequest, CreateLiquorMasterRequest, CreatePurchaseRequest } from '../../types';

const InventoryPage: React.FC = () => {
  const [tab, setTab] = useState(0);
  const [dishes, setDishes] = useState<Dish[]>([]);
  const [beers, setBeers] = useState<Beer[]>([]);
  const [liquors, setLiquors] = useState<Liquor[]>([]);
  const [liquorMasters, setLiquorMasters] = useState<LiquorMaster[]>([]);
  const [purchases, setPurchases] = useState<Purchase[]>([]);
  const [stock, setStock] = useState<Stock[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogType, setDialogType] = useState('');
  const [editingId, setEditingId] = useState<number | null>(null);
  const [saving, setSaving] = useState(false);

  const [dishForm, setDishForm] = useState<CreateDishRequest>({ dishName: '', category: '', rate: 0 });
  const [beerForm, setBeerForm] = useState<CreateBeerRequest>({ beerName: '', category: '', rate: 0, quantity: 0 });
  const [liquorForm, setLiquorForm] = useState<CreateLiquorRequest>({ liquorName: '', category: '', rate: 0, quantity: 0 });
  const [lmForm, setLmForm] = useState<CreateLiquorMasterRequest>({ liquorName: '', category: '', rate: 0 });
  const [purchaseForm, setPurchaseForm] = useState<CreatePurchaseRequest>({ itemName: '', category: '', quantity: 0, rate: 0, purchaseDate: '', supplier: '', notes: '' });

  const fetchData = async () => {
    try {
      setLoading(true);
      const [d, b, l, lm, p, s] = await Promise.all([
        api.get<Dish[]>('/Inventory/dishes'),
        api.get<Beer[]>('/Inventory/beers'),
        api.get<Liquor[]>('/Inventory/liquors'),
        api.get<LiquorMaster[]>('/Inventory/liquor-masters'),
        api.get<Purchase[]>('/Inventory/purchases'),
        api.get<Stock[]>('/Inventory/stock'),
      ]);
      setDishes(d.data); setBeers(b.data); setLiquors(l.data);
      setLiquorMasters(lm.data); setPurchases(p.data); setStock(s.data);
    } catch {
      setError('Failed to load inventory data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const handleSave = async () => {
    setSaving(true);
    try {
      switch (dialogType) {
        case 'dish':
          if (editingId) await api.put(`/Inventory/dishes/${editingId}`, dishForm);
          else await api.post('/Inventory/dishes', dishForm);
          break;
        case 'beer':
          if (editingId) await api.put(`/Inventory/beers/${editingId}`, beerForm);
          else await api.post('/Inventory/beers', beerForm);
          break;
        case 'liquor':
          if (editingId) await api.put(`/Inventory/liquors/${editingId}`, liquorForm);
          else await api.post('/Inventory/liquors', liquorForm);
          break;
        case 'liquorMaster':
          await api.post('/Inventory/liquor-masters', lmForm);
          break;
        case 'purchase':
          await api.post('/Inventory/purchases', purchaseForm);
          break;
      }
      setDialogOpen(false);
      setEditingId(null);
      fetchData();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to save');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (type: string, id: number) => {
    if (!window.confirm('Delete this item?')) return;
    try {
      await api.delete(`/Inventory/${type}/${id}`);
      fetchData();
    } catch {
      setError('Failed to delete');
    }
  };

  const openDialog = (type: string, item?: Dish | Beer | Liquor) => {
    setDialogType(type);
    setEditingId(null);
    if (type === 'dish') {
      const d = item as Dish | undefined;
      setEditingId(d?.id || null);
      setDishForm(d ? { dishName: d.dishName, category: d.category, rate: d.rate } : { dishName: '', category: '', rate: 0 });
    } else if (type === 'beer') {
      const b = item as Beer | undefined;
      setEditingId(b?.id || null);
      setBeerForm(b ? { beerName: b.beerName, category: b.category, rate: b.rate, quantity: b.quantity } : { beerName: '', category: '', rate: 0, quantity: 0 });
    } else if (type === 'liquor') {
      const l = item as Liquor | undefined;
      setEditingId(l?.id || null);
      setLiquorForm(l ? { liquorName: l.liquorName, category: l.category, rate: l.rate, quantity: l.quantity } : { liquorName: '', category: '', rate: 0, quantity: 0 });
    } else if (type === 'liquorMaster') {
      setLmForm({ liquorName: '', category: '', rate: 0 });
    } else if (type === 'purchase') {
      setPurchaseForm({ itemName: '', category: '', quantity: 0, rate: 0, purchaseDate: '', supplier: '', notes: '' });
    }
    setDialogOpen(true);
  };

  const renderDialogContent = () => {
    switch (dialogType) {
      case 'dish':
        return (
          <>
            <TextField fullWidth label="Dish Name" value={dishForm.dishName} onChange={(e) => setDishForm({ ...dishForm, dishName: e.target.value })} margin="normal" />
            <TextField fullWidth label="Category" value={dishForm.category} onChange={(e) => setDishForm({ ...dishForm, category: e.target.value })} margin="normal" />
            <TextField fullWidth label="Rate" type="number" value={dishForm.rate} onChange={(e) => setDishForm({ ...dishForm, rate: parseFloat(e.target.value) || 0 })} margin="normal" />
          </>
        );
      case 'beer':
        return (
          <>
            <TextField fullWidth label="Beer Name" value={beerForm.beerName} onChange={(e) => setBeerForm({ ...beerForm, beerName: e.target.value })} margin="normal" />
            <TextField fullWidth label="Category" value={beerForm.category} onChange={(e) => setBeerForm({ ...beerForm, category: e.target.value })} margin="normal" />
            <TextField fullWidth label="Rate" type="number" value={beerForm.rate} onChange={(e) => setBeerForm({ ...beerForm, rate: parseFloat(e.target.value) || 0 })} margin="normal" />
            <TextField fullWidth label="Quantity" type="number" value={beerForm.quantity} onChange={(e) => setBeerForm({ ...beerForm, quantity: parseInt(e.target.value) || 0 })} margin="normal" />
          </>
        );
      case 'liquor':
        return (
          <>
            <TextField fullWidth label="Liquor Name" value={liquorForm.liquorName} onChange={(e) => setLiquorForm({ ...liquorForm, liquorName: e.target.value })} margin="normal" />
            <TextField fullWidth label="Category" value={liquorForm.category} onChange={(e) => setLiquorForm({ ...liquorForm, category: e.target.value })} margin="normal" />
            <TextField fullWidth label="Rate" type="number" value={liquorForm.rate} onChange={(e) => setLiquorForm({ ...liquorForm, rate: parseFloat(e.target.value) || 0 })} margin="normal" />
            <TextField fullWidth label="Quantity" type="number" value={liquorForm.quantity} onChange={(e) => setLiquorForm({ ...liquorForm, quantity: parseInt(e.target.value) || 0 })} margin="normal" />
          </>
        );
      case 'liquorMaster':
        return (
          <>
            <TextField fullWidth label="Liquor Name" value={lmForm.liquorName} onChange={(e) => setLmForm({ ...lmForm, liquorName: e.target.value })} margin="normal" />
            <TextField fullWidth label="Category" value={lmForm.category} onChange={(e) => setLmForm({ ...lmForm, category: e.target.value })} margin="normal" />
            <TextField fullWidth label="Rate" type="number" value={lmForm.rate} onChange={(e) => setLmForm({ ...lmForm, rate: parseFloat(e.target.value) || 0 })} margin="normal" />
          </>
        );
      case 'purchase':
        return (
          <Grid container spacing={2} sx={{ mt: 0.5 }}>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="Item Name" value={purchaseForm.itemName} onChange={(e) => setPurchaseForm({ ...purchaseForm, itemName: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="Category" value={purchaseForm.category} onChange={(e) => setPurchaseForm({ ...purchaseForm, category: e.target.value })} /></Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="Quantity" type="number" value={purchaseForm.quantity} onChange={(e) => setPurchaseForm({ ...purchaseForm, quantity: parseInt(e.target.value) || 0 })} /></Grid>
            <Grid size={{ xs: 6, sm: 3 }}><TextField fullWidth label="Rate" type="number" value={purchaseForm.rate} onChange={(e) => setPurchaseForm({ ...purchaseForm, rate: parseFloat(e.target.value) || 0 })} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="Purchase Date" type="date" value={purchaseForm.purchaseDate} onChange={(e) => setPurchaseForm({ ...purchaseForm, purchaseDate: e.target.value })} InputLabelProps={{ shrink: true }} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="Supplier" value={purchaseForm.supplier} onChange={(e) => setPurchaseForm({ ...purchaseForm, supplier: e.target.value })} /></Grid>
            <Grid size={12}><TextField fullWidth label="Notes" value={purchaseForm.notes} onChange={(e) => setPurchaseForm({ ...purchaseForm, notes: e.target.value })} multiline rows={2} /></Grid>
          </Grid>
        );
      default: return null;
    }
  };

  const titles: Record<string, string> = { dish: 'Dish', beer: 'Beer', liquor: 'Liquor', liquorMaster: 'Liquor Master', purchase: 'Purchase' };

  return (
    <Box>
      <Typography variant="h4" fontWeight={700} color="#1a237e" gutterBottom>Inventory Management</Typography>

      {error && <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>{error}</Alert>}

      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 2 }}>
        <Tab label="Dishes" />
        <Tab label="Beers" />
        <Tab label="Liquors" />
        <Tab label="Liquor Masters" />
        <Tab label="Purchases" />
        <Tab label="Stock" />
      </Tabs>

      {loading ? <CircularProgress /> : (
        <>
          {tab === 0 && (
            <>
              <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
                <Button variant="contained" startIcon={<Add />} onClick={() => openDialog('dish')}>Add Dish</Button>
              </Box>
              <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
                <Table size="small">
                  <TableHead><TableRow sx={{ bgcolor: '#f5f5f5' }}>
                    <TableCell><strong>ID</strong></TableCell><TableCell><strong>Name</strong></TableCell><TableCell><strong>Category</strong></TableCell><TableCell><strong>Rate</strong></TableCell><TableCell align="right"><strong>Actions</strong></TableCell>
                  </TableRow></TableHead>
                  <TableBody>
                    {dishes.map((d) => (
                      <TableRow key={d.id} hover>
                        <TableCell>{d.id}</TableCell><TableCell>{d.dishName}</TableCell><TableCell>{d.category}</TableCell><TableCell>${d.rate.toFixed(2)}</TableCell>
                        <TableCell align="right">
                          <IconButton size="small" onClick={() => openDialog('dish', d)}><Edit fontSize="small" /></IconButton>
                          <IconButton size="small" color="error" onClick={() => handleDelete('dishes', d.id)}><Delete fontSize="small" /></IconButton>
                        </TableCell>
                      </TableRow>
                    ))}
                    {dishes.length === 0 && <TableRow><TableCell colSpan={5} align="center">No dishes</TableCell></TableRow>}
                  </TableBody>
                </Table>
              </TableContainer>
            </>
          )}

          {tab === 1 && (
            <>
              <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
                <Button variant="contained" startIcon={<Add />} onClick={() => openDialog('beer')}>Add Beer</Button>
              </Box>
              <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
                <Table size="small">
                  <TableHead><TableRow sx={{ bgcolor: '#f5f5f5' }}>
                    <TableCell><strong>ID</strong></TableCell><TableCell><strong>Name</strong></TableCell><TableCell><strong>Category</strong></TableCell><TableCell><strong>Rate</strong></TableCell><TableCell><strong>Qty</strong></TableCell><TableCell align="right"><strong>Actions</strong></TableCell>
                  </TableRow></TableHead>
                  <TableBody>
                    {beers.map((b) => (
                      <TableRow key={b.id} hover>
                        <TableCell>{b.id}</TableCell><TableCell>{b.beerName}</TableCell><TableCell>{b.category}</TableCell><TableCell>${b.rate.toFixed(2)}</TableCell><TableCell>{b.quantity}</TableCell>
                        <TableCell align="right">
                          <IconButton size="small" onClick={() => openDialog('beer', b)}><Edit fontSize="small" /></IconButton>
                          <IconButton size="small" color="error" onClick={() => handleDelete('beers', b.id)}><Delete fontSize="small" /></IconButton>
                        </TableCell>
                      </TableRow>
                    ))}
                    {beers.length === 0 && <TableRow><TableCell colSpan={6} align="center">No beers</TableCell></TableRow>}
                  </TableBody>
                </Table>
              </TableContainer>
            </>
          )}

          {tab === 2 && (
            <>
              <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
                <Button variant="contained" startIcon={<Add />} onClick={() => openDialog('liquor')}>Add Liquor</Button>
              </Box>
              <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
                <Table size="small">
                  <TableHead><TableRow sx={{ bgcolor: '#f5f5f5' }}>
                    <TableCell><strong>ID</strong></TableCell><TableCell><strong>Name</strong></TableCell><TableCell><strong>Category</strong></TableCell><TableCell><strong>Rate</strong></TableCell><TableCell><strong>Qty</strong></TableCell><TableCell align="right"><strong>Actions</strong></TableCell>
                  </TableRow></TableHead>
                  <TableBody>
                    {liquors.map((l) => (
                      <TableRow key={l.id} hover>
                        <TableCell>{l.id}</TableCell><TableCell>{l.liquorName}</TableCell><TableCell>{l.category}</TableCell><TableCell>${l.rate.toFixed(2)}</TableCell><TableCell>{l.quantity}</TableCell>
                        <TableCell align="right">
                          <IconButton size="small" onClick={() => openDialog('liquor', l)}><Edit fontSize="small" /></IconButton>
                          <IconButton size="small" color="error" onClick={() => handleDelete('liquors', l.id)}><Delete fontSize="small" /></IconButton>
                        </TableCell>
                      </TableRow>
                    ))}
                    {liquors.length === 0 && <TableRow><TableCell colSpan={6} align="center">No liquors</TableCell></TableRow>}
                  </TableBody>
                </Table>
              </TableContainer>
            </>
          )}

          {tab === 3 && (
            <>
              <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
                <Button variant="contained" startIcon={<Add />} onClick={() => openDialog('liquorMaster')}>Add Liquor Master</Button>
              </Box>
              <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
                <Table size="small">
                  <TableHead><TableRow sx={{ bgcolor: '#f5f5f5' }}>
                    <TableCell><strong>ID</strong></TableCell><TableCell><strong>Name</strong></TableCell><TableCell><strong>Category</strong></TableCell><TableCell><strong>Rate</strong></TableCell><TableCell align="right"><strong>Actions</strong></TableCell>
                  </TableRow></TableHead>
                  <TableBody>
                    {liquorMasters.map((lm) => (
                      <TableRow key={lm.id} hover>
                        <TableCell>{lm.id}</TableCell><TableCell>{lm.liquorName}</TableCell><TableCell>{lm.category}</TableCell><TableCell>${lm.rate.toFixed(2)}</TableCell>
                        <TableCell align="right">
                          <IconButton size="small" color="error" onClick={() => handleDelete('liquor-masters', lm.id)}><Delete fontSize="small" /></IconButton>
                        </TableCell>
                      </TableRow>
                    ))}
                    {liquorMasters.length === 0 && <TableRow><TableCell colSpan={5} align="center">No liquor masters</TableCell></TableRow>}
                  </TableBody>
                </Table>
              </TableContainer>
            </>
          )}

          {tab === 4 && (
            <>
              <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
                <Button variant="contained" startIcon={<Add />} onClick={() => openDialog('purchase')}>Add Purchase</Button>
              </Box>
              <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
                <Table size="small">
                  <TableHead><TableRow sx={{ bgcolor: '#f5f5f5' }}>
                    <TableCell><strong>ID</strong></TableCell><TableCell><strong>Item</strong></TableCell><TableCell><strong>Category</strong></TableCell><TableCell><strong>Qty</strong></TableCell><TableCell><strong>Rate</strong></TableCell><TableCell><strong>Total</strong></TableCell><TableCell><strong>Date</strong></TableCell><TableCell><strong>Supplier</strong></TableCell>
                  </TableRow></TableHead>
                  <TableBody>
                    {purchases.map((p) => (
                      <TableRow key={p.id} hover>
                        <TableCell>{p.id}</TableCell><TableCell>{p.itemName}</TableCell><TableCell>{p.category}</TableCell><TableCell>{p.quantity}</TableCell><TableCell>${p.rate.toFixed(2)}</TableCell><TableCell>${p.totalAmount.toFixed(2)}</TableCell><TableCell>{new Date(p.purchaseDate).toLocaleDateString()}</TableCell><TableCell>{p.supplier}</TableCell>
                      </TableRow>
                    ))}
                    {purchases.length === 0 && <TableRow><TableCell colSpan={8} align="center">No purchases</TableCell></TableRow>}
                  </TableBody>
                </Table>
              </TableContainer>
            </>
          )}

          {tab === 5 && (
            <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
              <Table size="small">
                <TableHead><TableRow sx={{ bgcolor: '#f5f5f5' }}>
                  <TableCell><strong>ID</strong></TableCell><TableCell><strong>Item</strong></TableCell><TableCell><strong>Category</strong></TableCell><TableCell><strong>Qty</strong></TableCell><TableCell><strong>Rate</strong></TableCell><TableCell><strong>Unit</strong></TableCell>
                </TableRow></TableHead>
                <TableBody>
                  {stock.map((s) => (
                    <TableRow key={s.id} hover>
                      <TableCell>{s.id}</TableCell><TableCell>{s.itemName}</TableCell><TableCell>{s.category}</TableCell><TableCell>{s.quantity}</TableCell><TableCell>${s.rate.toFixed(2)}</TableCell><TableCell>{s.unit}</TableCell>
                    </TableRow>
                  ))}
                  {stock.length === 0 && <TableRow><TableCell colSpan={6} align="center">No stock records</TableCell></TableRow>}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </>
      )}

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingId ? `Edit ${titles[dialogType]}` : `Add ${titles[dialogType]}`}</DialogTitle>
        <DialogContent>{renderDialogContent()}</DialogContent>
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

export default InventoryPage;
