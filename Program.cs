using System;
using System.Threading;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DTH11_Sensor_Project
{
    class Program
    {
        static void Main(string[] args)
        {
            try {

                var conn_string_builder = new SqliteConnectionStringBuilder();
                conn_string_builder.DataSource = "/home/pi/dotnet_projects/IotAPIFrameWork/iot.db";

                using (var sqlConnection = new SqliteConnection(conn_string_builder.ConnectionString))
                {
                    sqlConnection.Open();

                    Pi.Init<BootstrapWiringPi>();

                    var pin = Pi.Gpio[04];

                    var dht = new DHT(pin, DHTSensorTypes.DHT11);
                                                        
                        while (true)
                        {
                            try
                            {
                                var d = dht.ReadData();
                                var jsonString = JsonSerializer.Serialize(d);
                                using (var sqlTransaction = sqlConnection.BeginTransaction())
                                {
                                    var insertCmd = sqlConnection.CreateCommand();
                                    insertCmd.CommandText = "Insert INTO SensorMeasurements(SensorId,MeasurementTypeId,LocationId,MeasuredValue,RawData,MeasuredDate) VALUES(1,1,1,"+d.TempCelcius+",'" + jsonString + "','" + DateTime.Now.Date.ToString() +"')";
                                    insertCmd.ExecuteNonQuery();
                                    sqlTransaction.Commit();
                                }
                                Console.WriteLine(DateTime.UtcNow);
                                Console.WriteLine(" temp: " + d.TempCelcius);
                                Console.WriteLine(" hum: " + d.Humidity);
                            }
                            catch (DHTException)
                            {                                
                            }
                            Thread.Sleep(10000);                        
                    }
                }
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.Message + " - " + e.StackTrace);
            }
        }
    }
}