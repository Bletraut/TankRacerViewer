using System;
using System.Collections.Generic;

using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class ConsoleWindow : WindowElement
    {
        public const float DefaultToolPanelHeight = 30;

        // Static.
        private static MessageElement CreateMessageElement()
            => new MessageElement();

        private static ContentButtonElement CreateToggle()
        {
            var toggle = new ContentButtonElement(
                iconSize: new Vector2(20),
                text: "0",
                normalSkin: StandardSkin.RectanglePanel,
                hoverSkin: StandardSkin.RectanglePanel,
                pressedSkin: StandardSkin.RectanglePanel,
                disabledSkin: StandardSkin.RectanglePanel,
                hoverColor: Color.LightCyan,
                pressedColor: Color.DarkGoldenrod
            );
            toggle.Text.Color = Color.Black;

            return toggle;
        }

        // Class.
        private readonly ContentButtonElement _clearButton;
        private readonly ContentButtonElement _infoToggle;
        private readonly ContentButtonElement _warningToggle;
        private readonly ContentButtonElement _errorToggle;
        private readonly ContainerElement _toolPanel;

        private readonly ScrollViewElement _scrollView;
        private readonly LazyListViewElement<MessageData, MessageElement> _lazyListView;

        private readonly List<MessageData> _messages = [];

        private int _infoCount;
        private int _warningCount;
        private int _errorCount;

        private bool _isInfoEnabled = true;
        private bool _isWarningEnabled = true;
        private bool _isErrorEnabled = true;

        public ConsoleWindow() : base("Console")
        {
            _clearButton = new ContentButtonElement(
                text: "Clear",
                normalSkin: StandardSkin.RectanglePanel,
                hoverSkin: StandardSkin.RectanglePanel,
                pressedSkin: StandardSkin.RectanglePanel,
                disabledSkin: StandardSkin.RectanglePanel,
                hoverColor: Color.LightCyan,
                pressedColor: Color.DarkGoldenrod
            );
            _clearButton.Icon.IsEnabled = false;
            _clearButton.Text.Color = Color.Black;
            _clearButton.PointerClick += OnClearButtonClicked;

            _infoToggle = CreateToggle();
            _infoToggle.PointerClick += OnInfoToggleClicked;

            _warningToggle = CreateToggle();
            _warningToggle.Icon.Color = Color.Yellow;
            _warningToggle.PointerClick += OnWarningToggleClicked;

            _errorToggle = CreateToggle();
            _errorToggle.Icon.Color = Color.Red;
            _errorToggle.PointerClick += OnErrorToggleClicked;

            _toolPanel = new ContainerElement(
                size: new Vector2(DefaultToolPanelHeight),
                children: [
                    new ExpandedElement(
                        expandWidth: false,
                        innerElement: new AlignmentElement(
                            alignmentFactor: Alignment.MiddleLeft,
                            pivot: Alignment.MiddleLeft,
                            innerElement: _clearButton
                        )
                    ),
                    new ExpandedElement(
                        expandWidth: false,
                        innerElement: new AlignmentElement(
                            alignmentFactor: Alignment.MiddleRight,
                            pivot: Alignment.MiddleRight,
                            innerElement: new RowLayout(
                                alignmentFactor: Alignment.MiddleLeft,
                                sizeMainAxisToContent: true,
                                expandChildrenCrossAxis: true,
                                children: [
                                    _infoToggle,
                                    _warningToggle,
                                    _errorToggle
                                ]
                            )
                        )
                    )
                ]
            );
            ContentContainer.AddChild(new ExpandedElement(
                expandHeight: false,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.TopLeft,
                    pivot: Alignment.TopLeft,
                    innerElement: _toolPanel
                )
            ));

            _lazyListView = new LazyListViewElement<MessageData, MessageElement>(
                itemFactory: CreateMessageElement
            );
            _lazyListView.ItemColumn.ExpandChildrenCrossAxis = true;

            _scrollView = new ScrollViewElement(
                expandingContentWidthMode: ScrollViewElement.ExpandingMode.ExpandToFit,
                content: _lazyListView
            );
            ContentContainer.AddChild(new ExpandedElement(
                topPadding: DefaultToolPanelHeight,
                innerElement: _scrollView
            ));
        }

        public void LogMessage(MessageType type, string message)
        {
            switch(type)
            {
                case MessageType.Info:
                    _infoCount++;
                    break;
                case MessageType.Warning:
                    _warningCount++;
                    break;
                case MessageType.Error:
                    _errorCount++;
                    break;
            }

            var messageData = new MessageData(0, type, message, DateTime.Now);
            _messages.Add(messageData);

            if (CanShowMessage(messageData))
                _lazyListView.AddData(messageData with { Index = _lazyListView.Data.Count });

            RefreshMessageCounters();
        }

        public void Clear()
        {
            _infoCount = 0;
            _warningCount = 0;
            _errorCount = 0;

            _messages.Clear();
            _lazyListView.ClearData();
        }

        private void RefreshMessageCounters()
        {
            _infoToggle.Text.Text = _infoCount.ToString();
            _warningToggle.Text.Text = _warningCount.ToString();
            _errorToggle.Text.Text = _errorCount.ToString();
        }

        private void RefreshLazyListViewItems()
        {
            _lazyListView.ClearData();
            foreach (var messageData in _messages)
            {
                if (CanShowMessage(messageData))
                    _lazyListView.AddData(messageData with { Index = _lazyListView.Data.Count });
            }
        }

        private bool CanShowMessage(in MessageData messageData)
        {
            return _isInfoEnabled && messageData.Type is MessageType.Info
                || (_isWarningEnabled && messageData.Type is MessageType.Warning)
                || (_isErrorEnabled && messageData.Type is MessageType.Error);
        }

        private void OnClearButtonClicked(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            Clear();
        }

        private void OnInfoToggleClicked(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _isInfoEnabled = !_isInfoEnabled;
            RefreshLazyListViewItems();
        }

        private void OnWarningToggleClicked(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _isWarningEnabled = !_isWarningEnabled;
            RefreshLazyListViewItems();
        }

        private void OnErrorToggleClicked(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _isErrorEnabled = !_isErrorEnabled;
            RefreshLazyListViewItems();
        }
    }

    public enum MessageType
    {
        Info,
        Warning,
        Error
    }
}
