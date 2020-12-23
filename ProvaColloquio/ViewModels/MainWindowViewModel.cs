using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Text.Json;
using System.Configuration;
using static ProvaColloquio.BrowserEmulatorUtils;
using System.Windows;
using System.Text;

namespace ProvaColloquio.ViewModels
{

    public class MainWindowViewModel : ViewModelBase
    {
        #region [ exception messages ]
        private const string exListRead = "La lettura della lista dei file si è interrotta in modo inaspettato.";
        private const string exUrl = "Url non valido.";
        #endregion

        #region [ properties ]
        private string exceptionMessage = string.Empty;
        public string ExceptionMessage
        {
            get
            {
                return exceptionMessage;
            }
            set
            {
                if (exceptionMessage != value)
                {
                    exceptionMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        private List<Video> videos = new List<Video>();
        public List<Video> Videos
        {
            get
            {
                return videos;
            }
            set
            {
                if (videos != value)
                {
                    videos = value;
                    OnPropertyChanged();
                }
            }
        }

        private Video selectedVideo = null;
        public Video SelectedVideo
        {
            get
            {
                return selectedVideo;
            }
            set
            {
                if (selectedVideo != value)
                {
                    selectedVideo = value;
                    if (selectedVideo != null)
                    {
                        openVideo(selectedVideo.Include);
                    }
                }
            }
        }
        #endregion

        #region [ commands ]

        Task taskReader;
        CancellationTokenSource tokenSource = null;

        #region [ command start ] 

        private ICommand startCommand;
        public ICommand StartCommand
        {
            get
            {
                return startCommand ?? (startCommand = new CommandHandler(() => startTask(), () => CanExecuteStart));
            }
        }
        public bool CanExecuteStart
        {
            get
            {
                return tokenSource == null;
            }
        }

        private void startTask()
        {
            tokenSource = new CancellationTokenSource();
            try
            {
                var token = tokenSource.Token;
                taskReader = Task.Run(() => readVideoList(token));
            }
            catch(Exception ex)
            {
                ExceptionMessage = exListRead;
                if (tokenSource != null)
                {
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                    tokenSource = null;
                }
                return;
            }

        }

        #endregion

        #region [ command stop ]

        private ICommand stopCommand;
        public ICommand StopCommand
        {
            get
            {
                return stopCommand ?? (stopCommand = new CommandHandler(() => stopTask(), () => CanExecuteStop));
            }
        }
        public bool CanExecuteStop
        {
            get
            {
                return tokenSource != null && !tokenSource.IsCancellationRequested;
            }
        }
        private void stopTask()
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
            tokenSource = null;
        }

        #endregion

        #region [ command close ex ]

        private ICommand closeExceptionCommand;
        public ICommand CloseExceptionCommand
        {
            get
            {
                return closeExceptionCommand ?? (closeExceptionCommand = new CommandHandler(() => ExceptionMessage = string.Empty,null));
            }
        }

        #endregion

        #region [ command close app ]

        private ICommand closeAppCommand;
        public ICommand CloseAppCommand
        {
            get
            {
                return closeAppCommand ?? (closeAppCommand = new CommandHandler(() => closeEverything(), null));
            }
        }
        
        private void closeEverything()
        {
            if (tokenSource != null)
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
                tokenSource = null;
            }
            Application.Current.Shutdown();
        }

        #endregion

        #endregion

        #region [ private methods ]

        private void openVideo(string iframe)
        {
            string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            try
            {
                EnsureBrowserEmulationEnabled(String.Format("{0}{1}", appName, ".vchost.exe"), false);
                EnsureBrowserEmulationEnabled(String.Format("{0}{1}", appName, ".exe"), false);
            }
            catch (UnsupportedVersionException uve)
            {
                ExceptionMessage = uve.Message;
                return;
            }

            VideoWindowViewModel videoViewModel = new VideoWindowViewModel();

            int i = iframe.IndexOf('\"', iframe.IndexOf("src")) + 1;
            int j = iframe.IndexOf('\"', i);
            string source = iframe.Substring(i, j - i);
            try
            {
                videoViewModel.Source =  new Uri(iframe.Substring(i, j - i));
            }
            catch
            {
                ExceptionMessage = string.Format("{0}: {1}",exUrl, source);
            }

            int height;
            i = iframe.IndexOf('\"', iframe.IndexOf("height")) + 1;
            j = iframe.IndexOf('\"', i);
            if (int.TryParse(iframe.Substring(i, j - i).TrimEnd('\\'), out height) && height > 0)
                videoViewModel.Height = height;

            int witdh;
            i = iframe.IndexOf('\"', iframe.IndexOf("width")) + 1;
            j = iframe.IndexOf('\"', i);
            if (int.TryParse(iframe.Substring(i, j - i).TrimEnd('\\'), out witdh) && witdh > 0)
                videoViewModel.Width = witdh;

            WindowService ws = new WindowService();
            ws.showWindow(videoViewModel);

        }

        private void readVideoList(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;

                string json = string.Empty;
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    json = client.DownloadString(ConfigurationManager.AppSettings["Url"]);
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => Videos = JsonSerializer.Deserialize<List<Video>>(json, options)));

                Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["Interval"]));

            }
        }

        #endregion
    }
}
