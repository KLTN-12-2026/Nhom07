import { SubscriptionPackage, TenantCondo } from '../types';

/**
 * Platform API Base URL:
 * In development, defaults to '/api/platform' which is proxied by server.ts & vite.config.ts
 * to https://trandoi-001-site1.gtempurl.com (avoiding CORS and SSL issues).
 */
export const PLATFORM_API_BASE =
  (typeof process !== 'undefined' && process.env?.VITE_PLATFORM_API_BASE) ||
  (typeof import.meta !== 'undefined' && import.meta.env?.VITE_PLATFORM_API_BASE) ||
  '/api/platform';

export const REMOTE_PLATFORM_HOST = 'https://trandoi-001-site1.gtempurl.com';

// ============================================================
// BACKEND SCHEMAS (Swagger EzCondo.BackEndServer v1.0)
// ============================================================

export interface BackendSubscriptionPlan {
  id: string; // uuid
  name: string;
  description?: string;
  billingCycleMonths: number;
  price: number;
  featureLimits?: any;
  isAvailable: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateSubscriptionPlanRequest {
  name: string;
  description?: string;
  billingCycleMonths: number;
  price: number;
  featureLimits?: any;
  isAvailable: boolean;
}

export interface UpdateSubscriptionPlanRequest {
  name: string;
  description?: string;
  price: number;
  featureLimits?: any;
  isAvailable: boolean;
}

export interface CreateTenantRequest {
  legalName: string;
  taxCode: string;
  address: string;
  subscriptionPlanId?: string;
  trialEndsAt?: string;
}

export interface BackendTenantResponse {
  id: string;
  legalName?: string;
  taxCode?: string;
  address?: string;
  code?: string;
  slug?: string;
  trialEndsAt?: string;
  status: number; // 0: Trial, 1: Active, 2: Suspended, 3: Archived
  subscription?: {
    id: string;
    planId: string;
    planName?: string;
    status: number;
    startsAt: string;
    endsAt: string;
  };
  createdAt: string;
  updatedAt?: string;
}

export interface ScheduleTenantSuspensionRequest {
  reasonCode: 'Unspecified' | 'OwnerRequest' | 'PolicyViolation';
  reason: string;
  noticeHours: number; // 12 - 24
}

export interface TenantSuspensionResponse {
  tenantId: string;
  currentStatus: number;
  reasonCode: string;
  reason?: string;
  effectiveAt: string;
  noticeSentAt?: string;
  suspendedAt?: string;
}

export interface TenantRestoreResponse {
  tenatId?: string; // backend typo
  tenantId?: string;
  status: number;
  restoredAt: string;
}

// ============================================================
// HTTP CLIENT
// ============================================================

async function platformRequest<T>(endpoint: string, options?: RequestInit): Promise<T> {
  const url = `${PLATFORM_API_BASE}${endpoint}`;
  try {
    const res = await fetch(url, {
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json',
      },
      ...options,
    });

    if (!res.ok) {
      const errText = await res.text();
      let parsedMessage = errText;
      try {
        const errJson = JSON.parse(errText);
        parsedMessage = errJson.detail || errJson.title || errJson.message || errText;
      } catch {
        // use raw text
      }
      throw new Error(`Platform API Error (${res.status}): ${parsedMessage}`);
    }

    if (res.status === 204) {
      return undefined as T;
    }

    return (await res.json()) as T;
  } catch (error: any) {
    // If local proxy failed, try direct remote call as fallback
    if (!endpoint.startsWith('http') && PLATFORM_API_BASE.startsWith('/')) {
      const directUrl = `${REMOTE_PLATFORM_HOST}/api/platform${endpoint}`;
      try {
        const directRes = await fetch(directUrl, {
          headers: {
            'Content-Type': 'application/json',
            Accept: 'application/json',
          },
          ...options,
        });
        if (directRes.ok) {
          if (directRes.status === 204) return undefined as T;
          return (await directRes.json()) as T;
        }
      } catch {
        // throw original error
      }
    }
    throw error;
  }
}

// ============================================================
// API METHODS
// ============================================================

export const platformApi = {
  // Subscription Plans
  subscriptionPlans: {
    getAll: () => platformRequest<BackendSubscriptionPlan[]>('/subscription-plans'),
    getById: (id: string) => platformRequest<BackendSubscriptionPlan>(`/subscription-plans/${id}`),
    create: (data: CreateSubscriptionPlanRequest) =>
      platformRequest<BackendSubscriptionPlan>('/subscription-plans', {
        method: 'POST',
        body: JSON.stringify(data),
      }),
    update: (id: string, data: UpdateSubscriptionPlanRequest) =>
      platformRequest<BackendSubscriptionPlan>(`/subscription-plans/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data),
      }),
    delete: (id: string) =>
      platformRequest<void>(`/subscription-plans/${id}`, {
        method: 'DELETE',
      }),
    discontinue: (id: string) =>
      platformRequest<any>(`/subscription-plans/${id}/discontinue`, {
        method: 'POST',
      }),
  },

  // Tenants
  tenants: {
    create: (data: CreateTenantRequest) =>
      platformRequest<BackendTenantResponse>('/tenants', {
        method: 'POST',
        body: JSON.stringify(data),
      }),
    suspend: (id: string, data: ScheduleTenantSuspensionRequest) =>
      platformRequest<TenantSuspensionResponse>(`/tenants/${id}/suspension`, {
        method: 'POST',
        body: JSON.stringify(data),
      }),
    restore: (id: string) =>
      platformRequest<TenantRestoreResponse>(`/tenants/${id}/restore`, {
        method: 'POST',
      }),
  },

  // Health check
  healthCheck: async (): Promise<boolean> => {
    try {
      const res = await fetch(`${REMOTE_PLATFORM_HOST}/WeatherForecast`, {
        method: 'GET',
        headers: { Accept: 'application/json' },
      });
      return res.ok;
    } catch {
      return false;
    }
  },
};

// ============================================================
// DATA MAPPERS (Backend <-> Frontend)
// ============================================================

export function mapBackendPlanToFrontend(plan: BackendSubscriptionPlan): SubscriptionPackage {
  const code = (plan.name || 'Gói').toUpperCase().replace(/\s+/g, '-');
  return {
    id: plan.id,
    code,
    name: plan.name,
    tagline: plan.description || 'Gói giải pháp toàn diện cho vận hành chung cư',
    monthlyPrice: plan.price,
    annualDiscountPercent: 15,
    maxApartments: plan.featureLimits?.maxApartments || 500,
    status: plan.isAvailable ? 'active' : 'discontinued',
    features: Array.isArray(plan.featureLimits?.features)
      ? plan.featureLimits.features
      : [
          'Quản lý danh mục căn hộ & hồ sơ cư dân',
          'Tự động hóa phát hành hóa đơn hàng tháng',
          'Cổng tương tác cư dân trực tuyến 24/7',
          'AI Hỗ trợ vận hành thông minh & cảnh báo ưu tiên',
        ],
    createdAt: plan.createdAt ? plan.createdAt.split('T')[0] : new Date().toISOString().split('T')[0],
    updatedAt: plan.updatedAt ? plan.updatedAt.split('T')[0] : new Date().toISOString().split('T')[0],
  };
}

export function mapFrontendPackageToBackendCreate(
  pkg: Omit<SubscriptionPackage, 'id' | 'createdAt' | 'updatedAt'>
): CreateSubscriptionPlanRequest {
  return {
    name: pkg.name.slice(0, 50),
    description: pkg.tagline || `${pkg.name} - Tối đa ${pkg.maxApartments} căn hộ`,
    billingCycleMonths: 1,
    price: pkg.monthlyPrice,
    featureLimits: {
      maxApartments: pkg.maxApartments,
      annualDiscountPercent: pkg.annualDiscountPercent,
      features: pkg.features,
    },
    isAvailable: pkg.status === 'active',
  };
}

export function mapFrontendTenantToBackendCreate(
  tenant: Partial<TenantCondo>,
  planId?: string
): CreateTenantRequest {
  return {
    legalName: tenant.legalInfo?.companyName || `Ban Quản Trị ${tenant.name || 'Chung Cư'}`,
    taxCode: tenant.legalInfo?.taxCode || '0312345678',
    address: tenant.address || 'Hồ Chí Minh, Việt Nam',
    subscriptionPlanId: planId || (tenant.packageId && tenant.packageId.includes('-') && tenant.packageId.length === 36 ? tenant.packageId : undefined),
    trialEndsAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
  };
}

