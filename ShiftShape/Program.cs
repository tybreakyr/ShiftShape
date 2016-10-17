using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            Marketinfo marketInfo = new Marketinfo();
            while (true)
            {
                var request = WebRequest.Create("https://shapeshift.io/marketinfo/btc_eth");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Console.WriteLine(response.StatusDescription);
                using (var dataStream = response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(dataStream))
                    {
                        var ser = new DataContractJsonSerializer(typeof(Marketinfo));
                        marketInfo = (Marketinfo) ser.ReadObject(dataStream);
                        Console.WriteLine(marketInfo.rate);
                    }
                }
                System.Threading.Thread.Sleep(2000);
            }
            Console.ReadLine();
        }
    }
    [DataContract]
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
