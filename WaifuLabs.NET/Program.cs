using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

            var sessionToken = GetWaifuToken();
            

            if(args.Length > 0)
            {
                savePath = args[0];
            }

            GenerateWaifu(sessionToken);

            Console.WriteLine("Generation complete!");            
        }

        static void GenerateWaifu(string sessionToken)
        {
            Uri websocketUri = new Uri($"wss://waifulabs.com/creator/socket/websocket?token={sessionToken}&vsn=2.0.0");
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken cancelToken = cancelTokenSource.Token;

            // Step 0 - Establish Connection

            Console.WriteLine("Connecting to WaifuLabs...");
            ClientWebSocket webSocket = new ClientWebSocket();
            Encoding utf8 = Encoding.UTF8;
            webSocket.ConnectAsync(websocketUri, cancelToken);
            WebsocketMessage websocketMessage = new WebsocketMessage(3,"api","phx_join","{}");

            while (webSocket.State == WebSocketState.Connecting) { };

            ArraySegment<byte> arraySegment = new ArraySegment<byte>(utf8.GetBytes(websocketMessage.ToString()));
            webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, cancelToken).Wait();
            arraySegment = new ArraySegment<byte>(new byte[400000]);
            WebSocketReceiveResult result = webSocket.ReceiveAsync(arraySegment, cancelToken).Result;
            websocketMessage = new WebsocketMessage(Encoding.UTF8.GetString(arraySegment.Array, arraySegment.Offset, result.Count));

            if (!websocketMessage.Function.Equals("phx_reply") && websocketMessage.JsonBody.Equals("{\"response\":{},\"status\":\"ok\"}"))
            {
                throw new Exception("Unexpected response");
            }

            websocketMessage.MessageId++; // Not sure why but it gets incremented an extra time here 

            // Step 1-3 - Generate Waifu
            string nextSeed = null;
            for (int i = 0; i < 4; i++)
            {
                Console.WriteLine($"Performing step {i+1} of waifu generation...");
                GenerateRequest generateRequest = new GenerateRequest();
                generateRequest.id = 1;
                WaifuParameters waifuParameters = new WaifuParameters();
                waifuParameters.step = i;
                if (nextSeed != null) waifuParameters.currentGirl = nextSeed;
                generateRequest.parameters = waifuParameters;

                websocketMessage = new WebsocketMessage(++websocketMessage.MessageId, "api", "generate", JsonConvert.SerializeObject(generateRequest));

                arraySegment = new ArraySegment<byte>(utf8.GetBytes(websocketMessage.ToString()));
                webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, cancelToken).Wait();
                arraySegment = new ArraySegment<byte>(new byte[800000]);
                result = webSocket.ReceiveAsync(arraySegment, cancelToken).Result;
                websocketMessage = new WebsocketMessage(Encoding.UTF8.GetString(arraySegment.Array, arraySegment.Offset, result.Count));
                if (websocketMessage.JsonBody.Contains("\"newGirls\":{"))
                {
                   websocketMessage.JsonBody = websocketMessage.JsonBody.Replace("\"newGirls\":{", "\"newGirls\":[{").Replace("}},\"id", "}]},\"id");
                }
                Response waifuResponse = JsonConvert.DeserializeObject<Response>(websocketMessage.JsonBody);
                int nextWaifuIdx = RandomNumber(0, waifuResponse.response.data.newGirls.Count - 1);
                nextSeed = waifuResponse.response.data.newGirls[nextWaifuIdx].seeds;
            }

            // Step 4 - Generate Big Waifu

            GenerateRequest generateBigRequest = new GenerateRequest();
            generateBigRequest.id = 1;
            WaifuParameters bigWaifuParameters = new WaifuParameters();
            bigWaifuParameters.currentGirl = nextSeed;
            generateBigRequest.parameters = bigWaifuParameters;

            websocketMessage = new WebsocketMessage(++websocketMessage.MessageId, "api", "generate_big", JsonConvert.SerializeObject(generateBigRequest));
            websocketMessage.JsonBody = websocketMessage.JsonBody.Replace(",\"step\":0", "");

            arraySegment = new ArraySegment<byte>(utf8.GetBytes(websocketMessage.ToString()));
            webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, cancelToken);
            arraySegment = new ArraySegment<byte>(new byte[800000]);
            result = webSocket.ReceiveAsync(arraySegment, cancelToken).Result;
            websocketMessage = new WebsocketMessage(Encoding.UTF8.GetString(arraySegment.Array, arraySegment.Offset, result.Count));

            Response finalWaifu = JsonConvert.DeserializeObject<Response>(websocketMessage.JsonBody);

            Console.WriteLine("Saving waifu to " + savePath);
            byte[] bytes = Convert.FromBase64String(finalWaifu.response.data.girl);
            using (FileStream image = new FileStream(savePath, FileMode.Create))
            {
                image.Write(bytes, 0, bytes.Length);
                image.Flush();
            }

            return;
        }

        static int RandomNumber(int min, int max)
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            return random.Next(min, max);
        }

        static string GetWaifuToken()
        {
            using (var client = new WebClient())
            {
                var webpage = client.DownloadString("https://waifulabs.com/generate");
                var tokenIndex = webpage.IndexOf("window.authToken = \"") + 20;
                var foundEndOfToken = false;
                string token = "";
                while (!foundEndOfToken)
                {
                    char nextChar = webpage[tokenIndex];
                    if (nextChar != '"')
                    {
                        token += nextChar;
                        tokenIndex++;
                    } else
                    {
                        foundEndOfToken = true;
                    }
                }
                return token;
            }
        }
    }
}
