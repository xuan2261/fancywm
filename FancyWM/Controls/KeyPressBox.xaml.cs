﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

using FancyWM.Utilities;

namespace FancyWM.Controls
{
    /// <summary>
    /// Interaction logic for KeyPressBox.xaml
    /// </summary>
    public partial class KeyPressBox : UserControl
    {
        internal event KeyPatternChangedEventHandler? PatternChanged;

        public static readonly DependencyProperty PatternProperty = DependencyProperty.Register(
            nameof(Pattern),
            typeof(IReadOnlySet<Key>),
            typeof(KeyPressBox),
            new PropertyMetadata(null));

        public IReadOnlySet<Key> Pattern
        {
            get => (IReadOnlySet<Key>)GetValue(PatternProperty);
            set => SetValue(PatternProperty, value);
        }

        public static readonly DependencyProperty InitialPatternProperty = DependencyProperty.Register(
            nameof(InitialPattern),
            typeof(string),
            typeof(KeyPressBox),
            new PropertyMetadata(null));

        public string InitialPattern
        {
            get => (string)GetValue(InitialPatternProperty);
            set => SetValue(InitialPatternProperty, value);
        }

        private const string EmptyPlaceholder = "<None>";

        private KeyPatternListener m_patternListener;

        public KeyPressBox()
        {
            InitializeComponent();

            InputBox.Text = EmptyPlaceholder;
            InputBox.TextChanged += OnTextChanged;

            m_patternListener = new KeyPatternListener(InputBox);
            m_patternListener.PatternChanged += OnPatternChanged;

            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            m_patternListener.Dispose();
        }

        private void OnPatternChanged(object sender, KeyPatternChangedEventArgs e)
        {
            e = new KeyPatternChangedEventArgs(e.Keys.Normalize().ToHashSet());
            Pattern = e.Keys;
            PatternChanged?.Invoke(sender, e);
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(InputBox.Text))
            {
                InputBox.Text = EmptyPlaceholder;
                PatternChanged?.Invoke(this, new KeyPatternChangedEventArgs(new HashSet<Key>()));
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == PatternProperty)
            {
                InputBox.Text = Pattern.OrderByDescending(x => (int)x).ToPrettyString();
                Keyboard.ClearFocus();
            }
            else if (e.Property == InitialPatternProperty)
            {
                if (string.IsNullOrEmpty(InitialPattern))
                {
                    InputBox.Text = EmptyPlaceholder;
                }
                else
                {
                    InputBox.Text = FormatPattern(InitialPattern);
                }
            }
        }

        private string FormatPattern(string pattern)
        {
            return pattern.Split(',')
                .Select(x => Enum.Parse<Key>(x))
                .ToPrettyString();
        }
    }
}
