import { test as base, APIRequestContext } from '@playwright/test';

export const GATEWAY = 'https://localhost:7248';
export const IDENTITY = 'https://localhost:7259';
export const MEMBERSHIP = 'https://localhost:7297';

export const ADMIN = { email: 'Admin@fitteck.com', password: 'Admin@12345' };
export const MEMBER = { email: 'Member@fitteck.com', password: 'Member@12345' };
export const COACH = { email: 'Coach@fitteck.com', password: 'Coach@12345' };

export type Role = 'admin' | 'member' | 'coach';

function credentials(role: Role): { email: string; password: string } {
  switch (role) {
    case 'admin': return ADMIN;
    case 'member': return MEMBER;
    case 'coach': return COACH;
  }
}

async function loginAs(request: APIRequestContext, role: Role): Promise<string> {
  const creds = credentials(role);
  const res = await request.post(`${GATEWAY}/api/User/login`, {
    data: {
      emailOrUserName: creds.email,
      password: creds.password,
      clientId: 'web',
    },
  });
  if (!res.ok()) throw new Error(`Login failed for ${role}: ${res.status()} ${await res.text()}`);
  const body: any = await res.json();
  if (!body.Success || !body.Data?.Token) throw new Error(`Login returned no token: ${JSON.stringify(body)}`);
  return body.Data.Token as string;
}

export interface AuthFixtures {
  adminToken: string;
  memberToken: string;
  coachToken: string;
  authHeaders: (role: Role) => Record<string, string>;
}

export const test = base.extend<AuthFixtures>({
  adminToken: async ({ request }, use) => {
    const token = await loginAs(request, 'admin');
    await use(token);
  },
  memberToken: async ({ request }, use) => {
    const token = await loginAs(request, 'member');
    await use(token);
  },
  coachToken: async ({ request }, use) => {
    const token = await loginAs(request, 'coach');
    await use(token);
  },
  authHeaders: async ({ adminToken, memberToken, coachToken }, use) => {
    const tokens: Record<Role, string> = { admin: adminToken, member: memberToken, coach: coachToken };
    await use((role: Role) => ({
      Authorization: `Bearer ${tokens[role]}`,
    }));
  },
});

export { expect } from '@playwright/test';
