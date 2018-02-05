using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace ExampleRoverMotorDriver
{
    public class Program
    {
        public static void Main()
        {
            Debug.Print("Init driver");
            Thread.Sleep(1000);
            
            RoverMotorDriver driver = new RoverMotorDriver();
            Thread.Sleep(1000);

            while (true)
            {
                Debug.Print("SetForward");
                driver.SetForeward(600, 600);
                Thread.Sleep(1000);

                Debug.Print("SetBackward");
                driver.SetBackward(600, 600);
                Thread.Sleep(1000);

                Debug.Print("SetTurnLeft");
                driver.SetTurnLeft(600);
                Thread.Sleep(1000);

                Debug.Print("SetTurnRight");
                driver.SetTurnRight(600);
                Thread.Sleep(1000);

                Debug.Print("MotorStop");
                driver.MotorStop();
                Thread.Sleep(1000);
            }
        }

    }
}
