import { DeleteOutlined, EditOutlined, PlusOutlined } from '@ant-design/icons'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Form, Input, message, Modal, Select, Space, Table, Tag, Typography } from 'antd'
import { useState } from 'react'
import { api } from '../lib/api'
import dayjs from 'dayjs'

const { Title } = Typography

interface Article { id: string; title: string; slug: string; status: number; publishedAt?: string; viewCount: number }

export default function ArticlesPage() {
  const qc = useQueryClient()
  const [page, setPage] = useState(1)
  const [modalOpen, setModalOpen] = useState(false)
  const [editingArticle, setEditingArticle] = useState<Article | null>(null)
  const [form] = Form.useForm()

  const { data, isLoading } = useQuery({
    queryKey: ['articles', page],
    queryFn: () => api.get('/articles', { params: { page, pageSize: 20 } }).then(r => r.data),
  })

  const saveMutation = useMutation({
    mutationFn: (values: Record<string, unknown>) =>
      editingArticle ? api.put(`/articles/${editingArticle.id}`, values) : api.post('/articles', values),
    onSuccess: () => { message.success('Lưu thành công'); qc.invalidateQueries({ queryKey: ['articles'] }); setModalOpen(false) },
    onError: () => message.error('Có lỗi xảy ra'),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/articles/${id}`),
    onSuccess: () => { message.success('Đã xoá'); qc.invalidateQueries({ queryKey: ['articles'] }) },
  })

  const columns = [
    { title: 'Tiêu đề', dataIndex: 'title', key: 'title' },
    { title: 'Slug', dataIndex: 'slug', key: 'slug', render: (v: string) => <code>{v}</code> },
    {
      title: 'Trạng thái', dataIndex: 'status', key: 'status',
      render: (v: number) => <Tag color={v === 2 ? 'green' : v === 1 ? 'gold' : 'default'}>{v === 1 ? 'Draft' : v === 2 ? 'Published' : 'Archived'}</Tag>,
    },
    { title: 'Lượt xem', dataIndex: 'viewCount', key: 'viewCount' },
    { title: 'Ngày đăng', dataIndex: 'publishedAt', key: 'publishedAt', render: (v: string) => v ? dayjs(v).format('DD/MM/YYYY') : '—' },
    {
      title: '', key: 'actions',
      render: (_: unknown, r: Article) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={() => { setEditingArticle(r); form.setFieldsValue(r); setModalOpen(true) }} />
          <Button size="small" danger icon={<DeleteOutlined />} onClick={() => Modal.confirm({ title: 'Xoá?', onOk: () => deleteMutation.mutate(r.id) })} />
        </Space>
      ),
    },
  ]

  return (
    <div style={{ padding: 24, background: '#fff', borderRadius: 8 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>Bài viết</Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditingArticle(null); form.resetFields(); setModalOpen(true) }}>Thêm bài viết</Button>
      </div>
      <Table rowKey="id" loading={isLoading} dataSource={data?.items ?? []} columns={columns}
        pagination={{ total: data?.total ?? 0, current: page, pageSize: 20, onChange: setPage }} />

      <Modal open={modalOpen} title={editingArticle ? 'Sửa bài viết' : 'Thêm bài viết'}
        onCancel={() => { setModalOpen(false); form.resetFields() }} onOk={() => form.submit()}
        confirmLoading={saveMutation.isPending} width={700}>
        <Form form={form} layout="vertical" onFinish={vals => saveMutation.mutate(vals)}>
          <Form.Item name="title" label="Tiêu đề" rules={[{ required: true }]}><Input /></Form.Item>
          {!editingArticle && <Form.Item name="slug" label="Slug" rules={[{ required: true }]}><Input /></Form.Item>}
          <Form.Item name="excerpt" label="Tóm tắt"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item name="content" label="Nội dung" rules={[{ required: true }]}><Input.TextArea rows={8} /></Form.Item>
          <Form.Item name="status" label="Trạng thái" initialValue={1}>
            <Select options={[{ value: 1, label: 'Draft' }, { value: 2, label: 'Published' }, { value: 4, label: 'Archived' }]} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}
