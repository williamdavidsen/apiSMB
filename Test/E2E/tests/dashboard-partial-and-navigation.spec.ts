import { expect, test } from '@playwright/test'
import assessmentFixture from '../fixtures/assessment-response.json' assert { type: 'json' }

test('dashboard renders partial assessment messaging and e-mail error state', async ({ page }) => {
  const partialFixture = structuredClone(assessmentFixture)
  partialFixture.assessment.status = 'PARTIAL'
  partialFixture.assessment.grade = 'C'
  partialFixture.assessment.overallScore = 71
  partialFixture.assessment.emailModuleIncluded = false
  partialFixture.assessment.modules.emailSecurity.included = false
  partialFixture.assessment.modules.emailSecurity.status = 'ERROR'
  partialFixture.assessment.modules.emailSecurity.weightPercent = 0
  partialFixture.assessment.alerts = [
    {
      type: 'WARNING',
      message: 'Email security analysis could not be completed reliably, so the module was excluded from the final weighted score.',
    },
  ]
  partialFixture.email.hasMailService = false
  partialFixture.email.status = 'ERROR'
  partialFixture.email.overallScore = 0
  partialFixture.email.alerts = [
    {
      type: 'WARNING',
      message: 'Email security DNS lookups could not be completed reliably. MX lookup could not be completed.',
    },
  ]

  await page.route('**/api/**', async (route) => {
    const url = route.request().url()
    const body = url.includes('/api/assessment/')
      ? partialFixture.assessment
      : url.includes('/api/ssl/')
        ? partialFixture.ssl
        : url.includes('/api/headers/')
          ? partialFixture.headers
          : url.includes('/api/email/')
            ? partialFixture.email
            : url.includes('/api/reputation/')
              ? partialFixture.reputation
              : partialFixture.assessment

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(body),
    })
  })

  await page.goto('/dashboard?domain=example.com')

  await expect(page.getByText(/partial assessment/i)).toBeVisible()
  await expect(page.getByText(/could not evaluate/i).first()).toBeVisible()
  await expect(page.getByText(/could not be evaluated reliably/i)).toBeVisible()
})

test('user can open a module detail page from the dashboard', async ({ page }) => {
  await page.route('**/api/**', async (route) => {
    const url = route.request().url()
    const body = url.includes('/api/assessment/')
      ? assessmentFixture.assessment
      : url.includes('/api/ssl/details/')
        ? {
            ...assessmentFixture.ssl,
            dataSource: 'SSL_LABS',
            dataSourceStatus: 'READY',
            endpoints: [],
            certificate: {},
            supportedTlsVersions: ['TLS 1.3'],
            notableCipherSuites: ['TLS_AES_256_GCM_SHA384 (256 bits)'],
          }
        : url.includes('/api/ssl/')
          ? assessmentFixture.ssl
          : url.includes('/api/headers/')
            ? assessmentFixture.headers
            : url.includes('/api/email/')
              ? assessmentFixture.email
              : url.includes('/api/reputation/')
                ? assessmentFixture.reputation
                : assessmentFixture.assessment

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(body),
    })
  })

  await page.goto('/dashboard?domain=example.com')
  await page.getByRole('link', { name: /read more/i }).first().click()

  await expect(page).toHaveURL(/\/dashboard\/example\.com\/(ssl-tls|http-headers|email|reputation)$/)
  await expect(page.getByText(/analysis/i).first()).toBeVisible()
})
