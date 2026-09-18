namespace Resturant_Backend.Services
{
    using Resturant_Backend.DTO.payment;
    using System.Security.Cryptography;
    using System.Text;

    public class PaymobHmacValidator
    {
        public static bool Validate(PaymobCallbackDto payload, string owner, string hmacFromUrl, string secret)
        {
            // 1. build the concatenated string based on the specified order of fields
            var concatenated =
                $"{payload.AmountCents}" +
                $"{payload.CreatedAt}" +
                $"{payload.Currency}" +
                $"{payload.ErrorOccured.ToString().ToLower()}" +
                $"{payload.HasParentTransaction.ToString().ToLower()}" +
                $"{payload.Id}" +
                $"{payload.IntegrationId}" +
                $"{payload.Is3dSecure.ToString().ToLower()}" +
                $"{payload.IsAuth.ToString().ToLower()}" +
                $"{payload.IsCapture.ToString().ToLower()}" +
                $"{payload.IsRefunded.ToString().ToLower()}" +
                $"{payload.IsStandalonePayment.ToString().ToLower()}" +
                $"{payload.IsVoided.ToString().ToLower()}" +
                $"{payload.Order.Id}" +
                $"{owner}" +
                $"{payload.Pending.ToString().ToLower()}" +
                $"{payload.SourceData.Pan}" +
                $"{payload.SourceData.SubType}" +
                $"{payload.SourceData.Type}" +
                $"{payload.Success.ToString().ToLower()}";

            // 2. use HMAC-SHA512 to compute the hash of the concatenated string using the secret key
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var messageBytes = Encoding.UTF8.GetBytes(concatenated);

            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);

            // 3. transform the hash to a lowercase hexadecimal string  
            var computedHmac = Convert.ToHexString(hashBytes).ToLower();

            // 4. compare the computed HMAC with the
            // HMAC from the URL in a time-constant manner to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedHmac),
                Encoding.UTF8.GetBytes(hmacFromUrl));
        }
    }
}
