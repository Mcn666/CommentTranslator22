using CommentTranslator22.Translate.Format;
using System.Threading.Tasks;

namespace CommentTranslator22.Translate.Server
{
    internal static class TranslateServer
    {
        public static string GetLanguageCode(ServerEnum server, LanguageEnum language)
        {
            return LanguageCode.Code[server.GetHashCode()][language.GetHashCode()];
        }

        public static async Task<ApiRecvFormat> BaiduAsync(ApiSendFormat format, string appid, string key)
        {
            return await BaiduFanyi.BaiduAsync(format, appid, key);
        }

        public static async Task<ApiRecvFormat> BingAsync(ApiSendFormat format)
        {
            return await BingFanyi.BingAsync(format);
        }

        public static async Task<ApiRecvFormat> GoogleAsync(ApiSendFormat format)
        {
            return await GoogleFanyi.GoogleAsync(format);
        }

        //public static async Task<ApiRecvFormat> YoudaoAsync()
        //{
        //    return null;
        //}
    }
}
