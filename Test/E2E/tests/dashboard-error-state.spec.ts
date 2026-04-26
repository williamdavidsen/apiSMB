import { expect, test } from '@playwright/test'

test('dashboard shows retry state when the assessment API fails', async ({ page }) => {
  await page.route('**/api/**', async (route) => {
    const url = route.request().url()
    if (url.includes('/api/assessment/')) {
      await route.fulfill({
        status: 500,
        contentType: 'text/plain',
        body: 'Backend unavailable',
      })
      return
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: '{}',
    })
  })

  await page.goto('/dashboard?domain=example.com')

  await expect(page.getByText('Backend unavailable')).toBeVisible()
  await expect(page.getByRole('button', { name: /retry/i })).toBeVisible()
})
