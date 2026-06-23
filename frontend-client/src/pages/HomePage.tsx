import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { api } from '../lib/api'

interface Product { id: string; name: string; slug: string; price: number; salePrice?: number; thumbnailUrl?: string }

export default function HomePage() {
  const { data } = useQuery({
    queryKey: ['featured-products'],
    queryFn: () => api.get('/products?pageSize=8').then(r => r.data),
  })

  return (
    <div>
      {/* Hero */}
      <div style={{ background: 'linear-gradient(135deg, #1677ff, #722ed1)', color: '#fff', padding: '80px 0', textAlign: 'center' }}>
        <div className="container">
          <h1 style={{ fontSize: 40, margin: 0 }}>Chào mừng đến với cửa hàng</h1>
          <p style={{ fontSize: 18, opacity: 0.9, marginTop: 12 }}>Khám phá hàng nghìn sản phẩm chất lượng</p>
          <Link to="/san-pham">
            <button style={{ marginTop: 24, padding: '12px 32px', background: '#fff', color: '#1677ff', border: 'none', borderRadius: 8, fontSize: 16, fontWeight: 600, cursor: 'pointer' }}>
              Xem sản phẩm
            </button>
          </Link>
        </div>
      </div>

      {/* Products */}
      <div className="container">
        <h2 style={{ marginTop: 40 }}>Sản phẩm nổi bật</h2>
        <div className="product-grid">
          {(data?.items ?? []).map((p: Product) => (
            <Link key={p.id} to={`/san-pham/${p.slug}`} className="product-card">
              <img
                src={p.thumbnailUrl ?? 'https://placehold.co/400x300?text=No+Image'}
                alt={p.name}
              />
              <div className="product-card-body">
                <div className="product-card-name">{p.name}</div>
                <div>
                  <span className="product-card-price">
                    {(p.salePrice ?? p.price).toLocaleString('vi-VN')} ₫
                  </span>
                  {p.salePrice && (
                    <span className="product-card-original">
                      {p.price.toLocaleString('vi-VN')} ₫
                    </span>
                  )}
                </div>
              </div>
            </Link>
          ))}
        </div>
      </div>
    </div>
  )
}
