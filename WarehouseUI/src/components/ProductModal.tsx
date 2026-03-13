import { useState, useEffect } from 'react';
import {
  Dialog, DialogTitle, DialogContent, DialogActions,
  Button, TextField, Stack, MenuItem, Select,
  FormControl, InputLabel, FormHelperText, Box,
} from '@mui/material';
import type { ProductResponse } from '../api/types';

interface Props {
  open: boolean;
  onClose: () => void;
  onSave: (data: Omit<ProductResponse, 'id' | 'companyId' | 'currentStock' | 'createdAt' | 'updatedAt'>) => Promise<void>;
  initial: ProductResponse | null;
  categories: string[];
}

const empty = { productName: '', sku: '', category: '', unit: '', description: '', minStockLevel: 0 };

export default function ProductModal({ open, onClose, onSave, initial, categories }: Props) {
  const [form, setForm] = useState(empty);
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (initial) {
      setForm({
        productName: initial.productName, sku: initial.sku,
        category: initial.category, unit: initial.unit,
        description: initial.description || '', minStockLevel: initial.minStockLevel,
      });
    } else { setForm(empty); }
    setErrors({});
  }, [initial, open]);

  const validate = () => {
    const e: Record<string, string> = {};
    if (!form.productName.trim()) e.productName = 'Ürün adı zorunlu';
    if (!form.sku.trim()) e.sku = 'SKU zorunlu';
    if (!form.category) e.category = 'Kategori seçin';
    if (!form.unit.trim()) e.unit = 'Birim zorunlu';
    if (form.minStockLevel < 0) e.minStockLevel = 'Min stok 0 veya üzeri olmalı';
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleSubmit = async () => {
    if (!validate()) return;
    setSaving(true);
    try { await onSave({ ...form, minStockLevel: Number(form.minStockLevel) }); }
    finally { setSaving(false); }
  };

  const f = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm(p => ({ ...p, [field]: e.target.value }));

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth
      PaperProps={{ sx: { background: '#1a1d27', border: '1px solid rgba(255,255,255,0.1)', borderRadius: 3 } }}>
      <DialogTitle sx={{ fontWeight: 700 }}>
        {initial ? '✏️ Ürünü Düzenle' : '➕ Yeni Ürün Ekle'}
      </DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField fullWidth label="Ürün Adı *" value={form.productName}
            onChange={f('productName')} error={!!errors.productName} helperText={errors.productName} />

          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField fullWidth label="SKU *" value={form.sku}
              onChange={f('sku')} error={!!errors.sku} helperText={errors.sku} />
            <FormControl fullWidth error={!!errors.category}>
              <InputLabel>Kategori *</InputLabel>
              <Select value={form.category} label="Kategori *"
                onChange={e => setForm(p => ({ ...p, category: e.target.value }))}>
                {categories.map(c => <MenuItem key={c} value={c}>{c}</MenuItem>)}
              </Select>
              {errors.category && <FormHelperText>{errors.category}</FormHelperText>}
            </FormControl>
          </Box>

          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField fullWidth label="Birim *" value={form.unit}
              onChange={f('unit')} error={!!errors.unit} helperText={errors.unit}
              placeholder="adet, kg, kutu..." />
            <TextField fullWidth label="Min. Stok Seviyesi" type="number"
              value={form.minStockLevel} onChange={f('minStockLevel')}
              error={!!errors.minStockLevel} helperText={errors.minStockLevel}
              inputProps={{ min: 0 }} />
          </Box>

          <TextField fullWidth label="Açıklama" value={form.description}
            onChange={f('description')} multiline rows={2} />
        </Stack>
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 3, gap: 1 }}>
        <Button onClick={onClose} disabled={saving} variant="outlined" color="inherit">İptal</Button>
        <Button onClick={handleSubmit} disabled={saving} variant="contained">
          {saving ? 'Kaydediliyor...' : 'Kaydet'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
