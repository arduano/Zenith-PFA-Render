using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace PFARender
{
    /// <summary>
    /// Interaction logic for SettingsCtrl.xaml
    /// </summary>
    public partial class SettingsCtrl : UserControl
    {
        Settings settings;

        public event Action PaletteChanged
        {
            add { paletteList.PaletteChanged += value; }
            remove { paletteList.PaletteChanged -= value; }
        }

        public void SetValues()
        {
            firstNote.Value = settings.firstNote;
            lastNote.Value = settings.lastNote - 1;
            pianoHeight.Value = (int)(settings.pianoHeight * 100);
            noteDeltaScreenTime.Value = Math.Log(settings.deltaTimeOnScreen, 2);
            borderWidth.Value = (decimal)settings.borderWidth;
            sameWidth.IsChecked = settings.sameWidthNotes;
            //topColorSelect.SelectedIndex = (int)settings.topColor;
            middleCSquare.IsChecked = settings.middleC;
            blackNotesAbove.IsChecked = settings.blackNotesAbove;
            paletteList.SelectImage(settings.palette);
        }

        public SettingsCtrl(Settings settings) : base()
        {
            InitializeComponent();
            this.settings = settings;
            paletteList.SetPath("Plugins\\Assets\\Palettes", 0.8f);
            LoadSettings(true);
            SetValues();
        }

        private void Nud_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (sender == firstNote) settings.firstNote = (int)firstNote.Value;
                if (sender == lastNote) settings.lastNote = (int)lastNote.Value + 1;
                if (sender == pianoHeight) settings.pianoHeight = (double)pianoHeight.Value / 100;
                if (sender == noteDeltaScreenTime) settings.deltaTimeOnScreen = (int)noteDeltaScreenTime.Value;
                if (sender == borderWidth) settings.borderWidth = (double)borderWidth.Value;
            }
            catch (NullReferenceException) { }
            catch (InvalidOperationException) { }
        }

        private void NoteDeltaScreenTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (screenTimeLock) return;
                screenTimeLock = true;
                settings.deltaTimeOnScreen = Math.Pow(2, noteDeltaScreenTime.Value);
                screenTime_nud.Value = (decimal)settings.deltaTimeOnScreen;
                screenTimeLock = false;
            }
            catch (NullReferenceException)
            {
                screenTimeLock = false;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            settings.palette = paletteList.SelectedImage;
            try
            {
                string s = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText("Plugins/PFARender.json", s);
                Console.WriteLine("Saved settings to PFARender.json");
            }
            catch
            {
                Console.WriteLine("Could not save settings");
            }
        }

        void LoadSettings(bool startup = false)
        {
            try
            {
                string s = File.ReadAllText("Plugins/PFARender.json");
                var sett = JsonConvert.DeserializeObject<Settings>(s);
                injectSettings(sett);
                Console.WriteLine("Loaded settings from PFARender.json");
            }
            catch
            {
                if (!startup)
                    Console.WriteLine("Could not load saved plugin settings");
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        void injectSettings(Settings sett)
        {
            var sourceProps = typeof(Settings).GetFields().ToList();
            var destProps = typeof(Settings).GetFields().ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    var p = destProps.First(x => x.Name == sourceProp.Name);
                    p.SetValue(settings, sourceProp.GetValue(sett));
                }
            }
            SetValues();
        }

        private void DefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            injectSettings(new Settings());
        }

        private void SameWidth_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                settings.sameWidthNotes = (bool)sameWidth.IsChecked;
            }
            catch (NullReferenceException) { }
        }

        //private void TopColorSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    try
        //    {
        //        settings.topColor = (TopColor)topColorSelect.SelectedIndex;
        //    }
        //    catch (NullReferenceException) { }
        //}

        private void MiddleCSquare_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                settings.middleC = (bool)middleCSquare.IsChecked;
            }
            catch (NullReferenceException) { }
        }

        private void BlackNotesAbove_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                settings.blackNotesAbove = (bool)blackNotesAbove.IsChecked;
            }
            catch (NullReferenceException) { }
        }

        bool screenTimeLock = false;
        private void ScreenTime_nud_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (screenTimeLock) return;
                screenTimeLock = true;
                noteDeltaScreenTime.Value = Math.Log((double)screenTime_nud.Value, 2);
                settings.deltaTimeOnScreen = (double)screenTime_nud.Value;
                screenTimeLock = false;
            }
            catch
            {
                screenTimeLock = false;
            }
        }

        private void BarColorHex_TextChanged(object sender, TextChangedEventArgs e)
        {
            var hex = barColorHex.Text;
            if (hex.Length != 6) return;
            try
            {
                int col = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                settings.topBarR = ((col >> 16) & 0xFF) / 255.0f;
                settings.topBarG = ((col >> 8) & 0xFF) / 255.0f;
                settings.topBarB = ((col >> 0) & 0xFF) / 255.0f;
            }
            catch { return; }
        }
    }
}
