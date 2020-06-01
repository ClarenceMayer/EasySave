using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace EasySaveConsole
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        // Every protocol typically has a standard port number. For example, HTTP is typically 80, FTP is 20 and 21, etc.
        // For this example, we'll choose an arbitrary port number.
        static string outPortNumber = "1337";
        private string nameSelectedTask = null;

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.UpdateList();
            while (true)
            {
                await Task.Delay(4000);
                this.UpdateList();
                await Task.Delay(4000);
                if(this.nameSelectedTask != null)
                {
                    this.UpdateDataSave(this.nameSelectedTask);
                }
                
            }
            
        }

        private async void UpdateList()
        {
            try
            {
                // Create the StreamSocket and establish a connection to the echo server.
                using (var streamSocket2 = new Windows.Networking.Sockets.StreamSocket())
                {
                    // The server hostname that we will be establishing a connection to. In this example, the server and client are in the same process.
                    var hostName = new Windows.Networking.HostName(ip.Text);
                    await streamSocket2.ConnectAsync(hostName, MainPage.outPortNumber);
                    // Send a request to the server and update list.
                    string request = "UpdateSaveList";
                    using (Stream outputStream = streamSocket2.OutputStream.AsStreamForWrite())
                    {
                        using (var streamWriter = new StreamWriter(outputStream))
                        {
                            await streamWriter.WriteLineAsync(request);
                            await streamWriter.FlushAsync();
                        }
                    }

                    string response;
                    using (Stream inputStream = streamSocket2.InputStream.AsStreamForRead())
                    {
                        using (StreamReader streamReader = new StreamReader(inputStream))
                        {
                            this.SaveList.Items.Clear();
                            response = await streamReader.ReadLineAsync();
                            string[] responses = response.Split(',');
                            foreach (string rep in responses)
                            {
                                if(rep != "")
                                {
                                    this.SaveList.Items.Add(rep);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        private async void UpdateDataSave(string name)
        {
            try
            {
                // Create the StreamSocket and establish a connection to the echo server.
                using (var streamSocket = new Windows.Networking.Sockets.StreamSocket())
                {
                    // The server hostname that we will be establishing a connection to. In this example, the server and client are in the same process.
                    var hostName = new Windows.Networking.HostName("localhost");
                    await streamSocket.ConnectAsync(hostName, MainPage.outPortNumber);

                    // Send a request to the server and update list.
                    string request = name;
                    using (Stream outputStream = streamSocket.OutputStream.AsStreamForWrite())
                    {
                        using (var streamWriter = new StreamWriter(outputStream))
                        {
                            await streamWriter.WriteLineAsync(request);
                            await streamWriter.FlushAsync();
                        }
                    }

                    string response;
                    using (Stream inputStream = streamSocket.InputStream.AsStreamForRead())
                    {
                        using (StreamReader streamReader = new StreamReader(inputStream))
                        {
                            response = await streamReader.ReadLineAsync();
                            string[] responses = response.Split(',');
                            this.name.Text = "Task name : " + responses[0];
                            this.source.Text = "Source folder : " + responses[1];
                            this.target.Text = "Target folder : " + responses[2];
                            this.progressBar.Value = Convert.ToDouble(responses[3]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        private void SaveList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(!(this.SaveList.Items.Count() == 0))
            {
                this.UpdateDataSave(this.SaveList.Items[SaveList.SelectedIndex].ToString());
                this.nameSelectedTask = this.SaveList.Items[SaveList.SelectedIndex].ToString();
            }
            
        }

        private void Ip_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
