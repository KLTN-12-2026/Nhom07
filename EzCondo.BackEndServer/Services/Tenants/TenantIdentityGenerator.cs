using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;
using System.Text;

namespace EzCondo.BackEndServer.Services.Tenants
{
	public sealed class TenantIdentityGenerator : ITenantIdentityGenerator
	{
		public TenantIdentity Generate(
			Guid tenantId,
			string legalName)
		{
			if (tenantId == Guid.Empty)
			{
				throw new ArgumentException(
					"TenantId không được để trống.",
					nameof(tenantId));
			}

			ArgumentException.ThrowIfNullOrWhiteSpace(legalName);

			var encodedId = WebEncoders.Base64UrlEncode(
				tenantId.ToByteArray());

			// TEN- cộng 22 ký tự Base64Url, không vượt quá giới hạn 30.
			var code = $"TEN-{encodedId}";

			var baseSlug = ToSlug(legalName);

			// Dùng toàn bộ Guid để slug không phụ thuộc vào truy vấn kiểm tra trùng.
			var slug = $"{baseSlug}-{tenantId:N}";

			return new TenantIdentity(code, slug);
		}

		private static string ToSlug(string value)
		{
			var normalized = value
				.Trim()
				.ToLowerInvariant()
				.Replace('đ', 'd')
				.Normalize(NormalizationForm.FormD);

			var builder = new StringBuilder(normalized.Length);
			var previousWasSeparator = false;

			foreach (var character in normalized)
			{
				var category = CharUnicodeInfo.GetUnicodeCategory(character);

				if (category == UnicodeCategory.NonSpacingMark)
				{
					continue;
				}

				var isAsciiLetter =
					character is >= 'a' and <= 'z';

				var isDigit =
					character is >= '0' and <= '9';

				if (isAsciiLetter || isDigit)
				{
					builder.Append(character);
					previousWasSeparator = false;
					continue;
				}

				if (builder.Length > 0 && !previousWasSeparator)
				{
					builder.Append('-');
					previousWasSeparator = true;
				}
			}

			var slug = builder
				.ToString()
				.Trim('-');

			return string.IsNullOrWhiteSpace(slug)
				? "tenant"
				: slug;
		}
	}
}
