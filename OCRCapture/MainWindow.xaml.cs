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

namespace OCRCapture
{



    /// <summary>
    /// Interaction logic for S3TransferWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public const int FIVE_MINUTES = 5 * 60 * 1000;

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
            
            this.TesseractInstalDir = appConfig["tesseractInstalationPath"];
            //UploadFile = @"C:\Temp\q.png";
        }
        #endregion

        #region Bound Properties

        public string Bucket
        {
            get { return this._bucket; }
            set
            {
                this._bucket = value;
                this.notifyPropertyChanged("Bucket");
            }
        }

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


        public string TesseractInstalDir
        {
            get { return this._uploadDirectory; }
            set
            {
                this._uploadDirectory = value;
                this.notifyPropertyChanged("UploadDirectory");
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
            this.notifyPropertyChanged("UploadFile");
            //ThreadPool.QueueUserWorkItem(new WaitCallback(this.threadedUploadFile));
            //RecognizedText =  TransferPictureToText();
            RecognizedText = TransferPictureToText1();
            this.notifyPropertyChanged("RecognizedText");

            //updateIsEnabled(this._ctlRecognizedText, true);
            //this.threadedUploadFile(null);
        }

        /*private void browseDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "Select a folder to upload.";
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            System.Windows.Forms.DialogResult result = dlg.ShowDialog(new WindowWrapper(source.Handle));
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.UploadDirectory = dlg.SelectedPath;
            }
        }*/

        /*private void uploadDirectory_Click(object sender, RoutedEventArgs e)
        {
            resetProgressBars();
            updateIsEnabled(this._ctlUploadDirectory, false);

            ThreadPool.QueueUserWorkItem(new WaitCallback(this.threadedUploadDirectory));
        }*/
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

                //try
                //{
                //    // Make sure the bucket exists
                //    this._transferUtility.S3Client.EnsureBucketExists(this.Bucket);
                //    //this._transferUtility.S3Client.PutBucket(new PutBucketRequest() { BucketName = this.Bucket });
                //}
                //catch(Amazon.S3.AmazonS3Exception ex)
                //{
                //    if(ex.ErrorCode!= "BucketAlreadyOwnedByYou")
                //        displayMessageBox(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //}

                //TransferUtilityUploadRequest request = new TransferUtilityUploadRequest()
                //{
                //    BucketName = this.Bucket,
                //    FilePath = this.UploadFile
                //};
                //request.UploadProgressEvent += this.uploadFileProgressCallback;

                //this._transferUtility.Upload(request);

                displayMessageBox(string.Format("Recognize file completed!\n {0}", RecognizedText), "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private static int count = 0;

        private string TransferPictureToText1()
        {
            const string language = "eng";
            const string TessractData = @"C:\temp\tessdata\";

            Ocr ocr = new Ocr();
            using (Bitmap bmp = new Bitmap(UploadFile))
            {
                tessnet2.Tesseract tessocr = new tessnet2.Tesseract();
                tessocr.SetVariable("tessedit_char_whitelist", "0123456789"); // If digit only
                tessocr.SetVariable("-psm", "9"); // If digit only
                try
                {
                    tessocr.Init(@"C:\Users\vladi\Documents\visual studio 2015\Projects\OCRCapture\OCRCapture\bin\Debug", "eng",true);
                }
                catch(Exception ex)
                {

                }
                List<tessnet2.Word> result = tessocr.DoOCR(bmp, System.Drawing.Rectangle.Empty);
                foreach (tessnet2.Word word in result)
                    Console.WriteLine("{0} : {1}", word.Confidence, word.Text);


            }
            return "problem with image recognization";
        }
        private string TransferPictureToText()
        {
            //Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();


            // Stop the process from opening a new window
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            // Setup executable and parameters
            startInfo.FileName = this.TesseractInstalDir;
            //process.StartInfo.Arguments = string.Format(@" {0} {1}\out",UploadFile,Environment.SpecialFolder.ApplicationData );
            string outFile = System.IO.Path.GetTempPath() + string.Format(@"\{0}.tmp", _fileName);
            startInfo.Arguments = string.Format(@" {0} {1} -psm 9 digits", UploadFile, outFile);

            //displayMessageBox(string.Format("Process {0} file ", UploadFile), "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            // Go

            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }


            if (true)
            {
                //return (count++).ToString();//
                string fileText = System.IO.File.ReadAllText(outFile + ".txt");
                return string.Format("AU-{0}", fileText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]);

                //using (var connection = new MySqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=TestDb;Uid=root;Pwd=;"))
                //using (var command = connection.CreateCommand())
                //{
                //    connection.Open();
                //    command.CommandText = string.Format("insert into Table (chest_number,status) values ({0},'finished')  ", int.Parse(RecognizedText));

                //    using (var reader = command.ExecuteReader())
                //        while (reader.Read())
                //            Console.WriteLine(reader.GetString(0) + ": " + reader.GetString(1));
                //}


            }
            return "xcvxcvxcvx";
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            string outFile = System.IO.Path.GetTempPath() + @"\out";

        }

        /// <summary>
        /// This method is called in background thread so as not to block the UI as the upload is 
        /// going.
        /// </summary>
        /// <param name="state">unused</param>
        /*private void threadedUploadDirectory(object state)
        {
            try
            {
                // Make sure the bucket exists
                this._transferUtility.S3Client.PutBucket(new PutBucketRequest() { BucketName = this.Bucket });

                TransferUtilityUploadDirectoryRequest request = new TransferUtilityUploadDirectoryRequest()
                {
                    BucketName = this.Bucket,
                    Directory = this.UploadDirectory,
                    SearchOption = SearchOption.AllDirectories
                };
                request.UploadDirectoryProgressEvent += this.uploadDirectoryProgressCallback;
                this._transferUtility.UploadDirectory(request);

                displayMessageBox("Completed directory upload!", "Success", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                updateIsEnabled(this._ctlUploadDirectory, true);
            }
        }*/
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