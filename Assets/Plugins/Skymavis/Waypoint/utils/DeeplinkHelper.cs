using System;
using System.Collections.Generic;
using System.Linq;

namespace SkyMavis.Utils
{
    internal static class DeeplinkHelper
    {
        public static Dictionary<string, string> ParseDeeplink(string queryString)
        {
            try
            {
                Dictionary<string, string> map = new Dictionary<string, string>();

                var parsedUrl = queryString.Split('?')[1];
                return parsedUrl
                    .Split('&').ToDictionary(c => c.Split('=')[0],
                    c => System.Uri.UnescapeDataString(c.Split('=')[1]));
            }
            catch (Exception)
            {
                throw new Exception("Parse deeplink failed!");
            }

        }
    }
}
