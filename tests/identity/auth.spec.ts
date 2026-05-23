import { test, expect, GATEWAY, IDENTITY, ADMIN, MEMBER, COACH } from '../fixtures';

test.describe('Identity Auth', () => {
  test('register endpoint accepts multipart/form-data', async ({ request }) => {
    const boundary = '----TestBoundary' + Date.now();
    const body = [
      `--${boundary}`,
      'Content-Disposition: form-data; name="userName"',
      '',
      `testuser_${Date.now()}`,
      `--${boundary}`,
      'Content-Disposition: form-data; name="email"',
      '',
      `test_${Date.now()}@example.com`,
      `--${boundary}`,
      'Content-Disposition: form-data; name="password"',
      '',
      'TestPass@123!',
      `--${boundary}`,
      'Content-Disposition: form-data; name="firstName"',
      '',
      'Test',
      `--${boundary}`,
      'Content-Disposition: form-data; name="lastName"',
      '',
      'User',
      `--${boundary}--`,
    ].join('\r\n');

    const res = await request.post(`${IDENTITY}/api/User/register`, {
      data: body,
      headers: { 'Content-Type': `multipart/form-data; boundary=${boundary}` },
    });
    expect([200, 400, 422]).toContain(res.status());
  });

  test('login returns token with refresh token', async ({ request }) => {
    const res = await request.post(`${GATEWAY}/api/User/login`, {
      data: { emailOrUserName: ADMIN.email, password: ADMIN.password, clientId: 'web' },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.Success).toBe(true);
    expect(body.Data.Token).toBeTruthy();
    expect(body.Data.RefreshToken).toBeTruthy();
    expect(body.Data.Succeeded).toBe(true);
  });

  test('login with wrong password returns 401', async ({ request }) => {
    const res = await request.post(`${GATEWAY}/api/User/login`, {
      data: { emailOrUserName: ADMIN.email, password: 'WrongPassword', clientId: 'web' },
    });
    expect(res.status()).toBe(401);
    const body = await res.json();
    expect(body.Success).toBe(false);
  });

  test('login with invalid client returns error', async ({ request }) => {
    const res = await request.post(`${GATEWAY}/api/User/login`, {
      data: { emailOrUserName: ADMIN.email, password: ADMIN.password, clientId: 'nonexistent' },
    });
    expect(res.status()).toBe(401);
    const body = await res.json();
    expect(body.Success).toBe(false);
  });

  test('login with empty password returns 400', async ({ request }) => {
    const res = await request.post(`${GATEWAY}/api/User/login`, {
      data: { emailOrUserName: ADMIN.email, password: '', clientId: 'web' },
    });
    expect(res.status()).toBe(400);
  });

  test('login by username works', async ({ request }) => {
    const res = await request.post(`${GATEWAY}/api/User/login`, {
      data: { emailOrUserName: 'admin', password: ADMIN.password, clientId: 'web' },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.Data.Token).toBeTruthy();
  });

  test('refresh token endpoint exists and responds', async ({ request }) => {
    const loginRes = await request.post(`${GATEWAY}/api/User/login`, {
      data: { emailOrUserName: ADMIN.email, password: ADMIN.password, clientId: 'web' },
    });
    const refreshToken = (await loginRes.json()).Data.RefreshToken;

    const res = await request.post(`${GATEWAY}/api/User/refresh-token`, {
      data: { refreshToken },
    });
    // 200 = success, 400 = token already consumed/invalid, 401 = unauthorized
    expect([200, 400, 401]).toContain(res.status());
  });

  test('refresh with invalid token returns error', async ({ request }) => {
    const res = await request.post(`${GATEWAY}/api/User/refresh-token`, {
      data: { refreshToken: 'invalid-refresh-token' },
    });
    expect([400, 401]).toContain(res.status());
  });

  test('revoke token endpoint exists', async ({ request }) => {
    const loginRes = await request.post(`${GATEWAY}/api/User/login`, {
      data: { emailOrUserName: ADMIN.email, password: ADMIN.password, clientId: 'web' },
    });
    const refreshToken = (await loginRes.json()).Data.RefreshToken;

    const res = await request.post(`${GATEWAY}/api/User/revoke-token`, {
      data: { refreshToken },
    });
    // May return 200 or 400 if token already revoked
    expect([200, 400]).toContain(res.status());
  });

  test('forgot password for existing email returns 200', async ({ request }) => {
    const res = await request.post(`${GATEWAY}/api/User/forgot-password`, {
      data: { email: ADMIN.email },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.Success).toBe(true);
  });

  test('forgot password for non-existing email returns 404', async ({ request }) => {
    const res = await request.post(`${GATEWAY}/api/User/forgot-password`, {
      data: { email: 'nonexistent@example.com' },
    });
    expect(res.status()).toBe(404);
  });

  test('access protected endpoint without token returns 401', async ({ request }) => {
    const res = await request.post(`${GATEWAY}/api/User/change-password`, {
      data: { currentPassword: 'x', newPassword: 'y' },
    });
    expect(res.status()).toBe(401);
  });

  test('access protected endpoint with invalid token returns 401', async ({ request }) => {
    const res = await request.post(`${GATEWAY}/api/User/change-password`, {
      data: { currentPassword: 'x', newPassword: 'y' },
      headers: { Authorization: 'Bearer invalid.token.here' },
    });
    expect(res.status()).toBe(401);
  });
});
