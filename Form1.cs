using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace chatClient
{
    public partial class Form1 : Form
    {
        Socket sck;
        EndPoint epLocal, epRemote;
        public Form1()
        {
            InitializeComponent();

            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            textLocalip.Text = GetLocalip(); //Sets local ip into textbox
            textOtherip.Text = GetLocalip();
        }

        private string GetLocalip()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach(IPAddress ip in host.AddressList)
            {
                if(ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString(); //return local ip address as string
                }
            }
            return "127.0.0.1";
        }

        private void MessageCallBack(IAsyncResult aResult) 
        {
            try
            {
                int size = sck.EndReceiveFrom(aResult, ref epRemote);
                if(size >0) //check if there is info coming in or not
                {
                    byte[] receivedData = new byte[1464];
                    receivedData = (byte[])aResult.AsyncState;

                    ASCIIEncoding eEncoding = new ASCIIEncoding();
                    string receivedMessage = eEncoding.GetString(receivedData); //Collect a string variable from message

                    listMessage.Items.Add("User 2: "+receivedMessage); //string message added to list section of page
                }

                byte[] buffer = new byte[1500]; //buffer is 1500 long message is in bytes
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote,
                    new AsyncCallback(MessageCallBack), buffer);
            }
            catch(Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e) //Starting connection function
        {
            try
            {
                epLocal = new IPEndPoint(IPAddress.Parse(textLocalip.Text), Convert.ToInt32(textLocalPort.Text));
                sck.Bind(epLocal);

                epRemote = new IPEndPoint(IPAddress.Parse(textOtherip.Text), Convert.ToInt32(textOtherPort.Text));
                sck.Connect(epRemote);

                byte[] buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);

                button1.Text = "Connected";
                button1.Enabled = false;
                button2.Enabled = true;
                textMessage.Focus(); //sets focus on message box once connection is established
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e) //Send button function
        {
            try
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                byte[] msg = new byte[1500];
                msg = enc.GetBytes(textMessage.Text);

                sck.Send(msg); //Sends message to client

                listMessage.Items.Add("You: " + textMessage.Text); //displays chat of yourself in list
                textMessage.Clear();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        
    }
}  
