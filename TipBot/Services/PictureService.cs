using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace TipBot.Services
{
    public class PictureService
    {
        /// <remarks>Set by DI.</remarks>
        private readonly Settings Settings;

        private readonly HttpClient httpClient;

        public PictureService(HttpClient httpClient, Settings setting)
        {
            this.httpClient = httpClient;
            this.Settings = setting;
        }

        public async Task<Stream> GetLogoAsync()
        {
            HttpResponseMessage resp = await this.httpClient.GetAsync(this.Settings.LogoUrl);

            Stream stream = await resp.Content.ReadAsStreamAsync();

            // Streams must be seeked to their beginning before being uploaded!
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}
