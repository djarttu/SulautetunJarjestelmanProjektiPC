using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.IO;
using System.Diagnostics;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        int i, j;
        string address;
        string hostname;
        string password;
        string username;
        //SshClient sshclient = new SshClient();
        VlcProcessClass myProcess = new VlcProcessClass();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (ScpClient client = new ScpClient(hostname, username, password))
            {
                client.Connect();
                if (client.IsConnected)
                {
                    j = Nayta.GetCounter() - 1;
                    if (j > 0)

                    {

                        for (int i = j; i >= 0; i--)
                        {
                            address = "C:/Temp/" + Nayta.GetFileName(i);
                            if (!File.Exists(address))
                            {
                                using (Stream localFile = File.Create(address))
                                {
                                    client.Download("/home/pi/Pictures/" + Nayta.GetFileName(i), localFile);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Nayta nayta = new Nayta(hostname, username, password);
            i = Nayta.GetCounter()-1;
            for(int j=i;j>=0;j--)
            {
                textBox1.Text = textBox1.Text + Nayta.GetFileName(j) + Environment.NewLine;

            }

            button1.Enabled = true;

        }

        private void button4_Click(object sender, EventArgs e)
        {
            SshClient sshclient = new SshClient(hostname, username, password);
            {
                sshclient.Connect();
                if (sshclient.IsConnected)
                {
                    Console.WriteLine("ssh aukastu");
                    sshclient.CreateCommand("raspivid -o - -t 0 -hf -w 800 -h 400 -fps 24 |cvlc -vvv stream:///dev/stdin --sout '#standard{access=http,mux=ts,dst=:8160}' :demux=h264").BeginExecute();
                    sshclient.Disconnect();
                    sshclient.Dispose();
                    myProcess.openVlc(hostname);
                    // Configure the process using the StartInfo properties.
                    
                    
                    Console.WriteLine("video streami aukastu raspilta");
                        
                        
                    
                   // 
                }
            }
            
        }
        

        private void button5_Click(object sender, EventArgs e)
        {
            SshClient sshclient = new SshClient(hostname, username, password);
            {
                sshclient.Connect();
                if (sshclient.IsConnected)
                {
                    //Console.WriteLine("ssh aukastu");
                    sshclient.CreateCommand("pkill vlc").Execute();
                    sshclient.Disconnect();
                    sshclient.Dispose();

                }
            }   
        }
        private void button3_Click(object sender, EventArgs e)
        {
            hostname = textBoxHost.Text;
            username = textBoxUsername.Text;
            password = textBoxPassword.Text;
            myProcess.Password = password;
            myProcess.Hostname = hostname;
            myProcess.Username = username;
            //ConnectionInfo connectionInfo = new ConnectionInfo(hostname, username, password);
            button2.Enabled = true;

        }
    }
}


public class Nayta
{
    private static string[] names= new string[60];
    //private int counter2 = 0;
    private static int counter=0;
    public Nayta(string host, string username, string password)
    {
        
        using (var sftp = new SftpClient(host, username, password))
        {
           
            sftp.Connect();
            if (!sftp.IsConnected)
                throw new Exception("Failed to connect scp");
            if (sftp.IsConnected)
            {
                //sftp.ChangeDirectory("/home/pi/");
                var files = sftp.ListDirectory("/home/pi/Pictures/");

                foreach (var file in files)
                {
                    string remoteFileName = file.Name;
                    if (remoteFileName.Contains("png"))
                    {
                        //System.Diagnostics.Debug.WriteLine(remoteFileName + " " + counter);
                        names[counter] = remoteFileName;
                        counter++;
                    }
                    
                }
                
                sftp.Disconnect();
            }
            
        }

       
    
       
    }
    public static int GetCounter()
    {
        
        return counter;
    }

    public static string GetFileName(int i)
    {
        return names[i];
    }

}
public class VlcProcessClass
    {
    //private Process process;
    public string Hostname { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public void openVlc(string hostname)
        {
        var process = new Process();
            
                process.StartInfo.FileName = "C:/Program Files/VideoLAN/VLC/vlc.exe";
                process.StartInfo.Arguments = "http://" + hostname + ":8160";
            //process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            process.Start();

            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(process_Exited);
                //process.WaitForExit();
            

        }
    private void process_Exited(object sender, System.EventArgs e)
    {
        Console.WriteLine("vlc suljettu pitäs sulkia lähetys");
        SshClient sshclient = new SshClient(Hostname, Username, Password);
        {
            sshclient.Connect();
            if (sshclient.IsConnected)
            {
                
                sshclient.CreateCommand("pkill vlc").Execute();
                sshclient.Disconnect();
                sshclient.Dispose();

            }
        }


    }

}

 
