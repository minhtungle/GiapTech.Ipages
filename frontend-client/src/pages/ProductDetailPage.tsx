import { useQuery } from '@tanstack/react-query'
import { useParams } from 'react-router-dom'
import { api } from '../lib/api'

export default function ProductDetailPage() {
  const { slug } = useParams<{ slug: string }>()

  const { data: product, isLoading } = useQuery({
    queryKey: ['product', slug],
    queryFn: () => api.get(`/products/slug/${slug}`).then(r => r.data).catch(() => null),
    enabled: !!slug,
  })

  if (isLoading) return <div className="container" style={{ padding: '40px 0' }}>Đang tải...</div>
  if (!product) return <div className="container" style={{ padding: '40px 0' }}>Không tìm thấy sản phẩm.</div>

  return (
    <div className="container" style={{ padding: '40px 0' }}>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 40 }}>
        <div>
          <img
            src={product.thumbnailUrl ?? 'https://placehold.co/600x400?text=No+Image'}
            alt={product.name}
            style={{ width: '100%', borderRadius: 8 }}
          />
        </div>
        <div>
          <h1 style={{ marginTop: 0 }}>{product.name}</h1>
          {product.sku && <p style={{ color: '#666' }}>SKU: {product.sku}</p>}
          <div style={{ marginBottom: 16 }}>
            <span style={{ fontSize: 28, fontWeight: 700, color: '#f5222d' }}>
              {(product.salePrice ?? product.price).toLocaleString('vi-VN')} ₫
            </span>
            {product.salePrice && (
              <span style={{ marginLeft: 12, textDecoration: 'line-through', color: '#999', fontSize: 18 }}>
                {product.price.toLocaleString('vi-VN')} ₫
              </span>
            )}
          </div>
          <p style={{ color: '#666' }}>Còn {product.stockQuantity} sản phẩm</p>
          {product.description && <div dangerouslySetInnerHTML={{ __html: product.description }} />}
          <button style={{ marginTop: 24, padding: '12px 32px', background: '#1677ff', color: '#fff', border: 'none', borderRadius: 8, fontSize: 16, cursor: 'pointer' }}>
            Thêm vào giỏ hàng
          </button>
        </div>
      </div>
    </div>
  )
}
