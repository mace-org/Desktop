using Desktop.Infrastructures;
using Desktop.Infrastructures.Hooks;
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
        ///private KeyboardHook _keyboardHook;

        private WindowsHooks _hooks;

        private SingleAssignmentDisposable _disposable;

        public MainForm()
        {
            InitializeComponent();

            //_keyboardHook = new KeyboardHook();
            _hooks = new WindowsHooks();
            _disposable = new SingleAssignmentDisposable();
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

            _disposable.Dispose();
            _hooks.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Call(() =>
            {
                _disposable.Disposable = _hooks.KeyboardInputs.Subscribe(a =>
                {
                    Trace.WriteLine($"Keyboard input: {a}.");
                    //File.AppendAllLines("d:\\keyboard_hook.txt", new string[] { a.ToString() });
                });
            });

           
            //Call(() => _keyboardHook.Install());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _disposable.Dispose();
            _disposable = new SingleAssignmentDisposable();
            //Call(() => _keyboardHook.Uninstall());
        }
    }
}
