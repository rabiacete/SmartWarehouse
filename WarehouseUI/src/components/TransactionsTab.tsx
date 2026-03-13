import { useState, useEffect, useCallback } from 'react';
import {
  Box, Button, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow, Paper, TablePagination,
  CircularProgress, Typography, Chip, Dialog,
  DialogTitle, DialogContent, DialogActions,
  MenuItem, Select, FormControl, InputLabel,
  ToggleButtonGroup, ToggleButton, Stack,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import type { StockTransactionResponse, ProductResponse, WarehouseResponse } from '../api/types';
import { getStockTransactions, createStockTransaction, getProducts, getWarehouses } from '../api/services';

type FilterType = 1 | 2 | 'all';

export default function TransactionsTab() {
  const [rows, setRows] = useState<StockTransactionResponse[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(25);
  const [typeFilter, setTypeFilter] = useState<FilterType>('all');
  const [loading, setLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [snack, setSnack] = useState('');
  const [snackColor, setSnackColor] = useState<'success' | 'error'>('success');

  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [warehouses, setWarehouses] = useState<WarehouseResponse[]>([]);
  const [form, setForm] = useState({ productId: 0, warehouseId: 0, transactionType: 1, quantity: 1, note: '' });

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const filterVal = typeFilter === 'all' ? undefined : typeFilter;
      const res = await getStockTransactions(page + 1, pageSize, undefined, undefined, filterVal);
      setRows(res.data);
      setTotal(res.totalCount);
    } catch {
      setSnack('Veriler yüklenemedi.');
      setSnackColor('error');
    } finally { setLoading(false); }
  }, [page, pageSize, typeFilter]);

  useEffect(() => { load(); }, [load]);

  const openModal = async () => {
    try {
      const [p, w] = await Promise.all([getProducts(1, 100), getWarehouses(1, 100)]);
      setProducts(p.data);
      setWarehouses(w.data);
      setForm({ productId: p.data[0]?.id ?? 0, warehouseId: w.data[0]?.id ?? 0, transactionType: 1, quantity: 1, note: '' });
      setModalOpen(true);
    } catch {
      setSnack('Ürün/Depo listesi yüklenemedi.');
      setSnackColor('error');
    }
  };

  const handleSave = async () => {
    if (!form.productId || !form.warehouseId || form.quantity <= 0) return;
    setSaving(true);
    try {
      const res = await createStockTransaction({ ...form, quantity: Number(form.quantity) });
      setSnack(res.message);
      setSnackColor('success');
      setModalOpen(false);
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { message?: string } } };
      setSnack(err?.response?.data?.message || 'Bir hata oluştu.');
      setSnackColor('error');
    } finally { setSaving(false); }
  };

  const formatDate = (d: string) =>
    new Date(d).toLocaleString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });

  const tableStyle = { background: 'rgba(26,29,39,0.9)', border: '1px solid rgba(255,255,255,0.07)', borderRadius: 3 };
  const thStyle = { fontWeight: 700, color: 'text.secondary', borderColor: 'rgba(255,255,255,0.07)' };
  const trStyle = { '& td': { borderColor: 'rgba(255,255,255,0.05)' }, '&:hover': { background: 'rgba(255,255,255,0.03)' } };

  return (
    <Box>
      {snack && (
        <Box sx={{ mb: 2, p: 1.5, borderRadius: 2, background: snackColor === 'success' ? 'rgba(74,222,128,0.1)' : 'rgba(239,68,68,0.1)', border: `1px solid ${snackColor === 'success' ? 'rgba(74,222,128,0.3)' : 'rgba(239,68,68,0.3)'}` }}>
          <Typography variant="body2" color={snackColor === 'success' ? 'success.main' : 'error.main'}>{snack}</Typography>
        </Box>
      )}

      <Box sx={{ display: 'flex', gap: 2, mb: 2, flexWrap: 'wrap', alignItems: 'center' }}>
        <ToggleButtonGroup
          value={typeFilter}
          exclusive
          size="small"
          onChange={(_, v: FilterType | null) => { if (v !== null) { setTypeFilter(v); setPage(0); } }}
          sx={{ '& .MuiToggleButton-root': { borderColor: 'rgba(255,255,255,0.12)' } }}
        >
          <ToggleButton value="all">Tümü</ToggleButton>
          <ToggleButton value={1}><TrendingUpIcon sx={{ mr: 0.5, fontSize: 16 }} />Giriş</ToggleButton>
          <ToggleButton value={2}><TrendingDownIcon sx={{ mr: 0.5, fontSize: 16 }} />Çıkış</ToggleButton>
        </ToggleButtonGroup>
        <Box sx={{ flex: 1 }} />
        <Button variant="contained" startIcon={<AddIcon />} onClick={openModal}>
          Yeni Hareket
        </Button>
      </Box>

      <TableContainer component={Paper} sx={tableStyle}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}><CircularProgress /></Box>
        ) : (
          <Table size="small">
            <TableHead>
              <TableRow sx={{ '& th': thStyle }}>
                <TableCell>Ürün</TableCell>
                <TableCell>Depo</TableCell>
                <TableCell align="center">Tür</TableCell>
                <TableCell align="center">Miktar</TableCell>
                <TableCell>Not</TableCell>
                <TableCell>Tarih</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {rows.length === 0 ? (
                <TableRow><TableCell colSpan={6} align="center" sx={{ py: 4, color: 'text.secondary' }}>Kayıt bulunamadı</TableCell></TableRow>
              ) : rows.map(row => (
                <TableRow key={row.id} sx={trStyle}>
                  <TableCell>
                    <Typography variant="body2" fontWeight={600}>{row.productName}</Typography>
                    <Typography variant="caption" color="text.secondary">{row.productSKU}</Typography>
                  </TableCell>
                  <TableCell>{row.warehouseName}</TableCell>
                  <TableCell align="center">
                    {row.transactionType === 'Giriş'
                      ? <Chip label="↑ Giriş" color="success" size="small" />
                      : <Chip label="↓ Çıkış" color="error" size="small" />}
                  </TableCell>
                  <TableCell align="center">
                    <Typography fontWeight={700} color={row.transactionType === 'Giriş' ? 'success.main' : 'error.main'}>
                      {row.transactionType === 'Giriş' ? '+' : '-'}{row.quantity}
                    </Typography>
                  </TableCell>
                  <TableCell sx={{ color: 'text.secondary', fontSize: 12 }}>{row.note || '—'}</TableCell>
                  <TableCell sx={{ color: 'text.secondary', fontSize: 12, whiteSpace: 'nowrap' }}>{formatDate(row.createdAt)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
        <TablePagination
          component="div" count={total} page={page}
          onPageChange={(_, p) => setPage(p)}
          rowsPerPage={pageSize}
          onRowsPerPageChange={e => { setPageSize(+e.target.value); setPage(0); }}
          rowsPerPageOptions={[10, 25, 50]}
          labelRowsPerPage="Sayfa başına:"
          labelDisplayedRows={({ from, to, count }) => `${from}-${to} / ${count}`}
          sx={{ borderTop: '1px solid rgba(255,255,255,0.07)', color: 'text.secondary' }}
        />
      </TableContainer>

      {/* Modal */}
      <Dialog open={modalOpen} onClose={() => setModalOpen(false)} maxWidth="sm" fullWidth
        PaperProps={{ sx: { background: '#1a1d27', border: '1px solid rgba(255,255,255,0.1)', borderRadius: 3 } }}>
        <DialogTitle sx={{ fontWeight: 700 }}>📦 Stok Hareketi Ekle</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <FormControl fullWidth>
              <InputLabel>Ürün *</InputLabel>
              <Select value={form.productId} label="Ürün *" onChange={e => setForm(p => ({ ...p, productId: Number(e.target.value) }))}>
                {products.map(p => (
                  <MenuItem key={p.id} value={p.id}>
                    {p.productName}
                    <Typography component="span" variant="caption" color="text.secondary" sx={{ ml: 1 }}>({p.sku})</Typography>
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            <FormControl fullWidth>
              <InputLabel>Depo *</InputLabel>
              <Select value={form.warehouseId} label="Depo *" onChange={e => setForm(p => ({ ...p, warehouseId: Number(e.target.value) }))}>
                {warehouses.map(w => <MenuItem key={w.id} value={w.id}>{w.name}</MenuItem>)}
              </Select>
            </FormControl>

            <Box sx={{ display: 'flex', gap: 2 }}>
              <ToggleButtonGroup
                exclusive
                value={form.transactionType}
                onChange={(_, v: number) => v && setForm(p => ({ ...p, transactionType: v }))}
                sx={{ flex: 1 }}
              >
                <ToggleButton value={1} sx={{ flex: 1, color: 'success.main', '&.Mui-selected': { background: 'rgba(74,222,128,0.15)', color: 'success.main' } }}>
                  <TrendingUpIcon sx={{ mr: 1 }} /> Giriş
                </ToggleButton>
                <ToggleButton value={2} sx={{ flex: 1, color: 'error.main', '&.Mui-selected': { background: 'rgba(239,68,68,0.15)', color: 'error.main' } }}>
                  <TrendingDownIcon sx={{ mr: 1 }} /> Çıkış
                </ToggleButton>
              </ToggleButtonGroup>

              <Box sx={{ flex: 1 }}>
                <Typography variant="caption" color="text.secondary" sx={{ mb: 0.5, display: 'block' }}>Miktar *</Typography>
                <input
                  type="number" min={1} value={form.quantity}
                  onChange={e => setForm(p => ({ ...p, quantity: Number(e.target.value) }))}
                  style={{
                    width: '100%', padding: '14px 12px', borderRadius: 8,
                    border: '1px solid rgba(255,255,255,0.2)', background: 'rgba(255,255,255,0.05)',
                    color: 'white', fontSize: 16,
                  }}
                />
              </Box>
            </Box>

            <Box>
              <Typography variant="caption" color="text.secondary" sx={{ mb: 0.5, display: 'block' }}>Not</Typography>
              <input
                type="text" value={form.note}
                onChange={e => setForm(p => ({ ...p, note: e.target.value }))}
                placeholder="Opsiyonel not..."
                style={{
                  width: '100%', padding: '14px 12px', borderRadius: 8,
                  border: '1px solid rgba(255,255,255,0.2)', background: 'rgba(255,255,255,0.05)',
                  color: 'white', fontSize: 14,
                }}
              />
            </Box>
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 3, gap: 1 }}>
          <Button onClick={() => setModalOpen(false)} variant="outlined" color="inherit">İptal</Button>
          <Button onClick={handleSave} disabled={saving} variant="contained"
            color={form.transactionType === 1 ? 'success' : 'error'}>
            {saving ? 'Kaydediliyor...' : (form.transactionType === 1 ? '↑ Giriş Ekle' : '↓ Çıkış Ekle')}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
