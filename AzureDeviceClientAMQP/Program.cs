//---------------------------------------------------------------------------------
// Copyright (c) October 2020, devMobile Software
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//---------------------------------------------------------------------------------
namespace devMobile.IoT.AzureDeviceClientAMQP
{
   using System;
   using System.IO;
   using System.Text;
   using System.Threading;
   using System.Threading.Tasks;

   using Microsoft.Azure.Devices.Client;
   using Newtonsoft.Json;

class Program
{
   private static string payload;

   static async Task Main(string[] args)
   {
      string filename;
      string azureIoTHubconnectionString;
      DeviceClient azureIoTHubClient;
      Timer MessageSender;

      if (args.Length != 2)
      {
         Console.WriteLine("[JOSN file] [AzureIoTHubConnectionString]");
         Console.WriteLine("Press <enter> to exit");
         Console.ReadLine();
         return;
      }

      filename = args[0];
      azureIoTHubconnectionString = args[1];

      try
      {
         payload = File.ReadAllText(filename);

         // Open up the connection
         azureIoTHubClient = DeviceClient.CreateFromConnectionString(azureIoTHubconnectionString, TransportType.Amqp);
         //azureIoTHubClient = DeviceClient.CreateFromConnectionString(azureIoTHubconnectionString, TransportType.Amqp_Tcp_Only);
         //azureIoTHubClient = DeviceClient.CreateFromConnectionString(azureIoTHubconnectionString, TransportType.Amqp_WebSocket_Only);

         await azureIoTHubClient.OpenAsync();

         await azureIoTHubClient.SetMethodDefaultHandlerAsync(MethodCallbackDefault, null);

         //MessageSender = new Timer(TimerCallbackAsync, azureIoTHubClient, new TimeSpan(0, 0, 10), new TimeSpan(0, 0, 10));
         MessageSender = new Timer(TimerCallbackSync, azureIoTHubClient, new TimeSpan(0, 0, 10), new TimeSpan(0, 0, 10));

#if MESSAGE_PUMP
         Console.WriteLine("Press any key to exit");
         while (!Console.KeyAvailable)
         {
            await Task.Delay(100);
         }
#else
         Console.WriteLine("Press <enter> to exit");
         Console.ReadLine();
#endif
      }
      catch (Exception ex)
      {
         Console.WriteLine(ex.Message);
         Console.WriteLine("Press <enter> to exit");
         Console.ReadLine();
      }
   }

   public static async void TimerCallbackAsync(object state)
   {
      DeviceClient azureIoTHubClient = (DeviceClient)state;

      try
      {
         // I know having the payload as a global is a bit nasty but this is a demo..
         using (Message message = new Message(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(payload))))
         {
            Console.WriteLine(" {0:HH:mm:ss} AzureIoTHubDeviceClient SendEventAsync start", DateTime.UtcNow);
            await azureIoTHubClient.SendEventAsync(message);
            Console.WriteLine(" {0:HH:mm:ss} AzureIoTHubDeviceClient SendEventAsync finish", DateTime.UtcNow);
         }
      }
      catch (Exception ex)
      {
         Console.WriteLine(ex.Message);
      }
   }

   public static void TimerCallbackSync(object state)
   {
      DeviceClient azureIoTHubClient = (DeviceClient)state;

      try
      {
         // I know having the payload as a global is a bit nasty but this is a demo..
         using (Message message = new Message(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(payload))))
         {
            Console.WriteLine(" {0:HH:mm:ss} AzureIoTHubDeviceClient SendEventAsync start", DateTime.UtcNow);
            azureIoTHubClient.SendEventAsync(message).GetAwaiter();
            Console.WriteLine(" {0:HH:mm:ss} AzureIoTHubDeviceClient SendEventAsync finish", DateTime.UtcNow);
         }
      }
      catch (Exception ex)
      {
         Console.WriteLine(ex.Message);
      }
   }

   private static async Task<MethodResponse> MethodCallbackDefault(MethodRequest methodRequest, object userContext)
   {
      Console.WriteLine($"Default handler method {methodRequest.Name} was called.");

      return new MethodResponse(200);
   }
}
}

