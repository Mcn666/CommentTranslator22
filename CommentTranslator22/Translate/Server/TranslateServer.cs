using CommentTranslator22.Translate.Format;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

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
