import { useQuery } from '@tanstack/react-query'
import { Input, Table, Tag, Typography } from 'antd'
import { useState } from 'react'
import { api } from '../lib/api'

const { Title } = Typography

export default function CustomersPage() {
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)

  const { data, isLoading } = useQuery({
    queryKey: ['customers', search, page],
    queryFn: () => api.get('/customers', { params: { search, page, pageSize: 20 } }).then(r => r.data),
  })

  const columns = [
    { title: 'Họ tên', dataIndex: 'fullName', key: 'fullName' },
    { title: 'Email', dataIndex: 'email', key: 'email', render: (v: string) => v || '—' },
    { title: 'SĐT', dataIndex: 'phone', key: 'phone', render: (v: string) => v || '—' },
    { title: 'Điểm tích luỹ', dataIndex: 'loyaltyPoints', key: 'loyaltyPoints' },
    {
      title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive',
      render: (v: boolean) => <Tag color={v ? 'green' : 'red'}>{v ? 'Active' : 'Inactive'}</Tag>,
    },
  ]

  return (
    <div style={{ padding: 24, background: '#fff', borderRadius: 8 }}>
      <Title level={4} style={{ marginBottom: 16 }}>Khách hàng</Title>
      <Input.Search placeholder="Tìm tên, SĐT..." value={search} onChange={e => setSearch(e.target.value)} style={{ maxWidth: 400, marginBottom: 16 }} allowClear />
      <Table rowKey="id" loading={isLoading} dataSource={data?.items ?? []} columns={columns}
        pagination={{ total: data?.total ?? 0, current: page, pageSize: 20, onChange: setPage }} />
    </div>
  )
}
