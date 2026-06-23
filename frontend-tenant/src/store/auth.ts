import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface User {
  id: string
  username: string
  fullName?: string
  email?: string
  isHostAdmin: boolean
  tenantId?: string
  permissions: string[]
}

interface AuthState {
  token: string | null
  refreshToken: string | null
  user: User | null
  tenantSlug: string | null
  setAuth: (token: string, refreshToken: string, user: User) => void
  setTenantSlug: (slug: string) => void
  clearAuth: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      refreshToken: null,
      user: null,
      tenantSlug: null,
      setAuth: (token, refreshToken, user) => set({ token, refreshToken, user }),
      setTenantSlug: (tenantSlug) => set({ tenantSlug }),
      clearAuth: () => set({ token: null, refreshToken: null, user: null }),
    }),
    { name: 'tenant-auth' },
  ),
)
