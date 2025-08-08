using FUMiniTikiSystem.BLL.Helper;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Web;

namespace FUMiniTikiSystem.BLL
{
    public class VNPAYService
    {
        private readonly OrderService _orderService;
        private readonly PaymentService _paymentService;
        private readonly CartService _cartService;
        private readonly VNPAYSettings _VNPAYSettings;
        private readonly IConfiguration _configuration;

        public VNPAYService(OrderService orderService, PaymentService paymentService, IConfiguration configuration, CartService cartService)
        {
            _configuration = configuration;
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _paymentService = paymentService;
            _VNPAYSettings = configuration.GetSection("VNPAYSettings").Get<VNPAYSettings>();
            _cartService = cartService;
        }

        public string CreatePaymentUrl(decimal amount, int orderId, int paymentId)
        {
            try
            {
                string hostName = Dns.GetHostName();
                string clientIPAddress = Dns.GetHostAddresses(hostName).GetValue(0).ToString();
                PayLib pay = new PayLib();
                var vnp_Amount = amount * 100;
                pay.AddRequestData("vnp_Version", PayLib.VERSION);
                pay.AddRequestData("vnp_Command", "pay");
                pay.AddRequestData("vnp_TmnCode", _VNPAYSettings.VnPayTmnCode);
                pay.AddRequestData("vnp_Amount", vnp_Amount.ToString("0"));
                pay.AddRequestData("vnp_BankCode", "");
                pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                pay.AddRequestData("vnp_CurrCode", "VND");
                pay.AddRequestData("vnp_IpAddr", clientIPAddress);
                pay.AddRequestData("vnp_Locale", "vn");
                pay.AddRequestData("vnp_OrderInfo", $"PaymentId:{paymentId}");
                pay.AddRequestData("vnp_OrderType", "other");
                pay.AddRequestData("vnp_ReturnUrl", _VNPAYSettings.VnPayReturnUrl);
                pay.AddRequestData("vnp_TxnRef", "O-" + orderId.ToString() + "-" + Guid.NewGuid().ToString());

                string paymentUrl = pay.CreateRequestUrl(_VNPAYSettings.VnPayUrl, _VNPAYSettings.VnPayHashSecret);
                return paymentUrl;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while creating the payment URL: " + ex.Message);
            }
        }

        public bool ValidatePaymentResponse(string queryString)
        {
            try
            {
                var json = HttpUtility.ParseQueryString(queryString);
                var orderId = int.Parse(json["vnp_TxnRef"].Split('-')[1]);

                var order = _orderService.GetOrder(orderId);
                if (order == null)
                {
                    return false;
                }

                // Extract PaymentId from vnp_OrderInfo
                var orderInfo = json["vnp_OrderInfo"];
                var paymentId = int.Parse(orderInfo.Split(':')[1]);

                string vnp_ResponseCode = json["vnp_ResponseCode"].ToString();
                string vnp_SecureHash = json["vnp_SecureHash"].ToString();
                var pos = queryString.IndexOf("&vnp_SecureHash");
                bool checkSignature = ValidateSignature(queryString.Substring(1, pos - 1), vnp_SecureHash, _VNPAYSettings.VnPayHashSecret);

                if (order.OrderStatus != "Pending" && order != null)
                {
                    return false;
                }

                if (checkSignature && _VNPAYSettings.VnPayTmnCode == json["vnp_TmnCode"].ToString())
                {
                    if (vnp_ResponseCode == "00" && json["vnp_TransactionStatus"] == "00")
                    {
                        var payment = _paymentService.GetPayment(paymentId);

                        if (payment == null)
                        {
                            return false;
                        }
                        payment.PaymentStatus = "Successful";
                        _paymentService.UpdatePayment(payment);

                        order.OrderStatus = "Completed";
                        _orderService.UpdateOrder(order);

                        var cart = _cartService.GetCartByCustomerID(order.CustomerID);
                        if (cart != null)
                        {
                            cart.IsActive = false;
                            _cartService.Update(cart);
                        }

                        return true;
                    }
                    else
                    {
                        if (json["vnp_BankTranNo"]?.ToString() != null || json["vnp_TxnRef"]?.ToString() != null)
                        {
                            var payment = _paymentService.GetPayment(paymentId);

                            if (payment == null)
                            {
                                return false;
                            }
                            payment.PaymentStatus = "Failed";
                            _paymentService.UpdatePayment(payment);
                        }
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool ValidateSignature(string rspraw, string inputHash, string secretKey)
        {
            string myChecksum = Utils.HmacSHA512(secretKey, rspraw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        public class VNPAYSettings
        {
            public string VnPayUrl { get; set; }
            public string VnPayReturnUrl { get; set; }
            public string VnPayQueryUrl { get; set; }
            public string VnPayTmnCode { get; set; }
            public string VnPayHashSecret { get; set; }
        }
    }
}
