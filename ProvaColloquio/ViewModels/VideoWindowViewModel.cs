using System;
using System.Windows.Input;

namespace ProvaColloquio.ViewModels
{

    public class VideoWindowViewModel : ViewModelBase
    {
        public VideoWindowViewModel()
        { }

        public VideoWindowViewModel(Uri source, int height, int width)
        {
            this.source = source;
            this.height = height;
            this.width = width;
        }

        #region [ properties ]

        private bool isNavigationFailed = false;
        public bool IsNavigationFailed
        {
            get
            {
                return isNavigationFailed;
            }
            set
            {
                if(isNavigationFailed != value)
                {
                    isNavigationFailed = value;
                    OnPropertyChanged();
                }
            }
        }

        private Uri source;
        public Uri Source 
        { 
            get
            {
                return source;
            }
            set
            {
                if (source != value)
                {
                    source = value;
                    OnPropertyChanged();
                }
            }
        }

        private int height;
        public int Height 
        { 
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }

        private int width;
        public int Width 
        { 
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        #endregion

        #region [ command show ops ]

        private ICommand showOpsCommand;
        public ICommand ShowOpsCommand
        {
            get
            {
                return showOpsCommand ?? (showOpsCommand = new CommandHandler(() => IsNavigationFailed = true, null)); ;
            }
        }

        private ICommand shutCommand;
        public ICommand ShutCommand
        {
            get
            {
                return shutCommand ?? (shutCommand = new CommandHandler(() => Source = null, null)); ;
            }
        }

        #endregion
    }
}
