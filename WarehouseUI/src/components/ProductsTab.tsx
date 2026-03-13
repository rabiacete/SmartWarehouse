import { useState, useEffect, useCallback } from 'react';
import {
  Box, Button, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow, Paper, TablePagination, TextField,
  MenuItem, Select, FormControl, InputLabel, IconButton,
  Tooltip, Chip, CircularProgress, Typography,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import type { ProductResponse } from '../api/types';
import { getProducts, createProduct, updateProduct, deleteProduct } from '../api/services';
import ProductModal from './ProductModal';
import ConfirmDialog from './ConfirmDialog';

const CATEGORIES = ['Elektronik', 'Tekstil', 'Gıda', 'Kimyasal', 'Makine', 'Diğer'];

export default function ProductsTab() {
  const [rows, setRows] = useState<ProductResponse[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(25);
  const [search, setSearch] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [category, setCategory] = useState('');
  const [loading, setLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editItem, setEditItem] = useState<ProductResponse | null>(null);
  const [deleteId, setDeleteId] = useState<number | null>(null);
  const [snack, setSnack] = useState('');

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getProducts(page + 1, pageSize, search || undefined, category || undefined);
      setRows(res.data);
      setTotal(res.totalCount);
    } catch { setSnack('Veriler yüklenemedi.'); }
    finally { setLoading(false); }
  }, [page, pageSize, search, category]);

  useEffect(() => { load(); }, [load]);

  const handleSave = async (data: Omit<ProductResponse, 'id' | 'companyId' | 'currentStock' | 'createdAt' | 'updatedAt'>) => {
    try {
      if (editItem) {
        await updateProduct({ ...data, id: editItem.id });
        setSnack('Ürün güncellendi ✓');
      } else {
        await createProduct(data);
        setSnack('Ürün oluşturuldu ✓');
      }
      setModalOpen(false);
      setEditItem(null);
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { message?: string } } };
      setSnack(err?.response?.data?.message || 'Bir hata oluştu.');
    }
  };

  const handleDelete = async () => {
    if (!deleteId) return;
    try {
      await deleteProduct(deleteId);
      setSnack('Ürün silindi ✓');
      setDeleteId(null);
      load();
    } catch { setSnack('Silme başarısız.'); }
  };

  const tableStyle = { background: 'rgba(26,29,39,0.9)', border: '1px solid rgba(255,255,255,0.07)', borderRadius: 3 };
  const thStyle = { fontWeight: 700, color: 'text.secondary', borderColor: 'rgba(255,255,255,0.07)' };
  const trStyle = { '& td': { borderColor: 'rgba(255,255,255,0.05)' }, '&:hover': { background: 'rgba(255,255,255,0.03)' } };

  return (
    <Box>
      {snack && (
        <Box sx={{ mb: 2, p: 1.5, borderRadius: 2, background: 'rgba(99,102,241,0.12)', border: '1px solid rgba(99,102,241,0.3)' }}>
          <Typography variant="body2" color="primary.light">{snack}</Typography>
        </Box>
      )}

      {/* TOOLBAR */}
      <Box sx={{ display: 'flex', gap: 2, mb: 2, flexWrap: 'wrap' }}>
        <TextField
          size="small" placeholder="Ürün adı veya SKU ara..."
          value={searchInput}
          onChange={e => setSearchInput(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && setSearch(searchInput)}
          sx={{ flex: 1, minWidth: 200 }}
        />
        <FormControl size="small" sx={{ minWidth: 140 }}>
          <InputLabel>Kategori</InputLabel>
          <Select value={category} label="Kategori"
            onChange={e => { setCategory(e.target.value); setPage(0); }}>
            <MenuItem value="">Tümü</MenuItem>
            {CATEGORIES.map(c => <MenuItem key={c} value={c}>{c}</MenuItem>)}
          </Select>
        </FormControl>
        <Button variant="outlined" onClick={() => setSearch(searchInput)}>Ara</Button>
        <Button variant="contained" startIcon={<AddIcon />}
          onClick={() => { setEditItem(null); setModalOpen(true); }}>
          Yeni Ürün
        </Button>
      </Box>

      {/* TABLE */}
      <TableContainer component={Paper} sx={tableStyle}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}><CircularProgress /></Box>
        ) : (
          <Table size="small">
            <TableHead>
              <TableRow sx={{ '& th': thStyle }}>
                <TableCell>Ürün Adı</TableCell>
                <TableCell>SKU</TableCell>
                <TableCell>Kategori</TableCell>
                <TableCell>Birim</TableCell>
                <TableCell align="center">Mevcut Stok</TableCell>
                <TableCell align="center">Min. Stok</TableCell>
                <TableCell align="center">Durum</TableCell>
                <TableCell align="center">İşlemler</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {rows.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={8} align="center" sx={{ py: 4, color: 'text.secondary' }}>
                    Kayıt bulunamadı
                  </TableCell>
                </TableRow>
              ) : rows.map(row => (
                <TableRow key={row.id} sx={trStyle}>
                  <TableCell sx={{ fontWeight: 500 }}>{row.productName}</TableCell>
                  <TableCell><code style={{ fontSize: 12, opacity: 0.7 }}>{row.sku}</code></TableCell>
                  <TableCell>{row.category}</TableCell>
                  <TableCell>{row.unit}</TableCell>
                  <TableCell align="center">
                    <Typography fontWeight={700}
                      color={row.currentStock <= row.minStockLevel ? 'error.main' : 'success.main'}>
                      {row.currentStock}
                    </Typography>
                  </TableCell>
                  <TableCell align="center">{row.minStockLevel}</TableCell>
                  <TableCell align="center">
                    {row.currentStock <= row.minStockLevel
                      ? <Chip label="Düşük" color="error" size="small" />
                      : <Chip label="Normal" color="success" size="small" />}
                  </TableCell>
                  <TableCell align="center">
                    <Tooltip title="Düzenle">
                      <IconButton size="small" color="primary"
                        onClick={() => { setEditItem(row); setModalOpen(true); }}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Sil">
                      <IconButton size="small" color="error" onClick={() => setDeleteId(row.id)}>
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
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

      <ProductModal
        open={modalOpen}
        onClose={() => { setModalOpen(false); setEditItem(null); }}
        onSave={handleSave}
        initial={editItem}
        categories={CATEGORIES}
      />

      <ConfirmDialog
        open={!!deleteId}
        title="Ürünü sil"
        message="Bu ürünü silmek istediğinizden emin misiniz? İşlem geri alınamaz."
        onConfirm={handleDelete}
        onCancel={() => setDeleteId(null)}
      />
    </Box>
  );
}
