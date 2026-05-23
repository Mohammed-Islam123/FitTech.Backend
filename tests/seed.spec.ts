import { test, expect, GATEWAY, ADMIN } from './fixtures';

test.describe('Connectivity and Auth', () => {
  test('gateway responds via HTTPS', async ({ request }) => {
    const res = await request.get(`${GATEWAY}/api/User/login`);
    expect(res.status()).not.toBe(502);
  });

  test('admin login returns token', async ({ adminToken }) => {
    expect(adminToken).toBeTruthy();
    expect(adminToken.length).toBeGreaterThan(20);
  });

  test('member login returns token', async ({ memberToken }) => {
    expect(memberToken).toBeTruthy();
    expect(memberToken.length).toBeGreaterThan(20);
  });

  test('coach login returns token', async ({ coachToken }) => {
    expect(coachToken).toBeTruthy();
    expect(coachToken.length).toBeGreaterThan(20);
  });

  test('login with wrong password returns 401', async ({ request }) => {
    const res = await request.post(`${GATEWAY}/api/User/login`, {
      data: {
        emailOrUserName: ADMIN.email,
        password: 'WrongPassword',
        clientId: 'web',
      },
    });
    expect(res.status()).toBe(401);
    const body = await res.json();
    expect(body.Success).toBe(false);
  });
});
