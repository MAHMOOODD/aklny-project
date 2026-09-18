using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Resturant_Backend.Data;
using Resturant_Backend.DTO.payment;
using Resturant_Backend.Models;
using Resturant_Backend.Services;
using System.Text.Json;

namespace Resturant_Backend.Controller
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymobService _paymob;
        private readonly AppDbContext _db;
        private readonly PaymobSettings _settings;




        public PaymentController(PaymobService paymob, AppDbContext db, IOptions<PaymobSettings> settings)
        {
            _paymob = paymob;
            _db = db;
            _settings = settings.Value;
        }

        [HttpPost("initiate/{orderId}")]
        [Authorize]
        public async Task<IActionResult> InitiatePayment(int orderId)
        {
            var order = await _db.Orders.Include(o => o.Appuser)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if(order == null)
            {
                this.NotFoundEx("Order not found");
            }

            var amountCents = (int)( order.TotalPrice * 100 );

            var authToken = await _paymob.GetAuthTokenAsync();
            var paymobOrderId = await _paymob.RegisterOrderAsync(authToken, order.Id, amountCents);
            var paymentToken = await _paymob.GetPaymentKeyAsync(authToken, paymobOrderId, amountCents,
                new BillingData
                {
                    FirstName = string.IsNullOrWhiteSpace(order.Appuser.FullName) ? "NA" : order.Appuser.FullName,
                    Email = string.IsNullOrWhiteSpace(order.Appuser.Email) ? "NA" : order.Appuser.Email,
                    PhoneNumber = string.IsNullOrWhiteSpace(order.Appuser.PhoneNumber) ? "NA" : order.Appuser.PhoneNumber
                });

            order.PaymobOrderId = paymobOrderId;
            await _db.SaveChangesAsync();

            var iframeUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_settings.IframeId}?payment_token={paymentToken}";
            return this.Success(new { iframeUrl });
        }
        [HttpPost("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymobCallback([FromQuery] string hmac, [FromBody] JsonElement body)
        {
            var transactionObj = body.GetProperty("obj");

            var payload = JsonSerializer.Deserialize<PaymobCallbackDto>(transactionObj.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var owner = transactionObj.GetProperty("owner").GetInt64().ToString();

            var isValid = PaymobHmacValidator.Validate(payload, owner, hmac, _settings.HmacSecret);
            if(!isValid)
                this.UnauthorizedEx();

            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.PaymobOrderId == payload.Order.Id);
            if(order == null)
                this.NotFoundEx("Order not found");

            if(order.PaymentStatus == PaymentStatus.Paid)
                return this.Success();

            order.TransactionId = payload.Id.ToString();
            order.PaymentStatus = payload.Success ? PaymentStatus.Paid : PaymentStatus.Failed;
            await _db.SaveChangesAsync();

            return this.Success();
        }
    }
}