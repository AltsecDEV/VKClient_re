using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Graffiti;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKMessenger.Backend;
using VKMessenger.Utils;
using VKMessenger.Views;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace VKMessenger.Library
{
    public class ConversationViewModel : ViewModelBase, IBinarySerializableWithTrimSupport, IBinarySerializable, IHandle<NotificationSettingsChangedEvent>, IHandle
    {
        internal static int b__124_0(MessageViewModel m1, MessageViewModel m2)
        {
            if (m2.Message.mid > 0 && m1.Message.mid > 0)
            {
                return m2.Message.mid - m1.Message.mid;
            }
            return m2.Message.date - m1.Message.date;
        }
        public static Func<MessageViewModel, MessageViewModel, int> _comparisonFunc = new Func<MessageViewModel, MessageViewModel, int>(ConversationViewModel.b__124_0);
        public static readonly string UNREAD_ITEM_ACTION = "UNREAD_MESSAGES";
        private readonly int _numberOfMessagesToStore = 10;
        private string _uiTitle = string.Empty;
        private string _uiSubtitle = string.Empty;
        private Visibility _notificationsDisabledVisibility = Visibility.Collapsed;
        private OutboundMessageViewModel _outboundMessage = new OutboundMessageViewModel();
        private ObservableCollection<MessageViewModel> _messages = new ObservableCollection<MessageViewModel>();
        private ConversationAvatarViewModel _conversationAvatarVM = new ConversationAvatarViewModel();
        private DelayedExecutor _delayedExecutorResetTypingStatus = new DelayedExecutor(10000);
        private string _savedSubtitle = string.Empty;
        private DateTime _lastTimeUserIsTypingWasCalled = DateTime.MinValue;
        private long _userOrChatId;
        private bool _isChat;
        //private bool _isInitialized;
        private IScroll _scroll;
        private UserStatus _userStatus;
        private User _user;
        private ChatExtended _chat;
        private int _skipped;
        private string _chatTypeStatusText;
        private bool _isInSelectionMode;
        private bool _isBusyLoadingMessages;
        private bool _isBusyLoadingHeaderInfo;
        private bool _userIsTypingIsSet;
        private bool _changingNotifications;
        private bool _isInProgressPinToStart;

        private int Skipped
        {
            get
            {
                return this._skipped;
            }
            set
            {
                if (this._skipped == value)
                    return;
                this._skipped = value;
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.ArrowDownDarkVisibility));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.ArrowDownLightVisibility));
            }
        }

        public ConversationAvatarViewModel ConversationAvatarVM
        {
            get
            {
                return this._conversationAvatarVM;
            }
        }

        private bool IsDarkTheme
        {
            get
            {
                return (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible;
            }
        }

        private bool ShowArrowDown
        {
            get
            {
                return this._skipped > 0;
            }
        }

        public bool AreNotificationsDisabled
        {
            get
            {
                if (this._chat == null)
                    return false;
                return this._chat.push_settings.AreDisabledNow;
            }
            set
            {
                int num1 = value ? -1 : 0;
                if (this.AreNotificationsDisabled == value)
                    return;
                this._chat.push_settings.disabled_until = num1;
                EventAggregator current = EventAggregator.Current;
                NotificationSettingsChangedEvent settingsChangedEvent = new NotificationSettingsChangedEvent();
                int num2 = this.AreNotificationsDisabled ? 1 : 0;
                settingsChangedEvent.AreNotificationsDisabled = num2 != 0;
                long userOrCharId = this.UserOrCharId;
                settingsChangedEvent.ChatId = userOrCharId;
                current.Publish((object)settingsChangedEvent);
            }
        }

        public Visibility ArrowDownDarkVisibility
        {
            get
            {
                return !this.IsDarkTheme || !this.ShowArrowDown ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility ArrowDownLightVisibility
        {
            get
            {
                return this.IsDarkTheme || !this.ShowArrowDown ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool IsOnScreen { get; set; }

        public bool NoMessages
        {
            get
            {
                if (!this._isBusyLoadingMessages)
                    return this.Messages.Count == 0;
                return false;
            }
        }

        public string UITitle
        {
            get
            {
                return this._uiTitle.ToUpperInvariant();
            }
            private set
            {
                if (!(this._uiTitle != value))
                    return;
                this._uiTitle = value;
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.UITitle));
            }
        }

        public string Title
        {
            get
            {
                return this._uiTitle;
            }
        }

        public Visibility NotificationsDisabledVisibility
        {
            get
            {
                return this._notificationsDisabledVisibility;
            }
            set
            {
                if (this._notificationsDisabledVisibility == value)
                    return;
                this._notificationsDisabledVisibility = value;
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.NotificationsDisabledVisibility));
            }
        }

        public string UISubtitle
        {
            get
            {
                return this._uiSubtitle;
            }
            private set
            {
                if (!(this._uiSubtitle != value))
                    return;
                this._uiSubtitle = value;
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.UISubtitle));
            }
        }

        public string ChatTypeStatusText
        {
            get
            {
                return this._chatTypeStatusText;
            }
            set
            {
                if (!(this._chatTypeStatusText != value))
                    return;
                this._chatTypeStatusText = value;
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.ChatTypeStatusText));
            }
        }

        public bool IsChat
        {
            get
            {
                return this._isChat;
            }
        }

        public long UserOrCharId
        {
            get
            {
                return this._userOrChatId;
            }
        }

        // NEW: 4.8.0
        public ChatExtended Chat
        {
            get
            {
                return this._chat;
            }
        }

        public User User
        {
            get
            {
                return this._user;
            }
        }

        public bool CanAddMembers { get; private set; }

        public ObservableCollection<MessageViewModel> Messages
        {
            get
            {
                return this._messages;
            }
            private set
            {
                this._messages = value;
                this.NotifyPropertyChanged<ObservableCollection<MessageViewModel>>((System.Linq.Expressions.Expression<Func<ObservableCollection<MessageViewModel>>>)(() => this.Messages));
            }
        }

        public OutboundMessageViewModel OutboundMessageVm
        {
            get
            {
                return this._outboundMessage;
            }
            private set
            {
                this._outboundMessage = value;
                this.NotifyPropertyChanged<OutboundMessageViewModel>((System.Linq.Expressions.Expression<Func<OutboundMessageViewModel>>)(() => this.OutboundMessageVm));
            }
        }

        public IScroll Scroll
        {
            get
            {
                return this._scroll;
            }
            set
            {
                this._scroll = value;
            }
        }

        public bool IsInSelectionMode
        {
            get
            {
                return this._isInSelectionMode;
            }
            set
            {
                if (this._isInSelectionMode == value)
                    return;
                this._isInSelectionMode = value;
                Execute.ExecuteOnUIThread((Action)(() =>
                {
                    foreach (MessageViewModel message in (Collection<MessageViewModel>)this.Messages)
                    {
                        if (!this._isInSelectionMode)
                            message.IsSelected = false;
                        message.IsInSelectionMode = this._isInSelectionMode;
                    }
                }));
            }
        }

        public List<Photo> AttachedPhotos
        {
            get
            {
                List<Photo> photoList = new List<Photo>();
                foreach (MessageViewModel message in (Collection<MessageViewModel>)this.Messages)
                {
                    photoList.AddRange(message.Attachments.Where<AttachmentViewModel>((Func<AttachmentViewModel, bool>)(a => a.AttachmentType == AttachmentType.Photo)).Select<AttachmentViewModel, Photo>((Func<AttachmentViewModel, Photo>)(p => p.Photo ?? new Photo())));
                    foreach (MessageViewModel forwardedMessage in (Collection<MessageViewModel>)message.ForwardedMessages)
                        photoList.AddRange(forwardedMessage.Attachments.Where<AttachmentViewModel>((Func<AttachmentViewModel, bool>)(a => a.AttachmentType == AttachmentType.Photo)).Select<AttachmentViewModel, Photo>((Func<AttachmentViewModel, Photo>)(p => p.Photo ?? new Photo())));
                }
                return photoList;
            }
        }

        public bool CanDettachProductAttachment { get; set; }

        public ConversationViewModel()
        {
            EventAggregator.Current.Subscribe((object)this);
        }

        public void InitializeWith(long userOrChatId, bool isChat)
        {
            //this._isInitialized = true;
            this._isChat = isChat;
            this._userOrChatId = userOrChatId;
            this.OutboundMessageVm = new OutboundMessageViewModel(isChat, userOrChatId);
        }

        public void LoadFromLastUnread(Action<bool> callback = null)
        {
            this.LoadMessagesAsyncWithParams(-13, 21, false, false, callback, new int?(-1), false);
        }

        public void LoadFromMessageId(int messageId, Action<bool> callback = null, bool scrollToMessageId = false)
        {
            this.LoadMessagesAsyncWithParams(-13, 21, false, true, callback, new int?(messageId), scrollToMessageId);
        }

        public void LoadMoreConversations(Action<bool> callback = null)
        {
            this.LoadMessagesAsyncWithParams(this._messages.Where<MessageViewModel>((Func<MessageViewModel, bool>)(mvm => mvm.Message.mid > 0)).Count<MessageViewModel>() + this._skipped, 16, false, false, callback, new int?(), false);
        }

        public void LoadNewerConversations(Action<bool> callback = null)
        {
            if (this._skipped <= 0 || this._messages.Count <= 0)
                return;
            this.LoadMessagesAsyncWithParams(-15, 15, false, true, callback, new int?(this._messages.Last<MessageViewModel>().Message.id), false);
        }

        public void RefreshConversations()
        {
            this.LoadMessagesAsyncWithParams(0, 8, true, true, (Action<bool>)null, new int?(), false);
        }

        public void DeleteMessages(List<MessageViewModel> messageViewModels, Action callback = null)
        {
            this.RemoveUnreadMessagesItem();
            BackendServices.MessagesService.DeleteMessages(messageViewModels.Select<MessageViewModel, int>((Func<MessageViewModel, int>)(vm => vm.Message.id)).Where<int>((Func<int, bool>)(id => (uint)id > 0U)).ToList<int>(), (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (result.ResultCode == ResultCode.Succeeded)
                {
                    foreach (MessageViewModel messageViewModel in messageViewModels)
                    {
                        this.Messages.Remove(messageViewModel);
                        messageViewModel.CancelUpload();
                    }
                    if (callback == null)
                        return;
                    callback();
                }
                else
                    GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", (VKRequestsDispatcher.Error)null);
            }))));
        }

        public void SendMessage(string messageText)
        {
            this.SendMessage(messageText, (StickerItemData)null, "", (GraffitiAttachmentItem)null);
        }

        internal void SendSticker(StickerItemData stickerItemData, string stickerReferrer)
        {
            this.SendMessage("", stickerItemData, stickerReferrer, (GraffitiAttachmentItem)null);
        }

        public void SendGraffiti(GraffitiAttachmentItem graffitiAttachmentItem)
        {
            this.SendMessage("", (StickerItemData)null, "", graffitiAttachmentItem);
        }

        private void SendMessage(string messageText, StickerItemData stickerData, string stickerReferrer, GraffitiAttachmentItem graffitiAttachmentItem)
        {
            string str = messageText.Replace("\r\n", "\r").Replace("\r", "\r\n").Trim();
            this.RemoveUnreadMessagesItem();
            OutboundMessageViewModel outboundMessage = new OutboundMessageViewModel(this._isChat, this._userOrChatId);
            if (stickerData != null)
            {
                outboundMessage.StickerItem = stickerData;
                outboundMessage.StickerReferrer = stickerReferrer;
            }
            else if (graffitiAttachmentItem != null)
            {
                outboundMessage.GraffitiAttachmentItem = graffitiAttachmentItem;
            }
            else
            {
                outboundMessage.MessageText = str;
                outboundMessage.Attachments = this._outboundMessage.Attachments;
                this._outboundMessage.Attachments = new ObservableCollection<IOutboundAttachment>();
            }
            MessageViewModel messageViewModel = new MessageViewModel(outboundMessage);
            this.ResetUserIsTypingStatusIfNeeded(true);
            this._messages.AddOrdered<MessageViewModel>(messageViewModel, ConversationViewModel._comparisonFunc, true);
            if (!this.CanDettachProductAttachment && stickerData == null)
            {
                OutboundProductAttachment productAttachment = outboundMessage.Attachments.FirstOrDefault<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a is OutboundProductAttachment)) as OutboundProductAttachment;
                Product product = productAttachment != null ? productAttachment.Product : (Product)null;
                if (product != null)
                    EventAggregator.Current.Publish((object)new MarketContactEvent(string.Format("{0}_{1}", (object)product.owner_id, (object)product.id), MarketContactAction.write));
            }
            messageViewModel.Send();
            if (this._skipped > 0)
                this.RefreshConversations();
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.NoMessages));
            MessengerStateManagerInstance.Current.EnsureOnlineStatusIsSet(false);
        }

        public void AddForwardedMessagesToOutboundMessage(IList<Message> forwardedMessages)
        {
            this.OutboundMessageVm.RemoveForwardedMessages();
            this.OutboundMessageVm.Attachments.Add((IOutboundAttachment)new OutboundForwardedMessages(forwardedMessages.ToList<Message>()));
        }

        private void EnsureUserAndChatIdSet(Message message)
        {
            if (this._isChat)
                message.chat_id = (int)this._userOrChatId;
            else
                message.uid = (int)this._userOrChatId;
        }

        public void UserIsTyping()
        {
            if ((DateTime.Now - this._lastTimeUserIsTypingWasCalled).TotalSeconds <= 4.0)
                return;
            BackendServices.MessagesService.SetUserIsTyping(this._userOrChatId, this._isChat, (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>)(r => { }));
            this._lastTimeUserIsTypingWasCalled = DateTime.Now;
        }

        public void LoadHeaderInfoAsync()
        {
            if (this._isBusyLoadingHeaderInfo)
                return;
            this._isBusyLoadingHeaderInfo = true;
            if (this._isChat)
            {
                BackendServices.MessagesService.GetChat(this._userOrChatId, (Action<BackendResult<ChatExtended, ResultCode>>)(res =>
                {
                    if (res.ResultCode == ResultCode.Succeeded)
                        this._chat = res.ResultData;
                    this.RefreshUIPropertiesSafe();
                    this._isBusyLoadingHeaderInfo = false;
                }));
            }
            else
            {
                this.RefreshUIPropertiesSafe();
                UsersService instance = UsersService.Instance;
                List<long> userIds = new List<long>();
                userIds.Add(this._userOrChatId);
                Action<BackendResult<List<User>, ResultCode>> callback = (Action<BackendResult<List<User>, ResultCode>>)(r =>
                {
                    if (r.ResultCode == ResultCode.Succeeded)
                    {
                        this._user = r.ResultData.First<User>();
                        this.CanAddMembers = this._user.friend_status == 3;
                        this.RefreshUIPropertiesSafe();
                        UsersService.Instance.GetStatus(this._userOrChatId, (Action<BackendResult<UserStatus, ResultCode>>)(res =>
                        {
                            if (res.ResultCode == ResultCode.Succeeded)
                            {
                                this._userStatus = res.ResultData;
                                this.RefreshUIPropertiesSafe();
                            }
                            this._isBusyLoadingHeaderInfo = false;
                        }));
                    }
                    else
                        this._isBusyLoadingHeaderInfo = false;
                });
                instance.GetUsers(userIds, callback);
            }
        }

        private void RefreshUIPropertiesSafe()
        {
            if (this._isChat)
                Deployment.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (this._chat == null)
                        return;
                    this.UITitle = this._chat.title;
                    this.NotificationsDisabledVisibility = this._chat.push_settings.AreDisabledNow ? Visibility.Visible : Visibility.Collapsed;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.AreNotificationsDisabled));
                    if (this._chat.users == null)
                        return;
                    this._conversationAvatarVM.Initialize(this._chat.photo_200, true, this._chat.users.Select<User, long>((Func<User, long>)(u => u.uid)).ToList<long>(), this._chat.users);
                    this.UISubtitle = UIStringFormatterHelper.FormatNumberOfSomething(this._chat.users.Count, CommonResources.Conversation_OnePerson, CommonResources.Conversation_TwoToFourPersonsFrm, CommonResources.Conversation_FiveOrMorePersionsFrm, true, null, false);
                }));
            else
                Deployment.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (this._user == null)
                        return;
                    this._conversationAvatarVM.Initialize(this._user.photo_max, false, new List<long>(), new List<User>());
                    this.UITitle = this._user.first_name + " " + this._user.last_name;
                    this.NotificationsDisabledVisibility = Visibility.Collapsed;
                    this.CanAddMembers = this._user.friend_status == 3;
                    if (this._userStatus == null)
                        return;
                    this.UISubtitle = this._userStatus.GetUserStatusString(this._user.sex % 2 == 0);
                }));
        }

        private void LoadMessagesAsyncWithParams(int offset, int count, bool resetCollection, bool showProgress = true, Action<bool> callback = null, int? startMessageId = null, bool scrollToMessageId = false)
        {
            if (this._isBusyLoadingMessages)
                return;
            this._isBusyLoadingMessages = true;
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.NoMessages));
            if (showProgress || this.Messages.Count == 0)
                this.SetInProgress(true, CommonResources.Conversation_LoadingMessages);
            bool scrolledToUnreadItem = false;
            ObservableCollection<MessageViewModel> source = this._messages;

            Func<MessageViewModel, bool> arg_CD_1 = new Func<MessageViewModel, bool>(m => m.Message.action == ConversationViewModel.UNREAD_ITEM_ACTION);

            if (Enumerable.Any<MessageViewModel>(source, arg_CD_1) && this._scroll != null)
            {
                int? startMessageId2 = startMessageId;
                int num = -1;
                if (startMessageId2.GetValueOrDefault() == num && startMessageId2.HasValue)
                {
                    this._scroll.ScrollToUnreadItem();
                    scrolledToUnreadItem = true;
                }
            }
            BackendServices.MessagesService.GetHistory(this._userOrChatId, this._isChat, offset, count, startMessageId, (Action<BackendResult<MessageListResponse, ResultCode>>)(result =>
            {
                this.SetInProgress(false, "");
                if (callback != null)
                    callback(result.ResultCode == ResultCode.Succeeded);
                if (result.ResultCode == ResultCode.Succeeded)
                {
                    List<Message> messages = result.ResultData.Messages;
                    UsersService.Instance.GetUsers(Message.GetAssociatedUserIds(messages, true).Select<long, long>((Func<long, long>)(uid => uid)).ToList<long>(), (Action<BackendResult<List<User>, ResultCode>>)(r =>
                    {
                        if (r.ResultCode == ResultCode.Succeeded)
                        {
                            if (startMessageId.HasValue)
                                this.Skipped = result.ResultData.Skipped;
                            else if (offset == 0)
                                this.Skipped = 0;
                            List<User> resultData = r.ResultData;
                            List<MessageViewModel> messagesViewModels = new List<MessageViewModel>(messages.Count);
                            messages.ForEach((Action<Message>)(m =>
                            {
                                this.EnsureUserAndChatIdSet(m);
                                messagesViewModels.Add(new MessageViewModel(m));
                            }));
                            Deployment.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                int? nullable;
                                if (offset != 0)
                                {
                                    nullable = startMessageId;
                                    int num1 = -1;
                                    if ((nullable.GetValueOrDefault() == num1 ? (nullable.HasValue ? 1 : 0) : 0) == 0)
                                    {
                                        nullable = startMessageId;
                                        int num2 = 0;
                                        if ((nullable.GetValueOrDefault() > num2 ? (nullable.HasValue ? 1 : 0) : 0) != 0 && this._messages.Any<MessageViewModel>())
                                        {
                                            nullable = startMessageId;
                                            int id = this._messages.First<MessageViewModel>().Message.id;
                                            if ((nullable.GetValueOrDefault() < id ? (nullable.HasValue ? 1 : 0) : 0) == 0)
                                                goto label_6;
                                        }
                                        else
                                            goto label_6;
                                    }
                                }
                                MessageViewModel oldest = messagesViewModels.FirstOrDefault<MessageViewModel>();
                                if (oldest != null && !this._messages.Any<MessageViewModel>((Func<MessageViewModel, bool>)(m => m.Message.mid == oldest.Message.mid)))
                                    resetCollection = true;
                            label_6:
                                if (resetCollection)
                                {
                                    List<MessageViewModel> list = this._messages.Where<MessageViewModel>((Func<MessageViewModel, bool>)(m => m.Message.id == 0)).ToList<MessageViewModel>();
                                    this._messages.Clear();
                                    foreach (MessageViewModel messageViewModel in list)
                                        this._messages.Add(messageViewModel);
                                }
                                if (!resetCollection)
                                {
                                    foreach (MessageViewModel message in (Collection<MessageViewModel>)this._messages)
                                    {
                                        MessageViewModel mvm = message;
                                        if (mvm.Message != null)
                                        {
                                            MessageViewModel messageViewModel = messagesViewModels.FirstOrDefault<MessageViewModel>((Func<MessageViewModel, bool>)(m =>
                                            {
                                                if (m.Message != null)
                                                    return m.Message.mid == mvm.Message.mid;
                                                return false;
                                            }));
                                            if (messageViewModel != null && messageViewModel.Message.read_state != mvm.Message.read_state)
                                            {
                                                mvm.Message.read_state = messageViewModel.Message.read_state;
                                                mvm.RefreshUIProperties();
                                            }
                                        }
                                    }
                                }
                                messagesViewModels.ForEach((Action<MessageViewModel>)(messageVM =>
                                {
                                    messageVM.IsInSelectionMode = this.IsInSelectionMode;
                                    if (this._messages.Any<MessageViewModel>((Func<MessageViewModel, bool>)(m => m.Message.mid == messageVM.Message.mid)))
                                        return;
                                    this._messages.AddOrdered<MessageViewModel>(messageVM, ConversationViewModel._comparisonFunc, false);
                                }));
                                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.NoMessages));
                                nullable = startMessageId;
                                int num3 = -1;
                                if ((nullable.GetValueOrDefault() == num3 ? (nullable.HasValue ? 1 : 0) : 0) != 0)
                                    this.EnsureUnreadItem();
                                if (this._scroll != null)
                                {
                                    nullable = startMessageId;
                                    int num1 = -1;
                                    if ((nullable.GetValueOrDefault() == num1 ? (nullable.HasValue ? 1 : 0) : 0) != 0 && !scrolledToUnreadItem)
                                    {
                                        this._scroll.ScrollToUnreadItem();
                                        goto label_34;
                                    }
                                }
                                if (((this._scroll == null ? 0 : (messagesViewModels.Count > 0 ? 1 : 0)) & (resetCollection ? 1 : 0)) != 0)
                                    this._scroll.ScrollToBottom(false, false);
                                else if (((this._scroll == null ? 0 : (offset == 0 ? 1 : 0)) & (showProgress ? 1 : 0)) != 0)
                                    this._scroll.ScrollToBottom(true, false);
                                else if (this._scroll != null & scrollToMessageId)
                                {
                                    nullable = startMessageId;
                                    int num1 = 0;
                                    if ((nullable.GetValueOrDefault() > num1 ? (nullable.HasValue ? 1 : 0) : 0) != 0)
                                        this._scroll.ScrollToMessageId((long)startMessageId.Value);
                                }
                            label_34:
                                this.SetReadStatusIfNeeded();
                                this._isBusyLoadingMessages = false;
                            }));
                        }
                        else
                            this._isBusyLoadingMessages = false;
                    }));
                }
                else
                    this._isBusyLoadingMessages = false;
            }));
        }

        private void EnsureUnreadItem()
        {
            this.RemoveUnreadMessagesItem();
            int num = 0;
            for (int index = this._messages.Count - 1; index >= 0 && (this._messages[index].Message.@out == 0 && this._messages[index].Message.read_state == 0); --index)
                ++num;
            if (num <= 0)
                return;
            MessageViewModel messageViewModel = new MessageViewModel(new Message()
            {
                action = ConversationViewModel.UNREAD_ITEM_ACTION
            });
            if (this._messages.Count < num)
                return;
            this._messages.Insert(this._messages.Count - num, messageViewModel);
        }

        public void RemoveUnreadMessagesItem()
        {
            MessageViewModel messageViewModel = this._messages.FirstOrDefault<MessageViewModel>((Func<MessageViewModel, bool>)(m => m.Message.action == ConversationViewModel.UNREAD_ITEM_ACTION));
            if (messageViewModel == null)
                return;
            this._messages.Remove(messageViewModel);
        }

        private void SetReadStatusIfNeeded()
        {
            List<Message> listToBeMarkedAsRead = new List<Message>();
            foreach (MessageViewModel message1 in (Collection<MessageViewModel>)this.Messages)
            {
                Message message2 = message1.Message;
                if (message2 != null && message2.read_state == 0 && (message2.@out == 0 || (long)message2.user_id == AppGlobalStateManager.Current.LoggedInUserId))
                    listToBeMarkedAsRead.Add(message2);
            }
            if (listToBeMarkedAsRead.Count <= 0)
                return;
            BackendServices.MessagesService.MarkAsRead(listToBeMarkedAsRead.Select<Message, long>((Func<Message, long>)(m => (long)m.mid)).ToList<long>(), this.IsChat ? this.UserOrCharId + 2000000000L : this.UserOrCharId, (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>)(res =>
            {
                if (res.ResultCode != ResultCode.Succeeded)
                    return;
                Execute.ExecuteOnUIThread((Action)(() => listToBeMarkedAsRead.ForEach((Action<Message>)(m => m.read_state = 1))));
            }));
        }

        public void Write(BinaryWriter writer)
        {
            this.DoWrite(writer, false);
        }

        public void Read(BinaryReader reader)
        {
            try
            {
                int num1 = reader.ReadInt32();
                this._userOrChatId = reader.ReadInt64();
                this._isChat = reader.ReadBoolean();
                this._userStatus = reader.ReadGeneric<UserStatus>();
                this._user = reader.ReadGeneric<User>();
                int num2 = 1;
                if (num1 == num2)
                    reader.ReadGeneric<Chat>();
                this._messages = new ObservableCollection<MessageViewModel>(reader.ReadList<MessageViewModel>());
                this._outboundMessage = reader.ReadGeneric<OutboundMessageViewModel>();
                this._isInSelectionMode = reader.ReadBoolean();
                int num3 = 2;
                if (num1 >= num3)
                    this._chat = reader.ReadGeneric<ChatExtended>();
                //this._isInitialized = true;
                this.RefreshUIPropertiesSafe();
            }
            catch (Exception ex)
            {
                ServiceLocator.Resolve<IAppStateInfo>().ReportException(ex);
                throw ex;
            }
        }

        public void WriteTrimmed(BinaryWriter writer)
        {
            this.DoWrite(writer, true);
        }

        private void DoWrite(BinaryWriter writer, bool trim)
        {
            try
            {
                this.ResetUserIsTypingStatusIfNeeded(true);
                writer.Write(2);
                writer.Write(this._userOrChatId);
                writer.Write(this._isChat);
                writer.Write<UserStatus>(this._userStatus, false);
                writer.Write<User>(this._user, false);
                if (trim)
                    writer.WriteList<MessageViewModel>((IList<MessageViewModel>)this.TrimMessageViewModels(), 10000);
                else
                    writer.WriteList<MessageViewModel>((IList<MessageViewModel>)this._messages.ToList<MessageViewModel>(), 10000);
                writer.Write<OutboundMessageViewModel>(this._outboundMessage, false);
                writer.Write(this._isInSelectionMode);
                writer.Write<ChatExtended>(this._chat, false);
            }
            catch (Exception ex)
            {
                ServiceLocator.Resolve<IAppStateInfo>().ReportException(ex);
                throw ex;
            }
        }

        public void TrimMessages()
        {
            this.Messages = new ObservableCollection<MessageViewModel>(this.TrimMessageViewModels());
        }

        private List<MessageViewModel> TrimMessageViewModels()
        {
            return this._messages.Skip<MessageViewModel>(Math.Max(0, this._messages.Count - this._numberOfMessagesToStore)).Take<MessageViewModel>(this._numberOfMessagesToStore).ToList<MessageViewModel>();
        }

        private void SetUserIsTypingStatusWithDelayedReset(long userId, User user = null)
        {
            if (!this.IsOnScreen)
                return;
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (user == null || string.IsNullOrWhiteSpace(user.first_name) || (string.IsNullOrWhiteSpace(this.UISubtitle) || this._userIsTypingIsSet))
                    return;
                this._userIsTypingIsSet = true;
                this._savedSubtitle = this.UISubtitle;
                string str = CommonResources.Conversation_IsTyping;
                if (this._isChat)
                    str = string.Format(CommonResources.Conversation_UserIsTypingFrm, (object)user.FirstNameLastNameShort);
                this.UISubtitle = str;
                this._delayedExecutorResetTypingStatus.AddToDelayedExecution((Action)(() => Execute.ExecuteOnUIThread((Action)(() => this.ResetUserIsTypingStatusIfNeeded(false)))));
            }));
        }

        private void ResetUserIsTypingStatusIfNeeded(bool force = false)
        {
            if (!this._userIsTypingIsSet)
                return;
            this.UISubtitle = this._savedSubtitle;
            this._userIsTypingIsSet = false;
        }

        internal void ProcessInstantUpdates(List<LongPollServerUpdateData> updates)
        {
            List<MessageViewModel> newOrRestoredMessages = new List<MessageViewModel>();
            List<long> readMessages = new List<long>();
            List<long> deletedMessagesIds = new List<long>();
            bool atLeastOneNewMessage = false;
            foreach (LongPollServerUpdateData update in updates)
            {
                if (this._userOrChatId == (update.isChat ? update.chat_id : update.user_id) && update.isChat == this.IsChat)
                {
                    if ((update.UpdateType == LongPollServerUpdateType.MessageHasBeenAdded || update.UpdateType == LongPollServerUpdateType.MessageHasBeenRestored) && (update.user != null && update.message != null))
                    {
                        this.EnsureUserAndChatIdSet(update.message);
                        newOrRestoredMessages.Add(new MessageViewModel(update.message));
                        if (update.UpdateType == LongPollServerUpdateType.MessageHasBeenAdded)
                            atLeastOneNewMessage = true;
                    }
                    if (update.UpdateType == LongPollServerUpdateType.MessageHasBeenRead)
                        readMessages.Add(update.message_id);
                    if (update.UpdateType == LongPollServerUpdateType.MessageHasBeenDeleted)
                        deletedMessagesIds.Add(update.message_id);
                    if (update.UpdateType == LongPollServerUpdateType.UserIsTyping || update.UpdateType == LongPollServerUpdateType.UserIsTypingInChat)
                        this.SetUserIsTypingStatusWithDelayedReset(update.user_id, UsersService.Instance.GetCachedUser(update.user_id));
                    if (update.UpdateType == LongPollServerUpdateType.UserBecameOnline)
                    {
                        if (this._userStatus == null)
                            this._userStatus = new UserStatus();
                        this._userStatus.online = 1L;
                        this.RefreshUIPropertiesSafe();
                    }
                    if (update.UpdateType == LongPollServerUpdateType.UserBecameOffline)
                    {
                        if (this._userStatus == null)
                            this._userStatus = new UserStatus();
                        this._userStatus.online = 0L;
                        this._userStatus.time = (long)Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow, true);
                        this.RefreshUIPropertiesSafe();
                    }
                }
            }
            if (newOrRestoredMessages.Count <= 0 && readMessages.Count <= 0 && deletedMessagesIds.Count <= 0)
                return;
            
            int delayInMilliseconds = 0;
            IEnumerable<MessageViewModel> arg_221_0 = newOrRestoredMessages;
            Func<MessageViewModel, bool> arg_221_1 = new Func<MessageViewModel, bool>(m => m.Message.@out == 1);

            if (Enumerable.Any<MessageViewModel>(arg_221_0, arg_221_1))
            {
                delayInMilliseconds = 1500;
            }




            DelayedExecutorUtil.Execute((Action)(() => Deployment.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                double num1 = this._scroll == null ? 0.0 : this._scroll.VerticalOffset;
                Logger.Instance.Info("Applying new messages");
                if (this._skipped == 0)
                {
                    foreach (MessageViewModel messageViewModel in newOrRestoredMessages)
                    {
                        MessageViewModel newMessage = messageViewModel;
                        if (!this._messages.Any<MessageViewModel>((Func<MessageViewModel, bool>)(mes =>
                        {
                            if (mes.Message.mid == newMessage.Message.mid || mes.OutboundMessageVM != null && mes.OutboundMessageVM.DeliveredMessageId == (long)newMessage.Message.mid)
                                return true;
                            if (mes.SendStatus == OutboundMessageStatus.SendingNow)
                                return mes.Message.body == newMessage.Message.body;
                            return false;
                        })))
                        {
                            Logger.Instance.Info("Adding instant update new message {0}, viewModelHashCode={1}", (object)newMessage.Message.mid, (object)this.GetHashCode());
                            newMessage.IsInSelectionMode = this.IsInSelectionMode;
                            this._messages.AddOrdered<MessageViewModel>(newMessage, ConversationViewModel._comparisonFunc, true);
                            this.ResetUserIsTypingStatusIfNeeded(true);
                            this.ChatTypeStatusText = string.Empty;
                        }
                    }
                    if (newOrRestoredMessages.Count > 0 && this.IsOnScreen)
                    {
                        this.RemoveUnreadMessagesItem();
                        this.SetReadStatusIfNeeded();
                    }
                    if (!this.IsOnScreen)
                        this.EnsureUnreadItem();
                }
                foreach (long num2 in readMessages)
                {
                    long readMessageId = num2;
                    MessageViewModel messageViewModel = this._messages.FirstOrDefault<MessageViewModel>((Func<MessageViewModel, bool>)(m => (long)m.Message.mid == readMessageId));
                    if (messageViewModel != null)
                    {
                        messageViewModel.Message.read_state = 1;
                        messageViewModel.RefreshUIProperties();
                    }
                }
                foreach (long num2 in deletedMessagesIds)
                {
                    long deletedMessagesId = num2;
                    MessageViewModel messageViewModel = this._messages.FirstOrDefault<MessageViewModel>((Func<MessageViewModel, bool>)(m => (long)m.Message.mid == deletedMessagesId));
                    if (messageViewModel != null)
                        this._messages.Remove(messageViewModel);
                }
                if (((this._scroll == null ? 0 : (this._messages.Count > 0 ? 1 : 0)) & (atLeastOneNewMessage ? 1 : 0)) != 0 && num1 < 50.0)
                    this._scroll.ScrollToBottom(true, false);
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.NoMessages));
            }))), delayInMilliseconds);
        }

        internal void DeleteDialog()
        {
            BackendServices.MessagesService.DeleteDialog(this._userOrChatId, this._isChat, (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>)(res => { }));
            this.Messages.Clear();
        }

        internal void EnsureConversationIsUpToDate(bool force, long startMessageId = 0, Action<bool> callback = null)
        {
            if (!force && this._skipped > 0)
            {
                if (callback == null)
                    return;
                callback(true);
            }
            else if (startMessageId == -1L)
                this.LoadFromLastUnread(callback);
            else if (startMessageId > 0L)
                this.LoadFromMessageId((int)startMessageId, callback, force);
            else
                this.LoadMessagesAsyncWithParams(0, 8, false, false, callback, new int?(), false);
        }

        public void DisableEnableNotifications(Action<bool> resultCallback)
        {
            if (this._changingNotifications)
                return;
            this._changingNotifications = true;
            string notificationsUri = AppGlobalStateManager.Current.GlobalState.NotificationsUri;
            if (!string.IsNullOrEmpty(notificationsUri))
            {
                this.SetInProgress(true, "");
                AccountService.Instance.SetSilenceMode(notificationsUri, this.AreNotificationsDisabled ? 0 : -1, (Action<BackendResult<object, ResultCode>>)(res =>
                {
                    this._changingNotifications = false;
                    this.SetInProgress(false, "");
                    if (res.ResultCode == ResultCode.Succeeded)
                        this.AreNotificationsDisabled = !this.AreNotificationsDisabled;
                    resultCallback(res.ResultCode == ResultCode.Succeeded);
                }), this._isChat ? this._userOrChatId : 0L, !this._isChat ? this._userOrChatId : 0L);
            }
            else
            {
                this._changingNotifications = false;
                this.AreNotificationsDisabled = !this.AreNotificationsDisabled;
                resultCallback(true);
            }
        }

        internal void PinToStart()
        {
            if (this._isInProgressPinToStart || string.IsNullOrEmpty(this.UITitle))
                return;
            this._isInProgressPinToStart = true;
            this.SetInProgressMain(true, "");
            if (!this._isChat)
            {
                string error = CommonResources.Error;
                UsersService.Instance.GetUsersForTile(this._user.uid, (Action<BackendResult<List<User>, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
                {
                    if (res.ResultCode != ResultCode.Succeeded)
                    {
                        this._isInProgressPinToStart = false;
                        this.SetInProgressMain(false, "");
                        Execute.ExecuteOnUIThread((Action)(() => ExtendedMessageBox.ShowSafe(CommonResources.Error)));
                    }
                    else
                    {
                        string str = string.Format(CommonResources.DialogWithFrm, (object)res.ResultData[0].first_name);
                        List<string> imageUris = new List<string>();
                        imageUris.Add(res.ResultData[0].photo_max);
                        string title = str;
                        this.DoCreateTile(imageUris, title);
                    }
                }))));
            }
            else
                BackendServices.ChatService.GetChatInfo(this._userOrChatId, (Action<BackendResult<ChatInfo, ResultCode>>)(res =>
                {
                    if (res.ResultCode != ResultCode.Succeeded)
                    {
                        this._isInProgressPinToStart = false;
                        this.SetInProgressMain(false, "");
                        int num;
                        Execute.ExecuteOnUIThread((Action)(() => num = (int)MessageBox.Show(CommonResources.Error)));
                    }
                    else
                    {
                        List<string> list = res.ResultData.chat_participants.Select<ChatUser, string>((Func<ChatUser, string>)(c => c.photo_max)).ToList<string>();
                        if (!string.IsNullOrWhiteSpace(res.ResultData.chat.photo_200))
                            list.Insert(0, res.ResultData.chat.photo_200);
                        this.DoCreateTile(list, res.ResultData.chat.title);
                    }
                }));
        }

        private void DoCreateTile(List<string> imageUris, string title)
        {
            SecondaryTileCreator.CreateTileForConversation(this._userOrChatId, this._isChat, title, imageUris, (Action<bool>)(res =>
            {
                this._isInProgressPinToStart = false;
                this.SetInProgressMain(false, "");
                if (res)
                    return;
                Execute.ExecuteOnUIThread((Action)(() => ExtendedMessageBox.ShowSafe(CommonResources.Error)));
            }));
        }

        public void Handle(NotificationSettingsChangedEvent message)
        {
            if (message.ChatId != this.UserOrCharId || !this.IsChat)
                return;
            this.AreNotificationsDisabled = message.AreNotificationsDisabled;
            this.RefreshUIPropertiesSafe();
        }
        
        public void AddAttachmentsFromRepository()
        {
            WallPost wallPost = ParametersRepository.GetParameterForIdAndReset("WallPostAttachment") as WallPost;
            if (wallPost != null)
                this.OutboundMessageVm.AddWallPostAttachment(wallPost);
            GeoCoordinate geoCoordinate = ParametersRepository.GetParameterForIdAndReset("NewPositionToBeAttached") as GeoCoordinate;
            if (geoCoordinate != (GeoCoordinate)null)
                this.OutboundMessageVm.AddGeoAttachment(geoCoordinate.Latitude, geoCoordinate.Longitude);
            List<Stream> streamList1 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
            List<Stream> streamList2 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotosPreviews") as List<Stream>;
            ParametersRepository.GetParameterForIdAndReset("ChoosenPhotosSizes");
            if (streamList1 != null)
            {
                for (int index = 0; index < streamList1.Count; ++index)
                {
                    Stream stream = streamList1[index];
                    Stream previewStream = null;
                    if (streamList2 != null && streamList2.Count > index)
                        previewStream = streamList2[index];
                    this.OutboundMessageVm.AddPictureAttachment(stream, previewStream);
                }
            }
            Photo pickedPhoto = ParametersRepository.GetParameterForIdAndReset("PickedPhoto") as Photo;
            if (pickedPhoto != null)
                this.OutboundMessageVm.AddExistingPhotoAttachment(pickedPhoto);
            VKClient.Common.Backend.DataObjects.Video pickedVideo = ParametersRepository.GetParameterForIdAndReset("PickedVideo") as VKClient.Common.Backend.DataObjects.Video;
            if (pickedVideo != null)
                this.OutboundMessageVm.AddExistingVideoAttachment(pickedVideo);
            AudioObj pickedAudio = ParametersRepository.GetParameterForIdAndReset("PickedAudio") as AudioObj;
            if (pickedAudio != null)
                this.OutboundMessageVm.AddExistingAudioAttachment(pickedAudio);
            Doc pickedDocument = ParametersRepository.GetParameterForIdAndReset("PickedDocument") as Doc;
            if (pickedDocument != null)
                this.OutboundMessageVm.AddExistingDocAttachment(pickedDocument);
            Product product = ParametersRepository.GetParameterForIdAndReset("ShareProduct") as Product;
            if (product != null)
                this.OutboundMessageVm.AddProductAttachment(product, this.CanDettachProductAttachment);
            FileOpenPickerContinuationEventArgs continuationEventArgs = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
            if (continuationEventArgs != null && ((IEnumerable<StorageFile>)continuationEventArgs.Files).Any<StorageFile>() || ParametersRepository.Contains("PickedPhotoDocument"))
            {
                object parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("FilePickedType");
                StorageFile file = continuationEventArgs != null ? ((IEnumerable<StorageFile>)continuationEventArgs.Files).First<StorageFile>() : (StorageFile)ParametersRepository.GetParameterForIdAndReset("PickedPhotoDocument");
                VKClient.Common.Library.Posts.AttachmentType result;
                if (parameterForIdAndReset != null && Enum.TryParse<VKClient.Common.Library.Posts.AttachmentType>(parameterForIdAndReset.ToString(), out result))
                {
                    if (result != VKClient.Common.Library.Posts.AttachmentType.VideoFromPhone)
                    {
                        if (result == VKClient.Common.Library.Posts.AttachmentType.DocumentFromPhone || result == VKClient.Common.Library.Posts.AttachmentType.DocumentPhoto)
                            this.OutboundMessageVm.AddUploadDocAttachment(file);
                    }
                    else
                        this.OutboundMessageVm.AddUploadVideoAttachment(file);
                }
            }
            List<StorageFile> storageFileList1 = ParametersRepository.GetParameterForIdAndReset("ChosenDocuments") as List<StorageFile>;
            if (storageFileList1 != null && storageFileList1.Count > 0)
            {
                using (List<StorageFile>.Enumerator enumerator = storageFileList1.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                        this.OutboundMessageVm.AddUploadDocAttachment(enumerator.Current);
                }
            }
            List<StorageFile> storageFileList2 = ParametersRepository.GetParameterForIdAndReset("ChosenVideos") as List<StorageFile>;
            if (storageFileList2 == null || storageFileList2.Count <= 0)
                return;
            using (List<StorageFile>.Enumerator enumerator = storageFileList2.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    this.OutboundMessageVm.AddUploadVideoAttachment(enumerator.Current);
            }
        }
    }
}
