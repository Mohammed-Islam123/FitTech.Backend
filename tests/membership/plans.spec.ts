import { test, expect, GATEWAY, MEMBERSHIP, ADMIN } from '../fixtures';

let adminToken: string;
let createdPlanId: string;

test.beforeAll(async ({ request }) => {
  const adminRes = await request.post(`${GATEWAY}/api/User/login`, {
    data: { emailOrUserName: ADMIN.email, password: ADMIN.password, clientId: 'web' },
  });
  adminToken = (await adminRes.json()).Data.Token;
});

test.describe('Membership Plans', () => {
  test('create a plan via direct URL', async ({ request }) => {
    const res = await request.post(`${MEMBERSHIP}/api/plans`, {
      data: {
        name: `Test Plan ${Date.now()}`,
        description: 'Automated test plan',
        price: 99.99,
        durationValue: 1,
        durationUnit: 'Months',
        sessionCount: 12,
        accessRules: ['gym', 'pool'],
      },
      headers: { Authorization: `Bearer ${adminToken}` },
    });
    expect(res.status()).toBe(201);
    const body = await res.json();
    expect(body.Data?.planId).toBeTruthy();
    createdPlanId = body.Data.planId;
  });

  test('list plans via direct URL', async ({ request }) => {
    const res = await request.get(`${MEMBERSHIP}/api/plans`, {
      headers: { Authorization: `Bearer ${adminToken}` },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.Data).toBeTruthy();
    expect(Array.isArray(body.Data)).toBe(true);
  });

  test('create plan without auth fails', async ({ request }) => {
    const res = await request.post(`${MEMBERSHIP}/api/plans`, {
      data: { name: 'NoAuth Plan', price: 10 },
    });
    expect(res.status()).toBe(401);
  });

  test('update plan', async ({ request }) => {
    if (!createdPlanId) return;
    const res = await request.put(`${MEMBERSHIP}/api/plans/${createdPlanId}`, {
      data: {
        name: `Updated ${Date.now()}`,
        description: 'Updated description',
        price: 149.99,
      },
      headers: { Authorization: `Bearer ${adminToken}` },
    });
    expect(res.status()).toBe(200);
  });

  test('delete plan (soft)', async ({ request }) => {
    if (!createdPlanId) return;
    const res = await request.delete(`${MEMBERSHIP}/api/plans/${createdPlanId}`, {
      headers: { Authorization: `Bearer ${adminToken}` },
    });
    expect(res.status()).toBe(200);
  });
});
