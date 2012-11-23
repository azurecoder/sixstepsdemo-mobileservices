using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using SixStepsVoteApp;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;

namespace SixStepsVoteApp
{
        
    [DataContract(Name = "Speakers")]
    public class Speaker
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }
        [DataMember(Name = "Name")]
        public string Name { get; set; }
        [DataMember(Name = "Info")]
        public string Info { get; set; }
        [DataMember(Name = "TalkTitle")]
        public string TalkTitle { get; set; }
        [DataMember(Name = "TalkDescription")]
        public string TalkDescription { get; set; }
        [DataMember(Name = "PictureUrl")]
        public string PictureUrl { get; set; }
        [DataMember(Name = "TalkDate")]
        public DateTime TalkDate { get; set; }

        public ImageSource Image
        {
            get
            {
                if (PictureUrl == null) return null;
                var utils = new PhotoBlobUtils(PictureUrl);
                return utils.GetPhotoImage();
            }
        }
    }

    public class SpeakerSampleData
    {
       
        public async void InsertSpeakerSampleData()
        {
            //speakers 9th
            var speaker1 = new Speaker()
            {
                Name = "Richard Conway",
                Info = "Richard is a Microsoft Windows Azure MVP, Insider, co-founder of Elastacloud and the UK Windows Azure Users Group",
                TalkTitle = "Windows Azure Mobile Services",
                TalkDescription = "In this talk Richard will describe the new Windows Azure Mobile Services framework. He will describe what Mobile Services is and why it's so useful for device and Windows 8 developers. In this talk he will show how to manipulate data in Mobile Services, send emails, notifications, use node.js packages, log errors and scale to meet increased user demand. He will show how Mobile Services can be used to add authentication through a variety of identity providers including Facebook, Twitter, Microsoft Accounts and Google!",
                PictureUrl = "richard.jpg",
                TalkDate = new DateTime(2012, 11, 9, 14, 45, 0)
            };
            var speaker2 = new Speaker()
            {
                Name = "Steve Plank",
                Info = "Steve is the Microsoft UK Windows Azure evangelist. He runs WA bootcamps and IT Pro camps across the country.",
                TalkTitle = "Windows Azure Active Directory",
                TalkDescription = "In this session Steve 'Planky' Plank will discuss all of the new possibilities with Windows Azure Active Directory. This will describe some of the scenarios beginning with Active Directory Federation Services and discussing some of the ways Virtual Machine networks can be used to build effective Active Directory backbones and premise hybrid networks. Steve will also be describing the new WAAD or Active Directory as a Service and show how it can be used with Office 365 and administered programmatically through the object graph.",
                PictureUrl = "planky.jpg",
                TalkDate = new DateTime(2012, 11, 9, 9, 30, 0)
            };
            var speaker3 = new Speaker()
            {
                Name = "Nuno Godinho",
                Info = "Nuno is a Windows Azure MVP and a senior architect for Aditi. He has over 10 years experience and is in international speaker on Windows Azure.",
                TalkTitle = "Windows Azure Media Services",
                TalkDescription = "Nuno will describe the workflow and APIs surrounding Windows Azure Media Services. In this session Nuno will show to take any media content transform and transcode it and send it to Windows Azure for distribution. Nuno will describe how to apply features such as Digital Rights Management in post processing on the file outputs.",
                PictureUrl = "nuno.jpg",
                TalkDate = new DateTime(2012, 11, 9, 13, 15, 0)
            };
            var speaker4 = new Speaker()
            {
                Name = "Stuart Lodge",
                Info = "Stuart is a developer who works in the device and monotouch space. He has spoken around the country on both Windows Azure and Devices and is considered as one of the foremost experts in this field.",
                TalkTitle = "Cross-Platform device development with Windows Azure",
                TalkDescription = "In this talk Stuart discusses how monotouch can be used to build client applications which can be written in C# and in a device independent manner to run across iOS, Windows Phone and Android platforms. Stuart will demonstrate how to use monotouch with Windows Azure to provide seamless continuity between device and cloud.",
                PictureUrl = "stuart.jpg",
                TalkDate = new DateTime(2012, 11, 9, 11, 15, 0)
            };
            var speaker5 = new Speaker()
            {
                Name = "Jay Conner",
                Info = "Jay is a freelance developer with expertise in Windows 8 and Windows Azure",
                TalkTitle = "Windows Azure and Windows 8",
                TalkDescription = "Jay is a developer formerly involved in creating the international pre-sales Azure helpdesk. His interest now lies in using Windows 8 and the Web API with Windows Azure. In his spare time he works on Social Nest a new kind of Social Media application.",
                PictureUrl = "planky.jpg",
                TalkDate = new DateTime(2012, 11, 9, 16, 15, 0)
            };
            // 8th November
            var speaker6 = new Speaker()
            {
                Name = "Andy Cross",
                Info = "Andy is the co-founder of UKWAUG, a Microsoft MVP and Insider. He has been using Windows Azure since the days of reddog and has the most widely read Windows Azure blog in the UK.",
                TalkTitle = ".NET Microframework and Windows Azure",
                TalkDescription = "In this talk Andy discusses how the .NET Micro Framework with its limited resources can be used to connect to Azure to perform meaningful tasks. Andy builds a lot of things from the ground up to show illustrate how storage, service bus and other things can be used from limited capability microchips.",
                PictureUrl = "andy.jpg",
                TalkDate = new DateTime(2012, 11, 8, 13, 15, 0)
            };
            var speaker7 = new Speaker()
            {
                Name = "Carlos Oliveira",
                Info = "Carlos is one of the founders of UKWAUG and runs the Manchester chapter. He runs a successful Azure and office 365 consultancy called Shaping Cloud which is based in Manchester.",
                TalkTitle = "Sheffield University: Environmental Projects on Azure",
                TalkDescription = "This talk describes a project undertaken by Shaping Cloud on behalf of Sheffield University for carbon capture purposes with Azure.",
                PictureUrl = "carlos.jpg",
                TalkDate = new DateTime(2012, 11, 8, 11, 15, 0)
            };
            var speakers = new List<Speaker>(new[] { speaker1, speaker2, speaker3, speaker4, speaker5, speaker6, speaker7 });
            await InsertBatch(speakers);
        }

        private async Task<MobileServiceCollectionView<Speaker>> InsertBatch(IEnumerable<Speaker> speakers, string tableName = "speakers", string idProperty = "Id")
        {
            // convert to an array in JSON
            var arr = new JsonArray();
            foreach (var speaker in speakers)
            {
                arr.Add(MobileServiceTableSerializer.Serialize(speaker));
            }

            // Now create a JSON body
            var body = new JsonObject() { { tableName, arr } };

            // insert!
            IJsonValue response = await Table.InsertAsync(body);

            await Task.Delay(TimeSpan.FromSeconds(5));

            return GetSpeakerView();
        }

        public async void InsertSpeaker(Speaker speaker)
        {
            var speakers = new List<Speaker>(new[] { speaker });
            await InsertBatch(speakers);
        }

        private IMobileServiceTable<Speaker> Table
        {
            get { return App.MobileService.GetTable<Speaker>(); }
        }

        public MobileServiceCollectionView<Speaker> GetSpeakerView()
        {
            return Table.OrderBy(a => a.Name).IncludeTotalCount().ToCollectionView();
        }

      

        public MobileServiceCollectionView<Speaker> GetSpeakersByDate(DateTime dt)
        {
            return Table.Where(a => a.TalkDate == dt).ToCollectionView();
        }

        public MobileServiceCollectionView<Speaker> GetAllSpeakers()
        {
            return Table.OrderBy(a => a.TalkDate).IncludeTotalCount().ToCollectionView();
        }
    }
}
