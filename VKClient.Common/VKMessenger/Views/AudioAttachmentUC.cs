using Microsoft.Phone.BackgroundAudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Extensions;
using VKClient.Audio.Base.Library;
using VKClient.Common.AudioManager;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKMessenger.Library;

namespace VKMessenger.Views
{
    public class AudioAttachmentUC : UserControl, IHandle<AudioAttachmentStartedPlaying>, IHandle, IHandle<AudioPlayerStateChanged>
    {
        public static readonly DependencyProperty StartedPlayingCallbackProperty = DependencyProperty.Register("StartedPlayingCallback", typeof(Action<AudioAttachmentUC>), typeof(AudioAttachmentUC), new PropertyMetadata(new PropertyChangedCallback(AudioAttachmentUC.StartedPlayingCallback_OnChanged)));
        private readonly DateTime _lastTimeSetPositionManually = DateTime.MinValue;
        private DateTime _lastTimeSetPlayPauseManually = DateTime.MinValue;
        private bool _addingToMyAudios;
        //private bool _settingProgressFromUpdate;
        private bool _initializedUri;
        internal Border borderPlay;
        internal Border borderPause;
        internal TextBlock textBlockTrack;
        internal TextBlock textBlockArtist;
        internal TextBlock textBlockDuration;
        private bool _contentLoaded;

        private AttachmentViewModel AttachmentVM
        {
            get
            {
                return this.DataContext as AttachmentViewModel;
            }
        }

        public bool AssignedCurrentTrack
        {
            get
            {
                try
                {
                    AudioTrack track = BGAudioPlayerWrapper.Instance.Track;
                    return track != null && track.GetTagId() == this.AttachmentVM.Audio.UniqueId;
                }
                catch
                {
                    return false;
                }
            }
        }

        public Action<AudioAttachmentUC> NotifyStartedPlayingCallback { get; set; }

        public Action<AudioAttachmentUC> StartedPlayingCallback
        {
            get
            {
                return (Action<AudioAttachmentUC>)this.GetValue(AudioAttachmentUC.StartedPlayingCallbackProperty);
            }
            set
            {
                this.SetValue(AudioAttachmentUC.StartedPlayingCallbackProperty, value);
            }
        }

        public bool UseWhiteForeground { get; set; }

        public AudioAttachmentUC()
        {
            this.InitializeComponent();
            this.Loaded += this.AudioPlayer_OnLoaded;
            EventAggregator.Current.Subscribe(this);
        }

        private static void StartedPlayingCallback_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AudioAttachmentUC)d).NotifyStartedPlayingCallback = e.NewValue as Action<AudioAttachmentUC>;
        }

        private void LayoutRoot_OnTap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
            if (this.borderPlay.Visibility == Visibility.Visible)
                this.Play(e);
            else
                this.Pause(e);
        }

        private void AudioPlayer_OnLoaded(object sender, RoutedEventArgs e)
        {
            double num = 0.0;
            AttachmentViewModel attachmentVm = this.AttachmentVM;
            int result;
            if ((attachmentVm != null ? attachmentVm.Audio : null) != null && int.TryParse(this.AttachmentVM.Audio.duration, out result))
            {
                this.textBlockDuration.Text = UIStringFormatterHelper.FormatDuration(result);
                num = this.textBlockDuration.ActualWidth;
                Canvas.SetLeft((UIElement)this.textBlockDuration, this.Width - num - 16.0);
            }
            this.textBlockTrack.CorrectText(this.Width - Canvas.GetLeft((UIElement)this.textBlockTrack));
            this.textBlockArtist.CorrectText(this.Width - Canvas.GetLeft((UIElement)this.textBlockTrack) - num - (num > 0.0 ? 16.0 : 0.0));
            this.UpdateUIState();
        }

        private void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            this.UpdateUIState();
        }

        private void UpdateUIState()
        {
            try
            {
                bool flag = false;
                TimeSpan timeSpan;
                if (this.AssignedCurrentTrack)
                {
                    if (BGAudioPlayerWrapper.Instance.PlayerState == PlayState.Playing)
                        flag = true;
                    timeSpan = BGAudioPlayerWrapper.Instance.Track.Duration;
                    double totalSeconds1 = timeSpan.TotalSeconds;
                    timeSpan = BGAudioPlayerWrapper.Instance.Position;
                    double totalSeconds2 = timeSpan.TotalSeconds;
                    if (totalSeconds1 != 0.0)
                    {
                        double num = totalSeconds2 / totalSeconds1;
                    }
                }
                timeSpan = DateTime.Now - this._lastTimeSetPlayPauseManually;
                if (timeSpan.TotalMilliseconds >= 7000.0)
                {
                    this.borderPlay.Visibility = !flag ? Visibility.Visible : Visibility.Collapsed;
                    this.borderPause.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
                }
                timeSpan = DateTime.Now - this._lastTimeSetPositionManually;
                if (timeSpan.TotalMilliseconds < 2000.0)
                    return;
                //this._settingProgressFromUpdate = true;
                //this._settingProgressFromUpdate = false;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("AudioAttachmentUC.UpdateUIState failed", ex);
            }
        }

        private void Play(GestureEventArgs e)
        {
            bool assignedCurrentTrack = this.AssignedCurrentTrack;
            this.SetPlayPauseVisibilityManually(false);
            Logger.Instance.Info("AudioAttachmentUC.play_Tap, AssignedCurrentTrack = " + assignedCurrentTrack.ToString());
            this.NotifyPlaying();
            if (!assignedCurrentTrack)
            {
                AttachmentViewModel dc = this.DataContext as AttachmentViewModel;
                if (dc != null)
                {
                    AudioTrack track = AudioTrackHelper.CreateTrack(this.AttachmentVM.Audio);
                    Logger.Instance.Info("Starting track for uri: {0}", (object)dc.ResourceUri);
                    BGAudioPlayerWrapper.Instance.Track = track;
                    if (!this._initializedUri)
                    {
                        Logger.Instance.Info("AudioAttachmentUC initializedUri = false, getting updated audio info");
                        AudioService instance = AudioService.Instance;
                        List<string> ids = new List<string>();
                        ids.Add(dc.MediaOwnerId.ToString() + "_" + dc.MediaId);
                        Action<BackendResult<List<AudioObj>, ResultCode>> callback = (Action<BackendResult<List<AudioObj>, ResultCode>>)(res =>
                        {
                            if (res.ResultCode != ResultCode.Succeeded)
                                return;
                            this._initializedUri = true;
                            AudioObj audio = res.ResultData.FirstOrDefault<AudioObj>();
                            if (audio == null)
                                return;
                            string url = audio.url;
                            if (url != dc.ResourceUri)
                                return;
                            Logger.Instance.Info("Received different uri for audio: {0}", url);
                            dc.ResourceUri = url;
                            dc.Audio = audio;
                            this.Dispatcher.BeginInvoke(delegate
                            {
                                if (!this.AssignedCurrentTrack && BGAudioPlayerWrapper.Instance.Track != null || (BGAudioPlayerWrapper.Instance.PlayerState == PlayState.Paused || AudioTrackHelper.GetPositionSafe().TotalMilliseconds >= 1E-05))
                                    return;
                                BGAudioPlayerWrapper.Instance.Track = AudioTrackHelper.CreateTrack(audio);
                                this.Play();
                            });
                        });
                        instance.GetAudio(ids, callback);
                    }
                }
            }
            this.Play();
        }

        private void Play()
        {
            try
            {
                BGAudioPlayerWrapper.Instance.Volume = 1.0;
                BGAudioPlayerWrapper.Instance.Play();
                EventAggregator.Current.Publish((object)new AudioAttachmentStartedPlaying()
                {
                    UC = this
                });
            }
            catch
            {
            }
        }

        private void NotifyPlaying()
        {
            if (this.NotifyStartedPlayingCallback != null)
            {
                this.NotifyStartedPlayingCallback(this);
            }
            else
            {
                if (this.AttachmentVM == null || this.AttachmentVM.Audio == null)
                    return;
                List<AudioObj> tracks = new List<AudioObj>();
                tracks.Add(this.AttachmentVM.Audio);
                //int num = (int)CurrentMediaSource.AudioSource;
                //PlaylistManager.SetAudioAgentPlaylist(tracks, (StatisticsActionSource)num);
                PlaylistManager.SetAudioAgentPlaylist(tracks, CurrentMediaSource.AudioSource);
            }
        }

        private void Pause(GestureEventArgs e)
        {
            BGAudioPlayerWrapper.Instance.Pause();
            this.SetPlayPauseVisibilityManually(true);
        }

        private void SetPlayPauseVisibilityManually(bool showPlay)
        {
            this.borderPlay.Visibility = showPlay ? Visibility.Visible : Visibility.Collapsed;
            this.borderPause.Visibility = showPlay ? Visibility.Collapsed : Visibility.Visible;
            this._lastTimeSetPlayPauseManually = DateTime.Now;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }

        public void Handle(AudioAttachmentStartedPlaying message)
        {
            if (message.UC == this || this.AssignedCurrentTrack)
                return;
            this.SetPlayPauseVisibilityManually(true);
        }

        private void AudioAddToMyAudios_OnTap(object sender, RoutedEventArgs e)
        {
            if (this._addingToMyAudios)
                return;
            this._addingToMyAudios = true;
            AudioService.Instance.AddAudio(this.AttachmentVM.MediaOwnerId, this.AttachmentVM.MediaId, (Action<BackendResult<long, ResultCode>>)(res =>
            {
                this._addingToMyAudios = false;
                string messageStr = res.ResultCode == ResultCode.Succeeded ? CommonResources.Audio_AudioIsAdded : CommonResources.Error;
                Execute.ExecuteOnUIThread((Action)(() => new GenericInfoUC().ShowAndHideLater(messageStr, null)));
            }));
        }

        public void Handle(AudioPlayerStateChanged message)
        {
            this.UpdateUIState();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/UC/AudioAttachmentUC.xaml", UriKind.Relative));
            this.borderPlay = (Border)this.FindName("borderPlay");
            this.borderPause = (Border)this.FindName("borderPause");
            this.textBlockTrack = (TextBlock)this.FindName("textBlockTrack");
            this.textBlockArtist = (TextBlock)this.FindName("textBlockArtist");
            this.textBlockDuration = (TextBlock)this.FindName("textBlockDuration");
        }
    }
}
