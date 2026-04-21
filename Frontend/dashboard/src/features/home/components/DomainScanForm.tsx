import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { useState } from 'react'
import type { FormEvent } from 'react'

type DomainScanFormProps = {
  onSubmitDomain: (domain: string) => void
}

const domainPattern =
  /^(?!:\/\/)([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,63}$/

export function DomainScanForm({ onSubmitDomain }: DomainScanFormProps) {
  const [domain, setDomain] = useState('')
  const [error, setError] = useState('')

  const validateDomain = (value: string) => {
    const normalized = value.trim().toLowerCase()

    if (!normalized) {
      return 'Domain is required.'
    }

    if (normalized.startsWith('http://') || normalized.startsWith('https://')) {
      return 'Please enter only the domain name, for example firma.no'
    }

    if (!domainPattern.test(normalized)) {
      return 'Please enter a valid domain like example.com'
    }

    return ''
  }

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const normalized = domain.trim().toLowerCase()
    const validationMessage = validateDomain(normalized)

    if (validationMessage) {
      setError(validationMessage)
      return
    }

    setError('')
    onSubmitDomain(normalized)
  }

  return (
    <Paper
      component="section"
      variant="outlined"
      sx={{
        p: { xs: 2.5, md: 4 },
        borderRadius: '16px',
        bgcolor: 'rgba(255, 255, 255, 0.86)',
        backdropFilter: 'blur(8px)',
        WebkitBackdropFilter: 'blur(8px)',
        boxShadow: '0 8px 30px rgba(0, 0, 0, 0.08)',
        borderColor: 'rgba(255, 255, 255, 0.6)',
      }}
      aria-labelledby="scan-form-title"
    >
      <Typography id="scan-form-title" component="h2" variant="h6" align="center" gutterBottom sx={{ fontWeight: 700 }}>
        Analyse your security posture
      </Typography>

      <Box component="form" onSubmit={handleSubmit} noValidate>
        <Stack spacing={2} sx={{ maxWidth: 520, mx: 'auto' }}>
          <TextField
            id="domain-input"
            name="domain"
            label="Enter domain name"
            placeholder="e.g. firma.no"
            value={domain}
            onChange={(event) => {
              const nextValue = event.target.value
              setDomain(nextValue)
              if (error) {
                setError(validateDomain(nextValue))
              }
            }}
            onBlur={() => setError(validateDomain(domain))}
            autoComplete="off"
            required
            fullWidth
            error={Boolean(error)}
            helperText={error || 'Enter only the domain, without http:// or https://'}
          />

          {error ? <Alert severity="error">{error}</Alert> : null}

          <Button type="submit" variant="contained" size="large" sx={{ py: 1.2, mt: 0.5 }}>
            Run security scan
          </Button>
        </Stack>
      </Box>
    </Paper>
  )
}
