using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;
using Microsoft.Win32;

namespace MyMediaPlayerApp
{
    /// <summary>
    /// Interaction logic for MediaPlayerControl.xaml
    /// </summary>
    public partial class MediaPlayerControl : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private string FileName { get; set; }
        private bool isDragging = false;

        public MediaPlayerControl()
        {
            //InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += timer_Tick;
            timer.Start();

        }

        /// <summary>
        /// Contains actions to be performed between two Timer ticks.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                if (mediaPlayer.Source != null)
                    lblStatus.Content = String.Format("{0} / {1}", mediaPlayer.Position.ToString(@"mm\:ss"), mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                else
                    lblStatus.Content = "No File Selected";

                if (!isDragging)
                    audioSlider.Value = mediaPlayer.Position.TotalSeconds;
            }
            else
                lblStatus.Content = "File does not hava a Time Span, man!";
        }

        /// <summary>
        /// Play Button Click Event Handler. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
            selectAudioFile.Content = String.Format("Now Playing... {0}", FileName);
        }

        /// <summary>
        /// Pause Button Click Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }

        /// <summary>
        /// Stop Button Click Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }

        /// <summary>
        /// Select Audio File Handler 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectAudioFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            WavFileInfo wavFileInfo = null;
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3| WAV files (*.wav)|*.wav| All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                mediaPlayer.Open(new Uri(openFileDialog.FileName));
                mediaPlayer.MediaOpened += mediaPlayer_MediaOpened;
                FileName = openFileDialog.FileName;
                string fileExtension = System.IO.Path.GetExtension(FileName);
                if (fileExtension == ".wav")
                    wavFileInfo = ReadWAVFile(FileName);
            }
        }

        /// <summary>
        /// Reads the wav file and gets fileInfo and data information
        /// </summary>
        /// <param name="fileName"></param>
        private WavFileInfo ReadWAVFile(string fileName)
        {
            WavFileInfo fileInfo = new WavFileInfo();
            // Getting the data from the file path in binary format.
            FileStream wavFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(wavFileStream);
            byte[] fileBytes = File.ReadAllBytes(fileName);

            // Reading the wav file fileInfo and extracting important information. 
            // The Wav File Header size is 44 bytes.
            int chunkID = reader.ReadInt32();
            int chunkSize = reader.ReadInt32();
            int Format = reader.ReadInt32();
            int formatID = reader.ReadInt32();
            int formatSize = reader.ReadInt32();
            int audioEncodingFormat = reader.ReadInt16();
            int numChannels = reader.ReadInt16();                               // Number of channels: 1-Mono, 2-Stereo
            int sampleRate = reader.ReadInt32();                                // Sampling Rate - No of Samples/sec
            int byteRate = reader.ReadInt32();
            int blockAlign = reader.ReadInt16();
            int bitsPerSample = reader.ReadInt16();                             // Bits per Sample
            if (formatSize == 18)
            {
                // Read extra values
                int formatExtraSize = reader.ReadInt16();
                reader.ReadBytes(formatExtraSize);
            }
            int dataID = reader.ReadInt32();
            int dataSize = reader.ReadInt32();                                  // Audio data size
            byte[] wavData = reader.ReadBytes(dataSize);                        // Contains actual data 

            // Convert byte data to double
            reader.Read();

            // Populate Wav fileInfo object properties and return it
            fileInfo.SampleRate = sampleRate;
            fileInfo.BitsPerSample = bitsPerSample;
            fileInfo.NumChannels = numChannels;
            fileInfo.WavData = wavData;
            return fileInfo;

        }

        /// <summary>
        /// Contains handler code for when the Media Player Open call is made.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            TimeSpan timeSpan = mediaPlayer.NaturalDuration.TimeSpan;
            audioSlider.Maximum = timeSpan.TotalSeconds;
            audioSlider.LargeChange = Math.Min(10, timeSpan.Seconds / 10);
        }

        /// <summary>
        /// Decides what to do when the audioSlider has just been dragged. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void audioSlider_DragStarted(object sender, RoutedEventArgs e)
        {
            isDragging = true;
        }

        /// <summary>
        /// Contains code to handle when the audioSlider Drag event is completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void audioSlider_DragCompleted(object sender, RoutedEventArgs e)
        {
            isDragging = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(audioSlider.Value);
        }
    }

    internal class WavFileInfo
    {
        internal int SampleRate { get; set; }
        internal int BitsPerSample { get; set; }
        internal int NumChannels { get; set; }
        internal byte[] WavData { get; set; }
    }
}
