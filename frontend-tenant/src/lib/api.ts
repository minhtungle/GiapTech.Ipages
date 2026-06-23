import axios, { AxiosError } from 'axios'
import { message } from 'antd'
import { useAuthStore } from '../store/auth'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '/api/v1',
  timeout: 30_000,
})

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

let isRefreshing = false
let pendingQueue: Array<{ resolve: (v: string) => void; reject: (e: unknown) => void }> = []

function processQueue(error: unknown, token: string | null) {
  pendingQueue.forEach(p => (error ? p.reject(error) : p.resolve(token!)))
  pendingQueue = []
}

api.interceptors.response.use(
  (res) => res,
  async (err: AxiosError) => {
    const { refreshToken, setAuth, clearAuth } = useAuthStore.getState()
    const originalConfig = err.config as typeof err.config & { _retry?: boolean }

    if (err.response?.status === 401 && !originalConfig?._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          pendingQueue.push({ resolve, reject })
        }).then((token) => {
          originalConfig!.headers!.Authorization = `Bearer ${token}`
          return axios(originalConfig!)
        })
      }

      if (refreshToken) {
        originalConfig!._retry = true
        isRefreshing = true
        try {
          const res = await axios.post('/api/v1/auth/refresh', { refreshToken })
          const { accessToken, refreshToken: newRefresh, user } = res.data
          setAuth(accessToken, newRefresh, user)
          processQueue(null, accessToken)
          originalConfig!.headers!.Authorization = `Bearer ${accessToken}`
          return axios(originalConfig!)
        } catch (refreshErr) {
          processQueue(refreshErr, null)
          clearAuth()
          window.location.href = '/admin/login'
        } finally {
          isRefreshing = false
        }
      } else {
        clearAuth()
        window.location.href = '/admin/login'
      }
    }

    const data = err.response?.data as Record<string, unknown> | undefined
    const serverMessage = (data?.message ?? data?.title) as string | undefined
    if (serverMessage && err.response?.status !== 401) {
      message.error(serverMessage)
    } else if (!err.response && err.code === 'ECONNABORTED') {
      message.error('Kết nối quá thời gian, vui lòng thử lại')
    } else if (!err.response) {
      message.error('Không thể kết nối đến server')
    }

    return Promise.reject(err)
  },
)
