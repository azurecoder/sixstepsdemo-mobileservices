using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.WindowsAzure.MobileServices;
using SixStepsVoteApp.Common;
using Windows.Data.Json;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.PushNotifications;
using Windows.Security.Cryptography;
using Windows.System.Profile;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace SixStepsVoteApp
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class EventList : SixStepsVoteApp.Common.LayoutAwarePage
    {
        public EventList()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            EventListScroller.ItemsSource = Speakers;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

       
        public MobileServiceCollectionView<Speaker> Speakers
        {
            get
            {
                var speakers = new SpeakerSampleData();
                return speakers.GetAllSpeakers();
            }
        }

        #region New Speaker Insert

        private void SendNewSpeaker(object sender, RoutedEventArgs e)
        {
            DateTime speakerDate;
            if(!DateTime.TryParse(SpeakerDateTime.Text, out speakerDate))
            {
                ShowDialog("Date is not in the correct format. Please adjust and send form data again");
                return;
            }

            var speakers = new SpeakerSampleData();
            var speaker = new Speaker
                              {
                                  Name = SpeakerName.Text,
                                  Info = SpeakerInfo.Text,
                                  TalkTitle = SpeakerTalkTitle.Text,
                                  TalkDescription = SpeakerTalkDescription.Text,
                                  PictureUrl = Path.Combine(PhotoBlobUtils.BaseUri,SpeakerPictureUrl.Text),
                                  TalkDate = speakerDate
                              };
            speakers.InsertSpeaker(speaker);
            AddSpeakerForm.IsOpen = false;
            EventListScroller.ItemsSource = Speakers;
            //UpdateBadge(Speakers.Count);
        }

        private void OpenAddSpeakerForm(object sender, RoutedEventArgs e)
        {
            if (App.MobileService.CurrentUser == null)
            {
                ShowDialog("Please login before inserting into the database");
                return;
            }

            AddSpeakerForm.IsOpen = true;
        }

        private void CloseSpeakerForm(object sender, RoutedEventArgs e)
        {
            AddSpeakerForm.IsOpen = false;
        }

        #endregion

        #region Twitter/Microsoft Account Login

        public void LoginTwitter(object sender, RoutedEventArgs e)
        {
            LoginForm(MobileServiceAuthenticationProvider.Twitter);
        }

        public void LoginMicrosoftAccount(object sender, RoutedEventArgs e)
        {
            LoginForm(MobileServiceAuthenticationProvider.MicrosoftAccount);
        }

        private async void LoginForm(MobileServiceAuthenticationProvider provider)
        {
            if(App.MobileService.CurrentUser != null)
            {
                App.MobileService.Logout();
            }
            try
            {
                await App.MobileService.LoginAsync(provider);
                SetChannel();
            }
            catch (Exception exception)
            {
                ShowDialog(exception.ToString());
            }
        }

        #endregion

        #region Insert Batch 

        private void AddSampleData(object sender, RoutedEventArgs e)
        {
            if (App.MobileService.CurrentUser == null)
            {
                ShowDialog("Please login before inserting into the database");
                return;
            }
            var speakers = new SpeakerSampleData();
            speakers.InsertSpeakerSampleData();
            EventListScroller.ItemsSource = Speakers;
        }

        #endregion

        #region Helpers

        private void ShowDialog(string text)
        {
            var dialog = new Windows.UI.Popups.MessageDialog(text);
            dialog.ShowAsync();
        }

        private async static void SetChannel()
        {
            var pushNotifier = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

            var token = HardwareIdentification.GetPackageSpecificToken(null);
            // get this for the specific hardware to id the device being used
            string installationId = CryptographicBuffer.EncodeToBase64String(token.Id);

            var channel = new JsonObject
                         {
                             {"ChannelUri", JsonValue.CreateStringValue(pushNotifier.Uri)},
                             {"InstallationId", JsonValue.CreateStringValue(installationId)}
                         };

            await App.MobileService.GetTable("Channels").InsertAsync(channel);
        }

        public void RefreshView(object sender, RoutedEventArgs e)
        {
            EventListScroller.ItemsSource = Speakers;
        }

        //public void UpdateBadge(int count)
        //{
        //    var badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
        //    var badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
        //    badgeElement.SetAttribute("value", count.ToString());
        //    var badge = new BadgeNotification(badgeXml);
        //    BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
        //}

        #endregion

    }
}