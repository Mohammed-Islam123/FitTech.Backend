# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: membership\subscriptions.spec.ts >> Subscriptions >> setup - create plan
- Location: tests\membership\subscriptions.spec.ts:16:7

# Error details

```
Error: expect(received).toBe(expected) // Object.is equality

Expected: 201
Received: 502
```

# Test source

```ts
  1   | import { test, expect, GATEWAY, MEMBERSHIP, ADMIN } from '../fixtures';
  2   | 
  3   | let adminToken: string;
  4   | let createdPlanId: string;
  5   | let createdMemberId: string;
  6   | let createdSubscriptionId: string;
  7   | 
  8   | test.beforeAll(async ({ request }) => {
  9   |   const res = await request.post(`${GATEWAY}/api/User/login`, {
  10  |     data: { emailOrUserName: ADMIN.email, password: ADMIN.password, clientId: 'web' },
  11  |   });
  12  |   adminToken = (await res.json()).Data.Token;
  13  | });
  14  | 
  15  | test.describe('Subscriptions', () => {
  16  |   test('setup - create plan', async ({ request }) => {
  17  |     const res = await request.post(`${GATEWAY}/api/plans`, {
  18  |       data: {
  19  |         name: `Sub Plan ${Date.now()}`,
  20  |         description: 'For subscription tests',
  21  |         price: 99.99,
  22  |         durationValue: 3,
  23  |         durationUnit: 'Months',
  24  |         sessionCount: 36,
  25  |       },
  26  |       headers: { Authorization: `Bearer ${adminToken}` },
  27  |     });
  28  |     if (res.status() === 201) {
  29  |       createdPlanId = (await res.json()).Data.planId;
  30  |     }
> 31  |     expect(res.status()).toBe(201);
      |                          ^ Error: expect(received).toBe(expected) // Object.is equality
  32  |   });
  33  | 
  34  |   test('setup - create member', async ({ request }) => {
  35  |     const formData = new FormData();
  36  |     formData.append('firstName', `Sub_${Date.now()}`);
  37  |     formData.append('lastName', 'Member');
  38  |     formData.append('email', `sub_${Date.now()}@example.com`);
  39  |     formData.append('phoneNumber', '+213555777777');
  40  |     formData.append('dateOfBirth', '1990-06-15');
  41  |     formData.append('gender', 'Male');
  42  |     formData.append('planId', createdPlanId);
  43  | 
  44  |     const res = await request.post(`${GATEWAY}/api/members`, {
  45  |       multipart: formData,
  46  |       headers: { Authorization: `Bearer ${adminToken}` },
  47  |     });
  48  |     expect(res.status()).toBe(201);
  49  |     createdMemberId = (await res.json()).Data.memberId;
  50  |   });
  51  | 
  52  |   test('create subscription', async ({ request }) => {
  53  |     expect(createdMemberId).toBeTruthy();
  54  |     expect(createdPlanId).toBeTruthy();
  55  | 
  56  |     const res = await request.post(`${GATEWAY}/api/subscriptions`, {
  57  |       data: {
  58  |         memberId: createdMemberId,
  59  |         planId: createdPlanId,
  60  |         paymentMethod: 'Cash',
  61  |         notes: 'Test subscription',
  62  |       },
  63  |       headers: { Authorization: `Bearer ${adminToken}` },
  64  |     });
  65  |     expect(res.status()).toBe(201);
  66  |     const body = await res.json();
  67  |     expect(body.Data?.subscriptionId).toBeTruthy();
  68  |     createdSubscriptionId = body.Data.subscriptionId;
  69  |   });
  70  | 
  71  |   test('create subscription without auth fails', async ({ request }) => {
  72  |     const res = await request.post(`${GATEWAY}/api/subscriptions`, {
  73  |       data: {
  74  |         memberId: createdMemberId,
  75  |         planId: createdPlanId,
  76  |         paymentMethod: 'Cash',
  77  |       },
  78  |     });
  79  |     expect(res.status()).toBe(401);
  80  |   });
  81  | 
  82  |   test('get active subscription', async ({ request }) => {
  83  |     if (!createdMemberId) return;
  84  |     const loginRes = await request.post(`${GATEWAY}/api/User/login`, {
  85  |       data: { emailOrUserName: 'Member@fitteck.com', password: 'Member@12345', clientId: 'web' },
  86  |     });
  87  |     const memberToken = (await loginRes.json()).Data.Token;
  88  |     const payload = JSON.parse(Buffer.from(memberToken.split('.')[1], 'base64').toString());
  89  |     const memberUserId = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
  90  | 
  91  |     const res = await request.get(`${GATEWAY}/api/members/active-subscription`, {
  92  |       headers: { Authorization: `Bearer ${memberToken}` },
  93  |     });
  94  |     // May return 404 if member has no subscription in membership DB
  95  |     expect([200, 404]).toContain(res.status());
  96  |   });
  97  | 
  98  |   test('get subscription history', async ({ request }) => {
  99  |     if (!createdMemberId) return;
  100 |     const res = await request.get(`${GATEWAY}/api/members/${createdMemberId}/subscriptions`, {
  101 |       headers: { Authorization: `Bearer ${adminToken}` },
  102 |     });
  103 |     expect(res.status()).toBe(200);
  104 |     const body = await res.json();
  105 |     expect(body.Data).toBeTruthy();
  106 |     expect(Array.isArray(body.Data)).toBe(true);
  107 |   });
  108 | 
  109 |   test('confirm cash payment - missing amount fails', async ({ request }) => {
  110 |     if (!createdSubscriptionId) return;
  111 |     const res = await request.post(`${GATEWAY}/api/subscriptions/confirm-payment`, {
  112 |       data: {
  113 |         subscriptionId: createdSubscriptionId,
  114 |         amountReceived: 0,
  115 |         paymentMethod: 'Cash',
  116 |       },
  117 |       headers: { Authorization: `Bearer ${adminToken}` },
  118 |     });
  119 |     // Validation should catch zero amount
  120 |     expect([400, 422, 200]).toContain(res.status());
  121 |   });
  122 | 
  123 |   test('create subscription with invalid member fails', async ({ request }) => {
  124 |     const res = await request.post(`${GATEWAY}/api/subscriptions`, {
  125 |       data: {
  126 |         memberId: '00000000-0000-0000-0000-000000000000',
  127 |         planId: createdPlanId,
  128 |         paymentMethod: 'Cash',
  129 |       },
  130 |       headers: { Authorization: `Bearer ${adminToken}` },
  131 |     });
```