import { useState, useEffect, useCallback } from 'react';
import {
  Box, Button, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow, Paper, TablePagination, TextField,
  IconButton, Tooltip, CircularProgress, Typography,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import type { WarehouseResponse } from '../api/types';
import { getWarehouses, createWarehouse, updateWarehouse, deleteWarehouse } from '../api/services';
import WarehouseModal from './WarehouseModal';
import ConfirmDialog from './ConfirmDialog';

export default function WarehousesTab() {
  const [rows, setRows] = useState<WarehouseResponse[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(25);
  const [search, setSearch] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editItem, setEditItem] = useState<WarehouseResponse | null>(null);
  const [deleteId, setDeleteId] = useState<number | null>(null);
  const [snack, setSnack] = useState('');

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getWarehouses(page + 1, pageSize, search || undefined);
      setRows(res.data);
      setTotal(res.totalCount);
    } catch { setSnack('Veriler yüklenemedi.'); }
    finally { setLoading(false); }
  }, [page, pageSize, search]);

  useEffect(() => { load(); }, [load]);

  const handleSave = async (data: Omit<WarehouseResponse, 'id' | 'companyId' | 'createdAt' | 'updatedAt'>) => {
    try {
      if (editItem) {
        await updateWarehouse({ ...data, id: editItem.id });
        setSnack('Depo güncellendi ✓');
      } else {
        await createWarehouse(data);
        setSnack('Depo oluşturuldu ✓');
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
      await deleteWarehouse(deleteId);
      setSnack('Depo silindi ✓');
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
        <Box sx={{ mb: 2, p: 1.5, borderRadius: 2, background: 'rgba(52,211,153,0.1)', border: '1px solid rgba(52,211,153,0.3)' }}>
          <Typography variant="body2" color="success.main">{snack}</Typography>
        </Box>
      )}

      <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
        <TextField
          size="small" placeholder="Depo adı veya konum ara..."
          value={searchInput}
          onChange={e => setSearchInput(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && setSearch(searchInput)}
          sx={{ flex: 1 }}
        />
        <Button variant="outlined" onClick={() => setSearch(searchInput)}>Ara</Button>
        <Button variant="contained" startIcon={<AddIcon />}
          onClick={() => { setEditItem(null); setModalOpen(true); }}>
          Yeni Depo
        </Button>
      </Box>

      <TableContainer component={Paper} sx={tableStyle}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}><CircularProgress /></Box>
        ) : (
          <Table size="small">
            <TableHead>
              <TableRow sx={{ '& th': thStyle }}>
                <TableCell>Depo Adı</TableCell>
                <TableCell>Konum</TableCell>
                <TableCell align="center">Kapasite</TableCell>
                <TableCell>Açıklama</TableCell>
                <TableCell align="center">İşlemler</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {rows.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={5} align="center" sx={{ py: 4, color: 'text.secondary' }}>
                    Kayıt bulunamadı
                  </TableCell>
                </TableRow>
              ) : rows.map(row => (
                <TableRow key={row.id} sx={trStyle}>
                  <TableCell sx={{ fontWeight: 600 }}>{row.name}</TableCell>
                  <TableCell>{row.location}</TableCell>
                  <TableCell align="center">{row.capacity}</TableCell>
                  <TableCell sx={{ color: 'text.secondary', fontSize: 13 }}>{row.description || '—'}</TableCell>
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

      <WarehouseModal
        open={modalOpen}
        onClose={() => { setModalOpen(false); setEditItem(null); }}
        onSave={handleSave}
        initial={editItem}
      />

      <ConfirmDialog
        open={!!deleteId}
        title="Depoyu sil"
        message="Bu depoyu silmek istediğinizden emin misiniz?"
        onConfirm={handleDelete}
        onCancel={() => setDeleteId(null)}
      />
    </Box>
  );
}
