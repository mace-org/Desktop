using Desktop.Infrastructures;
using Desktop.Infrastructures.Location;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
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

        public MainForm()
        {
            InitializeComponent();

            //_keyboardHook = new KeyboardHook();
            _keyboardHooks = new KeyboardHook();
            _mouseHook = new MouseHook();

            _geolocation = new Geolocation();
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

            _keyboardHooks.Dispose();
            _mouseHook.Dispose();
            _geolocation.Dispose();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var pos = _geolocation.GetCurrentPosition();

            return;

            _keyboardHooks.Messages.Subscribe(a =>
            {
                Trace.WriteLine($"Keyboard input: {a}.");
                //File.AppendAllLines("d:\\keyboard_hook.txt", new string[] { a.ToString() });
            });
            _mouseHook.Messages.Subscribe(a =>
            {
                Trace.WriteLine($"Mouse input: {a}.");
                //File.AppendAllLines("d:\\keyboard_hook.txt", new string[] { a.ToString() });
            });

            //Call(() =>
            //{
            //    _disposable.Disposable = _hooks.KeyboardInputs.Subscribe(a =>
            //    {
            //        Trace.WriteLine($"Keyboard input: {a}.");
            //        //File.AppendAllLines("d:\\keyboard_hook.txt", new string[] { a.ToString() });
            //    });
            //});


            //Call(() => _keyboardHook.Install());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            return;

            _keyboardHooks.Dispose();
            _mouseHook.Dispose();

            //_disposable.Dispose();
            //_disposable = new SingleAssignmentDisposable();
            //Call(() => _keyboardHook.Uninstall());
        }

        //static void ResolveAddressSync()
        //{
        //    ILocation;
        //    GeoCoordinateWatcher watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
        //    watcher.MovementThreshold = 1.0; // set to one meter
        //    watcher.TryStart(false, TimeSpan.FromMilliseconds(1000));

        //    CivicAddressResolver resolver = new CivicAddressResolver();

        //    if (watcher.Position.Location.IsUnknown == false)
        //    {
        //        CivicAddress address = resolver.ResolveAddress(watcher.Position.Location);

        //        if (!address.IsUnknown)
        //        {
        //            Console.WriteLine("Country: {0}, Zip: {1}",
        //                    address.CountryRegion,
        //                    address.PostalCode);
        //        }
        //        else
        //        {
        //            Console.WriteLine("Address unknown.");
        //        }
        //    }
        //}
    }
}
