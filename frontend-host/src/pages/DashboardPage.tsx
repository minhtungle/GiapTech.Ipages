import { ApiOutlined, CheckCircleOutlined, CloseCircleOutlined, TeamOutlined } from '@ant-design/icons'
import { Card, Col, Row, Statistic, Typography } from 'antd'
import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api'

const { Title } = Typography

interface HostDashboard {
  totalTenants: number
  activeTenants: number
  inactiveTenants: number
  newTenantsThisMonth: number
}

export default function DashboardPage() {
  const { data, isLoading } = useQuery<HostDashboard>({
    queryKey: ['host-dashboard'],
    queryFn: () => api.get('/dashboard').then(r => r.data),
  })

  return (
    <div style={{ padding: 24 }}>
      <Title level={4} style={{ marginBottom: 24 }}>Dashboard</Title>
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card loading={isLoading}>
            <Statistic title="Tổng Tenant" value={data?.totalTenants ?? 0} prefix={<TeamOutlined />} valueStyle={{ color: '#1677ff' }} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card loading={isLoading}>
            <Statistic title="Đang Hoạt Động" value={data?.activeTenants ?? 0} prefix={<CheckCircleOutlined />} valueStyle={{ color: '#52c41a' }} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card loading={isLoading}>
            <Statistic title="Không Hoạt Động" value={data?.inactiveTenants ?? 0} prefix={<CloseCircleOutlined />} valueStyle={{ color: '#ff4d4f' }} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card loading={isLoading}>
            <Statistic title="Mới Tháng Này" value={data?.newTenantsThisMonth ?? 0} prefix={<ApiOutlined />} valueStyle={{ color: '#722ed1' }} />
          </Card>
        </Col>
      </Row>
    </div>
  )
}
