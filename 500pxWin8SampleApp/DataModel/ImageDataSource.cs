using _500pxWin8SampleApp.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace _500pxWin8SampleApp.DataModel
{
    /// <summary>
    /// Base class for <see cref="ImageDataItem"/> and <see cref="ImageDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class ImageDataCommon : BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public ImageDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(ImageDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class ImageDataItem : ImageDataCommon
    {
        public ImageDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, ImageDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private ImageDataGroup _group;
        public ImageDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class ImageDataGroup : ImageDataCommon
    {
        public ImageDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<ImageDataItem> _items = new ObservableCollection<ImageDataItem>();
        public ObservableCollection<ImageDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<ImageDataItem> _topItem = new ObservableCollection<ImageDataItem>();
        public ObservableCollection<ImageDataItem> TopItems
        {
            get { return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// ImageDataSource initializes with placeholder data rather than live production
    /// data so that Product data is provided at both design-time and run-time.
    /// </summary>
    public sealed class ImageDataSource
    {
        private static ImageDataSource _ImageDataSource = new ImageDataSource();

        private ObservableCollection<ImageDataGroup> _allGroups = new ObservableCollection<ImageDataGroup>();
        public ObservableCollection<ImageDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<ImageDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");

            return _ImageDataSource.AllGroups;
        }

        public static ImageDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _ImageDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static ImageDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _ImageDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public ImageDataSource()
        {
            // Get the "Fresh Today" photos
            this.AddGroup("popular", "Popular");
            this.AddGroup("editors", "Editors Picks");
            this.AddGroup("fresh_today", "Fresh Today");
            this.AddGroup("fresh_yesterday", "Fresh Yesterday");
            this.AddGroup("fresh_week", "Fresh This Week");
            this.AddGroup("upcoming", "Upcoming");
            
        }

        private void AddGroup(string id, string name)
        { 
            var api = new FiveHundredPxAPIClient("2c6841a7a043af073d5e4f63eefec79029601b03", new JsonDataTranslator());
            var result = api.Get(string.Format("/v1/photos?feature={0}&sort=created_at&image_size=4&include_store=store_download&include_states=voted", id));

            result.Completed = delegate(IAsyncOperation<dynamic> asyncAction, AsyncStatus asyncStatus)
            {
                var data = asyncAction.GetResults();

                var group = new ImageDataGroup(id,
                                    name,
                                    "",
                                    data.photos[0].image_url.ToString(),
                                    data.photos[0].description.ToString());

                foreach (var photo in data.photos)
                {
                    group.Items.Add(new ImageDataItem(photo.id.ToString(),
                                            photo.name.ToString(),
                                            photo.user.fullname.ToString(),
                                            photo.image_url.ToString(),
                                            photo.description.ToString(),
                                            photo.description.ToString(),
                                            group));
                }
                this.AllGroups.Add(group);
            };

        }
    }
}
