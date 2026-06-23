export default function Footer() {
  return (
    <footer>
      <div className="container">
        <div>
          <h4 style={{ color: '#fff', marginBottom: 12 }}>Về chúng tôi</h4>
          <p style={{ fontSize: 14, lineHeight: 1.6 }}>Powered by GiapTech.Ipages</p>
        </div>
        <div>
          <h4 style={{ color: '#fff', marginBottom: 12 }}>Liên kết</h4>
          <p style={{ fontSize: 14 }}><a href="/san-pham" style={{ color: '#ccc' }}>Sản phẩm</a></p>
          <p style={{ fontSize: 14 }}><a href="/tin-tuc" style={{ color: '#ccc' }}>Tin tức</a></p>
        </div>
        <div>
          <h4 style={{ color: '#fff', marginBottom: 12 }}>Liên hệ</h4>
          <p style={{ fontSize: 14 }}>Email: contact@example.com</p>
        </div>
      </div>
    </footer>
  )
}
