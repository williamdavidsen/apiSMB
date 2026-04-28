import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'

export function SectionCard({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <Paper variant="outlined" sx={{ p: { xs: 2, sm: 2.5 }, borderRadius: 3 }}>
      <Typography variant="h6" sx={{ fontWeight: 900, color: 'secondary.dark', mb: 1.25 }}>
        {title}
      </Typography>
      {children}
    </Paper>
  )
}

export function BulletList({ items }: { items: string[] }) {
  return (
    <Stack component="ul" spacing={0.9} sx={{ m: 0, pl: 2.3 }}>
      {items.map((item) => (
        <Typography component="li" key={item} variant="body1" sx={{ lineHeight: 1.55 }}>
          {item}
        </Typography>
      ))}
    </Stack>
  )
}
