import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { useState } from 'react'
import type { FormEvent } from 'react'
import { isValidDomain, normalizeDomainInput } from '../../../shared/lib/domain'
import { brandGradients } from '../../../styles/designTokens'

type DomainScanFormProps = {
  onSubmitDomain: (domain: string) => void
}

export function DomainScanForm({ onSubmitDomain }: DomainScanFormProps) {
  const [domain, setDomain] = useState('')
  const [error, setError] = useState('')

  const validateDomain = (value: string) => {
    const normalized = normalizeDomainInput(value)

    if (!normalized) {
      return 'Domain is required.'
    }

    if (!isValidDomain(value)) {
      return 'Please enter a valid domain like example.com'
    }

    return ''
  }

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const normalized = normalizeDomainInput(domain)
    const validationMessage = validateDomain(domain)

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
            helperText={error || 'You can enter a full URL or a domain like firma.no'}
          />

          {error ? <Alert severity="error">{error}</Alert> : null}

          <Button
            type="submit"
            variant="contained"
            size="large"
            sx={{
              py: 1.2,
              mt: 0.5,
              color: '#ffffff',
              background: brandGradients.appBar,
              fontWeight: 800,
              '&:hover': {
                background: brandGradients.appBarHover,
              },
            }}
          >
            Run security scan
          </Button>
        </Stack>
      </Box>
    </Paper>
  )
}
