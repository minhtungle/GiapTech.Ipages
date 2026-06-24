import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Card, DatePicker, Divider, Form, Input, message, Select, Typography } from 'antd'
import dayjs from 'dayjs'
import { useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../lib/api'

const { Title, Text } = Typography

export default function TenantFormPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const qc = useQueryClient()
  const [form] = Form.useForm()
  const isEdit = Boolean(id)

  const { data: tenant } = useQuery({
    queryKey: ['tenant', id],
    queryFn: () => api.get(`/tenants/${id}`).then((r) => r.data),
    enabled: isEdit,
  })

  useEffect(() => {
    if (tenant) {
      form.setFieldsValue({
        ...tenant,
        expiresAt: tenant.expiresAt ? dayjs(tenant.expiresAt) : null,
      })
    }
  }, [tenant, form])

  const mutation = useMutation({
    mutationFn: (values: Record<string, unknown>) =>
      isEdit
        ? api.put(`/tenants/${id}`, values)
        : api.post('/tenants', values),
    onSuccess: () => {
      message.success(isEdit ? 'Cập nhật thành công' : 'Tạo tenant thành công')
      qc.invalidateQueries({ queryKey: ['tenants'] })
      navigate('/admin/tenants')
    },
    onError: () => message.error('Có lỗi xảy ra'),
  })

  const handleSubmit = (values: Record<string, unknown>) => {
    const payload = {
      ...values,
      expiresAt: values.expiresAt ? dayjs(values.expiresAt as string).toISOString() : null,
    }
    mutation.mutate(payload)
  }

  return (
    <div style={{ padding: 24 }}>
      <Card>
        <Title level={4}>{isEdit ? 'Chỉnh sửa Tenant' : 'Tạo Tenant mới'}</Title>
        <Form form={form} layout="vertical" onFinish={handleSubmit} style={{ maxWidth: 600 }}>

          <Divider orientation="left" orientationMargin={0}>Thông tin tenant</Divider>

          <Form.Item name="name" label="Tên tenant" rules={[{ required: true }]}>
            <Input placeholder="VD: Cửa hàng ABC" />
          </Form.Item>
          {!isEdit && (
            <Form.Item
              name="slug"
              label="Slug (subdomain)"
              rules={[{ required: true, pattern: /^[a-z0-9-]+$/, message: 'Chỉ dùng chữ thường, số, dấu gạch ngang' }]}
            >
              <Input placeholder="VD: abc" addonAfter=".localhost" />
            </Form.Item>
          )}
          <Form.Item name="email" label="Email">
            <Input placeholder="contact@example.com" />
          </Form.Item>
          <Form.Item name="phone" label="Điện thoại">
            <Input placeholder="0901234567" />
          </Form.Item>
          {isEdit && (
            <Form.Item name="status" label="Trạng thái" rules={[{ required: true }]}>
              <Select options={[
                { value: 1, label: 'Active' },
                { value: 2, label: 'Inactive' },
                { value: 3, label: 'Suspended' },
                { value: 4, label: 'Expired' },
              ]} />
            </Form.Item>
          )}
          <Form.Item name="expiresAt" label="Ngày hết hạn">
            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
          </Form.Item>

          <Divider orientation="left" orientationMargin={0}>
            Tài khoản admin tenant
            {isEdit && <Text type="secondary" style={{ fontWeight: 400, marginLeft: 8, fontSize: 13 }}>— để trống nếu không đổi mật khẩu</Text>}
          </Divider>

          <Form.Item
            name="adminUsername"
            label="Tên đăng nhập admin"
            rules={isEdit ? [] : [{ required: false }]}
          >
            <Input placeholder={isEdit ? 'Nhập username cần đổi mật khẩu' : 'VD: admin'} />
          </Form.Item>
          <Form.Item
            name="adminPassword"
            label={isEdit ? 'Mật khẩu mới' : 'Mật khẩu admin'}
            rules={[
              { min: 6, message: 'Mật khẩu phải có ít nhất 6 ký tự' },
            ]}
          >
            <Input.Password placeholder={isEdit ? 'Để trống nếu không đổi' : 'Ít nhất 6 ký tự'} />
          </Form.Item>

          <Form.Item style={{ marginTop: 24 }}>
            <Button type="primary" htmlType="submit" loading={mutation.isPending}>
              {isEdit ? 'Cập nhật' : 'Tạo mới'}
            </Button>
            <Button style={{ marginLeft: 8 }} onClick={() => navigate('/admin/tenants')}>
              Huỷ
            </Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  )
}
