import { expect, test } from '@playwright/test'
import assessmentFixture from '../fixtures/assessment-response.json' assert { type: 'json' }

test('user can start a scan and reach the dashboard', async ({ page }) => {
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

  await page.goto('/')
  await page.getByLabel(/enter domain name/i).fill('example.com')
  await page.getByRole('button', { name: /run security scan/i }).click()

  await expect(page).toHaveURL(/scan|dashboard/)
  await expect(page.getByText(/security/i).first()).toBeVisible()
})
