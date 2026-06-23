import { DeleteOutlined, UploadOutlined } from '@ant-design/icons'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Card, Col, Image, message, Modal, Row, Typography, Upload } from 'antd'
import type { UploadProps } from 'antd'
import { api } from '../lib/api'
import { useAuthStore } from '../store/auth'

const { Title, Text } = Typography

interface MediaFile { id: string; fileName: string; url: string; contentType: string; fileSize: number; originalName: string }

export default function MediaPage() {
  const qc = useQueryClient()
  const { token } = useAuthStore()

  const { data, isLoading } = useQuery({
    queryKey: ['media'],
    queryFn: () => api.get('/media').then(r => r.data),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/media/${id}`),
    onSuccess: () => { message.success('Đã xoá'); qc.invalidateQueries({ queryKey: ['media'] }) },
  })

  const uploadProps: UploadProps = {
    name: 'file',
    action: '/api/v1/media/upload',
    headers: { Authorization: `Bearer ${token}` },
    showUploadList: false,
    onChange(info) {
      if (info.file.status === 'done') {
        message.success(`${info.file.name} tải lên thành công`)
        qc.invalidateQueries({ queryKey: ['media'] })
      } else if (info.file.status === 'error') {
        message.error(`${info.file.name} tải lên thất bại`)
      }
    },
  }

  const formatSize = (bytes: number) => {
    if (bytes < 1024) return bytes + ' B'
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB'
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB'
  }

  return (
    <div style={{ padding: 24, background: '#fff', borderRadius: 8 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>Thư viện Media</Title>
        <Upload {...uploadProps}>
          <Button type="primary" icon={<UploadOutlined />}>Tải lên</Button>
        </Upload>
      </div>
      <Row gutter={[16, 16]}>
        {(data?.items ?? []).map((file: MediaFile) => (
          <Col key={file.id} xs={12} sm={8} md={6} lg={4}>
            <Card
              size="small"
              cover={
                file.contentType.startsWith('image/') ? (
                  <Image src={file.url} alt={file.originalName} style={{ height: 120, objectFit: 'cover' }} />
                ) : (
                  <div style={{ height: 120, display: 'flex', alignItems: 'center', justifyContent: 'center', background: '#f5f5f5' }}>
                    <Text type="secondary">{file.contentType}</Text>
                  </div>
                )
              }
              actions={[
                <Button type="text" danger size="small" icon={<DeleteOutlined />}
                  onClick={() => Modal.confirm({ title: 'Xoá file?', onOk: () => deleteMutation.mutate(file.id) })} />,
              ]}
            >
              <Card.Meta
                title={<Text ellipsis title={file.originalName} style={{ fontSize: 12 }}>{file.originalName}</Text>}
                description={<Text type="secondary" style={{ fontSize: 11 }}>{formatSize(file.fileSize)}</Text>}
              />
            </Card>
          </Col>
        ))}
      </Row>
    </div>
  )
}
