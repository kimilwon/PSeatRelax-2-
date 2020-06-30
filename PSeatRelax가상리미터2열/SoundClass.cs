using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PSeatTester
{
    public class SoundMeter
    {
        private SerialPort SoundPort = null;
        private Stopwatch STOP_WATCH = new Stopwatch();
        private Timer timer1 = new Timer();
        private bool SendFlag = false;
        private long First;
        private long Last;
        

        public SoundMeter()
        {
            if (SoundPort == null) SoundPort = new SerialPort();
            SoundPort.BaudRate = 9600;
            SoundPort.Parity = Parity.None;
            SoundPort.DataBits = 8;
            SoundPort.StopBits = StopBits.One;
            SoundPort.DataReceived += new SerialDataReceivedEventHandler(SoundPortReceive);
            STOP_WATCH.Start();

            timer1.Interval = 10;
            timer1.Tick += new EventHandler(time1_tick);
            timer1.Enabled = true;
            return;
        }

        public SoundMeter(string PortName)
        {
            if (SoundPort == null) SoundPort = new SerialPort();
            if (0 < PortName.Length)
            {
                SoundPort.PortName = PortName;
            }
            SoundPort.BaudRate = 9600;
            SoundPort.Parity = Parity.None;
            SoundPort.DataBits = 8;
            SoundPort.StopBits = StopBits.One;
            SoundPort.DataReceived += new SerialDataReceivedEventHandler(SoundPortReceive);
            STOP_WATCH.Start();

            timer1.Interval = 10;
            timer1.Tick += new EventHandler(time1_tick);
            timer1.Enabled = true;
            return;
        }

        byte[] Request = { 0x02, 0x41, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03 };
        public void time1_tick(object sender, EventArgs e)
        {
            try
            {
                timer1.Enabled = false;

                if (SoundPort.IsOpen == true)
                {
                    if (SendFlag == false)
                    {
                        SoundPort.Write(Request, 0, Request.Length);
                        SendFlag = true;
                        First = timeGetTimems();
                        Last = timeGetTimems();
                    }
                    else
                    {
                        Last = timeGetTimems();
                        if (500 <= (Last - First))
                        {
                            SendFlag = false;
                        }
                    }
                }
            }
            catch  
            { 
            }
            finally
            {
                timer1.Enabled = true;
            }
            return;
        }

        public bool Open(string PortName)
        {
            if (SoundPort.IsOpen == false)
            {
                SoundPort.PortName = PortName;
                SoundPort.BaudRate = 9600;
                SoundPort.Open();                
            }
            return SoundPort.IsOpen;
        }

        public bool Open()
        {
            if(SoundPort.IsOpen == false) SoundPort.Open();
            return SoundPort.IsOpen;
        }

        public void Close()
        {
            if(SoundPort.IsOpen == true) SoundPort.Close();
            return;
        }

        public long timeGetTimems()
        {
            double ticks = STOP_WATCH.ElapsedTicks;

            //return STOP_WATCH.ElapsedMilliseconds;
            return (long)((ticks / Stopwatch.Frequency) * 1000);
        }

        public long timeGetNanoTimes()
        {
            //1ticks == 100 nanosecond
            //return STOP_WATCH.Elapsed.Ticks;// ElapsedTicks;
            double ticks = STOP_WATCH.ElapsedTicks;
            return (long)((ticks / Stopwatch.Frequency) * 1000000000);
        }


        private float SoundData = 0;
        private void SoundPortReceive(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                timedelay(30);
                int Length = SoundPort.BytesToRead;
                byte[] buffer = new byte[Length + 10];

                SoundPort.Read(buffer, 0, Length);
                if ((buffer[0] == 0x02) && (buffer[Length - 1] == 0x03))
                {
                    ushort Data = (ushort)((buffer[5] << 8) | (buffer[6] << 0));
                    SoundData = (float)Data * 0.1F;
                }
                SoundPort.DiscardInBuffer();
                SendFlag = false;
            }
            catch
            {

            }
            finally
            {
                SendFlag = false;
            }
            return;
        }
        private void timedelay(long time)
        {
            long first;
            long last;

            first = timeGetTimems();
            last = timeGetTimems();
            do
            {
                Application.DoEvents();
                last = timeGetTimems();
            } while ((last - first) < time);
            return;
        }

        public float GetSound
        {
            get { return SoundData; }
        }

        public bool Connection
        {
            get { return SoundPort.IsOpen; }
        }
        ~SoundMeter()
        {
            STOP_WATCH.Stop();
        }
    }    
}
