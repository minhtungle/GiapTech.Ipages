import { LockOutlined, ShopOutlined, UserOutlined } from '@ant-design/icons'
import { Button, Card, Form, Input, message, Typography } from 'antd'
import { useNavigate } from 'react-router-dom'
import { api } from '../lib/api'
import { useAuthStore } from '../store/auth'

const { Title, Text } = Typography

function detectTenantSlug(): string {
  const hostname = window.location.hostname
  if (!hostname.includes('.')) return ''
  const sub = hostname.split('.')[0]
  return sub === 'www' ? '' : sub
}

export default function LoginPage() {
  const navigate = useNavigate()
  const { setAuth, setTenantSlug } = useAuthStore()
  const [form] = Form.useForm()
  const detectedSlug = detectTenantSlug()

  const handleLogin = async (values: { tenantSlug: string; username: string; password: string }) => {
    try {
      const res = await api.post('/auth/login', {
        username: values.username,
        password: values.password,
        tenantSlug: values.tenantSlug,
      })
      setAuth(res.data.accessToken, res.data.refreshToken, res.data.user)
      setTenantSlug(values.tenantSlug)
      navigate('/admin')
    } catch {
      message.error('Tên đăng nhập hoặc mật khẩu không đúng.')
    }
  }

  return (
    <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', background: '#f0f2f5' }}>
      <Card style={{ width: 420, boxShadow: '0 4px 24px rgba(0,0,0,0.1)' }}>
        <div style={{ textAlign: 'center', marginBottom: 32 }}>
          <Title level={3} style={{ margin: 0 }}>Tenant Admin</Title>
          <Text type="secondary">Đăng nhập vào cổng quản trị</Text>
        </div>
        <Form
          form={form}
          onFinish={handleLogin}
          layout="vertical"
          initialValues={{ tenantSlug: detectedSlug }}
        >
          <Form.Item
            name="tenantSlug"
            label="Slug tenant"
            rules={[{ required: true, message: 'Nhập slug tenant (VD: demo)' }]}
          >
            <Input
              prefix={<ShopOutlined />}
              placeholder="VD: demo"
              addonAfter=".localhost"
              size="large"
            />
          </Form.Item>
          <Form.Item name="username" label="Tên đăng nhập" rules={[{ required: true }]}>
            <Input prefix={<UserOutlined />} placeholder="Tên đăng nhập" size="large" />
          </Form.Item>
          <Form.Item name="password" label="Mật khẩu" rules={[{ required: true }]}>
            <Input.Password prefix={<LockOutlined />} placeholder="Mật khẩu" size="large" />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" block size="large">
              Đăng nhập
            </Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  )
}
