import {
  AppstoreOutlined,
  BarsOutlined,
  DashboardOutlined,
  FileTextOutlined,
  LogoutOutlined,
  PictureOutlined,
  ShoppingCartOutlined,
  TeamOutlined,
} from '@ant-design/icons'
import { Avatar, Dropdown, Layout, Menu, Typography } from 'antd'
import { useState } from 'react'
import { Outlet, useNavigate } from 'react-router-dom'
import { useAuthStore } from '../store/auth'

const { Header, Sider, Content } = Layout
const { Text } = Typography

const menuItems = [
  { key: '/admin', icon: <DashboardOutlined />, label: 'Dashboard' },
  { key: '/admin/products', icon: <AppstoreOutlined />, label: 'Sản phẩm' },
  { key: '/admin/product-categories', icon: <BarsOutlined />, label: 'Danh mục SP' },
  { key: '/admin/orders', icon: <ShoppingCartOutlined />, label: 'Đơn hàng' },
  { key: '/admin/customers', icon: <TeamOutlined />, label: 'Khách hàng' },
  { key: '/admin/articles', icon: <FileTextOutlined />, label: 'Bài viết' },
  { key: '/admin/media', icon: <PictureOutlined />, label: 'Media' },
]

export default function AdminLayout() {
  const [collapsed, setCollapsed] = useState(false)
  const navigate = useNavigate()
  const { user, tenantSlug, clearAuth } = useAuthStore()

  const userMenu = [
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: 'Đăng xuất',
      danger: true,
      onClick: () => { clearAuth(); navigate('/admin/login') },
    },
  ]

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider collapsible collapsed={collapsed} onCollapse={setCollapsed} theme="dark" width={220}>
        <div style={{ padding: '16px', color: 'white', fontWeight: 700, fontSize: 14, borderBottom: '1px solid rgba(255,255,255,0.1)', overflow: 'hidden', whiteSpace: 'nowrap' }}>
          {collapsed ? 'TA' : (tenantSlug ? `${tenantSlug}.localhost` : 'Tenant Admin')}
        </div>
        <Menu
          theme="dark"
          mode="inline"
          items={menuItems}
          onClick={({ key }) => navigate(key)}
        />
      </Sider>
      <Layout>
        <Header style={{ background: '#fff', padding: '0 24px', display: 'flex', alignItems: 'center', justifyContent: 'space-between', boxShadow: '0 1px 4px rgba(0,0,0,0.08)' }}>
          <Text strong>Admin Dashboard</Text>
          <Dropdown menu={{ items: userMenu }} placement="bottomRight">
            <div style={{ cursor: 'pointer', display: 'flex', alignItems: 'center', gap: 8 }}>
              <Avatar style={{ background: '#1677ff' }}>{user?.username?.[0]?.toUpperCase()}</Avatar>
              <Text>{user?.fullName ?? user?.username}</Text>
            </div>
          </Dropdown>
        </Header>
        <Content style={{ margin: 24, background: '#f5f5f5', minHeight: 'calc(100vh - 64px)' }}>
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  )
}
