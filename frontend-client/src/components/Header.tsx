import { Link } from 'react-router-dom'

export default function Header() {
  const siteName = window.location.hostname.split('.')[0]

  return (
    <header>
      <div className="container">
        <Link to="/" style={{ fontWeight: 700, fontSize: 20, color: '#1677ff' }}>
          {siteName}
        </Link>
        <nav>
          <Link to="/">Trang chủ</Link>
          <Link to="/san-pham">Sản phẩm</Link>
          <Link to="/tin-tuc">Tin tức</Link>
        </nav>
      </div>
    </header>
  )
}
