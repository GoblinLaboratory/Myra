﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.Text;
using Myra.Utility;

namespace Myra.Graphics2D.UI
{
	public class Desktop
	{
		private RenderContext _renderContext;

		private bool _layoutDirty = true;
		private Rectangle _bounds;
		private bool _widgetsDirty = true;
		private Widget _focusedWidget;
		private readonly List<Widget> _widgetsCopy = new List<Widget>();
		private readonly List<Widget> _reversedWidgetsCopy = new List<Widget>();
		protected readonly ObservableCollection<Widget> _widgets = new ObservableCollection<Widget>();
		private readonly List<Widget> _focusableWidgets = new List<Widget>();
		private int? _focusedWidgetIndex;
		private Widget _modalWidget;
		private HorizontalMenu _menuBar;

		public Point MousePosition { get; private set; }
		public int MouseWheel { get; private set; }
		public MouseState MouseState { get; private set; }
		public KeyboardState KeyboardState { get; private set; }
		public HorizontalMenu MenuBar
		{
			get
			{
				return _menuBar;
			}

			set
			{
				if (_menuBar == value)
				{
					return;
				}

				_menuBar = value;
			}
		}

		private IEnumerable<Widget> WidgetsCopy
		{
			get
			{
				UpdateWidgetsCopy();
				return _widgetsCopy;
			}
		}

		private IEnumerable<Widget> ReversedWidgetsCopy
		{
			get
			{
				UpdateWidgetsCopy();
				return _reversedWidgetsCopy;
			}
		}

		public ObservableCollection<Widget> Widgets
		{
			get { return _widgets; }
		}

		public Rectangle Bounds
		{
			get { return _bounds; }

			set
			{
				if (value == _bounds)
				{
					return;
				}

				_bounds = value;
				InvalidateLayout();
			}
		}

		public Widget ContextMenu { get; private set; }

		public Widget ModalWidget
		{
			get { return _modalWidget; }

			set
			{
				if (_modalWidget == value)
				{
					return;
				}

				if (_modalWidget != null)
				{
					_widgets.Remove(_modalWidget);
				}

				_modalWidget = value;

				if (value != null && !_widgets.Contains(value))
				{
					_widgets.Add(value);
					FocusedWidget = value;
				}
			}
		}

		public Widget FocusedWidget
		{
			get { return _focusedWidget; }

			set
			{
				if (value == _focusedWidget)
				{
					return;
				}

				if (_focusedWidget != null)
				{
					_focusedWidget.IterateFocusable(w => w.IsFocused = false);
					_focusedWidgetIndex = null;
				}

				_focusedWidget = value;

				if (_focusedWidget != null)
				{
					_focusedWidget.IterateFocusable(w => w.IsFocused = true);
					_focusedWidgetIndex = _focusableWidgets.IndexOf(_focusedWidget);
				}
			}
		}

		public event EventHandler<GenericEventArgs<Point>> MouseMoved;
		public event EventHandler<GenericEventArgs<MouseButtons>> MouseDown;
		public event EventHandler<GenericEventArgs<MouseButtons>> MouseUp;

		public event EventHandler<GenericEventArgs<float>> MouseWheelChanged;

		public event EventHandler<GenericEventArgs<char>> KeyPressed;
		public event EventHandler<GenericEventArgs<Keys>> KeyUp;
		public event EventHandler<GenericEventArgs<Keys>> KeyDown;

		public event EventHandler<ContextMenuClosingEventArgs> ContextMenuClosing;
		public event EventHandler<GenericEventArgs<Widget>> ContextMenuClosed;

		public Desktop()
		{
			_widgets.CollectionChanged += WidgetsOnCollectionChanged;
		}

		private void InputOnMouseDown()
		{
			if (ContextMenu != null && !ContextMenu.Bounds.Contains(MousePosition))
			{
				var ev = ContextMenuClosing;
				if (ev != null)
				{
					var args = new ContextMenuClosingEventArgs(ContextMenu);
					ev(this, args);

					if (args.Cancel)
					{
						return;
					}
				}

				HideContextMenu();
			}
		}

		public void ShowContextMenu(Widget menu, Point position)
		{
			if (menu == null)
			{
				throw new ArgumentNullException("menu");
			}

			HideContextMenu();

			ContextMenu = menu;

			if (ContextMenu != null)
			{
				ContextMenu.HorizontalAlignment = HorizontalAlignment.Left;
				ContextMenu.VerticalAlignment = VerticalAlignment.Top;

				ContextMenu.XHint = position.X;
				ContextMenu.YHint = position.Y;

				ContextMenu.Visible = true;

				_widgets.Add(ContextMenu);
			}
		}

		public void HideContextMenu()
		{
			if (ContextMenu == null)
			{
				return;
			}

			_widgets.Remove(ContextMenu);
			ContextMenu.Visible = false;

			var ev = ContextMenuClosed;
			if (ev != null)
			{
				ev(this, new GenericEventArgs<Widget>(ContextMenu));
			}

			ContextMenu = null;
		}

		private void WidgetsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (Widget w in args.NewItems)
				{
					w.Desktop = this;
					w.MeasureChanged += WOnMeasureChanged;
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (Widget w in args.OldItems)
				{
					w.MeasureChanged -= WOnMeasureChanged;
					w.Desktop = null;
				}
			}

			InvalidateLayout();
			_widgetsDirty = true;
		}

		private void WOnMeasureChanged(object sender, EventArgs eventArgs)
		{
			InvalidateLayout();
		}

		public void Render()
		{
			if (Bounds.IsEmpty)
			{
				return;
			}

			UpdateInput();
			UpdateLayout();

			if (_renderContext == null)
			{
				var _spriteBatch = new SpriteBatch(MyraEnvironment.GraphicsDevice);
				_renderContext = new RenderContext
				{
					Batch = _spriteBatch
				};
			}

			var oldScissorRectangle = _renderContext.Batch.GraphicsDevice.ScissorRectangle;

			_renderContext.Batch.BeginUI();

			_renderContext.Batch.GraphicsDevice.ScissorRectangle = Bounds;
			_renderContext.View = Bounds;

			foreach (var widget in WidgetsCopy)
			{
				if (widget.Visible)
				{
					widget.Render(_renderContext);
				}
			}

			_renderContext.Batch.End();
			_renderContext.Batch.GraphicsDevice.ScissorRectangle = oldScissorRectangle;
		}

		public void InvalidateLayout()
		{
			_layoutDirty = true;
		}

		public void UpdateLayout()
		{
			if (!_layoutDirty)
			{
				return;
			}

			ProcessWidgets();

			foreach (var widget in WidgetsCopy)
			{
				if (widget.Visible)
				{
					widget.Layout(_bounds);
				}
			}

			_layoutDirty = false;
		}

		public int CalculateTotalWidgets(bool visibleOnly)
		{
			var result = 0;
			foreach (var w in _widgets)
			{
				if (visibleOnly && !w.Visible)
				{
					continue;
				}

				++result;

				var asContainer = w as Container;
				if (asContainer != null)
				{
					result += asContainer.CalculateTotalChildCount(visibleOnly);
				}
			}

			return result;
		}

		public void HandleButton(ButtonState buttonState, ButtonState lastState, MouseButtons buttons)
		{
			if (buttonState == ButtonState.Pressed && lastState == ButtonState.Released)
			{
				var ev = MouseDown;
				if (ev != null)
				{
					ev(this, new GenericEventArgs<MouseButtons>(buttons));
				}

				InputOnMouseDown();
				ReversedWidgetsCopy.HandleMouseDown(buttons);
			}
			else if (buttonState == ButtonState.Released && lastState == ButtonState.Pressed)
			{
				var ev = MouseUp;
				if (ev != null)
				{
					ev(this, new GenericEventArgs<MouseButtons>(buttons));
				}

				ReversedWidgetsCopy.HandleMouseUp(buttons);
			}
		}

		public void UpdateInput()
		{
			var lastState = MouseState;

			MouseState = Mouse.GetState();
			MousePosition = new Point(MouseState.X, MouseState.Y);
			MouseWheel = MouseState.ScrollWheelValue;

			if (MouseState.X != lastState.X || MouseState.Y != lastState.Y)
			{
				var ev = MouseMoved;
				if (ev != null)
				{
					ev(this, new GenericEventArgs<Point>(MousePosition));
				}

				ReversedWidgetsCopy.HandleMouseMovement(MousePosition);
			}

			HandleButton(MouseState.LeftButton, lastState.LeftButton, MouseButtons.Left);
			HandleButton(MouseState.MiddleButton, lastState.MiddleButton, MouseButtons.Middle);
			HandleButton(MouseState.RightButton, lastState.RightButton, MouseButtons.Right);

			var lastKeyboardState = KeyboardState;
			KeyboardState = Keyboard.GetState();

			var pressedKeys = KeyboardState.GetPressedKeys();

			GlyphRenderOptions.DrawUnderscores = (MenuBar != null && MenuBar.OpenMenuItem != null) ||
												KeyboardState.IsKeyDown(Keys.LeftAlt) ||
												KeyboardState.IsKeyDown(Keys.RightAlt);

			for (var i = 0; i < pressedKeys.Length; ++i)
			{
				var key = pressedKeys[i];
				if (!lastKeyboardState.IsKeyDown(key))
				{
					var ev = KeyDown;
					if (ev != null)
					{
						ev(this, new GenericEventArgs<Keys>(key));
					}

					if (_focusedWidget != null)
					{
						_focusedWidget.IterateFocusable(w => w.OnKeyDown(key));
					}

					if (key == Keys.Escape && ContextMenu != null)
					{
						HideContextMenu();
					}

					if (MenuBar != null && GlyphRenderOptions.DrawUnderscores)
					{
						MenuBar.OnKeyDown(key);
					}
				}
			}

			var lastPressedKeys = lastKeyboardState.GetPressedKeys();
			for (var i = 0; i < lastPressedKeys.Length; ++i)
			{
				var key = lastPressedKeys[i];
				if (!KeyboardState.IsKeyDown(key) && _focusedWidget != null)
				{
					// Key had been released
					_focusedWidget.IterateFocusable(w => w.OnKeyUp(key));
				}
			}

			if (MouseWheel != lastState.ScrollWheelValue)
			{
				var delta = MouseWheel - lastState.ScrollWheelValue;
				var ev = MouseWheelChanged;
				if (ev != null)
				{
					ev(null, new GenericEventArgs<float>(delta));
				}

				if (_focusedWidget != null)
				{
					_focusedWidget.IterateFocusable(w => w.OnMouseWheel(delta));
				}
			}
		}

		internal void AddFocusableWidget(Widget w)
		{
			w.MouseDown += FocusableWidgetOnMouseDown;
			_focusableWidgets.Add(w);
		}

		internal void RemoveFocusableWidget(Widget w)
		{
			w.MouseDown -= FocusableWidgetOnMouseDown;
			_focusableWidgets.Remove(w);
		}

		private void ProcessWidgets(IEnumerable<Widget> widgets)
		{
			foreach (var w in widgets)
			{
				if (!w.Visible)
				{
					continue;
				}


				if (MenuBar == null && w is HorizontalMenu)
				{
					MenuBar = (HorizontalMenu)w;
				}

				var asContainer = w as Container;
				if (asContainer != null)
				{
					ProcessWidgets(asContainer.Children);
				}
			}
		}

		private void ProcessWidgets()
		{
			MenuBar = null;

			ProcessWidgets(_widgets);
		}

		private void FocusableWidgetOnMouseDown(object sender, GenericEventArgs<MouseButtons> genericEventArgs)
		{
			var widget = (Widget)sender;

			if (!widget.IsFocused)
			{
				FocusedWidget = widget;
			}
		}

		private void UpdateWidgetsCopy()
		{
			if (!_widgetsDirty)
			{
				return;
			}

			_widgetsCopy.Clear();
			_widgetsCopy.AddRange(_widgets);

			_reversedWidgetsCopy.Clear();
			_reversedWidgetsCopy.AddRange(_widgets.Reverse());

			_widgetsDirty = false;
		}
	}
}