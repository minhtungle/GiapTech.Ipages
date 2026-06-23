import { DeleteOutlined, EditOutlined, PlusOutlined } from '@ant-design/icons'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Button, Input, message, Modal, Space, Table, Tag, Typography } from 'antd'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../lib/api'
import dayjs from 'dayjs'

const { Title } = Typography

export default function TenantsPage() {
  const navigate = useNavigate()
  const qc = useQueryClient()
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)

  const { data, isLoading } = useQuery({
    queryKey: ['tenants', search, page],
    queryFn: () =>
      api.get('/tenants', { params: { search, page, pageSize: 20 } }).then((r) => r.data),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/tenants/${id}`),
    onSuccess: () => {
      message.success('Đã xoá tenant')
      qc.invalidateQueries({ queryKey: ['tenants'] })
    },
    onError: () => message.error('Xoá thất bại'),
  })

  const handleDelete = (id: string) => {
    Modal.confirm({
      title: 'Xác nhận xoá tenant?',
      okText: 'Xoá',
      okType: 'danger',
      cancelText: 'Huỷ',
      onOk: () => deleteMutation.mutate(id),
    })
  }

  const columns = [
    { title: 'Tên', dataIndex: 'name', key: 'name' },
    { title: 'Slug', dataIndex: 'slug', key: 'slug', render: (v: string) => <code>{v}</code> },
    { title: 'Email', dataIndex: 'email', key: 'email' },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      render: (v: number) => (
        <Tag color={v === 1 ? 'green' : v === 3 ? 'red' : 'orange'}>
          {v === 1 ? 'Active' : v === 2 ? 'Inactive' : v === 3 ? 'Suspended' : 'Expired'}
        </Tag>
      ),
    },
    {
      title: 'Hết hạn',
      dataIndex: 'expiresAt',
      key: 'expiresAt',
      render: (v: string) => v ? dayjs(v).format('DD/MM/YYYY') : '—',
    },
    {
      title: 'Thao tác',
      key: 'actions',
      render: (_: unknown, record: { id: string }) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={() => navigate(`/admin/tenants/${record.id}/edit`)}>
            Sửa
          </Button>
          <Button size="small" danger icon={<DeleteOutlined />} onClick={() => handleDelete(record.id)}>
            Xoá
          </Button>
        </Space>
      ),
    },
  ]

  return (
    <div style={{ padding: 24, background: '#fff', borderRadius: 8 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>Quản lý Tenant</Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/admin/tenants/new')}>
          Thêm Tenant
        </Button>
      </div>
      <Input.Search
        placeholder="Tìm kiếm theo tên, slug..."
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        style={{ marginBottom: 16, maxWidth: 400 }}
        allowClear
      />
      <Table
        rowKey="id"
        loading={isLoading}
        dataSource={data?.items ?? []}
        columns={columns}
        pagination={{
          total: data?.total ?? 0,
          current: page,
          pageSize: 20,
          onChange: setPage,
          showTotal: (t) => `Tổng ${t} tenant`,
        }}
      />
    </div>
  )
}
