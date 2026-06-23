import { DeleteOutlined, EditOutlined, PlusOutlined } from '@ant-design/icons'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Form, Input, InputNumber, message, Modal, Select, Space, Table, Tag, Typography } from 'antd'
import { useState } from 'react'
import { api } from '../lib/api'

const { Title } = Typography

interface Product { id: string; name: string; slug: string; sku?: string; price: number; salePrice?: number; stockQuantity: number; status: number }

export default function ProductsPage() {
  const qc = useQueryClient()
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [modalOpen, setModalOpen] = useState(false)
  const [editingProduct, setEditingProduct] = useState<Product | null>(null)
  const [form] = Form.useForm()

  const { data, isLoading } = useQuery({
    queryKey: ['products', search, page],
    queryFn: () => api.get('/products', { params: { search, page, pageSize: 20 } }).then(r => r.data),
  })

  const saveMutation = useMutation({
    mutationFn: (values: Record<string, unknown>) =>
      editingProduct
        ? api.put(`/products/${editingProduct.id}`, values)
        : api.post('/products', values),
    onSuccess: () => {
      message.success('Lưu thành công')
      qc.invalidateQueries({ queryKey: ['products'] })
      setModalOpen(false)
      form.resetFields()
    },
    onError: () => message.error('Có lỗi xảy ra'),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/products/${id}`),
    onSuccess: () => { message.success('Đã xoá'); qc.invalidateQueries({ queryKey: ['products'] }) },
  })

  const openCreate = () => { setEditingProduct(null); form.resetFields(); setModalOpen(true) }
  const openEdit = (p: Product) => {
    setEditingProduct(p)
    form.setFieldsValue(p)
    setModalOpen(true)
  }

  const columns = [
    { title: 'Tên sản phẩm', dataIndex: 'name', key: 'name' },
    { title: 'SKU', dataIndex: 'sku', key: 'sku', render: (v: string) => v || '—' },
    { title: 'Giá', dataIndex: 'price', key: 'price', render: (v: number) => v.toLocaleString('vi-VN') + ' ₫' },
    { title: 'Tồn kho', dataIndex: 'stockQuantity', key: 'stockQuantity' },
    {
      title: 'Trạng thái', dataIndex: 'status', key: 'status',
      render: (v: number) => <Tag color={v === 1 ? 'green' : 'red'}>{v === 1 ? 'Active' : 'Inactive'}</Tag>,
    },
    {
      title: '', key: 'actions',
      render: (_: unknown, r: Product) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={() => openEdit(r)} />
          <Button size="small" danger icon={<DeleteOutlined />} onClick={() => Modal.confirm({ title: 'Xoá?', onOk: () => deleteMutation.mutate(r.id) })} />
        </Space>
      ),
    },
  ]

  return (
    <div style={{ padding: 24, background: '#fff', borderRadius: 8 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>Sản phẩm</Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>Thêm sản phẩm</Button>
      </div>
      <Input.Search placeholder="Tìm tên, SKU..." value={search} onChange={e => setSearch(e.target.value)} style={{ maxWidth: 400, marginBottom: 16 }} allowClear />
      <Table rowKey="id" loading={isLoading} dataSource={data?.items ?? []} columns={columns}
        pagination={{ total: data?.total ?? 0, current: page, pageSize: 20, onChange: setPage }} />

      <Modal
        open={modalOpen}
        title={editingProduct ? 'Sửa sản phẩm' : 'Thêm sản phẩm'}
        onCancel={() => { setModalOpen(false); form.resetFields() }}
        onOk={() => form.submit()}
        confirmLoading={saveMutation.isPending}
        width={600}
      >
        <Form form={form} layout="vertical" onFinish={vals => saveMutation.mutate(vals)}>
          <Form.Item name="name" label="Tên" rules={[{ required: true }]}><Input /></Form.Item>
          {!editingProduct && <Form.Item name="slug" label="Slug" rules={[{ required: true }]}><Input /></Form.Item>}
          {!editingProduct && <Form.Item name="sku" label="SKU"><Input /></Form.Item>}
          <Form.Item name="price" label="Giá" rules={[{ required: true }]}><InputNumber style={{ width: '100%' }} formatter={v => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')} min={0} /></Form.Item>
          <Form.Item name="salePrice" label="Giá giảm"><InputNumber style={{ width: '100%' }} min={0} /></Form.Item>
          <Form.Item name="stockQuantity" label="Tồn kho" rules={[{ required: true }]}><InputNumber style={{ width: '100%' }} min={0} /></Form.Item>
          <Form.Item name="description" label="Mô tả"><Input.TextArea rows={3} /></Form.Item>
          {editingProduct && (
            <Form.Item name="status" label="Trạng thái">
              <Select options={[{ value: 1, label: 'Active' }, { value: 2, label: 'Inactive' }]} />
            </Form.Item>
          )}
        </Form>
      </Modal>
    </div>
  )
}
