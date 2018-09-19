using System;
using RawInput_dll;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
//Barrowed from: https://www.codeproject.com/Articles/17123/Using-Raw-Input-from-C-to-handle-multiple-keyboard

namespace Keyboard
{
    public partial class Keyboard : Form
    {
        private readonly RawInput _rawinput;
        
        const bool CaptureOnlyInForeground = false;
        // Todo: add checkbox to form when checked/uncheck create method to call that does the same as Keyboard ctor 

        public Keyboard()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            _rawinput = new RawInput(Handle, CaptureOnlyInForeground);
           
            _rawinput.AddMessageFilter();   // Adding a message filter will cause keypresses to be handled
            Win32.DeviceAudit();            // Writes a file DeviceAudit.txt to the current directory

            _rawinput.KeyPressed += OnKeyPressed;   
        }

        private RawInputEventArg lastEvent;
        private Boolean ctrl_down = false;
        private Boolean shift_down = false;
        private Boolean preventDouble = false;
        private Boolean preventDoubleShift = false;

        private void OnKeyPressed(object sender, RawInputEventArg e)
        {
            lbHandle.Text = e.KeyPressEvent.DeviceHandle.ToString();
            lbType.Text = e.KeyPressEvent.DeviceType;
            lbName.Text = e.KeyPressEvent.DeviceName;
            lbDescription.Text = e.KeyPressEvent.Name;
            lbKey.Text = e.KeyPressEvent.VKey.ToString(CultureInfo.InvariantCulture);
            lbNumKeyboards.Text = _rawinput.NumberOfKeyboards.ToString(CultureInfo.InvariantCulture);
            lbVKey.Text = e.KeyPressEvent.VKeyName;
            lbSource.Text = e.KeyPressEvent.Source;
            lbKeyPressState.Text = e.KeyPressEvent.KeyPressState;
            lbMessage.Text = string.Format("0x{0:X4} ({0})", e.KeyPressEvent.Message);

            try
            {
                if (lastEvent.KeyPressEvent.VKeyName == "LSHIFT" || lastEvent.KeyPressEvent.VKeyName == "RSHIFT")
                {
                    switch (lastEvent.KeyPressEvent.Message)
                    {
                        case Win32.WM_KEYDOWN:
                            shift_down = true;
                            break;
                        case Win32.WM_KEYUP:
                            shift_down = false;
                            break;
                    }
                }

                if (lastEvent.KeyPressEvent.VKeyName == "LCONTROL" || lastEvent.KeyPressEvent.VKeyName == "RCONTROL") {
                    switch (lastEvent.KeyPressEvent.Message)
                    {
                        case Win32.WM_KEYDOWN:
                            ctrl_down = true;
                            break;
                        case Win32.WM_KEYUP:
                            ctrl_down = false;
                            break;
                    }
                }
                //Event will fire twice, once for the first when C is pressed, the second when CRTL or C is released.
                if (ctrl_down && e.KeyPressEvent.VKeyName == "C")
                {
                    if (preventDouble == false)
                    {
                    //Clipboard change event to handle right click or website changes: https://stackoverflow.com/questions/621577/clipboard-event-c-sharp
                        preventDouble = true;
                        MessageBox.Show(Clipboard.GetText());
                    }
                    else { preventDouble = false; }
                }
                if (ctrl_down && e.KeyPressEvent.VKeyName == "V")
                {
                    if (preventDouble == false)
                    {
                        //Log Paste action...
                        preventDouble = true;;
                    }
                    else { preventDouble = false; }
                }
                if (shift_down)
                {
                    if (preventDoubleShift == false)
                    {
                        preventDoubleShift = true;
                        //Save key to lower...
                        //Make a saveKey function that is thread safe and invoked outside main thread to not slow responses...
                        //Something like a Que that gets registered then processes in order.
                        //https://stackoverflow.com/questions/22688679/process-queue-with-multithreading-or-tasks
                    }
                    else { preventDoubleShift = false; }
                }
                if (ctrl_down)
                {
                    if (preventDouble == false)
                    {
                        //Clipboard change event to handle right click or website changes: https://stackoverflow.com/questions/621577/clipboard-event-c-sharp
                        preventDouble = true;
                        MessageBox.Show(Clipboard.GetText());
                    }
                    else { preventDouble = false; }
                }
            }
            catch (Exception) {
                lastEvent = e;
            }
            lastEvent = e;


        }

        private void Keyboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            _rawinput.KeyPressed -= OnKeyPressed;
        }

        private static void CurrentDomain_UnhandledException(Object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            if (null == ex) return;

            // Log this error. Logging the exception doesn't correct the problem but at least now
            // you may have more insight as to why the exception is being thrown.
            Debug.WriteLine("Unhandled Exception: " + ex.Message);
            Debug.WriteLine("Unhandled Exception: " + ex);
            MessageBox.Show(ex.Message);
        }

        private void Keyboard_Load(object sender, EventArgs e)
        {
        }
    }
}
