import {
  Dialog, DialogTitle, DialogContent, DialogActions,
  Button, Typography,
} from '@mui/material';
import WarningAmberIcon from '@mui/icons-material/WarningAmber';

interface Props {
  open: boolean;
  title: string;
  message: string;
  onConfirm: () => void;
  onCancel: () => void;
}

export default function ConfirmDialog({ open, title, message, onConfirm, onCancel }: Props) {
  return (
    <Dialog
      open={open} onClose={onCancel} maxWidth="xs" fullWidth
      PaperProps={{ sx: { background: '#1a1d27', border: '1px solid rgba(239,68,68,0.3)', borderRadius: 3 } }}
    >
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1.5, color: 'error.main', fontWeight: 700 }}>
        <WarningAmberIcon /> {title}
      </DialogTitle>
      <DialogContent>
        <Typography color="text.secondary">{message}</Typography>
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 3, gap: 1 }}>
        <Button onClick={onCancel} variant="outlined" color="inherit">İptal</Button>
        <Button onClick={onConfirm} variant="contained" color="error">Evet, Sil</Button>
      </DialogActions>
    </Dialog>
  );
}
