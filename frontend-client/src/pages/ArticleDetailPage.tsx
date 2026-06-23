import { useQuery } from '@tanstack/react-query'
import { useParams } from 'react-router-dom'
import { api } from '../lib/api'
import dayjs from 'dayjs'

export default function ArticleDetailPage() {
  const { slug } = useParams<{ slug: string }>()

  const { data: article, isLoading } = useQuery({
    queryKey: ['article', slug],
    queryFn: () => api.get(`/articles/slug/${slug}`).then(r => r.data).catch(() => null),
    enabled: !!slug,
  })

  if (isLoading) return <div className="container" style={{ padding: '40px 0' }}>Đang tải...</div>
  if (!article) return <div className="container" style={{ padding: '40px 0' }}>Không tìm thấy bài viết.</div>

  return (
    <div className="container" style={{ padding: '40px 0', maxWidth: 800 }}>
      <h1>{article.title}</h1>
      {article.publishedAt && (
        <p style={{ color: '#999', fontSize: 13 }}>Đăng ngày {dayjs(article.publishedAt).format('DD/MM/YYYY')}</p>
      )}
      {article.thumbnailUrl && (
        <img src={article.thumbnailUrl} alt={article.title} style={{ width: '100%', borderRadius: 8, marginBottom: 24 }} />
      )}
      <div
        style={{ lineHeight: 1.8, fontSize: 16 }}
        dangerouslySetInnerHTML={{ __html: article.content }}
      />
    </div>
  )
}
