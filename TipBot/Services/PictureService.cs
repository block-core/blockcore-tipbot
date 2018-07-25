using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TipBot.Services
{
    public class PictureService
    {
        private readonly HttpClient http;

        public PictureService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<Stream> GetCatPictureAsync()
        {
            HttpResponseMessage resp = await this.http.GetAsync("https://cataas.com/cat");
            return await resp.Content.ReadAsStreamAsync();
        }
    }
}
