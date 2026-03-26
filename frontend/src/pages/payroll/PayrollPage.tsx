import React, { useEffect, useState } from 'react';
import {
  Box, Typography, Button, TextField, Dialog, DialogTitle, DialogContent,
  DialogActions, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Paper, IconButton, Alert, CircularProgress, Grid,
  FormControl, InputLabel, Select, MenuItem, Tabs, Tab,
} from '@mui/material';
import { Add, Delete } from '@mui/icons-material';
import api from '../../api/client';
import { PaymentRecord, ProcessPaymentRequest, AdvanceEntry, AdvanceEntryRequest, Employee } from '../../types';

const PayrollPage: React.FC = () => {
  const [tab, setTab] = useState(0);
  const [payments, setPayments] = useState<PaymentRecord[]>([]);
  const [advances, setAdvances] = useState<AdvanceEntry[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [payDialogOpen, setPayDialogOpen] = useState(false);
  const [advDialogOpen, setAdvDialogOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [payForm, setPayForm] = useState<ProcessPaymentRequest>({
    employeeID: '', fromDate: '', toDate: '', overtimeRate: 0, deduction: 0,
  });
  const [advForm, setAdvForm] = useState<AdvanceEntryRequest>({
    employeeID: '', employeeName: '', workingDate: '', amount: 0,
  });
  const [advBalance, setAdvBalance] = useState<number | null>(null);

  const fetchData = async () => {
    try {
      setLoading(true);
      const [payRes, advRes, empRes] = await Promise.all([
        api.get<PaymentRecord[]>('/Payroll/payments'),
        api.get<AdvanceEntry[]>('/Payroll/advance'),
        api.get<Employee[]>('/Employees'),
      ]);
      setPayments(payRes.data);
      setAdvances(advRes.data);
      setEmployees(empRes.data);
    } catch {
      setError('Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const handleProcessPayment = async () => {
    setSaving(true);
    try {
      await api.post('/Payroll/process', payForm);
      setPayDialogOpen(false);
      fetchData();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to process payment');
    } finally {
      setSaving(false);
    }
  };

  const handleAdvanceEntry = async () => {
    setSaving(true);
    try {
      await api.post('/Payroll/advance', advForm);
      setAdvDialogOpen(false);
      fetchData();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to save advance');
    } finally {
      setSaving(false);
    }
  };

  const handleDeletePayment = async (id: number) => {
    if (!window.confirm('Delete this payment record?')) return;
    try {
      await api.delete(`/Payroll/payments/${id}`);
      fetchData();
    } catch {
      setError('Failed to delete');
    }
  };

  const checkAdvanceBalance = async (employeeID: string) => {
    try {
      const res = await api.get<{ balance: number }>(`/Payroll/advance/balance/${employeeID}`);
      setAdvBalance(res.data.balance);
    } catch {
      setAdvBalance(null);
    }
  };

  const handleAdvEmployeeSelect = (employeeID: string) => {
    const emp = employees.find((e) => e.employeeID === employeeID);
    setAdvForm({ ...advForm, employeeID, employeeName: emp?.employeeName || '' });
    checkAdvanceBalance(employeeID);
  };

  return (
    <Box>
      <Typography variant="h4" fontWeight={700} color="#1a237e" gutterBottom>Payroll</Typography>

      {error && <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>{error}</Alert>}

      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 2 }}>
        <Tab label="Payment Records" />
        <Tab label="Advance Payments" />
      </Tabs>

      {tab === 0 && (
        <>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
            <Button variant="contained" startIcon={<Add />} onClick={() => {
              setPayForm({ employeeID: '', fromDate: '', toDate: '', overtimeRate: 0, deduction: 0 });
              setPayDialogOpen(true);
            }}>Process Payment</Button>
          </Box>

          {loading ? <CircularProgress /> : (
            <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
              <Table size="small">
                <TableHead>
                  <TableRow sx={{ bgcolor: '#f5f5f5' }}>
                    <TableCell><strong>Payment ID</strong></TableCell>
                    <TableCell><strong>Employee</strong></TableCell>
                    <TableCell><strong>Department</strong></TableCell>
                    <TableCell><strong>Period</strong></TableCell>
                    <TableCell><strong>Basic Salary</strong></TableCell>
                    <TableCell><strong>Overtime</strong></TableCell>
                    <TableCell><strong>Deductions</strong></TableCell>
                    <TableCell><strong>Net Pay</strong></TableCell>
                    <TableCell align="right"><strong>Actions</strong></TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {payments.map((p) => (
                    <TableRow key={p.id} hover>
                      <TableCell>{p.paymentID}</TableCell>
                      <TableCell>{p.employeeName}</TableCell>
                      <TableCell>{p.department}</TableCell>
                      <TableCell>{new Date(p.fromDate).toLocaleDateString()} - {new Date(p.toDate).toLocaleDateString()}</TableCell>
                      <TableCell>${p.basicSalary.toFixed(2)}</TableCell>
                      <TableCell>${p.overtimeAmount.toFixed(2)}</TableCell>
                      <TableCell>${(p.deduction + p.advance).toFixed(2)}</TableCell>
                      <TableCell><strong>${p.netPay.toFixed(2)}</strong></TableCell>
                      <TableCell align="right">
                        <IconButton size="small" color="error" onClick={() => handleDeletePayment(p.id)}><Delete fontSize="small" /></IconButton>
                      </TableCell>
                    </TableRow>
                  ))}
                  {payments.length === 0 && <TableRow><TableCell colSpan={9} align="center">No payment records</TableCell></TableRow>}
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
              setAdvForm({ employeeID: '', employeeName: '', workingDate: '', amount: 0 });
              setAdvBalance(null);
              setAdvDialogOpen(true);
            }}>New Advance</Button>
          </Box>

          {loading ? <CircularProgress /> : (
            <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
              <Table size="small">
                <TableHead>
                  <TableRow sx={{ bgcolor: '#f5f5f5' }}>
                    <TableCell><strong>ID</strong></TableCell>
                    <TableCell><strong>Employee</strong></TableCell>
                    <TableCell><strong>Date</strong></TableCell>
                    <TableCell><strong>Amount</strong></TableCell>
                    <TableCell><strong>Deduction</strong></TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {advances.map((a) => (
                    <TableRow key={a.id} hover>
                      <TableCell>{a.id}</TableCell>
                      <TableCell>{a.employeeName}</TableCell>
                      <TableCell>{new Date(a.workingDate).toLocaleDateString()}</TableCell>
                      <TableCell>${a.amount.toFixed(2)}</TableCell>
                      <TableCell>${a.deduction.toFixed(2)}</TableCell>
                    </TableRow>
                  ))}
                  {advances.length === 0 && <TableRow><TableCell colSpan={5} align="center">No advance entries</TableCell></TableRow>}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </>
      )}

      {/* Process Payment Dialog */}
      <Dialog open={payDialogOpen} onClose={() => setPayDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Process Payment</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 0.5 }}>
            <Grid size={12}>
              <FormControl fullWidth>
                <InputLabel>Employee</InputLabel>
                <Select value={payForm.employeeID} label="Employee" onChange={(e) => setPayForm({ ...payForm, employeeID: e.target.value })}>
                  {employees.map((emp) => <MenuItem key={emp.employeeID} value={emp.employeeID}>{emp.employeeName} ({emp.employeeID})</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField fullWidth label="From Date" type="date" value={payForm.fromDate} onChange={(e) => setPayForm({ ...payForm, fromDate: e.target.value })} InputLabelProps={{ shrink: true }} />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField fullWidth label="To Date" type="date" value={payForm.toDate} onChange={(e) => setPayForm({ ...payForm, toDate: e.target.value })} InputLabelProps={{ shrink: true }} />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField fullWidth label="Overtime Rate" type="number" value={payForm.overtimeRate} onChange={(e) => setPayForm({ ...payForm, overtimeRate: parseFloat(e.target.value) || 0 })} />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField fullWidth label="Deduction" type="number" value={payForm.deduction} onChange={(e) => setPayForm({ ...payForm, deduction: parseFloat(e.target.value) || 0 })} />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPayDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleProcessPayment} disabled={saving}>
            {saving ? <CircularProgress size={20} /> : 'Process'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Advance Entry Dialog */}
      <Dialog open={advDialogOpen} onClose={() => setAdvDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Advance Payment</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 0.5 }}>
            <Grid size={12}>
              <FormControl fullWidth>
                <InputLabel>Employee</InputLabel>
                <Select value={advForm.employeeID} label="Employee" onChange={(e) => handleAdvEmployeeSelect(e.target.value)}>
                  {employees.map((emp) => <MenuItem key={emp.employeeID} value={emp.employeeID}>{emp.employeeName} ({emp.employeeID})</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            {advBalance !== null && (
              <Grid size={12}>
                <Alert severity="info">Current advance balance: ${advBalance.toFixed(2)}</Alert>
              </Grid>
            )}
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField fullWidth label="Date" type="date" value={advForm.workingDate} onChange={(e) => setAdvForm({ ...advForm, workingDate: e.target.value })} InputLabelProps={{ shrink: true }} />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField fullWidth label="Amount" type="number" value={advForm.amount} onChange={(e) => setAdvForm({ ...advForm, amount: parseFloat(e.target.value) || 0 })} />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAdvDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleAdvanceEntry} disabled={saving}>
            {saving ? <CircularProgress size={20} /> : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default PayrollPage;
