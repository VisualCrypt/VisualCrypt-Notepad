﻿using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using VisualCrypt.Desktop.Shared;
using VisualCrypt.Desktop.Shared.App;
using VisualCrypt.Desktop.Shared.Events;
using VisualCrypt.Desktop.Shared.Files;
using VisualCrypt.Desktop.Shared.Services;

namespace VisualCrypt.Desktop.Views
{
    [Export]
    public class ShellViewModel : ViewModelBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IMessageBoxService _messageBoxService;

        [ImportingConstructor]
        public ShellViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, IMessageBoxService messageBoxService)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            _messageBoxService = messageBoxService;
            _eventAggregator.GetEvent<EditorSendsStatusBarInfo>().Subscribe(OnEditorSendsStatusBarInfo);
            _eventAggregator.GetEvent<EditorSendsText>().Subscribe(OnEditorSendsText);
        }

        public void Init()
        {

            CreateEditor();
            ExecuteNewCommand();
        }

        void OnEditorSendsText(EditorSendsText args)
        {
            if (args != null && args.Callback != null)
                args.Callback(args.Text);
        }

        void OnEditorSendsStatusBarInfo(string statusBarInfo)
        {
            StatusBarText = statusBarInfo;
        }


      

       


        public string StatusBarText
        {
            get { return _statusBarText; }
            set
            {
                if (_statusBarText == value) return;
                _statusBarText = value;
                OnPropertyChanged(() => StatusBarText);
            }
        }
        string _statusBarText;

        public Visibility TextBlockClearPasswordVisibility
        {
            get { return _textBlockClearPasswordVisibility; }
            set
            {
                if (_textBlockClearPasswordVisibility == value) return;
                _textBlockClearPasswordVisibility = value;
                OnPropertyChanged(() => TextBlockClearPasswordVisibility);
            }
        }
        Visibility _textBlockClearPasswordVisibility = Visibility.Collapsed;




        public string PasswordStatus
        {
            get
            {
                //if (FileManager.FileModel.IsPasswordPresent == false)
                //    return "Not Set";

                // Unicode Character 'BLACK CIRCLE' (U+25CF)
                return new string('\u25CF', 5);
            }
        }
        #region ExitCommand


        public DelegateCommand<CancelEventArgs> ExitCommand
        {
            get { return CreateCommand(ref _exitCommand, ExecuteExitCommand, e => true); }
        }
        DelegateCommand<CancelEventArgs> _exitCommand;

        bool _isExitConfirmed;
        void ExecuteExitCommand(CancelEventArgs e)
        {

            bool isInvokedFromWindowCloseEvent = e != null;

            if (isInvokedFromWindowCloseEvent)
            {
                if(_isExitConfirmed)
                    return;
                if (ConfirmToDiscardText())
                    return;
                e.Cancel = true;
            }
            else
            {
               if (ConfirmToDiscardText())
                {
                    _isExitConfirmed = true;
                    Application.Current.Shutdown();
                }
            }
        }


        #endregion

        #region NewCommand

        DelegateCommand _newCommand;

        public DelegateCommand NewCommand
        {
            get { return CreateCommand(ref _newCommand, ExecuteNewCommand, () => true); }
        }

        void ExecuteNewCommand()
        {
            if (!ConfirmToDiscardText())
                return;
            FileManager.FileModel = new EmptyCleartextFileModel();
            _eventAggregator.GetEvent<EditorReceivesText>().Publish(FileManager.FileModel.Contents);
         
        }

        #endregion
     

        #region HelpCommand

        DelegateCommand _helpCommand;

        public DelegateCommand HelpCommand
        {
            get { return CreateCommand(ref _helpCommand, ExecuteHelpCommand, () => true); }
        }

        void ExecuteHelpCommand()
        {
            try
            {
                var process = new Process { StartInfo = { UseShellExecute = true, FileName = Constants.HelpUrl } };
                process.Start();
            }
            catch (Exception e)
            {
                _messageBoxService.ShowError(e);
            }

        }

        #endregion

        #region AboutCommand


        public DelegateCommand AboutCommand
        {
            get { return CreateCommand(ref _aboutCommand, ExecuteAboutCommand, () => true); }
        }
        DelegateCommand _aboutCommand;

        void ExecuteAboutCommand()
        {
            var aboutDialog = new About { WindowStyle = WindowStyle.ToolWindow, Owner = Application.Current.MainWindow };
            aboutDialog.ShowDialog();
        }

        #endregion

        #region LogCommand


        public DelegateCommand LogCommand
        {
            get { return CreateCommand(ref _logCommand, ExecuteLogCommand, () => true); }
        }


        DelegateCommand _logCommand;


        void ExecuteLogCommand()
        {
            ServiceLocator.Current.GetInstance<ModuleWindow>().Show();
        }

        #endregion

       
        public void ExecuteDecrpytEditorContentsCommand(string text)
        {
            
        }
        void OpenFileCommon(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename) || !File.Exists(filename))
                return;

            try
            {
                var fileModel = new FileModel();
                fileModel.SetFilename(filename);
                var loadResponse = fileModel.TryLoadVisualCryptTextOrCleartext();

                if (!loadResponse.Success)
                {
                    _messageBoxService.ShowError(loadResponse.Error);
                    return;
                }

                // we succeeded loading the file, now we can replace current content with the new content.

                FileManager.FileModel = fileModel;
                SettingsManager.CurrentDirectoryName = Path.GetDirectoryName(filename);

                _eventAggregator.GetEvent<EditorReceivesText>().Publish(loadResponse.Result);


                if (FileManager.FileModel.IsEncrypted)
                {
                    _eventAggregator.GetEvent<EditorShouldSendText>().Publish(ExecuteDecrpytEditorContentsCommand);

                }

            }
            catch (Exception e)
            {
                _messageBoxService.ShowError(e);
            }

        }


        public void OpenFileFromCommandLine(string[] args)
        {
            if (args == null)
                return;

            var commandline = string.Empty;

            // args is expected to hold one filename only
            // but can be segmented if it contains spaces.
            foreach (string a in args)
            {
                commandline += a;
                commandline += " ";
            }
            commandline = commandline.Trim();
            if (commandline.Length == 0)
                return;

            OpenFileCommon(commandline);
        }


        public void OpenFileFromDragDrop(string dropFilename)
        {
            if (!ConfirmToDiscardText())
                return;
            OpenFileCommon(dropFilename);
        }

      

     

        public bool CanExecuteClearPasswordCommand()
        {
            throw new NotImplementedException();
        }

        public void ExecuteClearPasswordCommand()
        {
            throw new NotImplementedException();
        }

        #region Private Methods

        bool ConfirmToDiscardText()
        {
            if (FileManager.FileModel.IsDirty)
                return (_messageBoxService.Show("Discard changes?", Constants.ProductName, MessageBoxButton.OKCancel, MessageBoxImage.Question) ==
                        MessageBoxResult.OK);

            return true;
        }

        void CreateEditor()
        {
            var mainRegion = _regionManager.Regions[RegionNames.EditorRegion];
            if (mainRegion == null) 
               throw new InvalidOperationException("The region {0} is missing and has probably not been defined in Xaml.".FormatInvariant(RegionNames.EditorRegion));

           var view = mainRegion.GetView(typeof(IEditor).Name) as IEditor;
            if (view == null)
            {
                view = ServiceLocator.Current.GetInstance<IEditor>();
                mainRegion.Add(view, typeof(IEditor).Name);  // automatically activates the view
            }
            else
            {
                mainRegion.Activate(view);
            }
        }

        #endregion
    }
}