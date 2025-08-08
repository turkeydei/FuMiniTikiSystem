using System.Reflection;

namespace FUMiniTikiSystem.DAL.Constants
{
    public class AppCts
    {
        public static readonly string AbsoluteProjectPath =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

        public static class SeederRelativePath
        {
            public static string JsonPath = Path.Combine("Helper", "SeedData");

            public static string CategoryPath = Path.Combine(JsonPath, "categoryData.json");
            public static string ProductPath = Path.Combine(JsonPath, "productData.json");
            public static string CustomerPath = Path.Combine(JsonPath, "customerData.json");
            public static string OrderPath = Path.Combine(JsonPath, "orderData.json");
            public static string OrderDetailPath = Path.Combine(JsonPath, "orderDetailData.json");
            public static string PaymentPath = Path.Combine(JsonPath, "paymentData.json");
            public static string ReviewPath = Path.Combine(JsonPath, "reviewData.json");
            public static string CartPath = Path.Combine(JsonPath, "cartData.json");
            public static string CartItemPath = Path.Combine(JsonPath, "cartItemData.json");
        }
    }
}
