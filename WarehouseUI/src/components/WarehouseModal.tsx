import { useState, useEffect } from 'react';
import {
  Dialog, DialogTitle, DialogContent, DialogActions,
  Button, TextField, Stack, Box,
} from '@mui/material';
import type { WarehouseResponse } from '../api/types';

interface Props {
  open: boolean;
  onClose: () => void;
  onSave: (data: Omit<WarehouseResponse, 'id' | 'companyId' | 'createdAt' | 'updatedAt'>) => Promise<void>;
  initial: WarehouseResponse | null;
}

const empty = { name: '', location: '', capacity: 0, description: '' };

export default function WarehouseModal({ open, onClose, onSave, initial }: Props) {
  const [form, setForm] = useState(empty);
  const [saving, setSaving] = useState(false);
  const [nameErr, setNameErr] = useState('');

  useEffect(() => {
    if (initial) {
      setForm({ name: initial.name, location: initial.location, capacity: initial.capacity, description: initial.description || '' });
    } else { setForm(empty); }
    setNameErr('');
  }, [initial, open]);

  const handleSubmit = async () => {
    if (!form.name.trim()) { setNameErr('Depo adı zorunlu'); return; }
    setSaving(true);
    try { await onSave({ ...form, capacity: Number(form.capacity) }); }
    finally { setSaving(false); }
  };

  const f = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm(p => ({ ...p, [field]: e.target.value }));

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth
      PaperProps={{ sx: { background: '#1a1d27', border: '1px solid rgba(255,255,255,0.1)', borderRadius: 3 } }}>
      <DialogTitle sx={{ fontWeight: 700 }}>
        {initial ? '✏️ Depoyu Düzenle' : '➕ Yeni Depo Ekle'}
      </DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField fullWidth label="Depo Adı *" value={form.name}
            onChange={f('name')} error={!!nameErr} helperText={nameErr} />
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField fullWidth label="Konum" value={form.location} onChange={f('location')} sx={{ flex: 2 }} />
            <TextField fullWidth label="Kapasite" type="number" value={form.capacity}
              onChange={f('capacity')} inputProps={{ min: 0 }} sx={{ flex: 1 }} />
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
