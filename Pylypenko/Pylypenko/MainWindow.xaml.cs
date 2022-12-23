using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using Microsoft.Windows.Themes;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Pylypenko
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isScaning;
        Timer scanTimer = new();

        public MainWindow()
        {
            isScaning = false;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool parsResult;
            int time = 0;

            if (isScaning)
            {
                isScaning = false;
                parsResult = int.TryParse(ScanFrequency_TextBox.Text, out time);

                if (parsResult)
                {
                    scanTimer.Elapsed += (sender, args) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Result_TextBox.Text = "";
                            Result_TextBox.Text += "Host device info\r\n";
                            Result_TextBox.Text += "Device name: " + Dns.GetHostName() + "\r\n";
                            Result_TextBox.Text += "IP: " + Dns.GetHostAddresses(Dns.GetHostName())[0] + "\r\n";
                            Result_TextBox.Text += "\r\n";

                            Process netUtility = new();
                            netUtility.StartInfo.FileName = "arp.exe";
                            netUtility.StartInfo.CreateNoWindow = true;
                            netUtility.StartInfo.Arguments = "-a";
                            netUtility.StartInfo.RedirectStandardOutput = true;
                            netUtility.StartInfo.UseShellExecute = false;
                            netUtility.StartInfo.RedirectStandardError = true;
                            netUtility.Start();

                            
                            StreamReader streamReader = new(netUtility.StandardOutput.BaseStream, netUtility.StandardOutput.CurrentEncoding);

                            string? line;
                            while ((line = streamReader.ReadLine()) != null)
                            {
                                if (line.StartsWith("  "))
                                {
                                    var ipInfoEntrie = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                                    if (ipInfoEntrie.Length == 3)
                                    {
                                        int firstPartOfIp = int.Parse(ipInfoEntrie[0].Split('.')[0]);
                                        int lastPartOfIp = int.Parse(ipInfoEntrie[0].Split('.')[3]);

                                        if (255 > firstPartOfIp && firstPartOfIp > 1 && lastPartOfIp == 192)
                                        {
                                            try
                                            {
                                                Result_TextBox.Text += "Device info\r\n";
                                                Result_TextBox.Text += "IP: " + ipInfoEntrie[1] + "\r\n";
                                                Result_TextBox.Text += "Device name:" + Dns.GetHostEntry(ipInfoEntrie[0].Trim()).HostName + "\r\n";
                                                Result_TextBox.Text += "\r\n";
                                            }
                                            catch (Exception e)
                                            {
                                                Result_TextBox.Text += ipInfoEntrie[0] + " - " + e.Message + "\r\n";
                                            }
                                        }
                                    }
                                    
                                }
                            }

                            streamReader.Close();

                            Result_TextBox.Text += $"________________________________\r\n";
                            Result_TextBox.Text += $"Result recived at: {DateTime.Now.ToString("h:mm:ss tt")}";
                        });
                    };
                    scanTimer.AutoReset = true;
                    scanTimer.Interval = time * 1000;
                    scanTimer.Start();
                    Button.Content = "Stop Scan";
                    ScanFrequency_TextBox.IsEnabled = false;
                }
            }
            else
            {
                isScaning = true;
                scanTimer.Stop();
                Button.Content = "Start Scan";
                ScanFrequency_TextBox.IsEnabled = true;
            }
        }
    }
}
