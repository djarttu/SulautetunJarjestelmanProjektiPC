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
using System.Timers;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        int i, j,k;
        string address;
        string hostname;
        string password;
        string username;
        
        
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
            Nayta nayta = new Nayta(hostname, username, password);
            
            
            
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
                                    client.Download("/home/pi/Riistakamerakuvat/" + Nayta.GetFileName(i), localFile);
                                }
                            }
                            
                        }
                    }
                }
            }
            button6.Enabled = true;
            k = j;
            pictureBox1.Image = Image.FromFile("C:/Temp/"+k+".jpg");
           

        }

        

        private void button4_Click(object sender, EventArgs e)
        {
            SshClient sshclient = new SshClient(hostname, username, password);
            {
                sshclient.Connect();
                if (sshclient.IsConnected)
                {
                    Console.WriteLine("ssh aukastu");
                    sshclient.CreateCommand("sudo killall -9 python3").Execute();
                    System.Threading.Thread.Sleep(2000);
                    //sshclient.CreateCommand("raspivid -o - -t 0 -hf -w 800 -h 400 -fps 24 |cvlc -vvv stream:///dev/stdin --sout '#standard{access=http,mux=ts,dst=:8160}' :demux=h264").BeginExecute();
                    sshclient.CreateCommand("cvlc v4l2:///dev/video0 --v4l2-width 800 --v4l2-height 600 --sout '#transcode{vcodec = h264, fps=10}: standard{access=http,mux=ts,mime=video/ts, dst=0.0.0.0:8160}' -vvv").BeginExecute();
                    sshclient.Disconnect();
                    sshclient.Dispose();
                    System.Threading.Thread.Sleep(5000);
                    myProcess.openVlc();
                    
                    
                    




                        Console.WriteLine("kello kulunut");
                    // Configure the process using the StartInfo properties.
                    
                    
                    Console.WriteLine("video streami aukastu raspilta");
                        
                        
                    
                   // 
                }
            }
            
        }
        
        /*
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
        */
        private void timer1_Tick(object sender, EventArgs e)
        {
            /*
            timer1.Stop();
            myProcess.closeVlc();
            */
        }

        private void button6_Click(object sender, EventArgs e)
        {
            
            if(k>1)
            {
                k--;
                pictureBox1.Image.Dispose();
                pictureBox1.Image = Image.FromFile("C:/Temp/" + k + ".jpg");
               
            }
            button5.Enabled = true;
            if (k == 1)
                button6.Enabled = false;
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            if(k!=j)
            {
                k++;
                pictureBox1.Image.Dispose();
                pictureBox1.Image = Image.FromFile("C:/Temp/" + k + ".jpg");
                
            }
            if (k == j)
                button5.Enabled = false;
            button6.Enabled = true;
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
            button1.Enabled = true;
            button4.Enabled = true;

        }
    }
}


public class Nayta
{
    private static string[] names= new string[100];
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
                var files = sftp.ListDirectory("/home/pi/Riistakamerakuvat/");

                foreach (var file in files)
                {
                    string remoteFileName = file.Name;
                   
                    if (remoteFileName.Contains("jpg"))
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
    private static System.Timers.Timer aika;
    
    //private Process process;
    Process process = new Process();
    public string Hostname { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public void openVlc()
        {
            aika = new System.Timers.Timer(300000);
            aika.AutoReset = false;
            aika.Elapsed += process_Exited; ;
            aika.Start();
            process.StartInfo.FileName = "C:/Program Files/VideoLAN/VLC/vlc.exe";
            process.StartInfo.Arguments = "http://" + Hostname + ":8160/";
            
            process.Start();
        
            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(process_Exited);
                

        }

    private void Aika_Elapsed(object sender, ElapsedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void process_Exited(object sender, System.EventArgs e)
    {
        if (process.HasExited == false)
            process.Kill();
        aika.Stop();
        Console.WriteLine("vlc suljettu pitäs sulkia lähetys");
        SshClient sshclient = new SshClient(Hostname, Username, Password);
        {
            sshclient.Connect();
            if (sshclient.IsConnected)
            {

                sshclient.CreateCommand("pkill vlc").Execute();
                System.Threading.Thread.Sleep(10000);
                sshclient.CreateCommand("python3 /home/pi/Documents/serialportTest.py").Execute();
                sshclient.Disconnect();
                sshclient.Dispose();

            }
        }


    }
    public void closeVlc()
    {
        if(process.HasExited==false)
            process.Kill();
    }

}

 
