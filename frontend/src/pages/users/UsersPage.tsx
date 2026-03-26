import React, { useEffect, useState } from 'react';
import {
  Box, Typography, Button, TextField, Dialog, DialogTitle, DialogContent,
  DialogActions, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Paper, Alert, CircularProgress, Grid, FormControl, InputLabel,
  Select, MenuItem, Tabs, Tab, Chip,
} from '@mui/material';
import { PersonAdd, Lock } from '@mui/icons-material';
import api from '../../api/client';
import { RegisterRequest, ChangePasswordRequest, LoginActivity } from '../../types';
import { useAuth } from '../../contexts/AuthContext';

const UsersPage: React.FC = () => {
  const [tab, setTab] = useState(0);
  const [loginActivity, setLoginActivity] = useState<LoginActivity[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [registerOpen, setRegisterOpen] = useState(false);
  const [passwordOpen, setPasswordOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const { user } = useAuth();

  const [regForm, setRegForm] = useState<RegisterRequest>({ username: '', password: '', userType: 'User' });
  const [pwForm, setPwForm] = useState<ChangePasswordRequest>({ username: '', oldPassword: '', newPassword: '' });

  const fetchData = async () => {
    try {
      setLoading(true);
      const res = await api.get<LoginActivity[]>('/Auth/login-activity');
      setLoginActivity(res.data);
    } catch {
      setError('Failed to load login activity');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const handleRegister = async () => {
    setSaving(true);
    setError('');
    try {
      await api.post('/Auth/register', regForm);
      setSuccess('User registered successfully');
      setRegisterOpen(false);
      setRegForm({ username: '', password: '', userType: 'User' });
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to register user');
    } finally {
      setSaving(false);
    }
  };

  const handleChangePassword = async () => {
    setSaving(true);
    setError('');
    try {
      await api.post('/Auth/change-password', pwForm);
      setSuccess('Password changed successfully');
      setPasswordOpen(false);
      setPwForm({ username: '', oldPassword: '', newPassword: '' });
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Failed to change password');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Box>
      <Typography variant="h4" fontWeight={700} color="#1a237e" gutterBottom>User Management</Typography>

      {error && <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>{error}</Alert>}
      {success && <Alert severity="success" onClose={() => setSuccess('')} sx={{ mb: 2 }}>{success}</Alert>}

      <Box sx={{ display: 'flex', gap: 1, mb: 3 }}>
        <Button variant="contained" startIcon={<PersonAdd />} onClick={() => { setRegForm({ username: '', password: '', userType: 'User' }); setRegisterOpen(true); }}>
          Register User
        </Button>
        <Button variant="outlined" startIcon={<Lock />} onClick={() => { setPwForm({ username: user?.username || '', oldPassword: '', newPassword: '' }); setPasswordOpen(true); }}>
          Change Password
        </Button>
      </Box>

      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 2 }}>
        <Tab label="Login Activity" />
      </Tabs>

      {loading ? <CircularProgress /> : (
        <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
          <Table size="small">
            <TableHead>
              <TableRow sx={{ bgcolor: '#f5f5f5' }}>
                <TableCell><strong>ID</strong></TableCell>
                <TableCell><strong>Username</strong></TableCell>
                <TableCell><strong>User Type</strong></TableCell>
                <TableCell><strong>Login Time</strong></TableCell>
                <TableCell><strong>IP Address</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {loginActivity.map((la) => (
                <TableRow key={la.id} hover>
                  <TableCell>{la.id}</TableCell>
                  <TableCell>{la.username}</TableCell>
                  <TableCell><Chip label={la.userType} size="small" color={la.userType === 'Admin' ? 'error' : 'primary'} /></TableCell>
                  <TableCell>{new Date(la.loginTime).toLocaleString()}</TableCell>
                  <TableCell>{la.ipAddress}</TableCell>
                </TableRow>
              ))}
              {loginActivity.length === 0 && <TableRow><TableCell colSpan={5} align="center">No login activity</TableCell></TableRow>}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {/* Register Dialog */}
      <Dialog open={registerOpen} onClose={() => setRegisterOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Register New User</DialogTitle>
        <DialogContent>
          <TextField fullWidth label="Username" value={regForm.username} onChange={(e) => setRegForm({ ...regForm, username: e.target.value })} margin="normal" required />
          <TextField fullWidth label="Password" type="password" value={regForm.password} onChange={(e) => setRegForm({ ...regForm, password: e.target.value })} margin="normal" required />
          <FormControl fullWidth margin="normal">
            <InputLabel>User Type</InputLabel>
            <Select value={regForm.userType} label="User Type" onChange={(e) => setRegForm({ ...regForm, userType: e.target.value })}>
              <MenuItem value="Admin">Admin</MenuItem>
              <MenuItem value="User">User</MenuItem>
            </Select>
          </FormControl>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRegisterOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleRegister} disabled={saving}>
            {saving ? <CircularProgress size={20} /> : 'Register'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Change Password Dialog */}
      <Dialog open={passwordOpen} onClose={() => setPasswordOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Change Password</DialogTitle>
        <DialogContent>
          <TextField fullWidth label="Username" value={pwForm.username} onChange={(e) => setPwForm({ ...pwForm, username: e.target.value })} margin="normal" required />
          <TextField fullWidth label="Old Password" type="password" value={pwForm.oldPassword} onChange={(e) => setPwForm({ ...pwForm, oldPassword: e.target.value })} margin="normal" required />
          <TextField fullWidth label="New Password" type="password" value={pwForm.newPassword} onChange={(e) => setPwForm({ ...pwForm, newPassword: e.target.value })} margin="normal" required />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPasswordOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleChangePassword} disabled={saving}>
            {saving ? <CircularProgress size={20} /> : 'Change Password'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default UsersPage;
