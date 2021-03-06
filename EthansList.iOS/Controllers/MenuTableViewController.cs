using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace ethanslist.ios
{
	partial class MenuTableViewController : UITableViewController
	{
		public MenuTableViewController (IntPtr handle) : base (handle)
		{
		}

        public override void LoadView()
        {
            base.LoadView();

            this.TableView.BackgroundColor = ColorScheme.Clouds;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var cityPickVC = Storyboard.InstantiateViewController("CityPickerViewController");
            ShowDetailViewController(cityPickVC, this);
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);

            var sections = new string[]{ "Search Craigslist", "Recent Cities", "Saved Searches", "Saved Postings" };

            this.TableView.Source = new MenuTableViewSource (this, sections);
        }

        public class MenuTableViewSource : UITableViewSource
        {
            private UITableViewController controller;
            private string[] data;

            static string CELL_ID = "cellid";

            public MenuTableViewSource(UITableViewController controller, string[] data)
            {
                this.data = data;
                this.controller = controller;
            }

            public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell (CELL_ID, indexPath);

                cell.TextLabel.AttributedText = new NSAttributedString(data[indexPath.Row], Constants.LabelAttributes);

                return cell;
            }

            public override nint RowsInSection (UITableView tableview, nint section)
            {
                return data.Length;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                switch (indexPath.Row)
                {
                    case 0: //Search Cities
                        var cityPickVC = controller.Storyboard.InstantiateViewController ("CityPickerViewController");
                        controller.ShowDetailViewController (cityPickVC, controller);
                        break;
                    case 1://recent searches
                        var recentVC = controller.Storyboard.InstantiateViewController("RecentCitiesTableViewController");
                        controller.ShowDetailViewController(recentVC, controller);
                        break;
                    case 2://Saved Searches
                        var searchesVC = controller.Storyboard.InstantiateViewController ("SavedSearchesTableViewController");
                        controller.ShowDetailViewController (searchesVC, controller);
                        break;
                    case 3://Saved Listings
                        var listingsVC = controller.Storyboard.InstantiateViewController ("SavedPostingsTableViewController");
                        controller.ShowDetailViewController (listingsVC, controller);
                        break;
                }
            }
        }
	}
}
