﻿using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;

namespace Myra.Samples.TabControlSample
{
	public class GridTabPage : Grid
	{
		private readonly Window _window;

		public GridTabPage()
		{
			RowSpacing = 3;
			ColumnSpacing = 3;

			ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			ColumnsProportions.Add(new Proportion(ProportionType.Part, 1.0f));
			ColumnsProportions.Add(new Proportion(ProportionType.Part, 2.0f));
			ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 150.0f));
			ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			ColumnsProportions.Add(new Proportion(ProportionType.Fill));

			RowsProportions.Add(new Proportion(ProportionType.Auto));
			RowsProportions.Add(new Proportion(ProportionType.Auto));
			RowsProportions.Add(new Proportion(ProportionType.Part, 1.0f));
			RowsProportions.Add(new Proportion(ProportionType.Part, 1.0f));
			RowsProportions.Add(new Proportion(ProportionType.Pixels, 200.0f));
			RowsProportions.Add(new Proportion(ProportionType.Auto));
			RowsProportions.Add(new Proportion(ProportionType.Fill));

			// Create headers
			for (var i = 1; i < ColumnsProportions.Count; ++i)
			{
				var header = new TextBlock
				{
					Text = ColumnsProportions[i].ToString(),
					GridPositionX = i,
					GridPositionY = 0
				};

				Widgets.Add(header);
			}

			// Combo
			var combo = new ComboBox
			{
				GridPositionX = 1,
				GridPositionY = 1
			};

			combo.Items.Add(new ListItem("Red", Color.Red));
			combo.Items.Add(new ListItem("Green", Color.Green));
			combo.Items.Add(new ListItem("Blue", Color.Blue));
			Widgets.Add(combo);

			// Button
			var button = new Button
			{
				GridPositionX = 2,
				GridPositionY = 1,
				GridSpanX = 2,
				GridSpanY = 1,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Text = "This is 2 columns button"
			};

			button.Down += (s, a) =>
			{
				var messageBox = Dialog.CreateMessageBox("2C", "2 Columns Button pushed!");
				messageBox.ShowModal(Desktop);
			};

			Widgets.Add(button);

			// Button
			var button2 = new Button
			{
				GridPositionX = 2,
				GridPositionY = 2,
				GridSpanX = 1,
				GridSpanY = 2,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				Text = "This is 2 rows button"
			};

			button2.Down += (s, a) =>
			{
				var messageBox = Dialog.CreateMessageBox("2R", "2 Rows Button pushed!");
				messageBox.ShowModal(Desktop);
			};
			Widgets.Add(button2);

			var label = new TextBlock
			{
				Text =
					"Lorem ipsum [Green]dolor sit amet, [Red]consectetur adipisicing elit, sed do eiusmod [#AAAAAAAA]tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. [white]Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum!",
				VerticalSpacing = 0,
				TextColor = Color.AntiqueWhite,
				Wrap = true
			};

			var pane = new ScrollPane
			{
				Widget = label,
				WidthHint = 200,
				HeightHint = 200
			};

			_window = new Window
			{
				Title = "Text",
				Content = pane
			};

			var button3 = new Button
			{
				Text = "Show Window",
				GridPositionX = 4,
				GridPositionY = 3
			};
			Widgets.Add(button3);

			button3.Up += (sender, args) =>
			{
				_window.ShowModal(Desktop);
			};

			// Horizontal slider
			var hslider = new HorizontalSlider
			{
				GridPositionX = 3,
				GridPositionY = 2
			};
			Widgets.Add(hslider);

			// Horizontal slider value
			var hsliderValue = new TextBlock
			{
				GridPositionX = 4,
				GridPositionY = 2,
				Text = "HSlider Value: 0"
			};

			hslider.ValueChanged += (sender, args) =>
			{
				hsliderValue.Text = string.Format("HSlider Value: {0:0.00}", hslider.Value);
			};

			Widgets.Add(hsliderValue);

			var textBlock = new TextBlock
			{
				WidthHint = 125,
				Text = "This is textblock which spans for several lines to demonstrate row proportion set to Auto",
				GridPositionX = 4,
				GridPositionY = 1
			};
			Widgets.Add(textBlock);

			var checkBox = new CheckBox
			{
				Text = "This is a checkbox",
				GridPositionX = 3,
				GridPositionY = 3,
			};
			Widgets.Add(checkBox);

			// Spin buttons
			var textField = new TextField
			{
				GridPositionX = 5,
				GridPositionY = 1,
				WidthHint = 100
			};
			Widgets.Add(textField);

			var spinButton2 = new SpinButton
			{
				GridPositionX = 5,
				GridPositionY = 2,
				WidthHint = 100,
				Integer = true
			};
			Widgets.Add(spinButton2);

			// Progress bars
			var horizontalProgressBar = new HorizontalProgressBar
			{
				GridPositionX = 5,
				GridPositionY = 3,
				WidthHint = 100
			};
			Widgets.Add(horizontalProgressBar);

			var verticalProgressBar = new VerticalProgressBar
			{
				GridPositionX = 6,
				GridPositionY = 1,
				HeightHint = 100
			};
			Widgets.Add(verticalProgressBar);

			// List box
			var list = new ListBox
			{
				GridPositionX = 5,
				GridPositionY = 4
			};

			list.Items.Add(new ListItem("Red", Color.Red));
			list.Items.Add(new ListItem("Green", Color.Green));
			list.Items.Add(new ListItem("Blue", Color.Blue));
			Widgets.Add(list);

			// Vertical slider
			var vslider = new VerticalSlider
			{
				GridPositionX = 2,
				GridPositionY = 4
			};
			Widgets.Add(vslider);

			// Vertical slider value
			var vsliderValue = new TextBlock
			{
				GridPositionX = 4,
				GridPositionY = 4,
				Text = "VSlider Value: 0"
			};

			vslider.ValueChanged += (sender, args) =>
			{
				vsliderValue.Text = string.Format("VSlider Value: {0:0.00}", vslider.Value);
			};

			Widgets.Add(vsliderValue);

			var tree = new Tree
			{
				HasRoot = false,
				GridPositionX = 3,
				GridPositionY = 4
			};
			var node1 = tree.AddSubNode("node1");
			var node2 = node1.AddSubNode("node2");
			var node3 = node2.AddSubNode("node3");
			node3.AddSubNode("node4");
			node3.AddSubNode("node5");
			node2.AddSubNode("node6");

			Widgets.Add(tree);

			var textBlock2 = new TextBlock
			{
				Text = "This is long textblock",
				GridPositionX = 1,
				GridPositionY = 4
			};
			Widgets.Add(textBlock2);

			var hsplitPane = new HorizontalSplitPane
			{
				GridPositionX = 1,
				GridPositionY = 5
			};
			var hsplitPaneLabel1 = new TextBlock
			{
				Text = "Left"
			};
			hsplitPane.Widgets.Add(hsplitPaneLabel1);
			var hsplitPaneLabel2 = new TextBlock
			{
				Text = "Right"
			};
			hsplitPane.Widgets.Add(hsplitPaneLabel2);
			Widgets.Add(hsplitPane);

			var vsplitPane = new VerticalSplitPane
			{
				GridPositionX = 6,
				GridPositionY = 4
			};
			var vsplitPaneLabel1 = new TextBlock
			{
				Text = "Top"
			};
			vsplitPane.Widgets.Add(vsplitPaneLabel1);
			var vsplitPaneLabel2 = new TextBlock
			{
				Text = "Bottom"
			};
			vsplitPane.Widgets.Add(vsplitPaneLabel2);
			Widgets.Add(vsplitPane);

			for (var i = 1; i < RowsProportions.Count; ++i)
			{
				var header = new TextBlock
				{
					Text = RowsProportions[i].ToString(),
					GridPositionX = 0,
					GridPositionY = i
				};

				Widgets.Add(header);
			}
		}
	}
}