using System;
using System.Collections.Generic;
using System.Windows;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private readonly List<string> whiteRules = new();
        private readonly List<string> blackRules = new();

        private static readonly string[] Actions =
        {
            "Повернуть направо",
            "Повернуть налево",
            "Идти прямо",
            "Перекрасить клетку"
        };

        public MainWindow()
        {
            InitializeComponent();

            foreach (var a in Actions)
            {
                CmbWhiteAction.Items.Add(a);
                CmbBlackAction.Items.Add(a);
            }
            CmbWhiteAction.SelectedIndex = 0;
            CmbBlackAction.SelectedIndex = 0;

            whiteRules.AddRange(new[] { "Повернуть направо", "Перекрасить клетку", "Идти прямо" });
            blackRules.AddRange(new[] { "Повернуть налево", "Перекрасить клетку", "Идти прямо" });
            UpdateRuleLists();
        }

        private void UpdateRuleLists()
        {
            ListWhiteActions.Items.Clear();
            foreach (var r in whiteRules) ListWhiteActions.Items.Add(r);

            ListBlackActions.Items.Clear();
            foreach (var r in blackRules) ListBlackActions.Items.Add(r);
        }

        private void BtnAddWhite_Click(object sender, RoutedEventArgs e)
        {
            if (CmbWhiteAction.SelectedItem is string action)
            {
                whiteRules.Add(action);
                UpdateRuleLists();
            }
        }

        private void BtnRemoveWhite_Click(object sender, RoutedEventArgs e)
        {
            int i = ListWhiteActions.SelectedIndex;
            if (i >= 0) { whiteRules.RemoveAt(i); UpdateRuleLists(); }
        }

        private void BtnAddBlack_Click(object sender, RoutedEventArgs e)
        {
            if (CmbBlackAction.SelectedItem is string action)
            {
                blackRules.Add(action);
                UpdateRuleLists();
            }
        }

        private void BtnRemoveBlack_Click(object sender, RoutedEventArgs e)
        {
            int i = ListBlackActions.SelectedIndex;
            if (i >= 0) { blackRules.RemoveAt(i); UpdateRuleLists(); }
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e) { }
        private void BtnStart_Click(object sender, RoutedEventArgs e) { }
        private void BtnPause_Click(object sender, RoutedEventArgs e) { }
    }
}
