using Sitecore;
using Sitecore.Data.Databases;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Layouts;
using Sitecore.Pipelines.RenderLayoutGridRendering;
using Sitecore.Reflection;
using Sitecore.Resources;
using Sitecore.Web.UI;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XmlControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace CustomPresentationDetails.Dialogs.LayoutDetails
{
    public class CustomLayoutGridBuilder
    {
        private string @class;
        /// <summary>The copy to click.</summary>
        private string copyToClick;
        /// <summary>The edit placeholder click.</summary>
        private string editPlaceholderClick;
        /// <summary>The edit rendering click.</summary>
        private string editRenderingClick;
        /// <summary>The identifier.</summary>
        private string id;
        /// <summary>The open device click.</summary>
        private string openDeviceClick;
        /// <summary>The value.</summary>
        private string value;

        /// <summary>
        /// Initialize new instance of <see cref="T:Sitecore.Shell.Web.UI.LayoutGridBuilder" />.
        /// </summary>
        public CustomLayoutGridBuilder() => this.DatabaseHelper = new DatabaseHelper();

        /// <summary>
        /// The instance of <see cref="P:Sitecore.Shell.Web.UI.LayoutGridBuilder.DatabaseHelper" />.
        /// </summary>
        protected DatabaseHelper DatabaseHelper { get; set; }

        /// <summary>Gets or sets the class.</summary>
        /// <value>The class.</value>
        public string Class
        {
            get => StringUtil.GetString(this.@class);
            set
            {
                Assert.ArgumentNotNull((object)value, nameof(value));
                this.@class = value;
            }
        }

        /// <summary>Gets or sets the copy to click.</summary>
        /// <value>The copy to click.</value>
        public string CopyToClick
        {
            get => StringUtil.GetString(this.copyToClick);
            set
            {
                Assert.ArgumentNotNullOrEmpty(value, nameof(value));
                this.copyToClick = value;
            }
        }

        /// <summary>Gets or sets the edit placeholder click.</summary>
        /// <value>The edit placeholder click.</value>
        public string EditPlaceholderClick
        {
            get => StringUtil.GetString(this.editPlaceholderClick);
            set
            {
                Assert.ArgumentNotNull((object)value, nameof(value));
                this.editPlaceholderClick = value;
            }
        }

        /// <summary>Gets or sets the edit rendering click.</summary>
        /// <value>The edit rendering click.</value>
        public string EditRenderingClick
        {
            get => StringUtil.GetString(this.editRenderingClick);
            set
            {
                Assert.ArgumentNotNull((object)value, nameof(value));
                this.editRenderingClick = value;
            }
        }

        /// <summary>Gets or sets the ID.</summary>
        /// <value>The Identifier.</value>
        public string ID
        {
            get => StringUtil.GetString(this.id);
            set
            {
                Assert.ArgumentNotNullOrEmpty(value, nameof(value));
                this.id = value;
            }
        }

        /// <summary>Gets or sets the open device click.</summary>
        /// <value>The open device click.</value>
        public string OpenDeviceClick
        {
            get => StringUtil.GetString(this.openDeviceClick);
            set
            {
                Assert.ArgumentNotNull((object)value, nameof(value));
                this.openDeviceClick = value;
            }
        }

        /// <summary>Gets or sets the value.</summary>
        /// <value>The value.</value>
        public string Value
        {
            get => StringUtil.GetString(this.value);
            set
            {
                Assert.ArgumentNotNull((object)value, nameof(value));
                this.value = value;
            }
        }
        public void BuildGrid(System.Web.UI.Control parent)
        {
            Assert.ArgumentNotNull((object)parent, nameof(parent));
            GridPanel gridPanel = new GridPanel();
            parent.Controls.Add((System.Web.UI.Control)gridPanel);
            gridPanel.RenderAs = RenderAs.Literal;
            gridPanel.Width = Unit.Parse("100%");
            gridPanel.Attributes["Class"] = this.Class;
            gridPanel.Attributes["CellSpacing"] = "2";
            gridPanel.Attributes["id"] = this.ID;
            LayoutDefinition layout = (LayoutDefinition)null;
            string xml = StringUtil.GetString(this.Value);
            if (xml.Length > 0)
                layout = LayoutDefinition.Parse(xml);
            foreach (DeviceItem deviceItem in Client.ContentDatabase.Resources.Devices.GetAll())
                this.BuildDevice(gridPanel, layout, deviceItem);
        }

        private void BuildPlaceholder(
      Border border,
      DeviceDefinition deviceDefinition,
      PlaceholderDefinition placeholderDefinition)
        {
            Assert.ArgumentNotNull((object)border, nameof(border));
            Assert.ArgumentNotNull((object)deviceDefinition, nameof(deviceDefinition));
            Assert.ArgumentNotNull((object)placeholderDefinition, nameof(placeholderDefinition));
            string metaDataItemId = placeholderDefinition.MetaDataItemId;
            Border child1 = new Border();
            border.Controls.Add((System.Web.UI.Control)child1);
            string str1 = StringUtil.GetString(this.EditPlaceholderClick).Replace("$Device", deviceDefinition.ID).Replace("$UniqueID", placeholderDefinition.UniqueId);
            Assert.IsNotNull((object)metaDataItemId, "placeholder id");
            Item itemByPathOrId = this.DatabaseHelper.GetItemByPathOrId(Client.ContentDatabase, metaDataItemId);
            string str2;
            if (itemByPathOrId != null)
            {
                string displayName = itemByPathOrId.DisplayName;
                str2 = Images.GetImage(itemByPathOrId.Appearance.Icon, 16, 16, "absmiddle", "0px 4px 0px 0px") + displayName;
            }
            else
                str2 = Images.GetImage("Imaging/16x16/layer_blend.png", 16, 16, "absmiddle", "0px 4px 0px 0px") + placeholderDefinition.Key;
            if (!string.IsNullOrEmpty(str1))
            {
                child1.RollOver = true;
                child1.Class = "scRollOver";
                child1.Click = str1;
            }
            Sitecore.Web.UI.HtmlControls.Literal child2 = new Sitecore.Web.UI.HtmlControls.Literal("<div style=\"padding:2\">" + str2 + "</div>");
            child1.Controls.Add((System.Web.UI.Control)child2);
        }

        /// <summary>Builds the rendering.</summary>
        /// <param name="border">The border.</param>
        /// <param name="deviceDefinition">The device definition.</param>
        /// <param name="renderingDefinition">The rendering definition.</param>
        /// <param name="index">The index.</param>
        /// <param name="conditionsCount">
        /// The number of conditions for the rendering.
        /// </param>
        private void BuildRendering(
          Border border,
          DeviceDefinition deviceDefinition,
          RenderingDefinition renderingDefinition,
          int index,
          int conditionsCount)
        {
            Assert.ArgumentNotNull((object)border, nameof(border));
            Assert.ArgumentNotNull((object)deviceDefinition, nameof(deviceDefinition));
            Assert.ArgumentNotNull((object)renderingDefinition, nameof(renderingDefinition));
            string itemId = renderingDefinition.ItemID;
            if (itemId == null)
                return;
            Item obj = Client.ContentDatabase.GetItem(itemId);
            if (obj == null)
                return;
            string displayName = obj.DisplayName;
            string icon = obj.Appearance.Icon;
            string empty = string.Empty;
            string initialMarkup = Images.GetImage(icon, 16, 16, "absmiddle", "0px 4px 0px 0px") + displayName;
            if (empty.Length > 0 && empty != "content")
                initialMarkup = initialMarkup + " " + Translate.Text("in") + " " + empty + ".";
            if (conditionsCount > 1)
                initialMarkup += string.Format("<span class=\"{0}\">{1}</span>", conditionsCount > 9 ? (object)"scConditionContainer scLongConditionContainer" : (object)"scConditionContainer", (object)conditionsCount);
            string str1 = RenderLayoutGridRenderingPipeline.Run(renderingDefinition, initialMarkup);
            Border child1 = new Border();
            border.Controls.Add((System.Web.UI.Control)child1);
            string str2 = StringUtil.GetString(this.EditRenderingClick).Replace("$Device", deviceDefinition.ID).Replace("$Index", index.ToString());
            if (!string.IsNullOrEmpty(str2))
            {
                child1.RollOver = true;
                child1.Class = "scRollOver";
                child1.Click = str2;
            }
            Sitecore.Web.UI.HtmlControls.Literal child2 = new Sitecore.Web.UI.HtmlControls.Literal("<div class='scRendering' style='padding:2px;position:relative'>" + str1 + "</div>");
            child1.Controls.Add((System.Web.UI.Control)child2);
        }

        private void BuildDevice(GridPanel grid, LayoutDefinition layout, DeviceItem deviceItem)
        {
            Assert.ArgumentNotNull((object)grid, nameof(grid));
            Assert.ArgumentNotNull((object)deviceItem, nameof(deviceItem));
            XmlControl child = new XmlControl();

            if (Sitecore.Context.User.Profile.UserName.ToLowerInvariant() == "sitecore\\admin")
            {
                child = string.IsNullOrEmpty(this.OpenDeviceClick) ? Resource.GetWebControl("LayoutFieldDeviceReadOnly") as XmlControl : Resource.GetWebControl("LayoutFieldDevice") as XmlControl;

            }
            else
            {
                child = string.IsNullOrEmpty(this.OpenDeviceClick) ? Resource.GetWebControl("LayoutFieldDeviceReadOnly") as XmlControl : Resource.GetWebControl("CustomLayoutFieldDevice") as XmlControl;
            }
            Assert.IsNotNull((object)child, typeof(XmlControl));
            grid.Controls.Add((System.Web.UI.Control)child);
            string str1 = StringUtil.GetString(this.OpenDeviceClick).Replace("$Device", deviceItem.ID.ToString());
            string str2 = StringUtil.GetString(this.CopyToClick).Replace("$Device", deviceItem.ID.ToString());
            ReflectionUtil.SetProperty((object)child, "DeviceName", (object)deviceItem.DisplayName);
            ReflectionUtil.SetProperty((object)child, "DeviceIcon", (object)deviceItem.InnerItem.Appearance.Icon);
            
            ReflectionUtil.SetProperty((object)child, "DblClick", (object)str1);
            ReflectionUtil.SetProperty((object)child, "Copy", (object)str2);
            
            string str3 = "<span style=\"color:#999999\">" + Translate.Text("[No layout specified]") + "</span>";
            int index = 0;
            int num = 0;
            System.Web.UI.Control parent1 = child["ControlsPane"] as System.Web.UI.Control;
            System.Web.UI.Control parent2 = child["PlaceholdersPane"] as System.Web.UI.Control;
            if (layout != null)
            {
                DeviceDefinition device = layout.GetDevice(deviceItem.ID.ToString());
                string layout1 = device.Layout;
                if (!string.IsNullOrEmpty(layout1))
                {
                    Item obj = Client.ContentDatabase.GetItem(layout1);
                    if (obj != null)
                        str3 = Images.GetImage(obj.Appearance.Icon, 16, 16, "absmiddle", "0px 4px 0px 0px") + obj.DisplayName;
                }
                ArrayList renderings = device.Renderings;
                if (renderings != null && renderings.Count > 0)
                {
                    Border border = new Border();
                    Context.ClientPage.AddControl(parent1, (System.Web.UI.Control)border);
                    foreach (RenderingDefinition renderingDefinition in renderings)
                    {
                        int conditionsCount = 0;
                        if (renderingDefinition.Rules != null && !renderingDefinition.Rules.IsEmpty)
                            conditionsCount = renderingDefinition.Rules.Elements((XName)"rule").Count<XElement>();
                        this.BuildRendering(border, device, renderingDefinition, index, conditionsCount);
                        ++index;
                    }
                }
                ArrayList placeholders = device.Placeholders;
                if (placeholders != null && placeholders.Count > 0)
                {
                    Border border = new Border();
                    Context.ClientPage.AddControl(parent2, (System.Web.UI.Control)border);
                    foreach (PlaceholderDefinition placeholderDefinition in placeholders)
                    {
                        this.BuildPlaceholder(border, device, placeholderDefinition);
                        ++num;
                    }
                }
            }
            ReflectionUtil.SetProperty((object)child, "LayoutName", (object)str3);
            if (index == 0)
                Context.ClientPage.AddControl(parent1, (System.Web.UI.Control)new LiteralControl("<span style=\"color:#999999\">" + Translate.Text("[No renderings specified.]") + "</span>"));
            if (num != 0)
                return;
            Context.ClientPage.AddControl(parent2, (System.Web.UI.Control)new LiteralControl("<span style=\"color:#999999\">" + Translate.Text("[No placeholder settings were specified]") + "</span>"));
        }

    }
}
