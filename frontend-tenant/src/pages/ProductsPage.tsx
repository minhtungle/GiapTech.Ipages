import { DeleteOutlined, EditOutlined, PlusOutlined } from '@ant-design/icons'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Image, Input, message, Modal, Select, Space, Table, Tag, Typography } from 'antd'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../lib/api'

const { Title } = Typography

interface Product {
  id: string; name: string; slug: string; sku?: string
  price: number; salePrice?: number; stockQuantity: number
  status: number; thumbnailUrl?: string
}

const STATUS_COLOR: Record<number, string> = { 1: 'default', 2: 'orange', 3: 'green', 4: 'red', 5: 'gray' }
const STATUS_LABEL: Record<number, string> = { 1: 'Bản nháp', 2: 'Đang ẩn', 3: 'Hoạt động', 4: 'Hết hàng', 5: 'Ngừng bán' }

export default function ProductsPage() {
  const navigate = useNavigate()
  const qc = useQueryClient()
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState<number | undefined>()
  const [page, setPage] = useState(1)

  const { data, isLoading } = useQuery({
    queryKey: ['products', search, status, page],
    queryFn: () => api.get('/products', { params: { search, status, page, pageSize: 20 } }).then(r => r.data),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/products/${id}`),
    onSuccess: () => { message.success('Đã xoá sản phẩm'); qc.invalidateQueries({ queryKey: ['products'] }) },
  })

  const columns = [
    {
      title: 'Sản phẩm', key: 'product',
      render: (_: unknown, r: Product) => (
        <Space>
          {r.thumbnailUrl
            ? <Image src={r.thumbnailUrl} width={40} height={40} style={{ objectFit: 'cover', borderRadius: 4 }} preview={false} />
            : <div style={{ width: 40, height: 40, background: '#f0f0f0', borderRadius: 4 }} />
          }
          <div>
            <div style={{ fontWeight: 500 }}>{r.name}</div>
            <div style={{ color: '#999', fontSize: 12 }}>{r.sku || r.slug}</div>
          </div>
        </Space>
      ),
    },
    {
      title: 'Giá', key: 'price',
      render: (_: unknown, r: Product) => (
        <div>
          {r.salePrice
            ? <>
                <span style={{ color: '#f5222d', fontWeight: 600 }}>{r.salePrice.toLocaleString('vi-VN')}₫</span>
                <br /><span style={{ textDecoration: 'line-through', color: '#999', fontSize: 12 }}>{r.price.toLocaleString('vi-VN')}₫</span>
              </>
            : <span>{r.price.toLocaleString('vi-VN')}₫</span>
          }
        </div>
      ),
    },
    { title: 'Tồn kho', dataIndex: 'stockQuantity', key: 'stock', width: 90 },
    {
      title: 'Trạng thái', dataIndex: 'status', key: 'status', width: 130,
      render: (v: number) => <Tag color={STATUS_COLOR[v]}>{STATUS_LABEL[v] ?? v}</Tag>,
    },
    {
      title: '', key: 'actions', width: 90,
      render: (_: unknown, r: Product) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={() => navigate(`/admin/products/${r.id}/edit`)} />
          <Button size="small" danger icon={<DeleteOutlined />}
            onClick={() => Modal.confirm({ title: `Xoá "${r.name}"?`, okType: 'danger', onOk: () => deleteMutation.mutate(r.id) })} />
        </Space>
      ),
    },
  ]

  return (
    <div style={{ padding: 24 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>Sản phẩm</Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/admin/products/new')}>
          Thêm sản phẩm
        </Button>
      </div>

      <Space style={{ marginBottom: 16 }} wrap>
        <Input.Search
          placeholder="Tìm tên, SKU, slug..."
          value={search}
          onChange={e => { setSearch(e.target.value); setPage(1) }}
          style={{ width: 300 }}
          allowClear
        />
        <Select
          placeholder="Lọc trạng thái"
          allowClear
          style={{ width: 180 }}
          value={status}
          onChange={v => { setStatus(v); setPage(1) }}
          options={[
            { value: 1, label: 'Bản nháp' },
            { value: 2, label: 'Đang ẩn' },
            { value: 3, label: 'Hoạt động' },
            { value: 4, label: 'Hết hàng' },
            { value: 5, label: 'Ngừng bán' },
          ]}
        />
      </Space>

      <Table
        rowKey="id"
        loading={isLoading}
        dataSource={data?.items ?? []}
        columns={columns}
        pagination={{ total: data?.totalCount ?? 0, current: page, pageSize: 20, onChange: setPage, showTotal: t => `${t} sản phẩm` }}
      />
    </div>
  )
}
