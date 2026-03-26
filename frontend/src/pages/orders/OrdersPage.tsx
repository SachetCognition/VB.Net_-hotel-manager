import React, { useEffect, useState } from 'react';
import {
  Box, Typography, Button, TextField, Dialog, DialogTitle, DialogContent,
  DialogActions, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Paper, IconButton, Alert, CircularProgress, Grid,
  FormControl, InputLabel, Select, MenuItem, Tabs, Tab,
} from '@mui/material';
import { Add, Delete } from '@mui/icons-material';
import api from '../../api/client';
import { Order, CreateOrderRequest, RestaurantOrder, CreateRestaurantOrderRequest, Guest, Room, Dish } from '../../types';

const OrdersPage: React.FC = () => {
  const [tab, setTab] = useState(0);
  const [orders, setOrders] = useState<Order[]>([]);
  const [restaurantOrders, setRestaurantOrders] = useState<RestaurantOrder[]>([]);
  const [guests, setGuests] = useState<Guest[]>([]);
  const [rooms, setRooms] = useState<Room[]>([]);
  const [dishes, setDishes] = useState<Dish[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogType, setDialogType] = useState<'room' | 'restaurant'>('room');
  const [saving, setSaving] = useState(false);

  const [roomForm, setRoomForm] = useState<CreateOrderRequest>({
    guestID: '', guestName: '', roomNo: '', itemName: '', category: '', quantity: 1, rate: 0, notes: '',
  });
  const [restForm, setRestForm] = useState<CreateRestaurantOrderRequest>({
    customerName: '', itemName: '', category: '', quantity: 1, rate: 0, notes: '',
  });

  const fetchData = async () => {
    try {
      setLoading(true);
      const [oRes, roRes, gRes, rRes, dRes] = await Promise.all([
        api.get<Order[]>('/Orders'),
        api.get<RestaurantOrder[]>('/Orders/restaurant'),
        api.get<Guest[]>('/Guests'),
        api.get<Room[]>('/Rooms'),
        api.get<Dish[]>('/Inventory/dishes'),
      ]);
      setOrders(oRes.data);
      setRestaurantOrders(roRes.data);
      setGuests(gRes.data);
      setRooms(rRes.data);
      setDishes(dRes.data);
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
      if (dialogType === 'room') {
        await api.post('/Orders', roomForm);
      } else {
        await api.post('/Orders/restaurant', restForm);
      }
      setDialogOpen(false);
      fetchData();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to save order');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (type: 'room' | 'restaurant', id: number) => {
    if (!window.confirm('Delete this order?')) return;
    try {
      if (type === 'room') await api.delete(`/Orders/${id}`);
      else await api.delete(`/Orders/restaurant/${id}`);
      fetchData();
    } catch {
      setError('Failed to delete');
    }
  };

  const handleDishSelect = (dishName: string) => {
    const dish = dishes.find((d) => d.dishName === dishName);
    if (dialogType === 'room') {
      setRoomForm({ ...roomForm, itemName: dishName, category: dish?.category || '', rate: dish?.rate || 0 });
    } else {
      setRestForm({ ...restForm, itemName: dishName, category: dish?.category || '', rate: dish?.rate || 0 });
    }
  };

  return (
    <Box>
      <Typography variant="h4" fontWeight={700} color="#1a237e" gutterBottom>Order Management</Typography>

      {error && <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>{error}</Alert>}

      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 2 }}>
        <Tab label={`Room Orders (${orders.length})`} />
        <Tab label={`Restaurant Orders (${restaurantOrders.length})`} />
      </Tabs>

      {tab === 0 && (
        <>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
            <Button variant="contained" startIcon={<Add />} onClick={() => {
              setDialogType('room');
              setRoomForm({ guestID: '', guestName: '', roomNo: '', itemName: '', category: '', quantity: 1, rate: 0, notes: '' });
              setDialogOpen(true);
            }}>New Room Order</Button>
          </Box>
          {loading ? <CircularProgress /> : (
            <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
              <Table size="small">
                <TableHead><TableRow sx={{ bgcolor: '#f5f5f5' }}>
                  <TableCell><strong>ID</strong></TableCell><TableCell><strong>Guest</strong></TableCell><TableCell><strong>Room</strong></TableCell>
                  <TableCell><strong>Item</strong></TableCell><TableCell><strong>Qty</strong></TableCell><TableCell><strong>Rate</strong></TableCell>
                  <TableCell><strong>Total</strong></TableCell><TableCell><strong>Date</strong></TableCell><TableCell align="right"><strong>Actions</strong></TableCell>
                </TableRow></TableHead>
                <TableBody>
                  {orders.map((o) => (
                    <TableRow key={o.id} hover>
                      <TableCell>{o.id}</TableCell><TableCell>{o.guestName}</TableCell><TableCell>{o.roomNo}</TableCell>
                      <TableCell>{o.itemName}</TableCell><TableCell>{o.quantity}</TableCell><TableCell>${o.rate.toFixed(2)}</TableCell>
                      <TableCell>${o.totalAmount.toFixed(2)}</TableCell><TableCell>{new Date(o.orderDate).toLocaleDateString()}</TableCell>
                      <TableCell align="right"><IconButton size="small" color="error" onClick={() => handleDelete('room', o.id)}><Delete fontSize="small" /></IconButton></TableCell>
                    </TableRow>
                  ))}
                  {orders.length === 0 && <TableRow><TableCell colSpan={9} align="center">No room orders</TableCell></TableRow>}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </>
      )}

      {tab === 1 && (
        <>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
            <Button variant="contained" startIcon={<Add />} onClick={() => {
              setDialogType('restaurant');
              setRestForm({ customerName: '', itemName: '', category: '', quantity: 1, rate: 0, notes: '' });
              setDialogOpen(true);
            }}>New Restaurant Order</Button>
          </Box>
          {loading ? <CircularProgress /> : (
            <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
              <Table size="small">
                <TableHead><TableRow sx={{ bgcolor: '#f5f5f5' }}>
                  <TableCell><strong>ID</strong></TableCell><TableCell><strong>Customer</strong></TableCell>
                  <TableCell><strong>Item</strong></TableCell><TableCell><strong>Qty</strong></TableCell><TableCell><strong>Rate</strong></TableCell>
                  <TableCell><strong>Total</strong></TableCell><TableCell><strong>Date</strong></TableCell><TableCell align="right"><strong>Actions</strong></TableCell>
                </TableRow></TableHead>
                <TableBody>
                  {restaurantOrders.map((o) => (
                    <TableRow key={o.id} hover>
                      <TableCell>{o.id}</TableCell><TableCell>{o.customerName}</TableCell>
                      <TableCell>{o.itemName}</TableCell><TableCell>{o.quantity}</TableCell><TableCell>${o.rate.toFixed(2)}</TableCell>
                      <TableCell>${o.totalAmount.toFixed(2)}</TableCell><TableCell>{new Date(o.orderDate).toLocaleDateString()}</TableCell>
                      <TableCell align="right"><IconButton size="small" color="error" onClick={() => handleDelete('restaurant', o.id)}><Delete fontSize="small" /></IconButton></TableCell>
                    </TableRow>
                  ))}
                  {restaurantOrders.length === 0 && <TableRow><TableCell colSpan={8} align="center">No restaurant orders</TableCell></TableRow>}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </>
      )}

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{dialogType === 'room' ? 'New Room Order' : 'New Restaurant Order'}</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 0.5 }}>
            {dialogType === 'room' ? (
              <>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <FormControl fullWidth><InputLabel>Guest</InputLabel>
                    <Select value={roomForm.guestID} label="Guest" onChange={(e) => {
                      const g = guests.find((g) => g.guestID === e.target.value);
                      setRoomForm({ ...roomForm, guestID: e.target.value, guestName: g?.guestName || '' });
                    }}>
                      {guests.map((g) => <MenuItem key={g.guestID} value={g.guestID}>{g.guestName}</MenuItem>)}
                    </Select>
                  </FormControl>
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <FormControl fullWidth><InputLabel>Room</InputLabel>
                    <Select value={roomForm.roomNo} label="Room" onChange={(e) => setRoomForm({ ...roomForm, roomNo: e.target.value })}>
                      {rooms.map((r) => <MenuItem key={r.roomNo} value={r.roomNo}>{r.roomNo}</MenuItem>)}
                    </Select>
                  </FormControl>
                </Grid>
              </>
            ) : (
              <Grid size={12}>
                <TextField fullWidth label="Customer Name" value={restForm.customerName} onChange={(e) => setRestForm({ ...restForm, customerName: e.target.value })} />
              </Grid>
            )}
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth><InputLabel>Item</InputLabel>
                <Select value={dialogType === 'room' ? roomForm.itemName : restForm.itemName} label="Item" onChange={(e) => handleDishSelect(e.target.value)}>
                  {dishes.map((d) => <MenuItem key={d.id} value={d.dishName}>{d.dishName} - ${d.rate}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 6, sm: 3 }}>
              <TextField fullWidth label="Quantity" type="number" value={dialogType === 'room' ? roomForm.quantity : restForm.quantity}
                onChange={(e) => {
                  const qty = parseInt(e.target.value) || 0;
                  if (dialogType === 'room') setRoomForm({ ...roomForm, quantity: qty });
                  else setRestForm({ ...restForm, quantity: qty });
                }} />
            </Grid>
            <Grid size={{ xs: 6, sm: 3 }}>
              <TextField fullWidth label="Rate" type="number" value={dialogType === 'room' ? roomForm.rate : restForm.rate}
                onChange={(e) => {
                  const rate = parseFloat(e.target.value) || 0;
                  if (dialogType === 'room') setRoomForm({ ...roomForm, rate });
                  else setRestForm({ ...restForm, rate });
                }} />
            </Grid>
            <Grid size={12}>
              <TextField fullWidth label="Notes" multiline rows={2}
                value={dialogType === 'room' ? roomForm.notes : restForm.notes}
                onChange={(e) => {
                  if (dialogType === 'room') setRoomForm({ ...roomForm, notes: e.target.value });
                  else setRestForm({ ...restForm, notes: e.target.value });
                }} />
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

export default OrdersPage;
