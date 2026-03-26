import React, { useEffect, useState } from 'react';
import {
  Box, Typography, Button, TextField, Dialog, DialogTitle, DialogContent,
  DialogActions, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Paper, IconButton, Alert, CircularProgress, InputAdornment,
  FormControl, InputLabel, Select, MenuItem, Grid,
} from '@mui/material';
import { Add, Edit, Delete, Search, FileDownload } from '@mui/icons-material';
import api from '../../api/client';
import { Employee, CreateEmployeeRequest } from '../../types';

const DEPARTMENTS = ['Front Desk', 'Housekeeping', 'Kitchen', 'Restaurant', 'Maintenance', 'Security', 'Management', 'Accounts'];
const DESIGNATIONS = ['Manager', 'Supervisor', 'Receptionist', 'Housekeeper', 'Chef', 'Waiter', 'Security Guard', 'Accountant', 'Bellboy', 'Concierge'];
const GENDERS = ['Male', 'Female', 'Other'];
const BLOOD_GROUPS = ['A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-'];

const emptyForm: CreateEmployeeRequest = {
  employeeName: '', address: '', mobileNo: '', email: '', bloodGroup: '',
  gender: '', department: '', designation: '', dateOfJoining: '', salary: 0, basicWorkingTime: '8',
};

const EmployeesPage: React.FC = () => {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<CreateEmployeeRequest>(emptyForm);
  const [saving, setSaving] = useState(false);

  const fetchEmployees = async () => {
    try {
      setLoading(true);
      const res = await api.get<Employee[]>('/Employees');
      setEmployees(res.data);
    } catch {
      setError('Failed to load employees');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchEmployees(); }, []);

  const handleSave = async () => {
    setSaving(true);
    try {
      if (editingId) {
        await api.put(`/Employees/${editingId}`, form);
      } else {
        await api.post('/Employees', form);
      }
      setDialogOpen(false);
      setEditingId(null);
      setForm(emptyForm);
      fetchEmployees();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to save employee');
    } finally {
      setSaving(false);
    }
  };

  const handleEdit = (emp: Employee) => {
    setEditingId(emp.employeeID);
    setForm({
      employeeName: emp.employeeName, address: emp.address, mobileNo: emp.mobileNo,
      email: emp.email, bloodGroup: emp.bloodGroup, gender: emp.gender,
      department: emp.department, designation: emp.designation,
      dateOfJoining: emp.dateOfJoining.split('T')[0], salary: emp.salary,
      basicWorkingTime: emp.basicWorkingTime,
    });
    setDialogOpen(true);
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Delete this employee?')) return;
    try {
      await api.delete(`/Employees/${id}`);
      fetchEmployees();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Cannot delete employee with existing records');
    }
  };

  const handleExport = async () => {
    try {
      const res = await api.get('/Employees/export', { responseType: 'blob' });
      const url = window.URL.createObjectURL(new Blob([res.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', 'employees.xlsx');
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch {
      setError('Failed to export');
    }
  };

  const filtered = employees.filter((e) =>
    e.employeeName.toLowerCase().includes(search.toLowerCase()) ||
    e.employeeID.toLowerCase().includes(search.toLowerCase()) ||
    e.department.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight={700} color="#1a237e">Employee Management</Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button variant="outlined" startIcon={<FileDownload />} onClick={handleExport}>Export</Button>
          <Button variant="contained" startIcon={<Add />} onClick={() => { setEditingId(null); setForm(emptyForm); setDialogOpen(true); }}>Add Employee</Button>
        </Box>
      </Box>

      {error && <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>{error}</Alert>}

      <TextField fullWidth placeholder="Search employees..." value={search} onChange={(e) => setSearch(e.target.value)}
        InputProps={{ startAdornment: <InputAdornment position="start"><Search /></InputAdornment> }} sx={{ mb: 2 }} />

      {loading ? <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}><CircularProgress /></Box> : (
        <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
          <Table size="small">
            <TableHead>
              <TableRow sx={{ bgcolor: '#f5f5f5' }}>
                <TableCell><strong>ID</strong></TableCell>
                <TableCell><strong>Name</strong></TableCell>
                <TableCell><strong>Department</strong></TableCell>
                <TableCell><strong>Designation</strong></TableCell>
                <TableCell><strong>Mobile</strong></TableCell>
                <TableCell><strong>Salary</strong></TableCell>
                <TableCell align="right"><strong>Actions</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filtered.map((emp) => (
                <TableRow key={emp.employeeID} hover>
                  <TableCell>{emp.employeeID}</TableCell>
                  <TableCell>{emp.employeeName}</TableCell>
                  <TableCell>{emp.department}</TableCell>
                  <TableCell>{emp.designation}</TableCell>
                  <TableCell>{emp.mobileNo}</TableCell>
                  <TableCell>${emp.salary.toFixed(2)}</TableCell>
                  <TableCell align="right">
                    <IconButton size="small" onClick={() => handleEdit(emp)}><Edit fontSize="small" /></IconButton>
                    <IconButton size="small" color="error" onClick={() => handleDelete(emp.employeeID)}><Delete fontSize="small" /></IconButton>
                  </TableCell>
                </TableRow>
              ))}
              {filtered.length === 0 && <TableRow><TableCell colSpan={7} align="center">No employees found</TableCell></TableRow>}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>{editingId ? 'Edit Employee' : 'Add Employee'}</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 0.5 }}>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="Name" value={form.employeeName} onChange={(e) => setForm({ ...form, employeeName: e.target.value })} required /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="Email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label="Mobile" value={form.mobileNo} onChange={(e) => setForm({ ...form, mobileNo: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth><InputLabel>Gender</InputLabel>
                <Select value={form.gender} label="Gender" onChange={(e) => setForm({ ...form, gender: e.target.value })}>
                  {GENDERS.map((g) => <MenuItem key={g} value={g}>{g}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12 }}><TextField fullWidth label="Address" value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} multiline rows={2} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth><InputLabel>Department</InputLabel>
                <Select value={form.department} label="Department" onChange={(e) => setForm({ ...form, department: e.target.value })}>
                  {DEPARTMENTS.map((d) => <MenuItem key={d} value={d}>{d}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth><InputLabel>Designation</InputLabel>
                <Select value={form.designation} label="Designation" onChange={(e) => setForm({ ...form, designation: e.target.value })}>
                  {DESIGNATIONS.map((d) => <MenuItem key={d} value={d}>{d}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}><TextField fullWidth label="Date of Joining" type="date" value={form.dateOfJoining} onChange={(e) => setForm({ ...form, dateOfJoining: e.target.value })} InputLabelProps={{ shrink: true }} /></Grid>
            <Grid size={{ xs: 12, sm: 4 }}><TextField fullWidth label="Salary" type="number" value={form.salary} onChange={(e) => setForm({ ...form, salary: parseFloat(e.target.value) || 0 })} /></Grid>
            <Grid size={{ xs: 12, sm: 4 }}><TextField fullWidth label="Basic Working Time (hrs)" value={form.basicWorkingTime} onChange={(e) => setForm({ ...form, basicWorkingTime: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth><InputLabel>Blood Group</InputLabel>
                <Select value={form.bloodGroup} label="Blood Group" onChange={(e) => setForm({ ...form, bloodGroup: e.target.value })}>
                  {BLOOD_GROUPS.map((b) => <MenuItem key={b} value={b}>{b}</MenuItem>)}
                </Select>
              </FormControl>
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

export default EmployeesPage;
