﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFHexaEditor.Control.Core;

namespace WPFHexaEditor.Control
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class HexaEditor : UserControl
    {
        private string _fileName = "";
        private const double _lineInfoHeight = 22;
        private int _bytePerLine = 16;
        private FileStream _file = null;
        private double _scrollLargeChange = 100;
        private bool _readOnlyMode = false;

        public HexaEditor()
        {
            InitializeComponent();

            //Height = Double.NaN;
            //Width = Double.NaN;

            RefreshView(true);
        }

        /// <summary>
        /// Set or Get the file with the control will show hex
        /// </summary>
        public string FileName
        {
            get
            {
                return this._fileName;
            }

            set
            {
                //TODO: make open method
                this._fileName = value;
                
                if (File.Exists(value))
                {
                    CloseFile();
                    _file = new FileStream(value, FileMode.Open, FileAccess.ReadWrite);

                    RefreshView(true);
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
        }

        public int BytePerLine
        {
            get
            {
                return _bytePerLine;
            }
            set
            {
                _bytePerLine = value;

                RefreshView(true);
            }
        }

        public double ScrollLargeChange {
            get
            {
                return _scrollLargeChange;
            }
            set
            {
                this._scrollLargeChange = value;

                UpdateVerticalScroll();
            }
        }

        /// <summary>
        /// Put the control on readonly mode.
        /// </summary>
        public bool ReadOnlyMode
        {
            get
            {
                return _readOnlyMode;
            }

            set
            {
                _readOnlyMode = value;

                RefreshView(true);
            }
        }

        /// <summary>
        /// Refresh currentview of hexeditor
        /// </summary>
        /// <param name="ControlResize"></param>
        private void RefreshView(bool ControlResize = false)
        {
            UpdateLinesInfo();
            UpdateVerticalScroll();
            UpdateHexHeader();
            UpdateStringDataViewer(ControlResize);
            UpdateDataViewer(ControlResize);            
        }
        
        /// <summary>
        /// Update the dataviewer stackpanel
        /// </summary>
        private void UpdateDataViewer(bool ControlResize)
        {
            if (_file != null)
            {
                if (ControlResize)
                {
                    HexDataStackPanel.Children.Clear();

                    foreach (Label infolabel in LinesInfoStackPanel.Children)
                    {
                        StackPanel dataLineStack = new StackPanel();
                        dataLineStack.Height = _lineInfoHeight;
                        dataLineStack.Orientation = Orientation.Horizontal;
                        //dataLineStack.Background = Brushes.Aqua;

                        long position = Converters.HexLiteralToLong(infolabel.Content.ToString());

                        for (int i = 0; i < _bytePerLine; i++)
                        {
                            _file.Position = position + i;

                            if (_file.Position >= _file.Length)
                                break;

                            HexByteControl byteControl = new HexByteControl();

                            byteControl.BytePositionInFile = _file.Position;
                            byteControl.ReadOnlyMode = _readOnlyMode;
                            byteControl.Byte = (byte)_file.ReadByte(); //Converters.ByteToHex((byte)_file.ReadByte());

                            dataLineStack.Children.Add(byteControl);                                                        
                        }

                        HexDataStackPanel.Children.Add(dataLineStack);
                    }
                }
                else
                {
                    int stackIndex = 0;
                    foreach (Label infolabel in LinesInfoStackPanel.Children)
                    {
                        StackPanel dataLineStack = new StackPanel();
                        dataLineStack.Height = _lineInfoHeight;
                        dataLineStack.Orientation = Orientation.Horizontal;
                        //dataLineStack.Background = Brushes.Aqua;

                        long position = Converters.HexLiteralToLong(infolabel.Content.ToString());

                        foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                        {
                            _file.Position = position++;

                            //TEMP WILL BE REPLACED BY BYTECONTROL
                            //HexByteControl byteControl = new HexByteControl();

                            if (_file.Position >= _file.Length)
                            {
                                byteControl.IsByteModified = false;
                                byteControl.BytePositionInFile = -1;
                                byteControl.Byte = null; //Converters.ByteToHex((byte)_file.ReadByte());
                            }
                            else
                            {
                                byteControl.IsByteModified = false;
                                byteControl.BytePositionInFile = _file.Position;
                                byteControl.Byte = (byte)_file.ReadByte(); //Converters.ByteToHex((byte)_file.ReadByte());
                            }
                            //dataLineStack.Children.Add(byteControl);
                        }

                        stackIndex++;
                        HexDataStackPanel.Children.Add(dataLineStack);
                    }
                }
            }
            else
            {
                HexDataStackPanel.Children.Clear();
            }
        }

        /// <summary>
        /// Update the dataviewer stackpanel
        /// </summary>
        private void UpdateStringDataViewer(bool ControlResize)
        {
            if (_file != null)
            {
                if (ControlResize)
                {
                    StringDataStackPanel.Children.Clear();

                    foreach (Label infolabel in LinesInfoStackPanel.Children)
                    {
                        StackPanel dataLineStack = new StackPanel();
                        dataLineStack.Height = _lineInfoHeight;
                        dataLineStack.Orientation = Orientation.Horizontal;
                        //dataLineStack.Background = Brushes.Aqua;

                        long position = Converters.HexLiteralToLong(infolabel.Content.ToString());

                        for (int i = 0; i < _bytePerLine; i++)
                        {
                            _file.Position = position + i;
                            
                            if (_file.Position >= _file.Length)
                                break;

                            //TEMP WILL BE REPLACED BY BYTECONTROL
                            Label label = new Label();
                            label.Padding = new Thickness(0);
                            label.Width = 12;
                            label.Content = Converters.ByteToChar((byte)_file.ReadByte());
                            //HexByteControl byteControl = new HexByteControl();

                            //byteControl.BytePositionInFile = _file.Position;
                            //byteControl.Byte = (byte)_file.ReadByte(); //Converters.ByteToHex((byte)_file.ReadByte());
                            
                            dataLineStack.Children.Add(label);
                        }

                        StringDataStackPanel.Children.Add(dataLineStack);
                    }
                }
                else
                {
                    int stackIndex = 0;
                    foreach (Label infolabel in LinesInfoStackPanel.Children)
                    {
                        StackPanel dataLineStack = new StackPanel();
                        dataLineStack.Height = _lineInfoHeight;
                        dataLineStack.Orientation = Orientation.Horizontal;
                        //dataLineStack.Background = Brushes.Aqua;

                        long position = Converters.HexLiteralToLong(infolabel.Content.ToString());

                        try
                        {
                            foreach (Label byteControl in ((StackPanel)StringDataStackPanel.Children[stackIndex]).Children)
                            {
                                _file.Position = position++;

                                if (_file.Position >= _file.Length)
                                {
                                    byteControl.Content = "";

                                }
                                else
                                {
                                    //TEMP WILL BE REPLACED BY BYTECONTROL
                                    //HexByteControl byteControl = new HexByteControl();

                                    byteControl.Content = Converters.ByteToChar((byte)_file.ReadByte());

                                }
                                
                            }
                        }
                        catch { }

                        stackIndex++;
                        HexDataStackPanel.Children.Add(dataLineStack);
                    }
                }
            }
            else
            {
                StringDataStackPanel.Children.Clear();
            }
        }

        /// <summary>
        /// Update the position info panel at left of the control
        /// </summary>
        public void UpdateHexHeader()
        {
            HexHeaderStackPanel.Children.Clear();

            if (_file != null)
            {
                for (int i = 0; i < _bytePerLine; i++)
                {
                    //Create control
                    Label LineInfoLabel = new Label();
                    LineInfoLabel.Height = _lineInfoHeight;
                    LineInfoLabel.Padding = new Thickness(0, 0, 10, 0);
                    LineInfoLabel.Foreground = Brushes.Gray;
                    LineInfoLabel.Width = 25;
                    LineInfoLabel.HorizontalContentAlignment = HorizontalAlignment.Right;
                    LineInfoLabel.VerticalContentAlignment = VerticalAlignment.Center;
                    LineInfoLabel.Content = Converters.ByteToHex((byte)i);
                    LineInfoLabel.ToolTip = $"Column : {i.ToString()}";

                    HexHeaderStackPanel.Children.Add(LineInfoLabel);
                }
            }
            else
            {

            }
        }

        /// <summary>
        /// Update the position info panel at left of the control
        /// </summary>
        public void UpdateLinesInfo()
        {
            LinesInfoStackPanel.Children.Clear();

            if (_file != null)
            {
                for(int i = 0; i < GetMaxVisibleLine(); i++)
                {
                    long fds = GetMaxVisibleLine();
                    //LineInfo 
                    //TODO: Fix last line to EOF.
                    long firstLineByte = ((long)VerticalScrollBar.Value + i) * _bytePerLine; 
                    string info = "0x" +  firstLineByte.ToString(Constant.HexLineInfoStringFormat, Thread.CurrentThread.CurrentCulture);

                    if (firstLineByte < _file.Length)
                    {
                        //Create control
                        Label LineInfoLabel = new Label();
                        LineInfoLabel.Height = _lineInfoHeight;
                        LineInfoLabel.Padding = new Thickness(0, 0, 10, 0);
                        LineInfoLabel.Foreground = Brushes.Gray;
                        LineInfoLabel.HorizontalContentAlignment = HorizontalAlignment.Right;
                        LineInfoLabel.VerticalContentAlignment = VerticalAlignment.Center;
                        LineInfoLabel.Content = info;
                        LineInfoLabel.ToolTip = $"Byte : {firstLineByte.ToString()}";  

                        LinesInfoStackPanel.Children.Add(LineInfoLabel);
                    }
                } 
            }
            else
            {

            }
        }

        /// <summary>
        /// Close file and clear control
        /// </summary>
        public void CloseFile()
        {
            if (this._file != null)
            {
                this._file.Close();
                this._file = null;
                VerticalScrollBar.Value = 0;
            }

            RefreshView();
        }

        /// <summary>
        /// Update vertical scrollbar with file info
        /// </summary>
        public void UpdateVerticalScroll()
        {
            VerticalScrollBar.Visibility = Visibility.Collapsed;

            if (_file != null)
            {
                //TODO : check if need to show
                VerticalScrollBar.Visibility = Visibility.Visible;

                VerticalScrollBar.SmallChange = 1;
                VerticalScrollBar.LargeChange = ScrollLargeChange;
                VerticalScrollBar.Maximum = GetMaxLine() - GetMaxVisibleLine() + 1;
            }     
                  
        }

        public void SetPosition(long position)
        {
            if (_file != null)
            {
                VerticalScrollBar.Value = position / _bytePerLine;
            }
            else
                VerticalScrollBar.Value = 0;
        }

        public void SetPosition(string HexLiteralPosition)
        {
            SetPosition(Converters.HexLiteralToLong(HexLiteralPosition));
        }

        /// <summary>
        /// Obtain the max line for verticalscrollbar
        /// </summary>
        public long GetMaxLine()
        {
            if (_file != null)
            {
                return _file.Length / _bytePerLine;
            }
            else
            {
                return 0;
            }
        }

        public long GetMaxVisibleLine()
        {
            return (long)(LinesInfoStackPanel.ActualHeight / _lineInfoHeight); // + 1; //TEMPS
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshView(true);
        }

        private void VerticalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RefreshView(false);
        }
    }
}