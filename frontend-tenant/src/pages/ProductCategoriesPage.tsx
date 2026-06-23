import { DeleteOutlined, EditOutlined, PlusOutlined } from '@ant-design/icons'
import { Button, Form, Input, InputNumber, Modal, Popconfirm, Select, Space, Table, Typography, message } from 'antd'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { api } from '../lib/api'

const { Title } = Typography

interface ProductCategory {
  id: string
  name: string
  slug: string
  description: string | null
  parentId: string | null
  parentName: string | null
  sortOrder: number
  isActive: boolean
}

interface FormValues {
  name: string
  slug: string
  description?: string
  parentId?: string
  sortOrder: number
  isActive: boolean
}

export default function ProductCategoriesPage() {
  const qc = useQueryClient()
  const [modalOpen, setModalOpen] = useState(false)
  const [editing, setEditing] = useState<ProductCategory | null>(null)
  const [form] = Form.useForm<FormValues>()

  const { data, isLoading } = useQuery<ProductCategory[]>({
    queryKey: ['product-categories'],
    queryFn: () => api.get('/product-categories').then(r => r.data?.items ?? r.data),
  })

  const createMutation = useMutation({
    mutationFn: (values: FormValues) => api.post('/product-categories', values),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['product-categories'] }); closeModal() },
    onError: () => message.error('Tạo danh mục thất bại'),
  })

  const updateMutation = useMutation({
    mutationFn: (values: FormValues) => api.put(`/product-categories/${editing!.id}`, values),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['product-categories'] }); closeModal() },
    onError: () => message.error('Cập nhật danh mục thất bại'),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/product-categories/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['product-categories'] }),
    onError: () => message.error('Xóa danh mục thất bại'),
  })

  function openCreate() {
    setEditing(null)
    form.resetFields()
    form.setFieldsValue({ sortOrder: 0, isActive: true })
    setModalOpen(true)
  }

  function openEdit(cat: ProductCategory) {
    setEditing(cat)
    form.setFieldsValue({
      name: cat.name,
      slug: cat.slug,
      description: cat.description ?? undefined,
      parentId: cat.parentId ?? undefined,
      sortOrder: cat.sortOrder,
      isActive: cat.isActive,
    })
    setModalOpen(true)
  }

  function closeModal() {
    setModalOpen(false)
    setEditing(null)
    form.resetFields()
  }

  function handleSubmit(values: FormValues) {
    if (editing) updateMutation.mutate(values)
    else createMutation.mutate(values)
  }

  const parentOptions = (data ?? [])
    .filter(c => !editing || c.id !== editing.id)
    .map(c => ({ value: c.id, label: c.name }))

  const columns = [
    { title: 'Tên', dataIndex: 'name', key: 'name' },
    { title: 'Slug', dataIndex: 'slug', key: 'slug' },
    { title: 'Danh mục cha', dataIndex: 'parentName', key: 'parentName', render: (v: string | null) => v ?? '—' },
    { title: 'Thứ tự', dataIndex: 'sortOrder', key: 'sortOrder', width: 80 },
    {
      title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive', width: 100,
      render: (v: boolean) => <span style={{ color: v ? '#52c41a' : '#ff4d4f' }}>{v ? 'Hoạt động' : 'Ẩn'}</span>,
    },
    {
      title: '', key: 'actions', width: 100,
      render: (_: unknown, record: ProductCategory) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={() => openEdit(record)} />
          <Popconfirm title="Xóa danh mục này?" onConfirm={() => deleteMutation.mutate(record.id)} okText="Xóa" cancelText="Hủy">
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ]

  return (
    <div style={{ padding: 24, background: '#fff', borderRadius: 8 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>Danh mục sản phẩm</Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>Thêm danh mục</Button>
      </div>
      <Table rowKey="id" columns={columns} dataSource={data ?? []} loading={isLoading} pagination={{ pageSize: 20 }} />

      <Modal
        title={editing ? 'Chỉnh sửa danh mục' : 'Thêm danh mục'}
        open={modalOpen}
        onCancel={closeModal}
        onOk={() => form.submit()}
        confirmLoading={createMutation.isPending || updateMutation.isPending}
        okText={editing ? 'Lưu' : 'Tạo'}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit} style={{ marginTop: 16 }}>
          <Form.Item name="name" label="Tên danh mục" rules={[{ required: true, message: 'Nhập tên danh mục' }]}>
            <Input onChange={e => {
              if (!editing) form.setFieldValue('slug', e.target.value.toLowerCase().replace(/\s+/g, '-').replace(/[^\w-]/g, ''))
            }} />
          </Form.Item>
          <Form.Item name="slug" label="Slug" rules={[{ required: true, message: 'Nhập slug' }]}>
            <Input />
          </Form.Item>
          <Form.Item name="description" label="Mô tả">
            <Input.TextArea rows={2} />
          </Form.Item>
          <Form.Item name="parentId" label="Danh mục cha">
            <Select allowClear placeholder="Không có" options={parentOptions} />
          </Form.Item>
          <Form.Item name="sortOrder" label="Thứ tự sắp xếp" rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="isActive" label="Trạng thái">
            <Select options={[{ value: true, label: 'Hoạt động' }, { value: false, label: 'Ẩn' }]} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}
