using MrDrone.Core.Classes;
using MrDrone.Core.Ros;
using RosSharp.RosBridgeClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MrDrone.Console
{
    public class Program
    {
        private static Drone Drone;
        private static RosSocket RosSocket;
        private static int numActuators = 6;

        static void Main(string[] args)
        {
            bool isConnected = false;
            new Thread(() =>
            {
                string host = "ws://localhost:9090";
                System.Console.WriteLine("Connecting to host: " + host);

                RosSocket = RosSocketFactory.GetStandardRosSocket(host,
                    () => { isConnected = true; System.Console.WriteLine("Connected to host"); },
                    () => { System.Console.WriteLine("Disconnected from host"); }
                );
            }).Start();

            while (!isConnected)
            {
                Thread.Sleep(250);
            }

            Cycle();

            System.Console.ReadLine();
        }

        /// <summary>
        /// The console tool's main cycle accepting user input
        /// </summary>
        private static void Cycle()
        {
            Drone = new Drone(() => RosSocket);

            string command = "help";

            do
            {
                var parts = command.Split(' ');
                if (commands.ContainsKey(parts[0])) commands[parts[0]](parts.Skip(1).ToArray());
                else if (command == "help")
                {
                    string helpText = string.Join("\n", commands.Keys.Select(x => "\n" + x));
                    System.Console.WriteLine("Enter one of the following: \n" + helpText);
                }

                System.Console.Write("\n\nCommand: ");
                command = System.Console.ReadLine();

            } while (command != "exit");
        }

        private static Dictionary<string, Action<string[]>> commands = new Dictionary<string, Action<string[]>>()
        {
            ["vel"] = (parts) =>
            {
                double[] efforts = new double[numActuators];
                if (parts.Length == 1)
                {
                    double effort = double.Parse(parts[0]);
                    for (int i = 0; i < numActuators; i++)
                    {
                        efforts[i] = effort;
                    }
                }
                else if (parts.Length == numActuators)
                {
                    for (int i = 0; i < numActuators; i++)
                    {
                        double effort = double.Parse(parts[i]);
                        efforts[i] = effort;
                    }

                }
                Drone.CommandActuator(efforts);
            }
        };
    }
}
