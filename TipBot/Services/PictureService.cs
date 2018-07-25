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

        public async Task<Stream> GetStratisLogoAsync()
        {
            HttpResponseMessage resp = await this.http.GetAsync(Constants.StratisLogoUrl);

            Stream stream = await resp.Content.ReadAsStreamAsync();

            // Streams must be seeked to their beginning before being uploaded!
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}
