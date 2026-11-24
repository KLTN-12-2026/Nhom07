using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EzCondo.BackEndServer.Mobile;

[ApiController]
[AllowAnonymous]
public sealed class MobileInvitationActivationController : ControllerBase
{
    [HttpGet("api/mobile/invitation-activation")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public ContentResult Activate()
    {
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(18));
        Response.Headers.ContentSecurityPolicy = $"default-src 'none'; script-src 'nonce-{nonce}'; style-src 'nonce-{nonce}'; connect-src 'self'; base-uri 'none'; form-action 'none'; frame-ancestors 'none'";
        Response.Headers["Referrer-Policy"] = "no-referrer";
        Response.Headers.XContentTypeOptions = "nosniff";

        return Content($$"""
            <!doctype html>
            <html lang="vi">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>Kích hoạt tài khoản EzCondo</title>
              <style nonce="{{nonce}}">
                body { font: 1rem system-ui, sans-serif; background: #f5f7fb; color: #182239; margin: 0; }
                main { box-sizing: border-box; margin: 2rem auto; max-width: 34rem; padding: 1.5rem; background: #fff; border-radius: .75rem; }
                label, input { display: block; } label { margin: 1rem 0 .3rem; }
                input, button { box-sizing: border-box; min-height: 2.75rem; padding: .6rem; font: inherit; }
                input { width: 100%; } button { margin-top: 1rem; background: #1746a2; color: #fff; border: 0; border-radius: .4rem; }
                button:disabled { opacity: .65; } :focus-visible { outline: 3px solid #1746a2; outline-offset: 3px; }
              </style>
            </head>
            <body>
              <main>
                <h1>Kích hoạt tài khoản EzCondo</h1>
                <p id="status" role="status" aria-live="polite"></p>
                <form id="activate" hidden>
                  <p>Mật khẩu cần 12–128 ký tự, có chữ hoa, chữ thường, số và ký tự đặc biệt.</p>
                  <label for="password">Mật khẩu mới</label>
                  <input id="password" type="password" autocomplete="new-password" minlength="12" maxlength="128" required>
                  <label for="confirmPassword">Nhập lại mật khẩu</label>
                  <input id="confirmPassword" type="password" autocomplete="new-password" minlength="12" maxlength="128" required>
                  <button>Đặt mật khẩu</button>
                </form>
                <noscript>Bật JavaScript hoặc dán liên kết vào mục Kích hoạt tài khoản trong ứng dụng EzCondo.</noscript>
              </main>
              <script nonce="{{nonce}}">
                const token = new URLSearchParams(location.hash.slice(1)).get('token');
                history.replaceState(null, '', location.pathname);
                const form = document.getElementById('activate');
                const status = document.getElementById('status');
                const password = document.getElementById('password');
                const confirmPassword = document.getElementById('confirmPassword');
                const button = form.querySelector('button');
                if (!token || !/^[A-Za-z0-9_-]{43}$/.test(token)) status.textContent = 'Hãy mở liên kết trong email lời mời.';
                else form.hidden = false;
                form.addEventListener('submit', async event => {
                  event.preventDefault();
                  if (button.disabled) return;
                  if (password.value !== confirmPassword.value) {
                    status.textContent = 'Hai mật khẩu chưa khớp.';
                    return;
                  }
                  button.disabled = true;
                  status.textContent = 'Đang kích hoạt…';
                  try {
                    const response = await fetch('../invitations/accept', {
                      method: 'POST', headers: { 'Content-Type': 'application/json' },
                      body: JSON.stringify({ token, password: password.value, confirmPassword: confirmPassword.value })
                    });
                    const result = await response.json();
                    if (!response.ok) {
                      status.textContent = result.detail || 'Không thể chấp nhận lời mời.';
                      return;
                    }
                    form.reset();
                    form.hidden = true;
                    status.textContent = 'Tài khoản đã kích hoạt. Hãy mở ứng dụng EzCondo và đăng nhập bằng email được mời.';
                  } catch {
                    status.textContent = 'Không kết nối được máy chủ. Vui lòng thử lại.';
                  } finally {
                    button.disabled = false;
                  }
                });
              </script>
            </body>
            </html>
            """, "text/html; charset=utf-8");
    }
}
