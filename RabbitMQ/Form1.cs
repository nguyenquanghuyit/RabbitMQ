using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RabbitMQ
{
    public partial class frmRabbitMQ : Form
    {
        string exchangeName = "RabbitHOB";
        string clientId = Guid.NewGuid().ToString();
        IConnection connection = null;
        IModel channelSend = null;
        IModel channelReceive = null;
        public frmRabbitMQ()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string input = txtUserName.Text + " > " + txtMessage.Text;
            byte[] message = Encoding.UTF8.GetBytes(input);
            channelSend.BasicPublish(exchangeName, "", null, message);
            txtMessage.Text = string.Empty;
            txtMessage.Focus();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (channelSend.IsOpen) channelSend.Close();
            if (channelReceive.IsOpen) channelReceive.Close();
            if (connection.IsOpen) connection.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtUserName.Text = clientId.Substring(0, 13);
            txtConversation.Text = "Welcome!\r\n";
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
            };

            connection = connectionFactory.CreateConnection();
            channelSend = connection.CreateModel();
            channelSend.ExchangeDeclare(exchangeName, ExchangeType.Fanout, false, true, null);
            channelReceive = connection.CreateModel();
            channelReceive.QueueDeclare(clientId, false, false, true, null);
            channelReceive.QueueBind(clientId, exchangeName, "");
            var consumer = new EventingBasicConsumer(channelReceive);
            channelReceive.BasicConsume(clientId, true, consumer);
            consumer.Received += (s, ee) =>
            {
                string message = Encoding.UTF8.GetString(ee.Body) + "\r\n";
                txtConversation.BeginInvoke(new Action(() =>
                {
                    txtConversation.Text += message;
                    ScrollToEnd(txtConversation);
                }));
            };
            txtMessage.Focus();
        }

        public void ScrollToEnd(TextBox textbox)
        {
            textbox.Select(textbox.Text.Length - 1, 0);
            textbox.ScrollToCaret();
        }
    }
}
