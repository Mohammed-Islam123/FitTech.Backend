# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: membership\plans.spec.ts >> Membership Plans >> create a plan via direct URL
- Location: tests\membership\plans.spec.ts:14:7

# Error details

```
Error: expect(received).toBe(expected) // Object.is equality

Expected: 201
Received: 400
```

# Test source

```ts
  1  | import { test, expect, GATEWAY, MEMBERSHIP, ADMIN } from '../fixtures';
  2  | 
  3  | let adminToken: string;
  4  | let createdPlanId: string;
  5  | 
  6  | test.beforeAll(async ({ request }) => {
  7  |   const adminRes = await request.post(`${GATEWAY}/api/User/login`, {
  8  |     data: { emailOrUserName: ADMIN.email, password: ADMIN.password, clientId: 'web' },
  9  |   });
  10 |   adminToken = (await adminRes.json()).Data.Token;
  11 | });
  12 | 
  13 | test.describe('Membership Plans', () => {
  14 |   test('create a plan via direct URL', async ({ request }) => {
  15 |     const res = await request.post(`${MEMBERSHIP}/api/plans`, {
  16 |       data: {
  17 |         name: `Test Plan ${Date.now()}`,
  18 |         description: 'Automated test plan',
  19 |         price: 99.99,
  20 |         durationValue: 1,
  21 |         durationUnit: 'Months',
  22 |         sessionCount: 12,
  23 |         accessRules: ['gym', 'pool'],
  24 |       },
  25 |       headers: { Authorization: `Bearer ${adminToken}` },
  26 |     });
> 27 |     expect(res.status()).toBe(201);
     |                          ^ Error: expect(received).toBe(expected) // Object.is equality
  28 |     const body = await res.json();
  29 |     expect(body.Data?.planId).toBeTruthy();
  30 |     createdPlanId = body.Data.planId;
  31 |   });
  32 | 
  33 |   test('list plans via direct URL', async ({ request }) => {
  34 |     const res = await request.get(`${MEMBERSHIP}/api/plans`, {
  35 |       headers: { Authorization: `Bearer ${adminToken}` },
  36 |     });
  37 |     expect(res.status()).toBe(200);
  38 |     const body = await res.json();
  39 |     expect(body.Data).toBeTruthy();
  40 |     expect(Array.isArray(body.Data)).toBe(true);
  41 |   });
  42 | 
  43 |   test('create plan without auth fails', async ({ request }) => {
  44 |     const res = await request.post(`${MEMBERSHIP}/api/plans`, {
  45 |       data: { name: 'NoAuth Plan', price: 10 },
  46 |     });
  47 |     expect(res.status()).toBe(401);
  48 |   });
  49 | 
  50 |   test('update plan', async ({ request }) => {
  51 |     if (!createdPlanId) return;
  52 |     const res = await request.put(`${MEMBERSHIP}/api/plans/${createdPlanId}`, {
  53 |       data: {
  54 |         name: `Updated ${Date.now()}`,
  55 |         description: 'Updated description',
  56 |         price: 149.99,
  57 |       },
  58 |       headers: { Authorization: `Bearer ${adminToken}` },
  59 |     });
  60 |     expect(res.status()).toBe(200);
  61 |   });
  62 | 
  63 |   test('delete plan (soft)', async ({ request }) => {
  64 |     if (!createdPlanId) return;
  65 |     const res = await request.delete(`${MEMBERSHIP}/api/plans/${createdPlanId}`, {
  66 |       headers: { Authorization: `Bearer ${adminToken}` },
  67 |     });
  68 |     expect(res.status()).toBe(200);
  69 |   });
  70 | });
  71 | 
```