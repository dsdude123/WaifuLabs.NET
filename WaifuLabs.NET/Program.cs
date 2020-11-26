using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using WaifuLabs.NET.Model;

namespace WaifuLabs.NET
{
    class Program
    {

        static readonly string generateUrl = "https://api.waifulabs.com/generate";
        static readonly string generateBigUrl = "https://api.waifulabs.com/generate_big";

        static HttpClient client = new HttpClient();
        static string savePath = "waifu.png";

        static void Main(string[] args)
        {
            Console.WriteLine("WaifuLabs.NET");
            Console.WriteLine("(c) 2020 dsdude123");
            Console.WriteLine("https://github.com/dsdude123/WaifuLabs.NET\n");
            Console.WriteLine("This program is powered by Waifu Labs which is made by Sizigi Studios.");
            Console.WriteLine("Please consider supporting them on Patreon or Ko-fi to keep this service avalible.");
            Console.WriteLine("https://www.patreon.com/bePatron?u=23037728");
            Console.WriteLine("https://ko-fi.com/B0B5106CI\n");

            if(args.Length > 0)
            {
                savePath = args[0];
            }

            Console.WriteLine("Performing step 1 of waifu generation...");
            GenerateWaifuRequest request = new GenerateWaifuRequest();
            request.step = 0;
            NewWaifusResponse response = GenerateWaifu(request);

            int nextWaifu;
            Waifu waifu;

            for (int i = 2; i < 5; i++) {
                Console.WriteLine("Performing step " + i + " of waifu generation...");
                nextWaifu = RandomNumber(0, 16);
                waifu = response.newGirls[nextWaifu];
                request.step = i - 1;
                request.currentGirl = waifu.seeds;
                response = GenerateWaifu(request);
            }

            Console.WriteLine("Getting finished waifu...");
            nextWaifu = RandomNumber(0, 16);
            waifu = response.newGirls[nextWaifu];
            request.step = 4;
            request.size = 512;
            request.currentGirl = waifu.seeds;
            BigWaifuResponse finalWaifu = GenerateBigWaifu(request);

            Console.WriteLine("Saving waifu to " + savePath);
            byte[] bytes = Convert.FromBase64String(finalWaifu.girl);
            using (FileStream image = new FileStream(savePath, FileMode.Create))
            {
                image.Write(bytes, 0, bytes.Length);
                image.Flush();
            }

            Console.WriteLine("Generation complete!");            
        }

        static NewWaifusResponse GenerateWaifu(GenerateWaifuRequest request)
        {
            string json = JsonConvert.SerializeObject(request);
            StringContent httpRequest = new StringContent(json);
            HttpResponseMessage httpResponse = client.PostAsync(generateUrl, httpRequest).Result;
            httpResponse.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<NewWaifusResponse>(httpResponse.Content.ReadAsStringAsync().Result);
        }

        static BigWaifuResponse GenerateBigWaifu(GenerateWaifuRequest request)
        {
            string json = JsonConvert.SerializeObject(request);
            StringContent httpRequest = new StringContent(json);
            HttpResponseMessage httpResponse = client.PostAsync(generateBigUrl, httpRequest).Result;
            httpResponse.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<BigWaifuResponse>(httpResponse.Content.ReadAsStringAsync().Result);
        }

        static int RandomNumber(int min, int max)
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            return random.Next(min, max);
        }
    }
}
