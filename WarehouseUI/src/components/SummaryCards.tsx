import { Card, CardContent, Typography, Box, Chip } from '@mui/material';
import InventoryIcon from '@mui/icons-material/Inventory';
import WarehouseIcon from '@mui/icons-material/Warehouse';
import SwapHorizIcon from '@mui/icons-material/SwapHoriz';
import WarningAmberIcon from '@mui/icons-material/WarningAmber';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import type { DashboardSummary } from '../api/types';

interface Props {
  summary: DashboardSummary | null;
}

interface StatCard {
  label: string;
  value: number | string;
  icon: React.ReactNode;
  color: string;
  bgColor: string;
  chip?: string;
  chipColor?: 'error' | 'success' | 'warning' | 'info';
}

export default function SummaryCards({ summary }: Props) {
  if (!summary) return null;

  const cards: StatCard[] = [
    {
      label: 'Ürün Çeşidi',
      value: summary.totalProducts,
      icon: <InventoryIcon sx={{ fontSize: 32 }} />,
      color: '#818cf8',
      bgColor: 'rgba(99,102,241,0.12)',
    },
    {
      label: 'Toplam Depo',
      value: summary.totalWarehouses,
      icon: <WarehouseIcon sx={{ fontSize: 32 }} />,
      color: '#34d399',
      bgColor: 'rgba(52,211,153,0.12)',
    },
    {
      label: 'Bugünkü Hareket',
      value: summary.todayTransactions,
      icon: <SwapHorizIcon sx={{ fontSize: 32 }} />,
      color: '#60a5fa',
      bgColor: 'rgba(96,165,250,0.12)',
    },
    {
      label: 'Düşük Stok',
      value: summary.lowStockCount,
      icon: <WarningAmberIcon sx={{ fontSize: 32 }} />,
      color: '#f87171',
      bgColor: 'rgba(239,68,68,0.12)',
      chip: summary.lowStockCount > 0 ? 'Dikkat!' : 'Normal',
      chipColor: summary.lowStockCount > 0 ? 'error' : 'success',
    },
    {
      label: 'Toplam Giriş',
      value: summary.totalStockIn,
      icon: <TrendingUpIcon sx={{ fontSize: 32 }} />,
      color: '#4ade80',
      bgColor: 'rgba(74,222,128,0.12)',
    },
    {
      label: 'Toplam Çıkış',
      value: summary.totalStockOut,
      icon: <TrendingDownIcon sx={{ fontSize: 32 }} />,
      color: '#fb923c',
      bgColor: 'rgba(251,146,60,0.12)',
    },
  ];

  return (
    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, mb: 3 }}>
      {cards.map((card) => (
        <Box key={card.label} sx={{ flex: '1 1 280px', minWidth: 200 }}>
          <Card
            sx={{
              background: 'rgba(26,29,39,0.9)',
              border: '1px solid rgba(255,255,255,0.07)',
              borderRadius: 3,
              transition: 'transform 0.2s, box-shadow 0.2s',
              '&:hover': {
                transform: 'translateY(-4px)',
                boxShadow: `0 8px 24px ${card.bgColor}`,
              },
            }}
          >
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
                <Box
                  sx={{
                    width: 56,
                    height: 56,
                    borderRadius: 2,
                    background: card.bgColor,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    color: card.color,
                  }}
                >
                  {card.icon}
                </Box>
                {card.chip && (
                  <Chip label={card.chip} color={card.chipColor} size="small" />
                )}
              </Box>
              <Typography variant="h4" fontWeight={700} color={card.color}>
                {card.value}
              </Typography>
              <Typography variant="body2" color="text.secondary" mt={0.5}>
                {card.label}
              </Typography>
            </CardContent>
          </Card>
        </Box>
      ))}
    </Box>
  );
}
