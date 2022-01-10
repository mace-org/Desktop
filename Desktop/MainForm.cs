using Desktop.Infrastructures;
using Desktop.Infrastructures.Hooks;
using Desktop.Infrastructures.Location;
using Desktop.Infrastructures.QWeather;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Desktop
{
    public partial class MainForm : Form
    {
        private KeyboardHook _keyboardHooks;
        private MouseHook _mouseHook;

        private Geolocation _geolocation;

        private CompositeDisposable _disposables;

        public MainForm()
        {
            InitializeComponent();

            _keyboardHooks = new KeyboardHook();
            _mouseHook = new MouseHook();

            _geolocation = new Geolocation(true, 5000);

            _disposables = new CompositeDisposable();
        }

        private void Call(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.GetBaseException().Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _disposables.Dispose();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var qwc = new QWeatherClient("https://devapi.qweather.com/v7/weather/", "20d61e66d1d64012849589fc6ce6ea06", "101010100");
            var r = await qwc.GetNowAsync();
            Trace.WriteLine(r);
            return;

            _geolocation.Positions.Take(4).Subscribe(a =>
            {
                Trace.WriteLine(a);
            });
            return;

            //var post = _geolocation.GetCurrentAddress();
            ////var pos = _geolocation.GetCurrentPosition(); //.GetCurrentAddress(); //.GetCurrentPosition();

            //return;


            var dispose = _keyboardHooks.Messages.Subscribe(a =>
            {
                Trace.WriteLine($"Keyboard input: {a}.");
            });
            _disposables.Add(dispose);

            dispose = _mouseHook.Messages.Subscribe(a =>
            {
                Trace.WriteLine($"Mouse input: {a}.");
            });
            _disposables.Add(dispose);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            _disposables.Clear();
            return;
        }
    }
}
