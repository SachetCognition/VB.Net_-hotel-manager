import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
  Drawer, List, ListItemButton, ListItemIcon, ListItemText, Toolbar,
  Divider, Typography, Box, Collapse,
} from '@mui/material';
import {
  Dashboard, People, MeetingRoom, BookOnline, Login as LoginIcon,
  Logout as LogoutIcon, Person, EventNote, AttachMoney, Inventory2,
  Restaurant, Receipt, Assessment, Settings, AccountBalance,
  CalendarMonth, AdminPanelSettings, ExpandLess, ExpandMore, Hotel,
} from '@mui/icons-material';
import { useAuth } from '../../contexts/AuthContext';

const DRAWER_WIDTH = 260;

interface MenuItem {
  text: string;
  icon: React.ReactElement;
  path: string;
  adminOnly?: boolean;
  children?: MenuItem[];
}

const menuItems: MenuItem[] = [
  { text: 'Dashboard', icon: <Dashboard />, path: '/dashboard' },
  { text: 'Guests', icon: <People />, path: '/guests' },
  { text: 'Rooms', icon: <MeetingRoom />, path: '/rooms' },
  { text: 'Reservations', icon: <BookOnline />, path: '/reservations' },
  { text: 'Check-In', icon: <LoginIcon />, path: '/checkin' },
  { text: 'Check-Out', icon: <LogoutIcon />, path: '/checkout' },
  { text: 'Employees', icon: <Person />, path: '/employees' },
  { text: 'Attendance', icon: <EventNote />, path: '/attendance' },
  { text: 'Payroll', icon: <AttachMoney />, path: '/payroll' },
  { text: 'Inventory', icon: <Inventory2 />, path: '/inventory' },
  { text: 'Orders', icon: <Restaurant />, path: '/orders' },
  { text: 'Transactions', icon: <Receipt />, path: '/transactions' },
  { text: 'Reports', icon: <Assessment />, path: '/reports' },
  { text: 'Hall & Garden', icon: <AccountBalance />, path: '/hall-garden' },
  { text: 'Schedule', icon: <CalendarMonth />, path: '/schedule' },
  { text: 'Settings', icon: <Settings />, path: '/settings', adminOnly: true },
  { text: 'User Management', icon: <AdminPanelSettings />, path: '/users', adminOnly: true },
];

interface SidebarProps {
  open: boolean;
  onClose: () => void;
}

const Sidebar: React.FC<SidebarProps> = ({ open, onClose }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const { isAdmin } = useAuth();

  const filteredItems = menuItems.filter((item) => !item.adminOnly || isAdmin);

  return (
    <Drawer
      variant="persistent"
      open={open}
      onClose={onClose}
      sx={{
        width: DRAWER_WIDTH,
        flexShrink: 0,
        '& .MuiDrawer-paper': {
          width: DRAWER_WIDTH,
          boxSizing: 'border-box',
          background: 'linear-gradient(180deg, #1a237e 0%, #0d47a1 100%)',
          color: '#fff',
        },
      }}
    >
      <Toolbar>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, py: 1 }}>
          <Hotel sx={{ fontSize: 32, color: '#ffd54f' }} />
          <Typography variant="h6" noWrap sx={{ fontWeight: 700, color: '#fff' }}>
            HMS
          </Typography>
        </Box>
      </Toolbar>
      <Divider sx={{ borderColor: 'rgba(255,255,255,0.15)' }} />
      <List sx={{ px: 1 }}>
        {filteredItems.map((item) => {
          const isActive = location.pathname === item.path ||
            location.pathname.startsWith(item.path + '/');
          return (
            <ListItemButton
              key={item.path}
              onClick={() => navigate(item.path)}
              sx={{
                borderRadius: 1,
                mb: 0.5,
                backgroundColor: isActive ? 'rgba(255,255,255,0.15)' : 'transparent',
                '&:hover': { backgroundColor: 'rgba(255,255,255,0.1)' },
              }}
            >
              <ListItemIcon sx={{ color: isActive ? '#ffd54f' : 'rgba(255,255,255,0.7)', minWidth: 40 }}>
                {item.icon}
              </ListItemIcon>
              <ListItemText
                primary={item.text}
                primaryTypographyProps={{
                  fontSize: 14,
                  fontWeight: isActive ? 600 : 400,
                  color: isActive ? '#fff' : 'rgba(255,255,255,0.85)',
                }}
              />
            </ListItemButton>
          );
        })}
      </List>
    </Drawer>
  );
};

export default Sidebar;
export { DRAWER_WIDTH };
