import { test, expect, GATEWAY, MEMBERSHIP, ADMIN, MEMBER } from '../fixtures';

let adminToken: string;
let memberToken: string;
let createdPlanId: string;
let createdMemberId: string;

test.beforeAll(async ({ request }) => {
  const adminRes = await request.post(`${GATEWAY}/api/User/login`, {
    data: { emailOrUserName: ADMIN.email, password: ADMIN.password, clientId: 'web' },
  });
  adminToken = (await adminRes.json()).Data.Token;

  const memberRes = await request.post(`${GATEWAY}/api/User/login`, {
    data: { emailOrUserName: MEMBER.email, password: MEMBER.password, clientId: 'web' },
  });
  memberToken = (await memberRes.json()).Data.Token;
});

test.describe('Membership Members', () => {
  test('create member via direct URL', async ({ request }) => {
    const boundary = '----FormBoundary' + Date.now();
    const body = [
      `--${boundary}`,
      'Content-Disposition: form-data; name="firstName"',
      '',
      `John_${Date.now()}`,
      `--${boundary}`,
      'Content-Disposition: form-data; name="lastName"',
      '',
      'Doe',
      `--${boundary}`,
      'Content-Disposition: form-data; name="email"',
      '',
      `john_${Date.now()}@example.com`,
      `--${boundary}`,
      'Content-Disposition: form-data; name="phoneNumber"',
      '',
      '+213555123456',
      `--${boundary}`,
      'Content-Disposition: form-data; name="dateOfBirth"',
      '',
      '1990-01-15',
      `--${boundary}`,
      'Content-Disposition: form-data; name="gender"',
      '',
      'Male',
      `--${boundary}--`,
    ].join('\r\n');

    const res = await request.post(`${MEMBERSHIP}/api/members`, {
      data: body,
      headers: {
        Authorization: `Bearer ${adminToken}`,
        'Content-Type': `multipart/form-data; boundary=${boundary}`,
      },
    });
    expect([201, 400]).toContain(res.status());
    if (res.status() === 201) {
      createdMemberId = (await res.json()).Data.memberId;
    }
  });

  test('list members via direct URL', async ({ request }) => {
    const res = await request.get(`${MEMBERSHIP}/api/members`, {
      headers: { Authorization: `Bearer ${adminToken}` },
      params: { page: 1, pageSize: 10 },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.Data?.items).toBeTruthy();
    expect(Array.isArray(body.Data.items)).toBe(true);
    expect(body.Data.totalCount).toBeGreaterThanOrEqual(0);
  });

  test('list members with search', async ({ request }) => {
    const res = await request.get(`${MEMBERSHIP}/api/members`, {
      headers: { Authorization: `Bearer ${adminToken}` },
      params: { search: 'FitTeck', page: 1, pageSize: 5 },
    });
    expect(res.status()).toBe(200);
  });

  test('list members with status filter', async ({ request }) => {
    const res = await request.get(`${MEMBERSHIP}/api/members`, {
      headers: { Authorization: `Bearer ${adminToken}` },
      params: { status: 'Active', page: 1, pageSize: 5 },
    });
    expect(res.status()).toBe(200);
  });

  test('unauthorized access returns 401', async ({ request }) => {
    const res = await request.get(`${MEMBERSHIP}/api/members`);
    expect(res.status()).toBe(401);
  });

  test('member cannot access plans endpoint', async ({ request }) => {
    const res = await request.post(`${MEMBERSHIP}/api/plans`, {
      data: { name: 'Hack Plan', price: 10 },
      headers: { Authorization: `Bearer ${memberToken}` },
    });
    expect([401, 403]).toContain(res.status());
  });
});
