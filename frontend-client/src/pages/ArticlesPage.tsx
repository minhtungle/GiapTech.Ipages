import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../lib/api'
import dayjs from 'dayjs'

interface Article { id: string; title: string; slug: string; excerpt?: string; thumbnailUrl?: string; publishedAt?: string }

export default function ArticlesPage() {
  const [page, setPage] = useState(1)

  const { data, isLoading } = useQuery({
    queryKey: ['articles-public', page],
    queryFn: () => api.get('/articles', { params: { page, pageSize: 10 } }).then(r => r.data),
  })

  return (
    <div className="container" style={{ padding: '40px 0' }}>
      <h1>Tin tức</h1>
      {isLoading ? <p>Đang tải...</p> : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
          {(data?.items ?? []).map((a: Article) => (
            <article key={a.id} style={{ display: 'flex', gap: 24, borderBottom: '1px solid #eee', paddingBottom: 24 }}>
              {a.thumbnailUrl && (
                <img src={a.thumbnailUrl} alt={a.title} style={{ width: 200, height: 130, objectFit: 'cover', borderRadius: 8, flexShrink: 0 }} />
              )}
              <div>
                <Link to={`/tin-tuc/${a.slug}`}><h2 style={{ margin: '0 0 8px', fontSize: 20 }}>{a.title}</h2></Link>
                {a.excerpt && <p style={{ color: '#666', margin: '0 0 8px' }}>{a.excerpt}</p>}
                {a.publishedAt && <p style={{ color: '#999', fontSize: 13, margin: 0 }}>{dayjs(a.publishedAt).format('DD/MM/YYYY')}</p>}
              </div>
            </article>
          ))}
        </div>
      )}
    </div>
  )
}
