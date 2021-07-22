using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Windows.Forms;

namespace SubtitleAttacher
{
    public partial class Main : Form
    {
        // Visibility options
        private bool FormVisible = false;
        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(FormVisible ? value : FormVisible);
        }

        // Main code
        private const string AppName = "Subtitle Attacher";
        private const string PipeName = "SubtitleAttacher.Pipe";
        private string[] videoFormats = new string[] { ".avi", ".mp4", ".flv", ".mkv", ".mpeg", ".m4p", ".m4v", ".mpg", ".webm", ".vob", ".ogv", ".ogg", ".drc", ".gifv", ".mng", ".mts", ".m2ts", ".ts", ".mov", ".qt", ".wmv", ".yuv", ".rm", ".rmvb", ".viv", ".asf", ".amv", ".mp2", ".mpe", ".mpv", ".m2v", ".m4v", ".svi", ".3gp", ".3g2", ".mxf", ".roq", ".nsv", ".flv", ".f4v", ".f4p", ".f4a", ".f4b", ".m4a" };
        private NotifyIcon icon = new NotifyIcon() { Visible = true, Icon = SystemIcons.Information };
        public Main()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();

            // In this case we have file name or directory name that is too long
            // or for other reason can't be passed as one parameter
            // thus we concatenate all leading parameters into one
            if (args.Length > 2)
            {
                string combinedArgs = string.Join(" ", args, 1, args.Length - 1);
                args = new string[] { args[0], combinedArgs };
            }

            // Processing the parameters, we need exactly two - executing dll and file full path
            if (args.Length == 2)
            {
                if (!string.IsNullOrEmpty(args[1]))
                {
                    var mutex = new System.Threading.Mutex(false, "AttacherMutex");
                    bool isMutexAcquired = mutex.WaitOne(0, true);
                    if (!isMutexAcquired)
                    {
                        var pipe = new NamedPipeServerStream(PipeName, PipeDirection.InOut);
                        mutex.Close();
                        pipe.WaitForConnection();
                        if (pipe.IsConnected && pipe.CanRead)
                        {
                            using (StreamReader sr = new StreamReader(pipe))
                            {
                                string buffer = sr.ReadLine();
                                AttachSubtitleToVideo(args[1], buffer);
                            }
                        }
                    }
                    else
                    {
                        var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.None);
                        try
                        {
                            pipe.Connect(300);
                        }
                        catch
                        {
                            icon.ShowBalloonTip(1000, AppName, "Single file selected. Two files are expected.", ToolTipIcon.Error);
                        }
                        if (pipe.IsConnected)
                        {
                            var pipeWriter = new StreamWriter(pipe);
                            pipeWriter.WriteLine(args[1]);
                            pipeWriter.Flush();
                            pipeWriter.Close();
                            pipeWriter.Dispose();
                        }
                        pipe.Close();
                        pipe.Dispose();
                        mutex.Close();
                    }
                }
                else
                {
                    icon.ShowBalloonTip(1000, AppName, "Critical error! No file provided!", ToolTipIcon.Error);
                }
            }
            icon.Visible = false; // because disposing the icon directly sometimes glitches the taskbar
            icon.Dispose();

            // There is an issue with closing the application properly that I assume is related to how forms work
            // and perhaps connected to overriding method SetVisibleCore.
            // It gets to this line but then Application.Exit() and this.Close() refuse to close it.
            // Tested in debugging with fake data, as well as with actual files.
            // Anyway, we freed all resources earlier so it shouldn't be a problem.
            Process.GetCurrentProcess().Kill();
        }
        private void AttachSubtitleToVideo(string file1, string file2)
        {
            if (string.IsNullOrEmpty(file1) || string.IsNullOrEmpty(file2)) return;

            string videoFile = null;
            string subtitleFile = null;
            if (IsFileVideo(file1))
            {
                videoFile = file1;
                subtitleFile = file2;
            }
            else if (IsFileVideo(file2))
            {
                videoFile = file2;
                subtitleFile = file1;
            }
            else
            {
                icon.ShowBalloonTip(1000, AppName, "No video file found.", ToolTipIcon.Info);
                return;
            }

            string videoFolder = Path.GetDirectoryName(videoFile);
            string videoNameWithoutExt = Path.GetFileNameWithoutExtension(videoFile);
            string subtitleExtension = Path.GetExtension(subtitleFile);
            string newSubtitlePath = Path.Combine(videoFolder, videoNameWithoutExt) + subtitleExtension;

            if (!File.Exists(newSubtitlePath))
            {
                File.Move(subtitleFile, newSubtitlePath);
            }
            else
            {
                icon.ShowBalloonTip(1000, AppName, "File with the video name already exists.", ToolTipIcon.Error);
            }
        }
        private bool IsFileVideo(string filePath)
        {
            return -1 != Array.IndexOf(videoFormats, Path.GetExtension(filePath).ToLowerInvariant());
        }
    }
}
