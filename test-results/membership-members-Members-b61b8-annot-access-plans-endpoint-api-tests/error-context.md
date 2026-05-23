# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: membership\members.spec.ts >> Membership Members >> member cannot access plans endpoint
- Location: tests\membership\members.spec.ts:97:7

# Error details

```
Error: expect(received).toContain(expected) // indexOf

Expected value: 500
Received array: [401, 403]
```

# Test source

```ts
  2   | 
  3   | let adminToken: string;
  4   | let memberToken: string;
  5   | let createdPlanId: string;
  6   | let createdMemberId: string;
  7   | 
  8   | test.beforeAll(async ({ request }) => {
  9   |   const adminRes = await request.post(`${GATEWAY}/api/User/login`, {
  10  |     data: { emailOrUserName: ADMIN.email, password: ADMIN.password, clientId: 'web' },
  11  |   });
  12  |   adminToken = (await adminRes.json()).Data.Token;
  13  | 
  14  |   const memberRes = await request.post(`${GATEWAY}/api/User/login`, {
  15  |     data: { emailOrUserName: MEMBER.email, password: MEMBER.password, clientId: 'web' },
  16  |   });
  17  |   memberToken = (await memberRes.json()).Data.Token;
  18  | });
  19  | 
  20  | test.describe('Membership Members', () => {
  21  |   test('create member via direct URL', async ({ request }) => {
  22  |     const boundary = '----FormBoundary' + Date.now();
  23  |     const body = [
  24  |       `--${boundary}`,
  25  |       'Content-Disposition: form-data; name="firstName"',
  26  |       '',
  27  |       `John_${Date.now()}`,
  28  |       `--${boundary}`,
  29  |       'Content-Disposition: form-data; name="lastName"',
  30  |       '',
  31  |       'Doe',
  32  |       `--${boundary}`,
  33  |       'Content-Disposition: form-data; name="email"',
  34  |       '',
  35  |       `john_${Date.now()}@example.com`,
  36  |       `--${boundary}`,
  37  |       'Content-Disposition: form-data; name="phoneNumber"',
  38  |       '',
  39  |       '+213555123456',
  40  |       `--${boundary}`,
  41  |       'Content-Disposition: form-data; name="dateOfBirth"',
  42  |       '',
  43  |       '1990-01-15',
  44  |       `--${boundary}`,
  45  |       'Content-Disposition: form-data; name="gender"',
  46  |       '',
  47  |       'Male',
  48  |       `--${boundary}--`,
  49  |     ].join('\r\n');
  50  | 
  51  |     const res = await request.post(`${MEMBERSHIP}/api/members`, {
  52  |       data: body,
  53  |       headers: {
  54  |         Authorization: `Bearer ${adminToken}`,
  55  |         'Content-Type': `multipart/form-data; boundary=${boundary}`,
  56  |       },
  57  |     });
  58  |     expect([201, 400]).toContain(res.status());
  59  |     if (res.status() === 201) {
  60  |       createdMemberId = (await res.json()).Data.memberId;
  61  |     }
  62  |   });
  63  | 
  64  |   test('list members via direct URL', async ({ request }) => {
  65  |     const res = await request.get(`${MEMBERSHIP}/api/members`, {
  66  |       headers: { Authorization: `Bearer ${adminToken}` },
  67  |       params: { page: 1, pageSize: 10 },
  68  |     });
  69  |     expect(res.status()).toBe(200);
  70  |     const body = await res.json();
  71  |     expect(body.Data?.items).toBeTruthy();
  72  |     expect(Array.isArray(body.Data.items)).toBe(true);
  73  |     expect(body.Data.totalCount).toBeGreaterThanOrEqual(0);
  74  |   });
  75  | 
  76  |   test('list members with search', async ({ request }) => {
  77  |     const res = await request.get(`${MEMBERSHIP}/api/members`, {
  78  |       headers: { Authorization: `Bearer ${adminToken}` },
  79  |       params: { search: 'FitTeck', page: 1, pageSize: 5 },
  80  |     });
  81  |     expect(res.status()).toBe(200);
  82  |   });
  83  | 
  84  |   test('list members with status filter', async ({ request }) => {
  85  |     const res = await request.get(`${MEMBERSHIP}/api/members`, {
  86  |       headers: { Authorization: `Bearer ${adminToken}` },
  87  |       params: { status: 'Active', page: 1, pageSize: 5 },
  88  |     });
  89  |     expect(res.status()).toBe(200);
  90  |   });
  91  | 
  92  |   test('unauthorized access returns 401', async ({ request }) => {
  93  |     const res = await request.get(`${MEMBERSHIP}/api/members`);
  94  |     expect(res.status()).toBe(401);
  95  |   });
  96  | 
  97  |   test('member cannot access plans endpoint', async ({ request }) => {
  98  |     const res = await request.post(`${MEMBERSHIP}/api/plans`, {
  99  |       data: { name: 'Hack Plan', price: 10 },
  100 |       headers: { Authorization: `Bearer ${memberToken}` },
  101 |     });
> 102 |     expect([401, 403]).toContain(res.status());
      |                        ^ Error: expect(received).toContain(expected) // indexOf
  103 |   });
  104 | });
  105 | 
```