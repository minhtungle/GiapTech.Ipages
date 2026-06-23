using GiapTech.Ipages.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GiapTech.Ipages.Infrastructure.BackgroundJobs;

public class EmailNotificationJob
{
    private readonly IEmailService _email;
    private readonly ILogger<EmailNotificationJob> _logger;

    public EmailNotificationJob(IEmailService email, ILogger<EmailNotificationJob> logger)
    {
        _email = email;
        _logger = logger;
    }

    public async Task SendOrderConfirmationAsync(string customerEmail, string orderCode, decimal total)
    {
        var html = $"""
            <h2>Xác nhận đơn hàng #{orderCode}</h2>
            <p>Cảm ơn bạn đã đặt hàng!</p>
            <p>Tổng tiền: <strong>{total:N0} ₫</strong></p>
            <p>Chúng tôi sẽ liên hệ với bạn trong thời gian sớm nhất.</p>
            <hr/>
            <small>GiapTech.Ipages</small>
            """;
        await _email.SendAsync(customerEmail, $"Xác nhận đơn hàng #{orderCode}", html);
        _logger.LogInformation("Order confirmation email sent to {Email} for order {Code}", customerEmail, orderCode);
    }

    public async Task SendWelcomeEmailAsync(string userEmail, string fullName, string tenantName)
    {
        var html = $"""
            <h2>Chào mừng {fullName}!</h2>
            <p>Tài khoản của bạn tại <strong>{tenantName}</strong> đã được tạo thành công.</p>
            <p>Bạn có thể đăng nhập và bắt đầu mua sắm ngay bây giờ.</p>
            <hr/>
            <small>GiapTech.Ipages</small>
            """;
        await _email.SendAsync(userEmail, $"Chào mừng đến với {tenantName}", html);
    }
}
