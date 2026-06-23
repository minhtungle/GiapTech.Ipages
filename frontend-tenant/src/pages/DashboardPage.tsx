import {
  AppstoreOutlined, ClockCircleOutlined, DollarOutlined,
  FileTextOutlined, ShoppingCartOutlined, TeamOutlined,
} from '@ant-design/icons'
import { Card, Col, Row, Statistic, Typography } from 'antd'
import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api'

const { Title } = Typography

interface TenantDashboard {
  totalProducts: number
  activeProducts: number
  totalOrders: number
  pendingOrders: number
  todayOrders: number
  todayRevenue: number
  thisMonthRevenue: number
  totalCustomers: number
  totalArticles: number
}

const fmt = (n: number) => n.toLocaleString('vi-VN') + ' ₫'

export default function DashboardPage() {
  const { data, isLoading } = useQuery<TenantDashboard>({
    queryKey: ['tenant-dashboard'],
    queryFn: () => api.get('/dashboard').then(r => r.data),
  })

  return (
    <div style={{ padding: 24 }}>
      <Title level={4} style={{ marginBottom: 24 }}>Dashboard</Title>
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card loading={isLoading}>
            <Statistic title="Đơn hàng hôm nay" value={data?.todayOrders ?? 0} prefix={<ShoppingCartOutlined />} valueStyle={{ color: '#1677ff' }} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card loading={isLoading}>
            <Statistic title="Doanh thu hôm nay" value={fmt(data?.todayRevenue ?? 0)} prefix={<DollarOutlined />} valueStyle={{ color: '#52c41a' }} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card loading={isLoading}>
            <Statistic title="Doanh thu tháng này" value={fmt(data?.thisMonthRevenue ?? 0)} prefix={<DollarOutlined />} valueStyle={{ color: '#fa8c16' }} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card loading={isLoading}>
            <Statistic title="Chờ xử lý" value={data?.pendingOrders ?? 0} prefix={<ClockCircleOutlined />} valueStyle={{ color: '#ff4d4f' }} />
          </Card>
        </Col>
      </Row>
      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col xs={24} sm={8}>
          <Card loading={isLoading}>
            <Statistic title="Sản phẩm đang bán" value={data?.activeProducts ?? 0} suffix={`/ ${data?.totalProducts ?? 0}`} prefix={<AppstoreOutlined />} valueStyle={{ color: '#1677ff' }} />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card loading={isLoading}>
            <Statistic title="Khách hàng" value={data?.totalCustomers ?? 0} prefix={<TeamOutlined />} valueStyle={{ color: '#722ed1' }} />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card loading={isLoading}>
            <Statistic title="Bài viết" value={data?.totalArticles ?? 0} prefix={<FileTextOutlined />} valueStyle={{ color: '#13c2c2' }} />
          </Card>
        </Col>
      </Row>
    </div>
  )
}
