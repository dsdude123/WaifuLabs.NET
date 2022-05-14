using System;
using System.Collections.Generic;
using System.Text;

namespace WaifuLabs.NET.Model
{
    public class WebsocketMessage
    {
        public int UnknownNumber { get; set; }
        public int MessageId { get; set; }
        public string Destination { get; set; }
        public string Function { get; set; }
        public string JsonBody { get; set; }

        public WebsocketMessage(string rawMessage)
        {
            string[] sections = rawMessage.Replace("[", "").Replace("]", "").Split(',');
            UnknownNumber = int.Parse(sections[0].Replace("\"", ""));
            MessageId = int.Parse(sections[1].Replace("\"", ""));
            Function = sections[2].Replace("\"", "");
            Function = sections[3].Replace("\"", "");
            for(int i = 4; i < sections.Length; i++)
            {
                JsonBody += sections[i];
                if (i + 1 < sections.Length) JsonBody += ',';
            }         
        }

        public WebsocketMessage(int messageId, string destination, string function, string jsonBody)
        {
            UnknownNumber = 3;
            MessageId = messageId;
            Destination = destination;
            Function = function;
            JsonBody = jsonBody;
        }

        public string ToString()
        {
            return $"[\"{UnknownNumber}\",\"{MessageId}\",\"{Destination}\",\"{Function}\",{JsonBody}]";
        }
    }
}
