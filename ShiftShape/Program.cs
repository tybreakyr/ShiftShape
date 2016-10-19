using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Devart.Data.PostgreSql;

namespace ShiftShape
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var marketInfo = new Marketinfo();
            var requestQueue = new Queue<WebRequest>();
            var endDate = DateTime.Now.AddDays('1');
            var currentTime = new DateTime();
            var count = 0;

            Console.WriteLine($"Starting Date and Time: {DateTime.Now}");
            Console.WriteLine($"Ending Date and Time: {endDate}");
            
            while (currentTime <= endDate)
            {
                requestQueue.Enqueue(WebRequest.Create("https://shapeshift.io/marketinfo/btc_eth"));
                FetchWebResponses(requestQueue);
                Console.WriteLine($"Adding request {count++} to Queue");
                currentTime = DateTime.Now;
            }

            
            
            Console.WriteLine("Press Enter to Continue.");
            Console.ReadLine();
        }

        //read in web requests from JSON file/DB/webservice

        public static void FetchWebResponses(Queue<WebRequest> webRequestQueue)
        {
            var count = 0;
            while (webRequestQueue.Count > 0)
            {
                var webRequest = webRequestQueue.Dequeue();
                using (var response = webRequest.GetResponse())
                {
                    var deserializedResponse = Convert_To_JSON_DeQueue(response);
                    InsertToDb(ToJson(deserializedResponse));
                }
                
                //Console.WriteLine($"Retrieved Response {count++}");
                System.Threading.Thread.Sleep(6000);//6 second
            }
        }

        private static dynamic Convert_To_JSON_DeQueue(WebResponse webResponse)
        {
            //"https://shapeshift.io/marketinfo/btc_eth"
            
            if (webResponse == null) return null;
            using (var dataStream = webResponse.GetResponseStream())
            {
                var ser = new DataContractJsonSerializer(typeof(Marketinfo));
                return dataStream != null ? ser.ReadObject(dataStream) : null;
            }

        }

        private static PgSqlConnection ConnectToDatabase()
        {
            return new PgSqlConnection
            {
                Host = "localhost",
                Port = 5432,
                Database = "shiftShaper",
                UserId = "postgres",
                Password = "postgres"
            };
        }

        private static void InsertToDb(string jsonObject)
        {
            var connection = ConnectToDatabase();
            var sqlCommand = new PgSqlCommand
            {
                CommandText = $"insert into public.initial_bucket (value) values (\'{jsonObject}\')",
                Connection = connection
            };
            connection.Open();
            using (connection)
            {
                try
                {
                    var aff = sqlCommand.ExecuteNonQuery();

                    Console.WriteLine(aff + " rows were affected.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(sqlCommand.CommandText);
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Error encountered during INSERT operation.");
                }
            }

        }

        public static string ToJson(dynamic objectToConvert)
        {
            var jsonSerializer = new DataContractJsonSerializer(objectToConvert.GetType());
            var memoryStream = new MemoryStream();

            jsonSerializer.WriteObject(memoryStream, objectToConvert);
            memoryStream.Position = 0;
            var streamReader = new StreamReader(memoryStream);
            return streamReader.ReadToEnd();
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
