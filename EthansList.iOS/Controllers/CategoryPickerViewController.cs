﻿using System;

using UIKit;
using System.Collections.Generic;
using EthansList.Shared;
using Foundation;

namespace ethanslist.ios
{
    public partial class CategoryPickerViewController : UIViewController
    {
        UITableView categoryTableView;
        CategoryTableViewSource categoryTableSource;
        FavoriteCategoryViewController favoritesVC;
        UIBarButtonItem Favorites;
        public Location SelectedCity { get; set;}
        public event EventHandler<EventArgs> PageReloaded;

        public CategoryPickerViewController()
            : base()
        {
        }

        public override void LoadView()
        {
            base.LoadView();

            this.View.BackgroundColor = ColorScheme.Clouds;
            this.Title = "Category";

            categoryTableView = new UITableView(this.View.Bounds, UITableViewStyle.Plain);
            categoryTableView.BackgroundColor = ColorScheme.Clouds;
            categoryTableView.AccessibilityIdentifier = "CategoryPickerTable";
            this.View.AddSubview(categoryTableView);

            categoryTableView.TranslatesAutoresizingMaskIntoConstraints = false;
            this.View.AddConstraints(new NSLayoutConstraint[]{
                NSLayoutConstraint.Create(categoryTableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.TopMargin, 1, 0),
                NSLayoutConstraint.Create(categoryTableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Left, 1, 0),
                NSLayoutConstraint.Create(categoryTableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Right, 1, 0),
                NSLayoutConstraint.Create(categoryTableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Bottom, 1, 0),
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            categoryTableSource = new CategoryTableViewSource(Categories.Groups);
            categoryTableView.Source = categoryTableSource;
            favoritesVC = new FavoriteCategoryViewController(this);

            Favorites = new UIBarButtonItem (
                UIImage.FromBundle("favorites.png"),
                UIBarButtonItemStyle.Plain,
                (object sender, EventArgs e) => {
                    if (favoritesVC.ViewedPreviously)
                    {
                        if (this.PageReloaded != null)
                            this.PageReloaded(this, new EventArgs());
                        else
                            favoritesVC.Favorites = AppDelegate.databaseConnection.GetAllFavoriteCategoriesAsync().Result;
                    }

                    this.PresentModalViewController(favoritesVC, true);
            });

            categoryTableSource.Favorited += delegate { favoritesVC.ViewedPreviously = true; };

            NavigationItem.RightBarButtonItem = Favorites;
            favoritesVC.FavoriteSelected += FavoritesVC_Selected;

            categoryTableSource.Selected += CategoryTableSource_Selected;

        }

        void FavoritesVC_Selected (object sender, FavoriteSelectedEventArgs e)
        {
            this.DismissModalViewController(true);
            Console.WriteLine (e.Selected.CategoryValue);
            CategoryTableSource_Selected(this, new CategorySelectedEventArgs() {SelectedCat = new KeyValuePair<string, string>(e.Selected.CategoryKey,e.Selected.CategoryValue)});
        }

        void CategoryTableSource_Selected (object sender, CategorySelectedEventArgs e)
        {
            var searchViewController = new SearchViewController();

            searchViewController.Location = SelectedCity;
            searchViewController.Category = e.SelectedCat;

            this.ShowViewController(searchViewController, this);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }

    public class CategoryTableViewSource : UITableViewSource
    {
        List<CatTableGroup> categories;
        const string cellID = "cellID";
        public event EventHandler<CategorySelectedEventArgs> Selected;
        public event EventHandler<CategorySelectedEventArgs> Favorited;

        public CategoryTableViewSource(List<CatTableGroup> categories)
        {
            this.categories = categories;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return categories.Count;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return categories[(int)section].Items.Count;
        }

        public override UIView GetViewForHeader(UITableView tableView, nint section)
        {
            var label = new UILabel(new CoreGraphics.CGRect(0,0,tableView.Bounds.Width, 32));
            label.AttributedText = new NSAttributedString("  " + categories[(int)section].Name, Constants.HeaderAttributes);

            label.BackgroundColor = ColorScheme.Silver;

            return label;
        }

        public override nfloat GetHeightForHeader(UITableView tableView, nint section)
        {
            return 32;
        }

        public override UITableViewCell GetCell(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(cellID);
            if (cell == null)
                cell = new UITableViewCell(UITableViewCellStyle.Default, cellID);

            cell.TextLabel.AttributedText = new NSAttributedString(categories[indexPath.Section].Items[indexPath.Row].Value, Constants.CityPickerCellAttributes);
            cell.BackgroundColor = ColorScheme.Clouds;

            return cell;
        }

        public override void RowSelected(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            if (this.Selected != null)
                this.Selected(this, new CategorySelectedEventArgs()
                    { 
                        SelectedCat = categories[indexPath.Section].Items[indexPath.Row], 
                    });
        }

        public override nfloat GetHeightForRow(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            return Constants.CityPickerRowHeight;
        }

        public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var item = categories[indexPath.Section].Items[indexPath.Row];
            var exists = AppDelegate.databaseConnection.FavoriteCategoryAlreadyPresent(item.Key);

            string title = exists ? "\u2605" : "\u2606";

            var favorite = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, title, async delegate {
                tableView.SetEditing(false,true);
                if (exists)
                {
                    await AppDelegate.databaseConnection.DeleteFavoriteCategoryAsync(item.Key);
                }
                else
                {
                    await AppDelegate.databaseConnection.AddNewFavoriteCategoryAsync(item.Key, item.Value);
                }
                Console.WriteLine (AppDelegate.databaseConnection.StatusMessage);

                if (this.Favorited != null)
                    this.Favorited(this, new CategorySelectedEventArgs(){SelectedCat = item});
                        
            });
            favorite.BackgroundColor = ColorScheme.Carrot;

            return new UITableViewRowAction[]{ favorite };
        }
    }

    public class CategorySelectedEventArgs : EventArgs
    {
        public KeyValuePair<string, string> SelectedCat {get;set;}
    }
}


