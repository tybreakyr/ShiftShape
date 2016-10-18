using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace ShiftShape
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var marketInfo = new Marketinfo();
            var requestQueue = new Queue<WebRequest>();
            requestQueue.Enqueue(WebRequest.Create("https://shapeshift.io/marketinfo/btc_eth"));

            var responseQueue = new Queue<WebResponse>();
            FetchQueueResponses(requestQueue,responseQueue);
            Console.ReadLine();
        }

        public static void FetchQueueResponses(Queue<WebRequest> webRequestQueue,Queue<WebResponse> webResponseQueue)
        {
            
            while (true)
            {
                foreach (dynamic webRequest in webRequestQueue)
                {
                    webResponseQueue.Enqueue((HttpWebResponse)webRequest.GetResponse());                    
                }
            }
        }

        private static dynamic Convert_To_JSON_DeQueue(Queue<WebResponse> webResponseQueue)
        {
            //"https://shapeshift.io/marketinfo/btc_eth"

            var response = webResponseQueue.Dequeue();

            try
            {
                using (var dataStream = response.GetResponseStream())
                {
                    var ser = new DataContractJsonSerializer(typeof(Marketinfo));
                    return dataStream != null ? ser.ReadObject(dataStream) : null;
                }
            }
            finally
            {
                //throttled for safety
                System.Threading.Thread.Sleep(3000);
            }
        }

    }
    [DataContract]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Marketinfo
    {
        [DataMember]
        public string pair { get; set; }
        [DataMember]
        public double rate { get; set; }
        [DataMember]
        public double minerFee { get; set; }
        [DataMember]
        public double limit { get; set; }
        [DataMember]
        public double minimum { get; set; }
        [DataMember]
        public double maxLimit { get; set; }
    }
}
