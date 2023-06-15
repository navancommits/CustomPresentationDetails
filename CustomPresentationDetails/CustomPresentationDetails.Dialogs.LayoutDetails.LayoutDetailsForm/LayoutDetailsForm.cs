using Sitecore;
using Sitecore.Data.Events;
using Sitecore.Data.Fields;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Sites;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using System;

namespace CustomPresentationDetails.Dialogs.LayoutDetails
{
    public class LayoutDetailsForm: Sitecore.Shell.Applications.ContentManager.Dialogs.LayoutDetails.LayoutDetailsForm
    {
        protected override void OnLoad(EventArgs e)
        {        
            Assert.CanRunApplication("Content Editor/Ribbons/Chunks/Layout");
            Assert.ArgumentNotNull((object)e, nameof(e));
            base.OnLoad(e);
            this.Tabs.OnChange += (EventHandler)((sender, args) => this.Refresh());
            if (!Context.ClientPage.IsEvent)
            {
                Sitecore.Data.Items.Item currentItem =GetCurrentItem();
                Assert.IsNotNull((object)currentItem, "Item not found");
                this.Layout = LayoutField.GetFieldValue(currentItem.Fields[FieldIDs.LayoutField]);
                Sitecore.Data.Fields.Field field = currentItem.Fields[FieldIDs.FinalLayoutField];
                this.LayoutDelta = !(currentItem.Name != "__Standard Values") ? field.GetStandardValue() : field.GetValue(false, false) ?? field.GetInheritedValue(false) ?? field.GetValue(false, false, true, false, false);
                this.ToggleVisibilityOfControlsOnFinalLayoutTab(currentItem);
                this.Refresh();
            }
            SiteContext site = Context.Site;
            if (site == null)
                return;
            site.Notifications.ItemSaved += new ItemSavedDelegate(this.ItemSavedNotification);
        }

        private void ItemSavedNotification(object sender, ItemSavedEventArgs args)
        {
            this.VersionCreated = true;
            this.ToggleVisibilityOfControlsOnFinalLayoutTab(args.Item);
            SheerResponse.SetDialogValue(this.GetDialogResult());
        }

        private enum TabType
        {
            /// <summary>The shared layout tab.</summary>
            Shared,
            /// <summary>The final layout tab.</summary>
            Final,
            /// <summary>The unknown tab.</summary>
            Unknown,
        }

        private LayoutDetailsForm.TabType ActiveTab
        {
            get
            {
                switch (this.Tabs.Active)
                {
                    case 0:
                        return LayoutDetailsForm.TabType.Shared;
                    case 1:
                        return LayoutDetailsForm.TabType.Final;
                    default:
                        return LayoutDetailsForm.TabType.Unknown;
                }
            }
        }

        private void RenderLayoutGridBuilder(string layoutValue, Sitecore.Web.UI.HtmlControls.Control renderingContainer)
        {
            string str = renderingContainer.ID + "LayoutGrid";
            CustomLayoutGridBuilder layoutGridBuilder = new CustomLayoutGridBuilder()
            {
                ID = str,
                Value = layoutValue,
                EditRenderingClick = "EditRendering(\"$Device\", \"$Index\")",
                EditPlaceholderClick = "EditPlaceholder(\"$Device\", \"$UniqueID\")",
                OpenDeviceClick = "OpenDevice(\"$Device\")",
                CopyToClick = "CopyDevice(\"$Device\")"
            };
            renderingContainer.Controls.Clear();
            layoutGridBuilder.BuildGrid((System.Web.UI.Control)renderingContainer);
            if (!Context.ClientPage.IsEvent)
                return;
            SheerResponse.SetOuterHtml(renderingContainer.ID, (System.Web.UI.Control)renderingContainer);
            SheerResponse.Eval("if (!scForm.browser.isIE) { scForm.browser.initializeFixsizeElements(); }");
        }

        private void Refresh() => this.RenderLayoutGridBuilder(this.GetActiveLayout(), this.ActiveTab == LayoutDetailsForm.TabType.Final ? (Sitecore.Web.UI.HtmlControls.Control)this.FinalLayoutPanel : (Sitecore.Web.UI.HtmlControls.Control)this.LayoutPanel);


        private static Sitecore.Data.Items.Item GetCurrentItem() => Client.ContentDatabase.GetItem(WebUtil.GetQueryString("id"), Language.Parse(WebUtil.GetQueryString("la")), Sitecore.Data.Version.Parse(WebUtil.GetQueryString("vs")));
    }
}
