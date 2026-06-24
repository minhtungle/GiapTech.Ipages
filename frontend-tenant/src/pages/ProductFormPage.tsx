import {
  DeleteOutlined, MinusCircleOutlined, PlusOutlined,
} from '@ant-design/icons'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  Button, Card, Col, DatePicker, Divider, Form, Input, InputNumber,
  message, Row, Select, Space, Switch, Typography,
} from 'antd'
import type { InputNumberProps } from 'antd'
import dayjs from 'dayjs'
import { useEffect } from 'react'

const vndFormatter: InputNumberProps['formatter'] = v => `${v ?? ''}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')
const vndParser: InputNumberProps['parser'] = v => Number(v?.replace(/,/g, '') ?? 0) as 0
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../lib/api'

const { Title, Text } = Typography
const { TextArea } = Input

const STATUS_OPTIONS = [
  { value: 1, label: 'Bản nháp (Draft)' },
  { value: 2, label: 'Đang ẩn (Hidden)' },
  { value: 3, label: 'Đang hoạt động (Active)' },
]

function slugify(text: string) {
  return text.toLowerCase()
    .normalize('NFD').replace(/[̀-ͯ]/g, '')
    .replace(/đ/g, 'd').replace(/[^a-z0-9\s-]/g, '')
    .trim().replace(/\s+/g, '-')
}

export default function ProductFormPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const qc = useQueryClient()
  const [form] = Form.useForm()
  const isEdit = Boolean(id)

  const { data: categories } = useQuery({
    queryKey: ['product-categories'],
    queryFn: () => api.get('/product-categories').then(r => r.data?.items ?? r.data ?? []),
  })

  const { data: product, isLoading } = useQuery({
    queryKey: ['product', id],
    queryFn: () => api.get(`/products/${id}`).then(r => r.data),
    enabled: isEdit,
  })

  useEffect(() => {
    if (!product) return
    const tags = product.tagsJson ? JSON.parse(product.tagsJson) : []
    const images = product.images ? JSON.parse(product.images) : []
    form.setFieldsValue({
      ...product,
      tags,
      imageList: images,
      salePriceRange: product.salePriceFrom
        ? [dayjs(product.salePriceFrom), product.salePriceTo ? dayjs(product.salePriceTo) : null]
        : null,
    })
  }, [product, form])

  const mutation = useMutation({
    mutationFn: (values: Record<string, unknown>) => {
      const { tags, imageList, salePriceRange, ...rest } = values as {
        tags?: string[]; imageList?: { url: string; alt: string; order: number }[];
        salePriceRange?: [dayjs.Dayjs, dayjs.Dayjs] | null;
        [k: string]: unknown
      }
      const payload = {
        ...rest,
        tagsJson: tags?.length ? JSON.stringify(tags) : null,
        images: imageList?.length ? JSON.stringify(imageList) : null,
        salePriceFrom: salePriceRange?.[0]?.toISOString() ?? null,
        salePriceTo: salePriceRange?.[1]?.toISOString() ?? null,
      }
      return isEdit ? api.put(`/products/${id}`, payload) : api.post('/products', payload)
    },
    onSuccess: () => {
      message.success(isEdit ? 'Cập nhật thành công' : 'Tạo sản phẩm thành công')
      qc.invalidateQueries({ queryKey: ['products'] })
      navigate('/admin/products')
    },
    onError: () => message.error('Có lỗi xảy ra, vui lòng kiểm tra lại'),
  })

  if (isEdit && isLoading) return <div style={{ padding: 24 }}>Đang tải...</div>

  return (
    <div style={{ padding: 24 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
        <Title level={4} style={{ margin: 0 }}>{isEdit ? 'Chỉnh sửa sản phẩm' : 'Thêm sản phẩm mới'}</Title>
        <Space>
          <Button onClick={() => navigate('/admin/products')}>Huỷ</Button>
          <Button type="primary" loading={mutation.isPending} onClick={() => form.submit()}>
            {isEdit ? 'Lưu thay đổi' : 'Tạo sản phẩm'}
          </Button>
        </Space>
      </div>

      <Form
        form={form}
        layout="vertical"
        onFinish={vals => mutation.mutate(vals as Record<string, unknown>)}
        initialValues={{ status: 1, trackInventory: true, stockQuantity: 0, sortOrder: 0 }}
      >
        <Row gutter={24}>
          {/* Cột trái — nội dung chính */}
          <Col xs={24} lg={16}>

            {/* 1. Thông tin cơ bản */}
            <Card style={{ marginBottom: 16 }}>
              <Divider orientation="left" orientationMargin={0}>Thông tin cơ bản</Divider>
              <Form.Item name="name" label="Tên sản phẩm" rules={[{ required: true }]}>
                <Input
                  placeholder="Nhập tên sản phẩm"
                  onChange={e => {
                    if (!isEdit) form.setFieldValue('slug', slugify(e.target.value))
                  }}
                />
              </Form.Item>
              <Row gutter={16}>
                <Col span={12}>
                  {!isEdit ? (
                    <Form.Item name="slug" label="Slug URL" rules={[{ required: true, pattern: /^[a-z0-9-]+$/, message: 'Chỉ dùng a-z, 0-9, dấu gạch ngang' }]}>
                      <Input placeholder="ten-san-pham" addonBefore="/" />
                    </Form.Item>
                  ) : null}
                </Col>
                <Col span={12}>
                  <Form.Item name="sku" label="Mã SKU">
                    <Input placeholder="VD: SP-001" />
                  </Form.Item>
                </Col>
              </Row>
              <Form.Item name="shortDescription" label="Mô tả ngắn">
                <TextArea rows={2} placeholder="Tóm tắt ngắn gọn hiển thị dưới giá sản phẩm" maxLength={500} showCount />
              </Form.Item>
              <Form.Item name="description" label="Mô tả chi tiết">
                <TextArea rows={8} placeholder="Mô tả đầy đủ, hỗ trợ HTML" />
              </Form.Item>
            </Card>

            {/* 2. Giá cả */}
            <Card style={{ marginBottom: 16 }}>
              <Divider orientation="left" orientationMargin={0}>Giá cả</Divider>
              <Row gutter={16}>
                <Col span={8}>
                  <Form.Item name="price" label="Giá bán lẻ (₫)" rules={[{ required: true }]}>
                    <InputNumber style={{ width: '100%' }} min={0} formatter={vndFormatter} parser={vndParser} />
                  </Form.Item>
                </Col>
                <Col span={8}>
                  <Form.Item name="salePrice" label="Giá khuyến mãi (₫)">
                    <InputNumber style={{ width: '100%' }} min={0} formatter={vndFormatter} parser={vndParser} />
                  </Form.Item>
                </Col>
                <Col span={8}>
                  <Form.Item name="costPerItem" label="Giá vốn (₫)" tooltip="Dùng tính lợi nhuận, không hiển thị khách hàng">
                    <InputNumber style={{ width: '100%' }} min={0} formatter={vndFormatter} parser={vndParser} />
                  </Form.Item>
                </Col>
              </Row>
              <Form.Item name="salePriceRange" label="Lên lịch khuyến mãi (Bắt đầu — Kết thúc)">
                <DatePicker.RangePicker showTime style={{ width: '100%' }} format="DD/MM/YYYY HH:mm" placeholder={['Ngày bắt đầu', 'Ngày kết thúc']} />
              </Form.Item>
            </Card>

            {/* 3. Media */}
            <Card style={{ marginBottom: 16 }}>
              <Divider orientation="left" orientationMargin={0}>Đa phương tiện</Divider>
              <Form.Item name="thumbnailUrl" label="Ảnh đại diện (URL)">
                <Input placeholder="https://..." />
              </Form.Item>

              <Form.Item label="Thư viện ảnh (URL + Alt text)">
                <Form.List name="imageList">
                  {(fields, { add, remove }) => (
                    <>
                      {fields.map(({ key, name }) => (
                        <Row key={key} gutter={8} style={{ marginBottom: 8 }}>
                          <Col span={14}>
                            <Form.Item name={[name, 'url']} noStyle rules={[{ required: true, message: 'Nhập URL' }]}>
                              <Input placeholder="URL ảnh" />
                            </Form.Item>
                          </Col>
                          <Col span={8}>
                            <Form.Item name={[name, 'alt']} noStyle>
                              <Input placeholder="Alt text (SEO)" />
                            </Form.Item>
                          </Col>
                          <Col span={2}>
                            <Button danger icon={<DeleteOutlined />} onClick={() => remove(name)} />
                          </Col>
                        </Row>
                      ))}
                      <Button type="dashed" onClick={() => add({ url: '', alt: '', order: fields.length })} icon={<PlusOutlined />}>
                        Thêm ảnh
                      </Button>
                    </>
                  )}
                </Form.List>
              </Form.Item>

              <Form.Item name="videoUrl" label="Video (YouTube URL hoặc link MP4)">
                <Input placeholder="https://youtube.com/watch?v=... hoặc https://.../video.mp4" />
              </Form.Item>
            </Card>

            {/* 4. Biến thể */}
            <Card style={{ marginBottom: 16 }}>
              <Divider orientation="left" orientationMargin={0}>Biến thể sản phẩm</Divider>
              <Text type="secondary" style={{ display: 'block', marginBottom: 16 }}>
                Thiết lập các phiên bản khác nhau (Màu sắc, Kích cỡ...). Mỗi biến thể có giá, SKU, khối lượng riêng.
              </Text>
              <Form.List name="variants">
                {(fields, { add, remove }) => (
                  <>
                    {fields.map(({ key, name }) => (
                      <Card
                        key={key}
                        size="small"
                        style={{ marginBottom: 12, background: '#fafafa' }}
                        extra={<Button type="text" danger icon={<MinusCircleOutlined />} onClick={() => remove(name)}>Xoá</Button>}
                        title={`Biến thể ${name + 1}`}
                      >
                        <Row gutter={12}>
                          <Col span={8}>
                            <Form.Item name={[name, 'name']} label="Tên biến thể" rules={[{ required: true }]}>
                              <Input placeholder="VD: Đỏ - XL" />
                            </Form.Item>
                          </Col>
                          <Col span={8}>
                            <Form.Item name={[name, 'sku']} label="SKU biến thể">
                              <Input placeholder="SP-001-DO-XL" />
                            </Form.Item>
                          </Col>
                          <Col span={8}>
                            <Form.Item name={[name, 'attributeValues']} label="Thuộc tính (JSON)" tooltip='VD: {"Màu":"Đỏ","Size":"XL"}'>
                              <Input placeholder='{"Màu":"Đỏ"}' />
                            </Form.Item>
                          </Col>
                        </Row>
                        <Row gutter={12}>
                          <Col span={6}>
                            <Form.Item name={[name, 'price']} label="Giá (₫)" rules={[{ required: true }]}>
                              <InputNumber style={{ width: '100%' }} min={0} />
                            </Form.Item>
                          </Col>
                          <Col span={6}>
                            <Form.Item name={[name, 'salePrice']} label="Giá KM (₫)">
                              <InputNumber style={{ width: '100%' }} min={0} />
                            </Form.Item>
                          </Col>
                          <Col span={6}>
                            <Form.Item name={[name, 'weight']} label="Khối lượng (kg)">
                              <InputNumber style={{ width: '100%' }} min={0} step={0.1} />
                            </Form.Item>
                          </Col>
                          <Col span={6}>
                            <Form.Item name={[name, 'stockQuantity']} label="Tồn kho">
                              <InputNumber style={{ width: '100%' }} min={0} />
                            </Form.Item>
                          </Col>
                        </Row>
                        <Form.Item name={[name, 'imageUrl']} label="URL ảnh biến thể">
                          <Input placeholder="URL ảnh khi chọn biến thể này" />
                        </Form.Item>
                        <Form.Item name={[name, 'isActive']} label="Kích hoạt" valuePropName="checked" initialValue={true}>
                          <Switch />
                        </Form.Item>
                      </Card>
                    ))}
                    <Button type="dashed" onClick={() => add({ price: 0, stockQuantity: 0, isActive: true })} icon={<PlusOutlined />} block>
                      Thêm biến thể
                    </Button>
                  </>
                )}
              </Form.List>
            </Card>

            {/* 5. SEO */}
            <Card style={{ marginBottom: 16 }}>
              <Divider orientation="left" orientationMargin={0}>SEO</Divider>
              <Form.Item name="metaTitle" label="Meta Title">
                <Input placeholder="Tiêu đề SEO" maxLength={70} showCount />
              </Form.Item>
              <Form.Item name="metaDescription" label="Meta Description">
                <TextArea rows={2} placeholder="Mô tả SEO" maxLength={160} showCount />
              </Form.Item>
              <Form.Item name="canonicalUrl" label="Canonical URL">
                <Input placeholder="https://..." />
              </Form.Item>
            </Card>

          </Col>

          {/* Cột phải — tổ chức & trạng thái */}
          <Col xs={24} lg={8}>

            {/* Trạng thái */}
            <Card style={{ marginBottom: 16 }}>
              <Divider orientation="left" orientationMargin={0}>Trạng thái</Divider>
              <Form.Item name="status" label="Trạng thái hiển thị" rules={[{ required: true }]}>
                <Select options={STATUS_OPTIONS} />
              </Form.Item>
              <Form.Item name="sortOrder" label="Thứ tự hiển thị">
                <InputNumber style={{ width: '100%' }} min={0} />
              </Form.Item>
            </Card>

            {/* Phân loại */}
            <Card style={{ marginBottom: 16 }}>
              <Divider orientation="left" orientationMargin={0}>Phân loại</Divider>
              <Form.Item name="categoryId" label="Danh mục">
                <Select
                  allowClear
                  placeholder="Chọn danh mục"
                  options={(categories ?? []).map((c: { id: string; name: string }) => ({ value: c.id, label: c.name }))}
                />
              </Form.Item>
              <Form.Item name="tags" label="Thẻ (Tags)">
                <Select
                  mode="tags"
                  placeholder="Nhập thẻ rồi Enter (VD: Hàng mùa hè, Sale sốc)"
                  tokenSeparators={[',']}
                />
              </Form.Item>
            </Card>

            {/* Kho hàng */}
            <Card style={{ marginBottom: 16 }}>
              <Divider orientation="left" orientationMargin={0}>Kho hàng</Divider>
              <Form.Item name="stockQuantity" label="Số lượng tồn kho">
                <InputNumber style={{ width: '100%' }} min={0} />
              </Form.Item>
              <Form.Item name="trackInventory" label="Theo dõi tồn kho" valuePropName="checked">
                <Switch />
              </Form.Item>
            </Card>

          </Col>
        </Row>
      </Form>
    </div>
  )
}
