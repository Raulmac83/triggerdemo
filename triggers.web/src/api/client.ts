const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';
const TOKEN_KEY = 'triggers.auth';

export interface Trigger {
  id: number;
  name: string;
  description?: string;
  isEnabled: boolean;
  createdAt: string;
}

export interface AuthenticatedUser {
  id: number;
  username: string;
  email: string;
  roles: string[];
}

export interface LoginResult {
  accessToken: string;
  expiresAt: string;
  user: AuthenticatedUser;
}

export function getStoredToken(): LoginResult | null {
  const raw = localStorage.getItem(TOKEN_KEY);
  if (!raw) return null;
  try {
    const parsed = JSON.parse(raw) as LoginResult;
    if (new Date(parsed.expiresAt).getTime() <= Date.now()) {
      localStorage.removeItem(TOKEN_KEY);
      return null;
    }
    return parsed;
  } catch {
    localStorage.removeItem(TOKEN_KEY);
    return null;
  }
}

export function storeToken(result: LoginResult) {
  localStorage.setItem(TOKEN_KEY, JSON.stringify(result));
}

export function clearToken() {
  localStorage.removeItem(TOKEN_KEY);
}

async function apiFetch(path: string, init: RequestInit = {}): Promise<Response> {
  const token = getStoredToken();
  const headers = new Headers(init.headers);
  if (!headers.has('Content-Type') && init.body) headers.set('Content-Type', 'application/json');
  if (token) headers.set('Authorization', `Bearer ${token.accessToken}`);
  const res = await fetch(`${BASE_URL}${path}`, { ...init, headers });
  if (res.status === 401 && token && !path.endsWith('/auth/login')) {
    clearToken();
    if (window.location.pathname !== '/login') window.location.assign('/login');
  }
  return res;
}

export async function login(username: string, password: string): Promise<LoginResult> {
  const res = await apiFetch('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({ username, password }),
  });
  if (res.status === 401) throw new Error('Invalid username or password.');
  if (!res.ok) throw new Error(`Login failed: ${res.status}`);
  const result = (await res.json()) as LoginResult;
  storeToken(result);
  return result;
}

export async function fetchMe(): Promise<AuthenticatedUser> {
  const res = await apiFetch('/api/auth/me');
  if (!res.ok) throw new Error(`Request failed: ${res.status}`);
  return res.json();
}

export interface TriggerInput {
  name: string;
  description?: string | null;
  isEnabled: boolean;
}

export async function fetchTriggers(): Promise<Trigger[]> {
  const res = await apiFetch('/api/triggers');
  if (!res.ok) throw new Error(`Request failed: ${res.status}`);
  return res.json();
}

export async function fetchTrigger(id: number): Promise<Trigger> {
  const res = await apiFetch(`/api/triggers/${id}`);
  if (res.status === 404) throw new Error('Trigger not found');
  if (!res.ok) throw new Error(`Request failed: ${res.status}`);
  return res.json();
}

export async function createTrigger(input: TriggerInput): Promise<Trigger> {
  const res = await apiFetch('/api/triggers', { method: 'POST', body: JSON.stringify(input) });
  if (!res.ok) throw new Error(await readError(res, 'Failed to create trigger'));
  return res.json();
}

export async function updateTrigger(id: number, input: TriggerInput): Promise<Trigger> {
  const res = await apiFetch(`/api/triggers/${id}`, { method: 'PUT', body: JSON.stringify(input) });
  if (!res.ok) throw new Error(await readError(res, 'Failed to update trigger'));
  return res.json();
}

export async function deleteTrigger(id: number): Promise<void> {
  const res = await apiFetch(`/api/triggers/${id}`, { method: 'DELETE' });
  if (!res.ok && res.status !== 204) throw new Error(await readError(res, 'Failed to delete trigger'));
}

async function readError(res: Response, fallback: string): Promise<string> {
  const text = await res.text().catch(() => '');
  try {
    const data = JSON.parse(text) as { message?: string; title?: string; detail?: string };
    const msg = data.message ?? data.detail ?? data.title;
    if (msg) return `${fallback} (${res.status}): ${msg}`;
  } catch {
    /* not json */
  }
  const snippet = text.replace(/\s+/g, ' ').trim().slice(0, 300);
  return snippet ? `${fallback} (${res.status}): ${snippet}` : `${fallback} (${res.status})`;
}

export interface TriggerMethodInfo {
  id: string;
  name: string;
  library: string;
  tagline: string;
  highlights: string[];
}

export interface NotificationRow {
  id: number;
  triggerMethod: string | null;
  type: string;
  severity: string;
  entityType: string | null;
  entityId: number | null;
  title: string;
  message: string | null;
  payload: string | null;
  isRead: boolean;
  createdAt: string;
}

export async function fetchTriggerMethods(): Promise<TriggerMethodInfo[]> {
  const res = await apiFetch('/api/trigger-methods');
  if (!res.ok) throw new Error(`Request failed: ${res.status}`);
  return res.json();
}

export async function fetchActiveMethod(): Promise<string> {
  const res = await apiFetch('/api/trigger-methods/active');
  if (!res.ok) throw new Error(`Request failed: ${res.status}`);
  const body = (await res.json()) as { method: string };
  return body.method;
}

export async function setActiveMethod(method: string): Promise<string> {
  const res = await apiFetch('/api/trigger-methods/active', {
    method: 'PUT',
    body: JSON.stringify({ method }),
  });
  if (!res.ok) throw new Error(await readError(res, 'Failed to set active method'));
  const body = (await res.json()) as { method: string };
  return body.method;
}

export async function fetchMethodDocs(id: string): Promise<string> {
  const res = await apiFetch(`/api/trigger-methods/${id}/docs`);
  if (!res.ok) throw new Error(`Request failed: ${res.status}`);
  return res.text();
}

export async function fetchNotifications(take = 100): Promise<NotificationRow[]> {
  const res = await apiFetch(`/api/notifications?take=${take}`);
  if (!res.ok) throw new Error(`Request failed: ${res.status}`);
  return res.json();
}

export async function clearNotifications(): Promise<number> {
  const res = await apiFetch('/api/notifications', { method: 'DELETE' });
  if (!res.ok) throw new Error(await readError(res, 'Failed to clear notifications'));
  const body = (await res.json()) as { deleted: number };
  return body.deleted;
}

// --- Products ---
export interface Product {
  id: number;
  name: string;
  description?: string | null;
  sku?: string | null;
  price: number;
  isActive: boolean;
  createdAt: string;
}

export interface ProductInput {
  name: string;
  description?: string | null;
  sku?: string | null;
  price: number;
  isActive: boolean;
}

export async function fetchProducts(): Promise<Product[]> {
  const res = await apiFetch('/api/products');
  if (!res.ok) throw new Error(`Request failed: ${res.status}`);
  return res.json();
}

export async function fetchProduct(id: number): Promise<Product> {
  const res = await apiFetch(`/api/products/${id}`);
  if (res.status === 404) throw new Error('Product not found');
  if (!res.ok) throw new Error(`Request failed: ${res.status}`);
  return res.json();
}

export async function createProduct(input: ProductInput): Promise<Product> {
  const res = await apiFetch('/api/products', { method: 'POST', body: JSON.stringify(input) });
  if (!res.ok) throw new Error(await readError(res, 'Failed to create product'));
  return res.json();
}

export async function updateProduct(id: number, input: ProductInput): Promise<Product> {
  const res = await apiFetch(`/api/products/${id}`, { method: 'PUT', body: JSON.stringify(input) });
  if (!res.ok) throw new Error(await readError(res, 'Failed to update product'));
  return res.json();
}

export async function deleteProduct(id: number): Promise<void> {
  const res = await apiFetch(`/api/products/${id}`, { method: 'DELETE' });
  if (!res.ok && res.status !== 204) throw new Error(await readError(res, 'Failed to delete product'));
}

// --- Customers ---
export interface Customer {
  id: number;
  name: string;
  email?: string | null;
  phone?: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface CustomerInput {
  name: string;
  email?: string | null;
  phone?: string | null;
  isActive: boolean;
}

export async function fetchCustomers(): Promise<Customer[]> {
  const res = await apiFetch('/api/customers');
  if (!res.ok) throw new Error(`Request failed: ${res.status}`);
  return res.json();
}

export async function fetchCustomer(id: number): Promise<Customer> {
  const res = await apiFetch(`/api/customers/${id}`);
  if (res.status === 404) throw new Error('Customer not found');
  if (!res.ok) throw new Error(`Request failed: ${res.status}`);
  return res.json();
}

export async function createCustomer(input: CustomerInput): Promise<Customer> {
  const res = await apiFetch('/api/customers', { method: 'POST', body: JSON.stringify(input) });
  if (!res.ok) throw new Error(await readError(res, 'Failed to create customer'));
  return res.json();
}

export async function updateCustomer(id: number, input: CustomerInput): Promise<Customer> {
  const res = await apiFetch(`/api/customers/${id}`, { method: 'PUT', body: JSON.stringify(input) });
  if (!res.ok) throw new Error(await readError(res, 'Failed to update customer'));
  return res.json();
}

export async function deleteCustomer(id: number): Promise<void> {
  const res = await apiFetch(`/api/customers/${id}`, { method: 'DELETE' });
  if (!res.ok && res.status !== 204) throw new Error(await readError(res, 'Failed to delete customer'));
}
