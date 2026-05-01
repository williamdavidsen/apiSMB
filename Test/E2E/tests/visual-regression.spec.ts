import { expect, test } from '@playwright/test'
import type { Page } from '@playwright/test'
import assessmentFixture from '../fixtures/assessment-response.json' with { type: 'json' }

async function closePqcDialogIfPresent(page: Page) {
  const closeButton = page.getByRole('button', { name: /close post-quantum dialog/i })

  if (await closeButton.isVisible().catch(() => false)) {
    await closeButton.click()
    await expect(closeButton).toBeHidden()
  }
}

test.describe('visual regression', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/**', async (route) => {
      const url = route.request().url()
      const body = url.includes('/api/assessment/')
        ? assessmentFixture.assessment
        : url.includes('/api/ssl/details/')
          ? {
              ...assessmentFixture.ssl,
              dataSource: 'DIRECT_TLS',
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
  })

  test('home page visual baseline remains stable', async ({ page }) => {
    await page.setViewportSize({ width: 1440, height: 1400 })
    await page.goto('/')

    await expect(page).toHaveScreenshot('home-page.png', {
      fullPage: true,
      animations: 'disabled',
    })
  })

  test('dashboard visual baseline remains stable', async ({ page }) => {
    await page.setViewportSize({ width: 1440, height: 1600 })
    await page.goto('/dashboard?domain=example.com')
    await closePqcDialogIfPresent(page)

    await expect(page.locator('main')).toHaveScreenshot('dashboard-main.png', {
      animations: 'disabled',
      mask: [page.getByText(/last scanned:/i)],
    })
  })

  test('module detail visual baseline remains stable', async ({ page }) => {
    await page.setViewportSize({ width: 1440, height: 1800 })
    await page.goto('/dashboard/example.com/ssl-tls')

    await expect(page.locator('main')).toHaveScreenshot('module-detail-ssl.png', {
      animations: 'disabled',
      mask: [page.getByText(/scanned /i)],
      maxDiffPixels: 100,
    })
  })
})
