import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../lib/api'

interface Product { id: string; name: string; slug: string; price: number; salePrice?: number; thumbnailUrl?: string }

export default function ProductsPage() {
  const [page, setPage] = useState(1)

  const { data, isLoading } = useQuery({
    queryKey: ['products-public', page],
    queryFn: () => api.get('/products', { params: { page, pageSize: 20 } }).then(r => r.data),
  })

  return (
    <div className="container">
      <h1>Sản phẩm</h1>
      {isLoading ? (
        <p>Đang tải...</p>
      ) : (
        <>
          <div className="product-grid">
            {(data?.items ?? []).map((p: Product) => (
              <Link key={p.id} to={`/san-pham/${p.slug}`} className="product-card">
                <img src={p.thumbnailUrl ?? 'https://placehold.co/400x300?text=No+Image'} alt={p.name} />
                <div className="product-card-body">
                  <div className="product-card-name">{p.name}</div>
                  <div>
                    <span className="product-card-price">{(p.salePrice ?? p.price).toLocaleString('vi-VN')} ₫</span>
                    {p.salePrice && <span className="product-card-original">{p.price.toLocaleString('vi-VN')} ₫</span>}
                  </div>
                </div>
              </Link>
            ))}
          </div>
          <div style={{ display: 'flex', justifyContent: 'center', gap: 8, padding: '24px 0' }}>
            {page > 1 && <button onClick={() => setPage(p => p - 1)} style={{ padding: '8px 16px', cursor: 'pointer' }}>← Trước</button>}
            <span style={{ padding: '8px 16px', background: '#1677ff', color: '#fff', borderRadius: 4 }}>Trang {page}</span>
            {data?.items?.length === 20 && <button onClick={() => setPage(p => p + 1)} style={{ padding: '8px 16px', cursor: 'pointer' }}>Tiếp →</button>}
          </div>
        </>
      )}
    </div>
  )
}
