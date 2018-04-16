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
        int scoreO, scoreX, turn;
        private Thread _listenThread;
        string whoAmI = string.Empty;

        // static IPAddress address = IPAddress.Parse("150.254.78.29");
        byte[] data;
        UdpClient server = new UdpClient("150.254.78.29", 4105);
        IPEndPoint serverIpAndPort = new IPEndPoint(IPAddress.Any, 0);

        public MainWindow()
            : base()
        {
            this.InitializeComponent();

            this._listenThread = new Thread(new ThreadStart(this.StartListening));
            this._listenThread.Start();

            yourMoveLabel.Visibility = Visibility.Collapsed;
            waitLabel.Visibility = Visibility.Visible;
            getFileButton.IsEnabled = false;


        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            turn = 1;
        }

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
                    MessageBox.Show("PLAYER O WINS");
                    playerO_currentScoreLabel.Content = ++scoreO;
                }
                else if (btnContent == "X")
                {
                    MessageBox.Show("PLAYER X WINS");
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
                MessageBox.Show("GAME OVER NO ONE WINS");
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

                        foreach (Button bttn in mainWrapPanel.Children)
                        {
                            if (bttn.Name == lines[0]
                                && Convert.ToString(lines[1][0]) != whoAmI)
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
                                turnLabel.Content = whoAmI;
                            }
                        }

                        //determine starting player
                        if (lines[0][0] == 'X')
                        {
                            turn = 1;
                            whoAmI = "X";
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
                            whoAmI = "O";
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
                MessageBox.Show(ex.Message);
                this._listenThread = new Thread(new ThreadStart(this.StartListening));
                this._listenThread.Start();
            }
        }

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

        private void GetFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                data = Encoding.ASCII.GetBytes("getfile");
                server.Send(data, data.Length);
                Thread.Sleep(2500);

                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(LoadImage(data)));

                using (var fileStream = new System.IO.FileStream("data.png", System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
                Process.Start("data.png");

            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            data = Encoding.ASCII.GetBytes("ready");
            server.Send(data, data.Length);

            scoreO = 0; scoreX = 0;
            playerO_currentScoreLabel.Content = scoreO;
            playerX_currentScoreLabel.Content = scoreX;
            ResetButtons();
            // server.Close();  
        }
    }
}