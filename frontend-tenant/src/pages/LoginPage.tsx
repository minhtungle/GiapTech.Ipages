import { LockOutlined, UserOutlined } from '@ant-design/icons'
import { Button, Card, Form, Input, message, Typography } from 'antd'
import { useNavigate } from 'react-router-dom'
import { api } from '../lib/api'
import { useAuthStore } from '../store/auth'

const { Title, Text } = Typography

export default function LoginPage() {
  const navigate = useNavigate()
  const { setAuth, setTenantSlug } = useAuthStore()
  const [form] = Form.useForm()
  const tenantSlug = window.location.hostname.split('.')[0]

  const handleLogin = async (values: { username: string; password: string }) => {
    try {
      const res = await api.post('/auth/login', {
        username: values.username,
        password: values.password,
        tenantSlug,
      })
      setAuth(res.data.accessToken, res.data.refreshToken, res.data.user)
      setTenantSlug(tenantSlug)
      navigate('/admin')
    } catch {
      message.error('Tên đăng nhập hoặc mật khẩu không đúng.')
    }
  }

  return (
    <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', background: '#f0f2f5' }}>
      <Card style={{ width: 400, boxShadow: '0 4px 24px rgba(0,0,0,0.1)' }}>
        <div style={{ textAlign: 'center', marginBottom: 32 }}>
          <Title level={3} style={{ margin: 0 }}>Tenant Admin</Title>
          <Text type="secondary">{tenantSlug}.localhost</Text>
        </div>
        <Form form={form} onFinish={handleLogin} layout="vertical">
          <Form.Item name="username" rules={[{ required: true }]}>
            <Input prefix={<UserOutlined />} placeholder="Tên đăng nhập" size="large" />
          </Form.Item>
          <Form.Item name="password" rules={[{ required: true }]}>
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
