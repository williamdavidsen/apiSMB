import { expect, test } from '@playwright/test'
import assessmentFixture from '../fixtures/assessment-response.json' assert { type: 'json' }

test('dashboard route renders with fixture-backed API responses', async ({ page }) => {
  await page.route('**/api/**', async (route) => {
    const url = route.request().url()
    const body = url.includes('/api/assessment/')
      ? assessmentFixture.assessment
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

  await expect(page.getByText('example.com')).toBeVisible()
  await expect(page.getByText(/security analysis dashboard/i)).toBeVisible()
})
