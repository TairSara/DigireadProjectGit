using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;


namespace DigireadProject.Controllers
{ 
    public class CheckoutController : Controller
    {
        private readonly libraryProject_digireadEntities _db = new libraryProject_digireadEntities();
        private readonly string _paypalClientId = System.Configuration.ConfigurationManager.AppSettings["PayPal:ClientId"];
        private readonly string _paypalSecret = System.Configuration.ConfigurationManager.AppSettings["PayPal:Secret"];
        private readonly string _paypalUrl = System.Configuration.ConfigurationManager.AppSettings["PayPal:Url"];

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateOrder()
        {
            try
            {
                var userId = GetCurrentUserId();
                decimal cartTotal = _db.ShoppingCart
                    .Where(s => s.UserID == userId)
                    .Sum(s => (s.Price ?? 0) * (s.Quantity ?? 0));

                var accessToken = await GetPaypalAccessToken();
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var orderRequest = new
                    {
                        intent = "CAPTURE",
                        application_context = new
                        {
                            brand_name = "Digiread",
                            locale = "he-IL",
                            shipping_preference = "NO_SHIPPING",
                            user_action = "PAY_NOW"
                        },
                        purchase_units = new[]
                        {
                            new
                            {
                                amount = new
                                {
                                    currency_code = "ILS",
                                    value = cartTotal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                                }
                            }
                        }
                    };

                    var jsonContent = JsonConvert.SerializeObject(orderRequest);
                    var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{_paypalUrl}/v2/checkout/orders", httpContent);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var orderData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        return Json(new { id = orderData.id }, JsonRequestBehavior.AllowGet);
                    }

                    Response.StatusCode = (int)response.StatusCode;
                    return Json(new { error = "Failed to create PayPal order" });
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CapturePayment(string orderId)
        {
            try
            {
                var accessToken = await GetPaypalAccessToken();
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    var response = await client.PostAsync(
                        $"{_paypalUrl}/v2/checkout/orders/{orderId}/capture",
                        new StringContent("{}", Encoding.UTF8, "application/json")
                    );

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Success", "Order");
                    }

                    TempData["Error"] = "Failed to capture payment";
                    return RedirectToAction("Cart", "ShoppingCart");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Cart", "ShoppingCart");
            }
        }

        private async Task<string> GetPaypalAccessToken()
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_paypalClientId}:{_paypalSecret}"));
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                
                var content = new StringContent("grant_type=client_credentials", 
                    Encoding.UTF8, 
                    "application/x-www-form-urlencoded");

                var response = await client.PostAsync($"{_paypalUrl}/v1/oauth2/token", content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var tokenData = JsonConvert.DeserializeObject<dynamic>(responseText);
                    return tokenData.access_token;
                }

                throw new Exception("Failed to get PayPal access token");
            }
        }

        private int GetCurrentUserId()
        {
            var username = User.Identity.Name;
            var user = _db.Users.FirstOrDefault(u => u.Username == username);
            return user?.UserID ?? 0;
        }
    }
}