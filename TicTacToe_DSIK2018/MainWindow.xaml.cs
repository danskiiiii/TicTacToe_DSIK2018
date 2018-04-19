using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace TicTacToe_DSIK2018
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int scoreO, scoreX, turn=1;
        private Thread listenThread;
        string playerID = string.Empty;

        // static IPAddress address = IPAddress.Parse("150.254.78.29");
        byte[] data;
        UdpClient server;
        IPEndPoint serverIpAndPort;

        public MainWindow()
        {
            this.InitializeComponent();            

            yourMoveLabel.Visibility = Visibility.Collapsed;
            waitLabel.Visibility = Visibility.Visible;
            getFileButton.IsEnabled = false;
        }

       
        //method for determining if one of the players won the round
        private void IsRoundOver(string btnContent)
        {
            if (((button1.Content).ToString() == btnContent & button2.Content.ToString() == btnContent &
                 button3.Content.ToString() == btnContent)
               | (button1.Content.ToString() == btnContent & button4.Content.ToString() == btnContent &
                 button7.Content.ToString() == btnContent)
               | (button1.Content.ToString() == btnContent & button5.Content.ToString() == btnContent &
                 button9.Content.ToString() == btnContent)
               | (button2.Content.ToString() == btnContent & button5.Content.ToString() == btnContent &
                 button8.Content.ToString() == btnContent)
               | (button3.Content.ToString() == btnContent & button6.Content.ToString() == btnContent &
                 button9.Content.ToString() == btnContent)
               | (button4.Content.ToString() == btnContent & button5.Content.ToString() == btnContent &
                 button6.Content.ToString() == btnContent)
               | (button7.Content.ToString() == btnContent & button8.Content.ToString() == btnContent &
                 button9.Content.ToString() == btnContent)
               | (button3.Content.ToString() == btnContent & button5.Content.ToString() == btnContent &
                 button7.Content.ToString() == btnContent))
            {
                if (btnContent == "O")
                {
                    MessageBox.Show("Player O WINS");
                    playerO_currentScoreLabel.Content = ++scoreO;
                }
                else if (btnContent == "X")
                {
                    MessageBox.Show("Player X WINS");
                    playerX_currentScoreLabel.Content = ++scoreX;
                }
                ResetButtons();
            }

            else
            {
                foreach (Button btn in mainWrapPanel.Children)
                {
                    if (btn.IsEnabled == true)
                        return;
                }
                MessageBox.Show("GAME OVER, No Winner");
                ResetButtons();
            }
        }

        private void ResetButtons()
        {
            foreach (Button btn in mainWrapPanel.Children)
            {
                btn.Content = "";
                btn.IsEnabled = true;
            }
        }
        //logic behind main Wrap Panel buttons
        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (turn == 1)
            {
                btn.Content = "O";
                turnLabel.Content = "X";
            }
            else
            {
                btn.Content = "X";
                turnLabel.Content = "O";
            }

            btn.IsEnabled = false;

            data = Encoding.ASCII.GetBytes(btn.Name + ' ' + btn.Content);
            server.Send(data, data.Length);

            IsRoundOver(Convert.ToString(btn.Content));

            turn += 1;
            if (turn > 2)
                turn = 1;

            yourMoveLabel.Visibility = Visibility.Collapsed;
            waitLabel.Visibility = Visibility.Visible;
        }

        // listener method for awaiting server packets
        private void StartListening()
        {
            try
            {
                while (true)
                {
                    data = this.server.Receive(ref serverIpAndPort);
                    this.Dispatcher.Invoke(() =>
                    {
                        string temp = Encoding.ASCII.GetString(data, 0, data.Length);
                        string[] lines = Regex.Split(temp, " ");

                        //determines in-game actions based on data from server
                        foreach (Button bttn in mainWrapPanel.Children)
                        {
                            if (bttn.Name == lines[0]
                                && Convert.ToString(lines[1][0]) != playerID)
                            {
                                bttn.Content = Convert.ToString(lines[1][0]);
                                bttn.IsEnabled = false;
                                IsRoundOver(Convert.ToString(lines[1][0]));
                                turn += 1;
                                if (turn > 2)
                                {
                                    turn = 1;
                                }
                                yourMoveLabel.Visibility = Visibility.Visible;
                                waitLabel.Visibility = Visibility.Collapsed;
                                turnLabel.Content = playerID;
                            }
                        }

                        //determines sides (X and O) and who starts in  first turn
                        if (lines[0][0] == 'X')
                        {
                            turn = 1;
                            playerID = "X";
                            playerX_scoreLabel.Content = "ME(X)";
                            playerO_scoreLabel.Content = "O";
                            turnLabel.Content = "O";
                            yourMoveLabel.Visibility = Visibility.Collapsed;
                            waitLabel.Visibility = Visibility.Visible;
                            getFileButton.IsEnabled = true;

                        }
                        if (lines[0][0] == 'O')
                        {
                            turn = 1;
                            playerID = "O";
                            playerO_scoreLabel.Content = "ME(O)";
                            playerX_scoreLabel.Content = "X";
                            turnLabel.Content = "O";
                            yourMoveLabel.Visibility = Visibility.Visible;
                            waitLabel.Visibility = Visibility.Collapsed;
                            getFileButton.IsEnabled = true;
                        }

                    });
                }
            }
            catch (Exception ex) {
                this.listenThread = new Thread(new ThreadStart(this.StartListening));
                this.listenThread.Start();
            }
        }
        //static method for processing data from byte format into image format
        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        // logic for file download button
        private void GetFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                data = Encoding.ASCII.GetBytes("getfile");
                server.Send(data, data.Length);
                Thread.Sleep(2000);

                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(LoadImage(data)));

                using (var fileStream = new System.IO.FileStream("data.png", System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
                Process.Start("data.png");

            }
            catch (Exception ex) { MessageBox.Show("Panda not found, try again later"); }
        }


        // logic for new game/server connection button
        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (server != null) { server.Close(); listenThread.Abort(); } 
            server = new UdpClient(serverIpTextBox.Text, Convert.ToInt32(serverPortTextBox.Text));
            serverIpAndPort = new IPEndPoint(IPAddress.Any, 0);

            this.listenThread = new Thread(new ThreadStart(this.StartListening));
            this.listenThread.Start();

            data = Encoding.ASCII.GetBytes("ready");
            server.Send(data, data.Length);

            scoreO = 0; scoreX = 0;
            playerO_currentScoreLabel.Content = scoreO;
            playerX_currentScoreLabel.Content = scoreX;
            ResetButtons();
        }
    }
}