using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using Microsoft.Win32;


using System.Diagnostics;

using System.Drawing;
using TesseractConsole;
using Tesseract;

namespace OCRCapture
{



    /// <summary>
    /// Interaction logic for S3TransferWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //public const int FIVE_MINUTES = 5 * 60 * 1000;
        private const string TESS_WHITE_CHARECTERS = "";

        //TransferUtility _transferUtility;

        string _bucket;
        string _uploadFile;
        string _uploadDirectory;
        string _recognizedText;
        string _fileName;

        #region Initialization
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            loadConfiguration();
        }

        /// <summary>
        /// This method loads the access keys that are set in the App.config and creates the transfer utility.
        /// </summary>
        private void loadConfiguration()
        {
            NameValueCollection appConfig = ConfigurationManager.AppSettings;
            
            //this.TesseractInstalDir = appConfig["tesseractInstalationPath"];
            //UploadFile = @"C:\Temp\q.png";
        }
        #endregion

        #region Bound Properties

        

        public string UploadFile
        {
            get { return this._uploadFile; }
            set
            {
                this._uploadFile = value;
                this.notifyPropertyChanged("UploadFile");
            }
        }


        public string RecognizedText
        {
            get { return this._recognizedText; }
            set
            {
                this._recognizedText = value;
                this.notifyPropertyChanged("RecognizedText");
            }
        }


        
        #endregion

        #region Button Click Event Handlers
        private void browseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                this.UploadFile = dlg.FileName;
                _fileName = dlg.SafeFileName;
            }
        }

        private void upload_Click(object sender, RoutedEventArgs e)
        {
            // Ensure the progress bar is empty.
            resetProgressBars();
            updateIsEnabled(this._ctlUploadFile, false);
            
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.threadedUploadFile));
            //RecognizedText =  TransferPictureToText();
            
            

            //updateIsEnabled(this._ctlRecognizedText, true);
            //this.threadedUploadFile(null);
        }

        
        #endregion

        #region S3 Upload Call

        /// <summary>
        /// This method is called in a background thread so as not to block the UI as the upload is 
        /// going.
        /// </summary>
        /// <param name="state">unused</param>
        private void threadedUploadFile(object state)
        {
            try
            {

                RecognizedText = TransferPictureToText();

                //displayMessageBox(string.Format("Recognize file completed!\n {0}", RecognizedText), "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception e)
            {
                displayMessageBox(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                updateIsEnabled(this._ctlUploadFile, true);
            }
        }

        private string TransferPictureToText()
        {
            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {
                engine.DefaultPageSegMode = PageSegMode.CircleWord;
                engine.SetVariable("tessedit_char_whitelist", "AU-0123456789");
                using (var img = Pix.LoadFromFile(UploadFile))
                {
                    using (var page = engine.Process(img))
                    {
                        var text = page.GetText();
                        var words = text.Replace("\n", "").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        return words[0];

                    }
                }
            }


                    //return "problem with image recognization";
        }
        

        

                #endregion

        #region Upload Event Callbacks

        // This gets called as bytes are written to the request stream.  The sender is the TransferUtilityUploadRequest
        // that was used to start the upload. The UploadProgressArgs contains the total bytes to be transferred and how many bytes 
        // have already been transferred.


        // This gets called as bytes are written to the request stream.  The sender is the TransferUtilityUploadDirectoryRequest
        // that was used to start the upload. The UploadDirectoryProgressArgs contains the total number of files that will be upload, 
        // how many files have been upload so far, total number of bytes to be transferred for the current file being upload and
        // how many bytes have been upload so far for the current file being uploaded.


        #endregion

        #region UI Utility Methods
        private void updateProgressBar(ProgressBar bar, long min, long max, long value, TextBlock label, string labelPostFix, string filepath)
        {
            bar.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new DispatcherOperationCallback(delegate
                {
                    bar.Minimum = min;
                    bar.Maximum = max;
                    bar.Value = value;

                    if (label != null)
                    {
                        string labelText = string.Format("{0} / {1} {2}",
                            value == int.MinValue ? "0" : value.ToString("#,0"),
                            max == int.MaxValue ? "0" : max.ToString("#,0"),
                            labelPostFix);

                        if (!string.IsNullOrEmpty(filepath))
                        {
                            labelText += string.Format(" ({0})", new FileInfo(filepath).Name);
                        }

                        label.Text = labelText;
                    }
                    return null;
                }), null);
        }

        private void updateIsEnabled(Control btn, bool enabled)
        {
            btn.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new DispatcherOperationCallback(delegate
                {
                    btn.IsEnabled = enabled;
                    return null;
                }), null);
        }

        private void resetProgressBars()
        {
            //updateProgressBar(this._ctlDirectoryCurrentFileProgressBar, int.MinValue, int.MaxValue, int.MinValue,
            //    this._ctlRecognizedText, "", null);
            /*updateProgressBar(this._ctlDirectoryFileProgressBar, int.MinValue, int.MaxValue, int.MinValue,
                this._ctlNumberOfFilesLabel, "Files", null);
            updateProgressBar(this._ctlDirectoryCurrentFileProgressBar, int.MinValue, int.MaxValue, int.MinValue,
                this._ctlCurrentFilesTransferLabel, "Bytes", null);*/
        }

        private void displayMessageBox(string message, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new DispatcherOperationCallback(delegate
                {
                    MessageBox.Show(this, message, caption, button, image);
                    return null;
                }), null);
        }




        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void notifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

}