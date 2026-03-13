import { useState, useEffect } from 'react';
import {
  ThemeProvider, createTheme, CssBaseline,
  Box, Container, Typography, Tabs, Tab,
  AppBar, Toolbar, Chip,
} from '@mui/material';
import InventoryIcon from '@mui/icons-material/Inventory';
import WarehouseIcon from '@mui/icons-material/Warehouse';
import SwapHorizIcon from '@mui/icons-material/SwapHoriz';
import LocalShippingIcon from '@mui/icons-material/LocalShipping';
import SummaryCards from './components/SummaryCards';
import ProductsTab from './components/ProductsTab';
import WarehousesTab from './components/WarehousesTab';
import TransactionsTab from './components/TransactionsTab';
import { getDashboardSummary } from './api/services';
import { COMPANY_ID } from './api/apiClient';
import type { DashboardSummary } from './api/types';

const darkTheme = createTheme({
  palette: {
    mode: 'dark',
    primary: { main: '#6366f1' },
    success: { main: '#4ade80' },
    error: { main: '#f87171' },
    background: { default: '#0f1117', paper: '#1a1d27' },
    text: { primary: '#e4e4e7', secondary: '#71717a' },
  },
  typography: {
    fontFamily: "'Inter', -apple-system, sans-serif",
  },
  components: {
    MuiOutlinedInput: {
      styleOverrides: {
        root: {
          '& fieldset': { borderColor: 'rgba(255,255,255,0.12)' },
          '&:hover fieldset': { borderColor: 'rgba(255,255,255,0.25) !important' },
        },
      },
    },
    MuiTableCell: {
      styleOverrides: {
        root: { borderColor: 'rgba(255,255,255,0.07)' },
      },
    },
  },
});

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel({ children, value, index }: TabPanelProps) {
  return (
    <Box role="tabpanel" hidden={value !== index} sx={{ pt: 2 }}>
      {value === index && children}
    </Box>
  );
}

export default function App() {
  const [tab, setTab] = useState(0);
  const [summary, setSummary] = useState<DashboardSummary | null>(null);

  const loadSummary = async () => {
    try {
      const data = await getDashboardSummary();
      setSummary(data);
    } catch { /* API henüz başlamamış olabilir */ }
  };

  useEffect(() => {
    loadSummary();
    const interval = setInterval(loadSummary, 30000); // 30s'de bir yenile
    return () => clearInterval(interval);
  }, []);

  // Tab değişince dashboard'u yenile
  const handleTabChange = (_: React.SyntheticEvent, newVal: number) => {
    setTab(newVal);
    if (newVal === 0) loadSummary();
  };

  return (
    <ThemeProvider theme={darkTheme}>
      <CssBaseline />

      {/* APP BAR */}
      <AppBar
        position="sticky"
        elevation={0}
        sx={{
          background: 'rgba(15,17,23,0.85)',
          backdropFilter: 'blur(12px)',
          borderBottom: '1px solid rgba(255,255,255,0.07)',
        }}
      >
        <Toolbar>
          <LocalShippingIcon sx={{ color: '#6366f1', mr: 1.5, fontSize: 28 }} />
          <Typography variant="h6" fontWeight={800} sx={{
            background: 'linear-gradient(135deg, #6366f1, #a78bfa)',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
            mr: 2,
          }}>
            SmartWarehouse
          </Typography>
          <Chip
            label={`Şirket: ${COMPANY_ID}`}
            size="small"
            sx={{ background: 'rgba(99,102,241,0.15)', color: '#818cf8', border: '1px solid rgba(99,102,241,0.3)', fontSize: 11 }}
          />
        </Toolbar>
      </AppBar>

      <Container maxWidth="xl" sx={{ py: 3 }}>
        {/* SUMMARY CARDS */}
        <SummaryCards summary={summary} />

        {/* TABS */}
        <Box sx={{
          background: 'rgba(26,29,39,0.9)',
          border: '1px solid rgba(255,255,255,0.07)',
          borderRadius: 3,
          overflow: 'hidden',
        }}>
          <Tabs
            value={tab} onChange={handleTabChange}
            sx={{
              borderBottom: '1px solid rgba(255,255,255,0.07)',
              px: 2,
              '& .MuiTab-root': { fontWeight: 600, minHeight: 52 },
              '& .Mui-selected': { color: '#818cf8 !important' },
              '& .MuiTabs-indicator': { background: '#6366f1' },
            }}
          >
            <Tab icon={<InventoryIcon sx={{ fontSize: 18 }} />} iconPosition="start" label="Ürünler" />
            <Tab icon={<WarehouseIcon sx={{ fontSize: 18 }} />} iconPosition="start" label="Depolar" />
            <Tab icon={<SwapHorizIcon sx={{ fontSize: 18 }} />} iconPosition="start" label="Stok Hareketleri" />
          </Tabs>

          <Box sx={{ p: 3 }}>
            <TabPanel value={tab} index={0}>
              <ProductsTab />
            </TabPanel>
            <TabPanel value={tab} index={1}>
              <WarehousesTab />
            </TabPanel>
            <TabPanel value={tab} index={2}>
              <TransactionsTab />
            </TabPanel>
          </Box>
        </Box>

        {/* FOOTER */}
        <Typography variant="caption" color="text.secondary" sx={{ display: 'block', textAlign: 'center', mt: 3 }}>
          SmartWarehouse Management System · .NET 9 + React 18 + MUI
        </Typography>
      </Container>
    </ThemeProvider>
  );
}
