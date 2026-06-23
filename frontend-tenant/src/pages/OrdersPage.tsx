import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Button, message, Modal, Select, Space, Table, Tag, Typography } from 'antd'
import { useState } from 'react'
import { api } from '../lib/api'
import dayjs from 'dayjs'

const { Title } = Typography

const statusMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Chờ xác nhận', color: 'gold' },
  2: { label: 'Đã xác nhận', color: 'blue' },
  3: { label: 'Đang giao', color: 'cyan' },
  4: { label: 'Hoàn thành', color: 'green' },
  5: { label: 'Đã huỷ', color: 'red' },
}

interface Order { id: string; orderCode: string; customerName: string; customerPhone: string; total: number; status: number; createdAt: string }

export default function OrdersPage() {
  const qc = useQueryClient()
  const [page, setPage] = useState(1)

  const { data, isLoading } = useQuery({
    queryKey: ['orders', page],
    queryFn: () => api.get('/orders', { params: { page, pageSize: 20 } }).then(r => r.data),
  })

  const statusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: number }) =>
      api.patch(`/orders/${id}/status`, { status }),
    onSuccess: () => { message.success('Cập nhật trạng thái thành công'); qc.invalidateQueries({ queryKey: ['orders'] }) },
  })

  const handleStatus = (id: string, current: number) => {
    const next = current === 1 ? 2 : current === 2 ? 3 : current === 3 ? 4 : null
    if (!next) return
    Modal.confirm({
      title: `Chuyển sang "${statusMap[next].label}"?`,
      onOk: () => statusMutation.mutate({ id, status: next }),
    })
  }

  const columns = [
    { title: 'Mã đơn', dataIndex: 'orderCode', key: 'orderCode', render: (v: string) => <code>{v}</code> },
    { title: 'Khách hàng', dataIndex: 'customerName', key: 'customerName' },
    { title: 'SĐT', dataIndex: 'customerPhone', key: 'customerPhone' },
    { title: 'Tổng tiền', dataIndex: 'total', key: 'total', render: (v: number) => v.toLocaleString('vi-VN') + ' ₫' },
    {
      title: 'Trạng thái', dataIndex: 'status', key: 'status',
      render: (v: number) => <Tag color={statusMap[v]?.color}>{statusMap[v]?.label}</Tag>,
    },
    { title: 'Ngày đặt', dataIndex: 'createdAt', key: 'createdAt', render: (v: string) => dayjs(v).format('DD/MM/YYYY HH:mm') },
    {
      title: '', key: 'actions',
      render: (_: unknown, r: Order) => (
        <Space>
          {r.status < 4 && r.status !== 5 && (
            <Button size="small" type="primary" onClick={() => handleStatus(r.id, r.status)}>
              Tiến trình
            </Button>
          )}
        </Space>
      ),
    },
  ]

  return (
    <div style={{ padding: 24, background: '#fff', borderRadius: 8 }}>
      <Title level={4} style={{ marginBottom: 16 }}>Đơn hàng</Title>
      <Table rowKey="id" loading={isLoading} dataSource={data?.items ?? []} columns={columns}
        pagination={{ total: data?.total ?? 0, current: page, pageSize: 20, onChange: setPage }} />
    </div>
  )
}
