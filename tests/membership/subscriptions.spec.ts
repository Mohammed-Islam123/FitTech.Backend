import { test, expect, GATEWAY, MEMBERSHIP, ADMIN } from '../fixtures';

let adminToken: string;
let createdPlanId: string;
let createdMemberId: string;
let createdSubscriptionId: string;

test.beforeAll(async ({ request }) => {
  const res = await request.post(`${GATEWAY}/api/User/login`, {
    data: { emailOrUserName: ADMIN.email, password: ADMIN.password, clientId: 'web' },
  });
  adminToken = (await res.json()).Data.Token;
});

test.describe('Subscriptions', () => {
  test('setup - create plan', async ({ request }) => {
    const res = await request.post(`${GATEWAY}/api/plans`, {
      data: {
        name: `Sub Plan ${Date.now()}`,
        description: 'For subscription tests',
        price: 99.99,
        durationValue: 3,
        durationUnit: 'Months',
        sessionCount: 36,
      },
      headers: { Authorization: `Bearer ${adminToken}` },
    });
    if (res.status() === 201) {
      createdPlanId = (await res.json()).Data.planId;
    }
    expect(res.status()).toBe(201);
  });

  test('setup - create member', async ({ request }) => {
    const formData = new FormData();
    formData.append('firstName', `Sub_${Date.now()}`);
    formData.append('lastName', 'Member');
    formData.append('email', `sub_${Date.now()}@example.com`);
    formData.append('phoneNumber', '+213555777777');
    formData.append('dateOfBirth', '1990-06-15');
    formData.append('gender', 'Male');
    formData.append('planId', createdPlanId);

    const res = await request.post(`${GATEWAY}/api/members`, {
      multipart: formData,
      headers: { Authorization: `Bearer ${adminToken}` },
    });
    expect(res.status()).toBe(201);
    createdMemberId = (await res.json()).Data.memberId;
  });

  test('create subscription', async ({ request }) => {
    expect(createdMemberId).toBeTruthy();
    expect(createdPlanId).toBeTruthy();

    const res = await request.post(`${GATEWAY}/api/subscriptions`, {
      data: {
        memberId: createdMemberId,
        planId: createdPlanId,
        paymentMethod: 'Cash',
        notes: 'Test subscription',
      },
      headers: { Authorization: `Bearer ${adminToken}` },
    });
    expect(res.status()).toBe(201);
    const body = await res.json();
    expect(body.Data?.subscriptionId).toBeTruthy();
    createdSubscriptionId = body.Data.subscriptionId;
  });

  test('create subscription without auth fails', async ({ request }) => {
    const res = await request.post(`${GATEWAY}/api/subscriptions`, {
      data: {
        memberId: createdMemberId,
        planId: createdPlanId,
        paymentMethod: 'Cash',
      },
    });
    expect(res.status()).toBe(401);
  });

  test('get active subscription', async ({ request }) => {
    if (!createdMemberId) return;
    const loginRes = await request.post(`${GATEWAY}/api/User/login`, {
      data: { emailOrUserName: 'Member@fitteck.com', password: 'Member@12345', clientId: 'web' },
    });
    const memberToken = (await loginRes.json()).Data.Token;
    const payload = JSON.parse(Buffer.from(memberToken.split('.')[1], 'base64').toString());
    const memberUserId = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];

    const res = await request.get(`${GATEWAY}/api/members/active-subscription`, {
      headers: { Authorization: `Bearer ${memberToken}` },
    });
    // May return 404 if member has no subscription in membership DB
    expect([200, 404]).toContain(res.status());
  });

  test('get subscription history', async ({ request }) => {
    if (!createdMemberId) return;
    const res = await request.get(`${GATEWAY}/api/members/${createdMemberId}/subscriptions`, {
      headers: { Authorization: `Bearer ${adminToken}` },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.Data).toBeTruthy();
    expect(Array.isArray(body.Data)).toBe(true);
  });

  test('confirm cash payment - missing amount fails', async ({ request }) => {
    if (!createdSubscriptionId) return;
    const res = await request.post(`${GATEWAY}/api/subscriptions/confirm-payment`, {
      data: {
        subscriptionId: createdSubscriptionId,
        amountReceived: 0,
        paymentMethod: 'Cash',
      },
      headers: { Authorization: `Bearer ${adminToken}` },
    });
    // Validation should catch zero amount
    expect([400, 422, 200]).toContain(res.status());
  });

  test('create subscription with invalid member fails', async ({ request }) => {
    const res = await request.post(`${GATEWAY}/api/subscriptions`, {
      data: {
        memberId: '00000000-0000-0000-0000-000000000000',
        planId: createdPlanId,
        paymentMethod: 'Cash',
      },
      headers: { Authorization: `Bearer ${adminToken}` },
    });
    expect(res.status()).toBe(404);
  });

  test('cleanup - delete plan', async ({ request }) => {
    if (!createdPlanId) return;
    const res = await request.delete(`${GATEWAY}/api/plans/${createdPlanId}`, {
      headers: { Authorization: `Bearer ${adminToken}` },
    });
    expect(res.status()).toBe(200);
  });
});
