#define PROGRAM_RUNNING

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.Design;
using System.Windows.Forms;
using GrapeCity.Win.Spread.InputMan.CellType;
using MES;

namespace PSeatRelax가상리미터2열
{    
    public interface MyInterface
    {
        /// <summary>
        /// 공용 함수를 호출한다.
        /// </summary>
        COMMON_FUCTION 공용함수 { get; }
        /// <summary>
        /// 환경 변수 설정을 진해안다.
        /// </summary>
        __Config__ GetConfig { get; set; }
        /// <summary>
        /// LIN 통신 함수를 호출한다.
        /// </summary>
        //LinControl GetLin { get; }
        /// <summary>
        /// CAN 통신 함수를 호출한다.
        /// </summary>
        __CanControl GetCan { get; }
        /// <summary>
        /// 현재 검사를 진행하는지 정보를 읽어 오거나 설정한다.
        /// </summary>
        bool isRunning { get; set; }
        /// <summary>
        /// 프로그램 종료 여부를 같는다.
        /// </summary>
        bool isExit { get; }
        short GetCanChannel { get; }
        //short GetLinChannel { get; }
        CanMap GetCanReWrite { get; }
        PanelMeter GetPMeter { get; }
        /// <summary>
        /// 파워 제어 함수를 호출한다.
        /// </summary>
        PowerControl GetPower { get; }
        IOControl GetIO { get; }
        //SoundMeter GetSound { get; }
        bool C_RelaxReturnOnOff { set; }
        bool C_RelaxOnOff { set; }
        bool C_LegrestReturnOnOff { set; }
        bool C_LegrestOnOff { set; }
        bool C_ReclineBwdOnOff { set; }
        bool C_ReclineFwdOnOff { set; }
        bool C_LegrestExtOnOff { set; }
        bool C_LegrestExtReturnOnOff { set; }
        bool C_HeightUpOnOff { set; }
        bool C_HeightDnOnOff { set; }

        void VirtualLimiteToAll();
        void VirtualLimiteToLegrest();
        void VirtualLimiteToLegrestExt();
        void VirtualLimiteToRecline();
        void VirtualLimiteToRelax();
        void VirtualLimiteToHeight();
        void VirtualLimiteToTilt();
        bool CanOpenClose { set; }
    }
    public partial class MainForm : Form, MyInterface
    {
        private COMMON_FUCTION ComF = new COMMON_FUCTION();
        private __Config__ Config;
        private IOControl IOPort = null;
        private MES_Control PopCtrl = null;
        //private LinControl LinCtrl = null;
        private __CanControl CanCtrl = null;
        private __CheckItem__ CheckItem = new __CheckItem__();
        private __Spec__ mSpec = new __Spec__();
        private PowerControl PwCtrl = null;
        private PanelMeter pMeter = null;
        private Color NotSelectColor;
        private Color SelectColor;
        private __Infor__ TInfor = new __Infor__();
        // SoundMeter Sound = null;
        private __TestData__ mData = new __TestData__();
        private KALMAN_FILETER CurrFilter = new KALMAN_FILETER();
        private CanMap CanReWrite = null;
        public MainForm()
        {
            InitializeComponent();
        }

        public COMMON_FUCTION 공용함수
        {
            get { return ComF; }
        }

        public __Config__ GetConfig
        {
            get { return Config; }
            set
            {
                Config = value;
                ConfigSetting ReadConfig = new ConfigSetting();
                ReadConfig.ReadWriteConfig = Config;
            }
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;

            ConfigSetting ReadConfig = new ConfigSetting();
            Config = ReadConfig.ReadWriteConfig;

            NotSelectColor = fpSpread1.ActiveSheet.Cells[0, 0].BackColor;
            SelectColor = Color.FromArgb(172, 227, 175);

            PopCtrl = new MES_Control(ClientIp: Config.Client, ServerIp: Config.Server, mControl: this);
            PopCtrl.Open();
            IOPort = new IOControl(Board: Config.Board, PC: Config.PC, mControl: this);
            IOPort.Open();

            if (IOPort.isOpen == false) MessageBox.Show("I/O Card IP 를 확인해 주십시오.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (IOPort.isConnection == false) MessageBox.Show("I/O Card 와 접속되지 않습니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (PopCtrl.isClientConnection == false)
            {
                //MessageBox.Show("서버와 접속되지 않습니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            label2.Text = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();


            테스ToolStripMenuItem_Click(테스ToolStripMenuItem, new EventArgs());
#if PROGRAM_RUNNING

            comboBox4.SelectedItem = null;
            comboBox3.SelectedItem = null;
            //comboBox5.SelectedItem = "12 WAY";

            if (SerialOpen() == false)
            {

            }

            CanCtrl = new __CanControl(this);
            //LinCtrl = new LinControl(false, this);

            CanReWrite = new CanMap(this);
            CanLinDefaultSetting();
            //can 통신 장애가 간혹 발생하는 문제로 프로그램 수정, 파워가 들어가고 컨넥션이 되면 처리
            //if (0 <= Config.Can.Device)
            //{
            //    CanCtrl.OpenCan(0, CanPosition(), (short)Config.Can.Speed, false);
            //    if (CanCtrl.isOpen(0) == false) { }

            //}
            //CreateFileName();
            //if (0 <= Config.Lin.Device)
            //{
            //    if (LinCtrl.isOpen(LinChannel) == false)
            //    {
            //        LinCtrl.LinOpen(LinPosition(Config.Lin.Device), LinControl.HW_MODE.SLAVE, Config.Lin.Speed);
            //        if (LinCtrl.isOpen(LinChannel) == false) { }
            //    }
            //}

            ComF.ReadFileListNotExt(Program.SPEC_PATH.ToString(), "*.Spc", COMMON_FUCTION.FileSortMode.FILENAME_ODERBY);
            List<string> FList = ComF.GetFileList;

            if (0 < FList.Count)
            {
                ModelBoxChangeFlag = true;
                comboBox1.Items.Clear();
                foreach (string s in FList) comboBox1.Items.Add(s);

                if (0 < comboBox1.Items.Count)
                {
                    comboBox1.SelectedIndex = 0;
                }

                if (comboBox1.SelectedItem != null)
                {
                    string sName = Program.SPEC_PATH.ToString() + "\\" + comboBox1.SelectedItem.ToString() + ".Spc";
                    if (File.Exists(sName) == true) ComF.OpenSpec(sName, ref mSpec);
                    mName = comboBox1.SelectedItem.ToString();
                }
                ModelBoxChangeFlag = false;
            }
#endif
            CheckItem.LhdRhd = LHD_RHD.LHD;
            CheckItem.LhRh = LH_RH.LH;
            CheckItem.ProductTestRunFlag = false;
            CheckItem.Recline = false;
            CheckItem.Height = false;
            CheckItem.Relax = false;
            CheckItem.Legrest = false;
            CheckItem.LegrestExt = false;
            CheckItem.Can = false;

            FormLoadFirstSpecDisplayTime = ComF.timeGetTimems();
            comboBox4.SelectedItem = "IMS";
            timer1.Enabled = true;
            timer2.Enabled = true;

            if (0 < comboBox1.Items.Count) comboBox1.SelectedIndex = 0;
            OpenInfor();
            DisplaySpec();
            return;
        }

        private bool FormLoadFirstSpecDisplay = true;
        private long FormLoadFirstSpecDisplayTime = 0;

        private string mName = null;

        private bool SerialOpen()
        {
            bool Flag = true;

            //Sound = new SoundMeter(Config.NoiseMeter.Port);
            //if ((Config.NoiseMeter.Port != null) && (Config.NoiseMeter.Port != string.Empty))
            //{
            //    if (Sound.Open() == false) Flag = false;
            //}

            PwCtrl = new PowerControl(Config.Power);
            if (PwCtrl.IsOpen == false)
            {
                MessageBox.Show("파워 제어용 포트를 열수 없습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Flag = false;
            }

            pMeter = new PanelMeter(this);
            pMeter.Open(Config.Panel);
            if (pMeter.isOpen == false)
            {
                MessageBox.Show("판넬메타 통신 포트를 열수 없습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Flag = false;
            }

            return Flag;
        }
        //public SoundMeter GetSound
        //{
        //    get { return Sound; }
        //}
        public PowerControl GetPower
        {
            get { return PwCtrl; }
        }

        //public LinControl GetLin
        //{
        //    get { return LinCtrl; }
        //}
        public __CanControl GetCan
        {
            get { return CanCtrl; }
        }

        public CanMap GetCanReWrite
        {
            get { return CanReWrite; }
        }
        //public short GetLinChannel
        //{
        //    get { return LinChannel; }
        //}
        public short GetCanChannel
        {
            get { return CanChannel; }
        }
        private void CanLinDefaultSetting()
        {
            CanReWrite.CanLinDefaultSetting();
            return;
        }

        public PanelMeter GetPMeter
        {
            get
            {
                return pMeter;
            }
        }

        private bool ExitFlag { get; set; }
        private bool RunningFlag { get; set; }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (RunningFlag == true)
            {
                e.Cancel = true;
                return;
            }


            ExitFlag = true;

            CloseForm xClose = new CloseForm()
            {
                Owner = this,
                WindowState = FormWindowState.Normal,
                StartPosition = FormStartPosition.CenterScreen,
                //Location = new Point(1, 1),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MinimizeBox = false,
                MaximizeBox = false,
                TopMost = true
            };


            xClose.FormClosing += delegate (object sender1, FormClosingEventArgs e1)
            {
                e1.Cancel = false;
                xClose.Dispose();
#if PROGRAM_RUNNING
                if (CanCtrl.isOpen(0) == true) CanCtrl.CanClose(0);
                //if (LinCtrl != null)
                //{
                //    if (LinCtrl.isOpen(LinChannel) == true) LinCtrl.LinClose();
                //}

                if (PwCtrl.IsOpen == true) PwCtrl.Close();
                if (pMeter.isOpen == true) pMeter.Close();
#endif
                System.Diagnostics.Process[] mProcess = System.Diagnostics.Process.GetProcessesByName(Application.ProductName);
                foreach (System.Diagnostics.Process p in mProcess) p.Kill();
            };

            e.Cancel = true;
            xClose.Show();
            return;
        }
        public bool isRunning
        {
            get { return RunningFlag; }
            set { RunningFlag = value; }
        }
        public bool isExit
        {
            get { return ExitFlag; }
        }


        //        private short LinChannel = 0;
        //        public short LinPosition(short Pos)
        //        {
        //#if PROGRAM_RUNNING
        //            short ID = 0;

        //            string[] Device = LinCtrl.GetDevice;

        //            for (short i = 0; i < Device.Length; i++)
        //            {
        //                string s = Device[i];
        //                string s1 = Pos.ToString() + " - ID";

        //                if (0 <= s.IndexOf(s1))
        //                {
        //                    ID = i;
        //                    break;
        //                }
        //            }

        //            LinChannel = ID;
        //            return ID;
        //#else
        //            return 0;
        //#endif
        //        }

        private short CanChannel = 0;
        public short CanPosition()
        {
#if PROGRAM_RUNNING
            //short ID = 0;

            string[] Device = CanCtrl.GetDevice;

            //for (short i = 0; i < Device.Length; i++)
            //{
            //    string s = Device[i];
            //    string s1 = "0x" + Pos.ToString("X2");

            //    if (0 <= s.IndexOf(s1))
            //    {
            //        ID = i;
            //        break;
            //    }
            //}
            short Pos = -1;
            string s1 = "Device=" + Config.Can.Device.ToString();
            string s2 = "Channel=" + Config.Can.Channel.ToString() + "h";

            foreach (string s in Device)
            {

                if (0 <= s.IndexOf(s1))
                {
                    if (0 <= s.IndexOf(s2))
                    {
                        string ss = s.Substring(s.IndexOf("ID=") + "ID=".Length);
                        string[] ss1 = ss.Split(',');
                        if (1 < ss1.Length)
                        {
                            string ss2 = ss1[0].Replace("(", null);

                            ss2 = ss2.Replace(")", null);
                            Pos = (short)ComF.StringToHex(ss2);
                        }
                    }
                }
            }

            if (Pos == -1)
            {
                Pos = Config.Can.ID;
            }
            CanChannel = Pos;
            return Pos;
#else
            return 0;
#endif
        }

        //private void toolStripButton4_Click(object sender, EventArgs e)
        //{
        //    //Master Lin 설정
        //    if (toolStripButton6.Text == "로그인")
        //    {
        //        //MessageBox.Show("로그인을 먼저 진행하십시오.", "경고");
        //        //uMessageBox.Show(promptText: "로그인을 먼저 진행하십시오.", title: "경고");
        //        MessageBox.Show(this, "로그인을 먼저 진행하십시오.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }
        //    if (RunningFlag == false)
        //    {
        //        if (0 <= Config.Lin.Device)
        //        {
        //            if (LinCtrl.isOpen(LinChannel) == false)
        //            {
        //                LinCtrl.LinOpen(LinPosition(Config.Lin.Device), LinControl.HW_MODE.MASTER, Config.Lin.Speed);
        //                if (LinCtrl.isOpen(LinChannel) == false) { }
        //            }
        //        }
        //        LinCtrl.LinConfigSetting(LinChannel);
        //    }
        //    return;
        //}

        private SelfTest SelfForm = null;

        private void OpenFormClose()
        {
            if (SelfForm != null) SelfForm.Close();
            if (SpecForm != null) SpecForm.Close();
            return;
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            if (IOPort.GetAuto == false)
            {
                if (SelfForm == null)
                {
                    OpenFormClose();
                    panel1.SendToBack();
                    panel1.Visible = false;

                    panel2.Visible = true;
                    if (panel2.Parent != this) panel2.Parent = this;
                    panel2.BringToFront();
                    SelfForm = new SelfTest(this)
                    {
                        MaximizeBox = false,
                        MinimizeBox = false,
                        ControlBox = false,
                        ShowIcon = false,
                        StartPosition = FormStartPosition.CenterScreen,
                        WindowState = FormWindowState.Maximized,
                        TopMost = false,
                        TopLevel = false,
                        FormBorderStyle = FormBorderStyle.None,
                        Location = new Point(0, 0),
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
                    };

                    SelfForm.FormClosing += delegate (object sender1, FormClosingEventArgs e1)
                    {
                        e1.Cancel = false;
                        SelfForm.Parent = null;
                        SelfForm.Dispose();
                        SelfForm = null;
                    };

                    SelfForm.Parent = panel2;
                    SelfForm.Show();
                }
            }
            return;
        }

        private void 테스ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripButton1.Visible = true; //테스트
            toolStripButton4.Visible = IOPort.GetAuto == false ? true : false;
            //설정
            toolStripButton2.Visible = false;
            toolStripButton3.Visible = false;
            //toolStripButton4.Visible = false;
            //toolStripButton5.Visible = false;            
            toolStripSeparator1.Visible = false;
            //toolStripSeparator2.Visible = false;
            //toolStripSeparator3.Visible = false;            
            //로그인
            toolStripButton6.Visible = false;
            toolStripButton10.Visible = false;

            //보기
            toolStripButton7.Visible = false;
            //종료
            toolStripButton8.Visible = false;
            toolStripButton9.Visible = false;
            toolStripSeparator4.Visible = false;
            return;
        }

        private void 설정ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripButton1.Visible = false; //테스트
            toolStripButton4.Visible = false;
            //설정
            toolStripButton2.Visible = true;
            toolStripButton3.Visible = true;
            //toolStripButton4.Visible = true;
            //toolStripButton5.Visible = true;
            toolStripSeparator1.Visible = true;
            //toolStripSeparator2.Visible = true;
            //toolStripSeparator3.Visible = true;
            //로그인
            toolStripButton6.Visible = false;
            toolStripButton10.Visible = false;
            //toolStripSeparator4.Visible = false;
            //보기
            toolStripButton7.Visible = false;
            //종료
            toolStripButton8.Visible = false;
            toolStripButton9.Visible = false;
            toolStripSeparator4.Visible = false;
            return;
        }

        private void 로그인ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripButton1.Visible = false; //테스트
            toolStripButton4.Visible = false;
            //설정
            toolStripButton2.Visible = false;
            toolStripButton3.Visible = false;
            //toolStripButton4.Visible = false;
            //toolStripButton5.Visible = false;
            toolStripSeparator1.Visible = false;
            //toolStripSeparator2.Visible = false;
            //toolStripSeparator3.Visible = false;
            //로그인
            toolStripButton6.Visible = true;
            toolStripButton10.Visible = true;
            toolStripSeparator4.Visible = true;
            //보기
            toolStripButton7.Visible = false;
            //종료
            toolStripButton8.Visible = false;
            toolStripButton9.Visible = false;
            //toolStripSeparator4.Visible = false;
            return;
        }

        private void 보기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripButton1.Visible = false; //테스트
            toolStripButton4.Visible = false;
            //설정
            toolStripButton2.Visible = false;
            toolStripButton3.Visible = false;
            //toolStripButton4.Visible = false;
            //toolStripButton5.Visible = false;
            toolStripSeparator1.Visible = false;
            //toolStripSeparator2.Visible = false;
            //toolStripSeparator3.Visible = false;
            //로그인
            toolStripButton6.Visible = false;
            toolStripButton10.Visible = false;
            toolStripSeparator4.Visible = false;
            //보기
            toolStripButton7.Visible = true;
            //종료
            toolStripButton8.Visible = false;
            toolStripButton9.Visible = false;
            //toolStripSeparator4.Visible = false;
            return;
        }

        private void 종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripButton1.Visible = false; //테스트
            toolStripButton4.Visible = false;
            //설정
            toolStripButton2.Visible = false;
            toolStripButton3.Visible = false;
            //toolStripButton4.Visible = false;
            //toolStripButton5.Visible = false;
            toolStripSeparator1.Visible = false;
            //toolStripSeparator2.Visible = false;
            //toolStripSeparator3.Visible = false;
            //로그인
            toolStripButton6.Visible = false;
            toolStripButton10.Visible = false;
            //toolStripSeparator4.Visible = false;
            //보기
            toolStripButton7.Visible = false;
            //종료
            toolStripButton8.Visible = true;
            toolStripButton9.Visible = true;
            toolStripSeparator4.Visible = true;
            return;
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (toolStripButton6.Text == "로그인")
            {
                PasswordCheckForm pass = new PasswordCheckForm();
                //아래와 같이 해 주면 폼을 닫을때 Dialog로 오픈을 하지 않아도 {} 안을 실행하게 된다. 동시에 해당 폼의 FormClosing이 동시에 실행되므로 Dialog로 오픈한것 같은 효과를 얻는다.
                pass.FormClosing += delegate (object sender1, FormClosingEventArgs e1)
                {
                    if (pass.result == true)
                    {
                        toolStripButton6.Text = "로그아웃";
                        로그인ToolStripMenuItem.Text = "로그아웃";
                        toolStripButton6.Image = Properties.Resources.Pad_Unlock_36Pixel1;

                        toolStripButton2.Enabled = true;
                        toolStripButton3.Enabled = true;
                        //toolStripButton4.Enabled = true;
                        //toolStripButton5.Enabled = true;
                    }
                };
                pass.Show();
            }
            else
            {
                toolStripButton6.Text = "로그인";
                로그인ToolStripMenuItem.Text = "로그인";
                toolStripButton6.Image = Properties.Resources.Pad_Lock3;
                toolStripButton2.Enabled = false;
                toolStripButton3.Enabled = false;
                //toolStripButton4.Enabled = false;
                //toolStripButton5.Enabled = false;
            }
            return;
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            if (toolStripButton6.Text == "로그인")
            {
                //MessageBox.Show("로그인을 먼저 진행하십시오.", "경고");
                //uMessageBox.Show(promptText: "로그인을 먼저 진행하십시오.", title: "경고");
                MessageBox.Show(this, "로그인을 먼저 진행하십시오.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            PasswordSetForm set = new PasswordSetForm();
            set.MinimizeBox = false;
            set.MaximizeBox = false;
            set.FormBorderStyle = FormBorderStyle.FixedSingle;
            set.Show();
            return;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (toolStrip1.Enabled == IOPort.GetAuto) toolStrip1.Enabled = !IOPort.GetAuto;
            label2.Text = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();

            if (IOPort.isConnection != led1.Value.AsBoolean) led1.Value.AsBoolean = IOPort.isConnection;
            if (led7.Value.AsBoolean != PopCtrl.isClientConnection) led7.Value.AsBoolean = PopCtrl.isClientConnection;


            if (TInfor.Date != DateTime.Now.ToString("yyyyMMdd"))
            {
                string dPath = Program.DATA_PATH.ToString() + "\\" + DateTime.Now.ToString("yyyyMM") + ".xls";
                TInfor.Date = DateTime.Now.ToString("yyyyMMdd");
                TInfor.DataName = dPath;
                TInfor.Count.Total = 0;
                TInfor.Count.OK = 0;
                TInfor.Count.NG = 0;
                TInfor.Model = comboBox1.SelectedItem.ToString();
                SaveInfor();
            }
            return;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (panel1.Visible == false)
            {
                OpenFormClose();
                panel2.SendToBack();
                panel2.Visible = false;

                panel1.Visible = true;
                panel1.BringToFront();
                if (panel1.Parent != this) panel1.Parent = this;
            }
            return;
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            if (IOPort.GetAuto == false)
            {
                if (RunningFlag == false)
                {
                    this.Close();
                }
            }
            return;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ModelBoxChangeFlag == true) return;
            string s = comboBox1.SelectedItem.ToString();

            string sName = Program.SPEC_PATH.ToString() + "\\" + s + ".Spc";

            if (File.Exists(sName) == true) ComF.OpenSpec(sName, ref mSpec);
            DisplaySpec();
            mName = comboBox1.SelectedItem.ToString();
            return;
        }

        private MES_Control.__ReadMesData__ GetReadData = new MES_Control.__ReadMesData__()
        {
            Barcode = null,
            Check = null,
            Date = null,
            LineCode = null,
            MachineNo = null
        };
        private void CheckPopData()
        {
            if (GetReadData.Check == null) return;
            ScreenInit();
            if (IOPort.TestOKOnOff == true) IOPort.TestOKOnOff = false;
            //if (IOPort.TestNGOnOff == true) IOPort.TestNGOnOff = false;
            if (IOPort.YellowLampOnOff == true) IOPort.YellowLampOnOff = false;
            if (IOPort.RedLampOnOff == true) IOPort.RedLampOnOff = false;
            if (IOPort.GreenLampOnOff == true) IOPort.GreenLampOnOff = false;

            ProductOutFlag = false;
            textBox1.Text = GetReadData.Barcode;
            label18.Text = label17.Text;
            label17.Text = GetReadData.Barcode;
            label23.Text = GetReadData.Check;

            if (IOPort.GetAuto == true)
            {
                PopReadOk = true;
                bool Flag = false;

                if (GetReadData.Check.Substring(16, 1) == "1")
                {
                    if (comboBox3.SelectedItem != null)
                    {
                        if (comboBox3.SelectedIndex != 1) comboBox3.SelectedIndex = 1;
                    }
                    else
                    {
                        comboBox3.SelectedIndex = 1;
                    }
                }
                else
                {
                    if (comboBox3.SelectedItem != null)
                    {
                        if (comboBox3.SelectedIndex != 0) comboBox3.SelectedIndex = 0;
                    }
                    else
                    {
                        comboBox3.SelectedIndex = 0;
                    }
                }

                if (comboBox3.SelectedItem.ToString() == "LH")
                    CanReWrite.LhRhSelect = LH_RH.LH;
                else CanReWrite.LhRhSelect = LH_RH.RH;

                //1 열 파워
                //if (GetReadData.Check.Substring(0, 1) == "0")
                //{
                //    if (CheckItem.IMS == true) Flag = true;
                //    CheckItem.IMS = false;
                //}
                //else
                //{
                //    if (CheckItem.IMS == false) Flag = true;
                //    CheckItem.IMS = true;
                //}


                //2열 파워

                CheckItem.Relax = false;
                CheckItem.Height = false;
                CheckItem.Legrest = false;
                CheckItem.LegrestExt = false;
                CheckItem.Recline = false;

                if (GetReadData.Check.Substring(2, 1) == "1")
                {
                    if (CheckItem.Relax == false) Flag = true;
                    CheckItem.Relax = true;
                    //CheckItem.Height = false;
                    //CheckItem.Legrest = true;
                    //CheckItem.LegrestExt = true;
                    //CheckItem.Recline = true;

                    if (comboBox4.SelectedItem != null)
                    {
                        if (comboBox4.SelectedIndex != 0) comboBox4.SelectedIndex = 0;
                    }
                    else
                    {
                        comboBox4.SelectedIndex = 0;
                    }
                }

                if (GetReadData.Check.Substring(3, 1) == "1")
                {
                    if (CheckItem.Recline == false) Flag = true;
                    CheckItem.Recline = true;
                    //CheckItem.Relax = false;
                    //CheckItem.Height = false;
                    //CheckItem.Legrest = false;
                    //CheckItem.LegrestExt = false;

                    if (CheckItem.Relax == false)
                    {
                        if (comboBox4.SelectedItem != null)
                        {
                            if (comboBox4.SelectedIndex != 1) comboBox4.SelectedIndex = 1;
                        }
                        else
                        {
                            comboBox4.SelectedIndex = 1;
                        }
                    }
                }
                if (GetReadData.Check.Substring(4, 1) == "1")
                {
                    if (CheckItem.Legrest == false) Flag = true;
                    CheckItem.Legrest = true;
                }
                if (GetReadData.Check.Substring(5, 1) == "1")
                {
                    if (CheckItem.LegrestExt == false) Flag = true;
                    CheckItem.LegrestExt = true;
                }
                //else
                //{
                //    if (CheckItem.Relax == true) Flag = true;
                //    if (CheckItem.Recline == true) Flag = true;

                //    CheckItem.Relax = false;
                //    CheckItem.Height = false;
                //    CheckItem.Legrest = false;
                //    CheckItem.LegrestExt = false;
                //    CheckItem.Recline = false;
                //}
                //if (GetReadData.Check.Substring(1, 1) == "0")
                //{
                //    if (CheckItem.Relax == true) Flag = true;
                //    CheckItem.Relax = false;
                //    CheckItem.Height = false;
                //    CheckItem.Legrest = false;
                //    CheckItem.LegrestExt = false;
                //    CheckItem.Recline = false;
                //}
                //else
                //{
                //    if (CheckItem.Relax == false) Flag = true;
                //    CheckItem.Relax = true;
                //    CheckItem.Height = false;
                //    CheckItem.Legrest = true;
                //    CheckItem.LegrestExt = true;
                //    CheckItem.Recline = true;

                //    if (comboBox4.SelectedItem != null)
                //    {
                //        if (comboBox4.SelectedIndex != 0) comboBox4.SelectedIndex = 0;
                //    }
                //    else
                //    {
                //        comboBox4.SelectedIndex = 0;
                //    }
                //}

                //if (GetReadData.Check.Substring(2, 1) == "0")
                //{
                //    if (CheckItem.Recline == true) Flag = true;
                //    CheckItem.Recline = false;
                //    CheckItem.Relax = false;
                //    CheckItem.Height = false;
                //    CheckItem.Legrest = false;
                //    CheckItem.LegrestExt = false;
                //}
                //else
                //{
                //    if (CheckItem.Recline == false) Flag = true;
                //    CheckItem.Recline = true;
                //    CheckItem.Relax = false;
                //    CheckItem.Height = false;
                //    CheckItem.Legrest = false;
                //    CheckItem.LegrestExt = false;

                //    if (comboBox4.SelectedItem != null)
                //    {
                //        if (comboBox4.SelectedIndex != 1) comboBox4.SelectedIndex = 1;
                //    }
                //    else
                //    {
                //        comboBox4.SelectedIndex = 1;
                //    }
                //}

                if (Flag == true) DisplaySpec();

                if (isCheckItem == false) label5.Text = "검사 사양이 아닙니다.";
            }
            else
            {
                PopReadOkOld = true;
            }
            return;            
        }

        private bool BuzzerRunFlag = false;
        private bool BuzzerOnOff = false;
        private long BuzzerLast = 0;
        private long BuzzerFirst = 0;
        private short BuzzerOnCount = 0;
        private bool ProductOutFlag = false;
        private bool StopButtonOnFlag = false;
        private short JigUpCount = 0;
        private bool JigUpFlag = false;
        private bool RetestFlag = false;
        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                timer2.Enabled = false;


                if((RunningFlag == false) && (IOPort.GetAuto == false) && (panel4.Visible == true))
                {
                    if (led10.Value.AsBoolean != CanReWrite.GetRelaxLeftRightLimitSensor) led10.Value.AsBoolean = CanReWrite.GetRelaxLeftRightLimitSensor;
                    if (label31.Text != CanReWrite.GetSWVersion) label31.Text = CanReWrite.GetSWVersion;

                    __CanMsg Can = CanReWrite.GetCanData;

                    string CanData = string.Format("ID - {0:X4}  : {1:X2}, {2:X2}, {3:X2}, {4:X2}, {5:X2}, {6:X2}, {7:X2}, {8:X2}", Can.ID,
                        Can.DATA[0], Can.DATA[1], Can.DATA[2], Can.DATA[3], Can.DATA[4], Can.DATA[5], Can.DATA[6], Can.DATA[7]);

                    if (CanData != OldCanData)
                    {
                        richTextBox1.AppendText(CanData + "\r");
                        richTextBox1.ScrollToCaret();
                        OldCanData = CanData;
                    }
                }

                if (PopCtrl.isReading == true)
                {
                    if (RunningFlag == false)
                    {
                        GetReadData = PopCtrl.GetReadData;
                        CheckPopData();
                        SaveLogData = "Receive - " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
                        SaveLogData = PopCtrl.SourceData;
                    }
                    PopCtrl.isReading = false;
                }

                if ((DateTime.Now.Hour == 5) && (DateTime.Now.Minute == 1))
                {
                    if (TInfor.ReBootingFlag == false)
                    {
                        TInfor.ReBootingFlag = true;
                        SaveInfor();
                        ComF.WindowRestartToDelay(0);

                        System.Diagnostics.Process[] mProcess = System.Diagnostics.Process.GetProcessesByName(Application.ProductName);
                        foreach (System.Diagnostics.Process p in mProcess) p.Kill();
                    }
                }

                //this.Text = string.Format("{0:X}", IOPort.GetInData[1]);
                DisplayIO();

                if (RunningFlag == false)
                {
                    if (IOPort.GetJigUp == true)
                    {
                        if (JigUpFlag == false) JigUpFlag = true;
                    }


                    //검사중이 아닐경우
                    if (IOPort.GetAuto == true)
                    {
                        bool Flag = false;

                        if (Config.AutoConnection == true)
                        {
                            if ((RetestFlag == false) && (JigUpFlag == true) && (PopReadOk == true) && (ProductOutFlag == false) && (label16.Text == "")) Flag = true;
                            

                            if(Flag == false)
                            {
                                //if ((RetestFlag == false) && (JigUpFlag == true) && (PopReadOk == false) && (ProductOutFlag == false) && (label16.Text == "")) PopReadOk = PopReadOkOld;
                                if ((IOPort.GetStartSw == true) && (JigUpFlag == true) && (PopReadOk == false) && (ProductOutFlag == false) && (label16.Text == ""))
                                {
                                    PopReadOk = PopReadOkOld;
                                    if ((JigUpFlag == true) && (PopReadOk == true) && (ProductOutFlag == false) && (label16.Text == "")) Flag = true;
                                }
                            }
                        }
                        else
                        {
                            if ((IOPort.GetStartSw == true) && (JigUpFlag == true) && (PopReadOk == true) && (ProductOutFlag == false) && (label16.Text == "")) Flag = true;
                            if (Flag == false)
                            {
                                if ((IOPort.GetStartSw == true) && (JigUpFlag == true) && (PopReadOk == false) && (ProductOutFlag == false) && (label16.Text == "")) PopReadOk = PopReadOkOld;
                            }
                        }

                        

                        if (Flag == true)
                        {
                            if (isCheckItem == false)
                            {
                                IOPort.TestINGOnOff = false;
                                IOPort.TestOKOnOff = true;
                                PopReadOk = false;
                            }
                            else
                            {
                                if(RunningFlag == false) StartSetting();
                            }
                        }
                        else if ((IOPort.GetStartSw == true) && (JigUpFlag == true) && (ProductOutFlag == true) && (label16.Text == "OK"))
                        {
                            if (IOPort.TestOKOnOff == false) IOPort.TestOKOnOff = true;
                            if (BattOnOff == true) BattOnOff = false;
                        }
                        else if ((IOPort.GetStartSw == true) && (JigUpFlag == true) && (ProductOutFlag == true) && (label16.Text == "NG"))
                        {
                            //if (IOPort.TestOKOnOff == false) IOPort.TestNGOnOff = true;
                            if (BattOnOff == true) BattOnOff = false;
                        }
                    }
                    else
                    {
                        bool Flag = false;

                        //if (Config.AutoConnection == true)
                        //{
                        //    if ((RetestFlag == false) && (JigUpFlag == true) && (ProductOutFlag == false) && (label16.Text == "")) Flag = true;
                        //    if ((IOPort.GetStartSw == true) && (JigUpFlag == true) && (ProductOutFlag == false) && (label16.Text == "")) Flag = true;
                        //}
                        //else
                        //{
                         if ((IOPort.GetStartSw == true) && (JigUpFlag == true) && (ProductOutFlag == false) && (label16.Text == "")) Flag = true;
                        //}

                        if (Flag == true)
                        {
                            StartSetting();
                        }
                        else if ((IOPort.GetStartSw == true) && (JigUpFlag == true) && (ProductOutFlag == true) && (label16.Text == "OK"))
                        {
                            if (IOPort.TestOKOnOff == false) IOPort.TestOKOnOff = true;
                            if (BattOnOff == true) BattOnOff = false;
                        }
                        else if ((IOPort.GetStartSw == true) && (JigUpFlag == true) && (ProductOutFlag == true) && (label16.Text == "NG"))
                        {
                            //if (IOPort.TestOKOnOff == false) IOPort.TestNGOnOff = true;
                            if (BattOnOff == true) BattOnOff = false;
                        }
                    }

                    if ((IOPort.GetResetSw == true) && (label16.Text != ""))
                    {
                        IOPort.TestINGOnOff = false;
                        IOPort.TestOKOnOff = false;
                        ProductOutFlag = false;
                        ScreenInit();
                        PopReadOk = PopReadOkOld;
                    }
                }
                else
                {
                    //검사중 일경우
                    if (IOPort.GetResetSw == true)
                    {
                        if (StopButtonOnFlag == false)
                        {
                            StopSetting();
                            StopButtonOnFlag = true;
                        }
                    }
                    else
                    {
                        StopButtonOnFlag = false;
                    }
                }

                if ((IOPort.GetStartSw == true) && (JigUpFlag == true) && (ProductOutFlag == true) && (label16.Text == "OK"))
                {
                    if (IOPort.TestOKOnOff == false) IOPort.TestOKOnOff = true;
                    if (BattOnOff == true) BattOnOff = false;
                }
                else if ((IOPort.GetStartSw == true) && (JigUpFlag == true) && (ProductOutFlag == true) && (label16.Text == "NG"))
                {
                    //if (IOPort.TestNGOnOff == false) IOPort.TestNGOnOff = true;
                    if (BattOnOff == true) BattOnOff = false;
                }


                if (IOPort.GetAuto == false)
                {
                    if (RunningFlag == false) IOInCheck();
                }
                if (FormLoadFirstSpecDisplay == true)
                {
                    if (1000 <= (ComF.timeGetTimems() - FormLoadFirstSpecDisplayTime))
                    {
                        FormLoadFirstSpecDisplay = false;
                        DisplaySpec();
                    }
                }
                if (sevenSegmentAnalog2.Value.AsDouble != pMeter.GetBatt) sevenSegmentAnalog2.Value.AsDouble = pMeter.GetBatt;
                if (sevenSegmentAnalog5.Value.AsDouble != pMeter.GetPSeat) sevenSegmentAnalog5.Value.AsDouble = pMeter.GetPSeat;
                //if (sevenSegmentAnalog1.Value.AsDouble != Sound.GetSound) sevenSegmentAnalog1.Value.AsDouble = Sound.GetSound;
            }
            catch
            {

            }
            finally
            {
                timer2.Enabled = !ExitFlag;
            }
        }

        //private string NAutoBarcode = "";
        private void StartSetting()
        {
            //bool Flag = false;

            //if (IOPort.GetAuto == false)
            //{
            //    //잠조에서 Micosoft.WisualBasic 을 먼저 등록해야 한다.
            //    string input = Microsoft.VisualBasic.Interaction.InputBox("바코드 정보", "바코드 정보를 입력해 주십시오.", NAutoBarcode);
            //    if (input == "")
            //    {
            //        if (MessageBox.Show(this, "바코드 정보가 입력되지 않았습니다. 검사를 진행하시겠습니까?", "확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            //            Flag = true;
            //        else Flag = false;

            //        label17.Text = input;
            //    }
            //    else
            //    {
            //        Flag = true;
            //    }
            //    NAutoBarcode = input;
            //}
            //else
            //{
            //    Flag = true;
            //}

            //if (Flag == true)
            //{

            if (comboBox3.SelectedItem.ToString() == "LH")
                CanReWrite.LhRhSelect = LH_RH.LH;
            else CanReWrite.LhRhSelect = LH_RH.RH;

            if (IOPort.GetAuto == true)
            {
                if (CheckItem.Relax == true)
                    CanReWrite.ModelTypeToRelax = CanMap.MODELTYPE.RELAX;
                else CanReWrite.ModelTypeToRelax = CanMap.MODELTYPE.NOT_RELAX;
            }
            else
            {
                if(IOPort.GetSeatRelax == true)
                    CanReWrite.ModelTypeToRelax = CanMap.MODELTYPE.RELAX;
                else CanReWrite.ModelTypeToRelax = CanMap.MODELTYPE.NOT_RELAX;
            }
            if ((Config.AutoConnection == true) && (IOPort.GetAuto == true))
            {
                IOPort.SetPinConnection = true;
                StepTimeFirst = ComF.timeGetTimems();
                StepTimeLast = ComF.timeGetTimems();
                do
                {
                    StepTimeLast = ComF.timeGetTimems();
                    Application.DoEvents();
                    if ((long)(Config.ConnectionDelay * 1000) <= (StepTimeLast - StepTimeFirst)) break;
                } while (IOPort.GetPinConnectionFwd == false);
            }

            CanOpenClose = true;
            ScreenInit();
            CanLinDefaultSetting();
            PopReadOkOld = PopReadOk;
            PopReadOk = false;
            ProductOutFlag = true;
            RunningFlag = true;
            Step = 0;
            SpecOutputFlag = false;
            if (PowerOnOff == false)
            {
                PowerOnOff = true;
            }
            else
            {
                if (SetVolt != mSpec.TestVolt) PowerChange = mSpec.TestVolt;
            }
            label16.Text = "검사중";
            label16.ForeColor = Color.Yellow;
            IOPort.YellowLampOnOff = true;
            IOPort.GreenLampOnOff = false;
            IOPort.RedLampOnOff = false;

            if (IOPort.TestOKOnOff == true) IOPort.TestOKOnOff = false;
            //if (IOPort.TestNGOnOff == true) IOPort.TestNGOnOff = false; 
            IOPort.TestINGOnOff = true;

            TInfor.Count.Total++;

            plot1.Channels[0].Clear();
            //plot2.Channels[0].Clear();

            richTextBox1.Clear();

            IOPort.FunctionIOInit();
            ThreadSetting();
            TotalTimeToFirst = ComF.timeGetTimems();
            TotalTimeToLast = ComF.timeGetTimems();
            //}
            return;
        }

        private void ScreenInit()
        {
            if (panel7.Visible == true) panel7.Visible = false;
            for (int i = 2; i < fpSpread1.ActiveSheet.RowCount; i++)
            {
                fpSpread1.ActiveSheet.Cells[i, 4].Text = "";
                fpSpread1.ActiveSheet.Cells[i, 5].Text = "";
                fpSpread1.ActiveSheet.Cells[i, 5].ForeColor = Color.White;
            }

            mData.Height.Data1 = 0;
            mData.Height.Result1 = RESULT.CLEAR;
            mData.Height.Data2 = 0;
            mData.Height.Result2 = RESULT.CLEAR;
            mData.Height.Test = false;

            mData.Recline.Data1 = 0;
            mData.Recline.Result1 = RESULT.CLEAR;
            mData.Recline.Data2 = 0;
            mData.Recline.Result2 = RESULT.CLEAR;
            mData.Recline.Test = false;

            mData.Relax.Data1 = 0;
            mData.Relax.Result1 = RESULT.CLEAR;
            mData.Relax.Data2 = 0;
            mData.Relax.Result2 = RESULT.CLEAR;
            mData.Relax.Test = false;

            mData.Legrest.Data1 = 0;
            mData.Legrest.Result1 = RESULT.CLEAR;
            mData.Legrest.Data2 = 0;
            mData.Legrest.Result2 = RESULT.CLEAR;
            mData.Legrest.Test = false;

            mData.LegrestExt.Data1 = 0;
            mData.LegrestExt.Result1 = RESULT.CLEAR;
            mData.LegrestExt.Data2 = 0;
            mData.LegrestExt.Result2 = RESULT.CLEAR;
            mData.LegrestExt.Test = false;

            mData.SWVer.Data = "";
            mData.SWVer.Test = false;
            mData.SWVer.Result = RESULT.CLEAR;

            mData.Result = RESULT.CLEAR;
            mData.Time = "";

            label16.Text = "";
            label5.Text = "";
            return;
        }

        private void StopSetting()
        {
            CanOpenClose = false;
            if ((Config.AutoConnection == true) && (IOPort.GetAuto == true))
            {
                IOPort.SetPinConnection = false;
                ComF.timedelay(1000);
            }
            RunningFlag = false;
            //PowerOnOff = false;
            label16.Text = "NG";
            label16.ForeColor = Color.Red;
            IOPort.YellowLampOnOff = false;
            IOPort.GreenLampOnOff = false;
            IOPort.RedLampOnOff = true;
            IOPort.TestINGOnOff = false;
            TInfor.Count.Total--;
            return;
        }

        private bool PowerOnOffFlag = false;
        private bool PowerOnOff
        {
            set
            {
                if (value == true)
                {                    
                    PwCtrl.POWER_PWSetting(mSpec.TestVolt);
                    PwCtrl.POWER_CURRENTSetting(29);
                    PwCtrl.POWER_PWON();
                }
                else
                {
                    PwCtrl.POWER_PWSetting(0);
                    PwCtrl.POWER_PWOFF();
                }

                PowerOnOffFlag = value;
            }
            get
            {
                return PowerOnOffFlag;
            }
        }

        private float SetVolt = 0;

        private float PowerChange
        {
            set
            {
                PwCtrl.POWER_PWSetting(value);
                SetVolt = value;
            }
        }

        private bool BattOnOff
        {
            set
            {
                //IOPort.SetPSeatBatt = value;
                //IOPort.SetIgn1 = value;
                //IOPort.SetIgn2 = value;

                IOPort.PSeatBattOnOff = value;
            }
            get
            {
                if ((IOPort.SetPSeatBatt == true) || (IOPort.SetIgn1 == true) || (IOPort.SetIgn2 == true))
                    return true;
                else return false;
            }
        }

        private bool PopReadOk { get; set; }
        private bool PopReadOkOld { get; set; }


        private void IMSSetButton(short Pos)
        {
            if (Pos == 0)
                CanReWrite.CanDataOutput(OUT_CAN_LIST.C_AVNIMSButtonCmd, (byte)C_AVNIMSButtonCmd.Data.Memory_P1_CMD);
            else CanReWrite.CanDataOutput(OUT_CAN_LIST.C_AVNIMSButtonCmd, (byte)C_AVNIMSButtonCmd.Data.Memory_P2_CMD);
            ComF.timedelay(600);
            CanReWrite.CanDataOutput(OUT_CAN_LIST.C_AVNIMSButtonCmd, (byte)C_AVNIMSButtonCmd.Data.Default);

            return;
        }

        private void IMSM1Button()
        {
            CanReWrite.CanDataOutput(OUT_CAN_LIST.C_AVNIMSButtonCmd, (byte)C_AVNIMSButtonCmd.Data.PBack_P1_CMD);
            ComF.timedelay(600);
            CanReWrite.CanDataOutput(OUT_CAN_LIST.C_AVNIMSButtonCmd, (byte)C_AVNIMSButtonCmd.Data.Default);
            return;
        }
        private void IMSM2Button()
        {
            CanReWrite.CanDataOutput(OUT_CAN_LIST.C_AVNIMSButtonCmd, (byte)C_AVNIMSButtonCmd.Data.PBack_P2_CMD);
            ComF.timedelay(600);
            CanReWrite.CanDataOutput(OUT_CAN_LIST.C_AVNIMSButtonCmd, (byte)C_AVNIMSButtonCmd.Data.Default);

            return;
        }

        //private void IMS1Set()
        //{
        //    if (comboBox4.SelectedItem.ToString() == "IMS")
        //    {
        //        IMSSetButton(0);
        //    }
        //    return;
        //}

        //private void IMS2Set()
        //{
        //    if (comboBox4.SelectedItem.ToString() == "IMS")
        //    {
        //        IMSSetButton(1);
        //    }
        //    return;
        //}


        private bool RelaxOnOff
        {
            set
            {
                IOPort.SetRelax = value;
            }
            get
            {
                return IOPort.SetRelax;
            }
        }


        private bool RelaxReturnOnOff
        {
            set
            {
                IOPort.SetRelaxReturn = value;
            }
            get
            {
                return IOPort.SetRelaxReturn;
            }
        }
        private bool ReclineFwdOnOff
        {
            set
            {
                IOPort.SetReclineFwd = value;
            }
            get
            {
                return IOPort.SetReclineFwd;
            }
        }
        private bool ReclineBwdOnOff
        {
            set
            {
                IOPort.SetReclineBwd = value;
            }
            get
            {
                return IOPort.SetReclineBwd;
            }
        }


        private bool LegrestOnOff
        {
            set
            {
                IOPort.SetLegrest = value;
            }
            get
            {
                return IOPort.SetLegrest;
            }
        }

        private bool LegrestReturnOnOff
        {
            set
            {
                IOPort.SetLegrestReturn = value;
            }
            get
            {
                return IOPort.SetLegrestReturn;
            }
        }

        private bool HeightUpOnOff
        {
            set
            {
                IOPort.SetHeightUp = value;
            }
            get
            {
                return IOPort.SetHeightUp;
            }
        }

        private bool HeightDnOnOff
        {
            set
            {
                IOPort.SetHeightDown = value;
            }
            get
            {
                return IOPort.SetHeightDown;
            }
        }

        private bool LegrestExtOnOff
        {
            set
            {
                IOPort.SetLegrestExt = value;
            }
            get
            {
                return IOPort.SetLegrestExt;
            }
        }
        private bool LegrestExtReturnOnOff
        {
            set
            {
                IOPort.SetLegrestExtReturn = value;
            }
            get
            {
                return IOPort.SetLegrestExtReturn;
            }
        }


        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (toolStripButton6.Text == "로그인")
            {
                MessageBox.Show(this, "로그인을 먼저 진행하십시오.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (RunningFlag == false)
            {
                SystemSet SysSet = new SystemSet(this)
                {
                    Owner = this,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    ControlBox = false,
                    FormBorderStyle = FormBorderStyle.FixedSingle,
                    WindowState = FormWindowState.Normal,
                    StartPosition = FormStartPosition.CenterParent
                };

                SysSet.FormClosing += delegate (object sender1, FormClosingEventArgs e1)
                {
                    e1.Cancel = false;
                    SysSet.Dispose();
                    SysSet = null;
                };

                SysSet.Show();
            }
            return;
        }
        private SpecSetting SpecForm = null;
        private bool ModelBoxChangeFlag = false;
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (toolStripButton6.Text == "로그인")
            {
                //MessageBox.Show("로그인을 먼저 진행하십시오.", "경고");
                //uMessageBox.Show(promptText: "로그인을 먼저 진행하십시오.", title: "경고");
                MessageBox.Show(this, "로그인을 먼저 진행하십시오.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (IOPort.GetAuto == false)
            {
                if (SpecForm == null)
                {
                    OpenFormClose();
                    panel1.SendToBack();
                    panel1.Visible = false;

                    panel2.Visible = true;
                    if (panel2.Parent != this) panel2.Parent = this;
                    panel2.BringToFront();
                    SpecForm = new SpecSetting(this, mSpec.CarName)
                    {
                        MaximizeBox = false,
                        MinimizeBox = false,
                        ControlBox = false,
                        ShowIcon = false,
                        StartPosition = FormStartPosition.CenterScreen,
                        WindowState = FormWindowState.Maximized,
                        TopMost = false,
                        TopLevel = false,
                        FormBorderStyle = FormBorderStyle.None,
                        Location = new Point(0, 0),
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
                    };

                    SpecForm.FormClosing += delegate (object sender1, FormClosingEventArgs e1)
                    {
                        e1.Cancel = false;
                        SpecForm.Parent = null;
                        SpecForm.Dispose();
                        SpecForm = null;

                        ComF.ReadFileListNotExt(Program.SPEC_PATH.ToString(), "*.Spc", COMMON_FUCTION.FileSortMode.FILENAME_ODERBY);
                        List<string> FList = ComF.GetFileList;

                        if (0 < FList.Count)
                        {
                            ModelBoxChangeFlag = true;
                            comboBox1.Items.Clear();
                            foreach (string s in FList) comboBox1.Items.Add(s);

                            if (0 < comboBox1.Items.Count)
                            {
                                if ((mSpec.CarName != null) && (mSpec.CarName != "") && (mSpec.CarName != string.Empty))
                                {
                                    if (comboBox1.Items.Contains(mSpec.CarName) == true) comboBox1.SelectedItem = mSpec.CarName;
                                }
                            }
                            if (mSpec.CarName != null)
                            {
                                string sName = Program.SPEC_PATH.ToString() + "\\" + mSpec.CarName + ".Spc";
                                ComF.OpenSpec(sName, ref mSpec);
                                DisplaySpec();
                            }
                            ModelBoxChangeFlag = false;
                        }
                    };

                    SpecForm.Parent = panel2;
                    SpecForm.Show();
                }
            }
            return;
        }

        private void DisplaySpec()
        {
            if ((CheckItem.Recline == true) || (CheckItem.Relax == true) || (CheckItem.Height == true) || (CheckItem.Legrest == true) || (CheckItem.LegrestExt == true))
            {
                fpSpread1.ActiveSheet.Cells[2, 0].BackColor = SelectColor;
                fpSpread1.ActiveSheet.Cells[2, 1].BackColor = SelectColor;

                if (CheckItem.Recline == true)
                {
                    fpSpread1.ActiveSheet.Cells[2, 2].BackColor = SelectColor;
                    fpSpread1.ActiveSheet.Cells[2, 4].ForeColor = Color.Yellow;
                    fpSpread1.ActiveSheet.Cells[2, 5].ForeColor = Color.White;
                }
                else
                {
                    fpSpread1.ActiveSheet.Cells[2, 2].BackColor = NotSelectColor;
                    fpSpread1.ActiveSheet.Cells[2, 4].ForeColor = Color.Silver;
                    fpSpread1.ActiveSheet.Cells[2, 5].ForeColor = Color.Silver;
                }

                if (CheckItem.Relax == true)
                {
                    fpSpread1.ActiveSheet.Cells[3, 2].BackColor = SelectColor;
                    fpSpread1.ActiveSheet.Cells[3, 4].ForeColor = Color.Yellow;
                    fpSpread1.ActiveSheet.Cells[3, 5].ForeColor = Color.White;
                }
                else
                {
                    fpSpread1.ActiveSheet.Cells[3, 2].BackColor = NotSelectColor;
                    fpSpread1.ActiveSheet.Cells[3, 4].ForeColor = Color.Silver;
                    fpSpread1.ActiveSheet.Cells[3, 5].ForeColor = Color.Silver;
                }
                if (CheckItem.Height == true)
                {
                    fpSpread1.ActiveSheet.Cells[4, 2].BackColor = SelectColor;
                    fpSpread1.ActiveSheet.Cells[4, 4].ForeColor = Color.Yellow;
                    fpSpread1.ActiveSheet.Cells[4, 5].ForeColor = Color.White;
                }
                else
                {
                    fpSpread1.ActiveSheet.Cells[4, 2].BackColor = NotSelectColor;
                    fpSpread1.ActiveSheet.Cells[4, 4].ForeColor = Color.Silver;
                    fpSpread1.ActiveSheet.Cells[4, 5].ForeColor = Color.Silver;
                }
                if (CheckItem.Legrest == true)
                {
                    fpSpread1.ActiveSheet.Cells[5, 2].BackColor = SelectColor;
                    fpSpread1.ActiveSheet.Cells[5, 4].ForeColor = Color.Yellow;
                    fpSpread1.ActiveSheet.Cells[5, 5].ForeColor = Color.White;
                }
                else
                {
                    fpSpread1.ActiveSheet.Cells[5, 2].BackColor = NotSelectColor;
                    fpSpread1.ActiveSheet.Cells[5, 4].ForeColor = Color.Silver;
                    fpSpread1.ActiveSheet.Cells[5, 5].ForeColor = Color.Silver;
                }
                if (CheckItem.LegrestExt == true)
                {
                    fpSpread1.ActiveSheet.Cells[6, 2].BackColor = SelectColor;
                    fpSpread1.ActiveSheet.Cells[6, 4].ForeColor = Color.Yellow;
                    fpSpread1.ActiveSheet.Cells[6, 5].ForeColor = Color.White;
                }
                else
                {
                    fpSpread1.ActiveSheet.Cells[6, 2].BackColor = NotSelectColor;
                    fpSpread1.ActiveSheet.Cells[6, 4].ForeColor = Color.Silver;
                    fpSpread1.ActiveSheet.Cells[6, 5].ForeColor = Color.Silver;
                }
            }
            else
            {
                fpSpread1.ActiveSheet.Cells[2, 0].BackColor = NotSelectColor;
                fpSpread1.ActiveSheet.Cells[2, 1].BackColor = NotSelectColor;
                fpSpread1.ActiveSheet.Cells[2, 2].BackColor = NotSelectColor;
                fpSpread1.ActiveSheet.Cells[3, 2].BackColor = NotSelectColor;
                fpSpread1.ActiveSheet.Cells[4, 2].BackColor = NotSelectColor;
                fpSpread1.ActiveSheet.Cells[5, 2].BackColor = NotSelectColor;
                fpSpread1.ActiveSheet.Cells[6, 2].BackColor = NotSelectColor;

                fpSpread1.ActiveSheet.Cells[2, 4].ForeColor = Color.Silver;
                fpSpread1.ActiveSheet.Cells[2, 5].ForeColor = Color.Silver;
                fpSpread1.ActiveSheet.Cells[3, 4].ForeColor = Color.Silver;
                fpSpread1.ActiveSheet.Cells[3, 5].ForeColor = Color.Silver;
                fpSpread1.ActiveSheet.Cells[4, 4].ForeColor = Color.Silver;
                fpSpread1.ActiveSheet.Cells[4, 5].ForeColor = Color.Silver;
                fpSpread1.ActiveSheet.Cells[5, 4].ForeColor = Color.Silver;
                fpSpread1.ActiveSheet.Cells[5, 5].ForeColor = Color.Silver;
                fpSpread1.ActiveSheet.Cells[6, 4].ForeColor = Color.Silver;
                fpSpread1.ActiveSheet.Cells[6, 5].ForeColor = Color.Silver;
            }

            fpSpread1.ActiveSheet.Cells[7, 0].BackColor = SelectColor;
            fpSpread1.ActiveSheet.Cells[7, 1].BackColor = SelectColor;
            fpSpread1.ActiveSheet.Cells[7, 2].BackColor = SelectColor;
            fpSpread1.ActiveSheet.Cells[7, 4].ForeColor = Color.Yellow;
            fpSpread1.ActiveSheet.Cells[7, 5].ForeColor = Color.White;
            if (CheckItem.Relax == true)
                fpSpread1.ActiveSheet.Cells[7, 3].Text = mSpec.RelaxSWVer;
            else fpSpread1.ActiveSheet.Cells[7, 3].Text = mSpec.PowerReclinerSWVer;
            
            if(PowerOnOff == false) 
                PowerOnOff = true;
            else PowerChange = mSpec.TestVolt;
            
            return;
        }
        public IOControl GetIO
        {
            get { return IOPort; }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            this.Location = new Point(1, 1);
            return;
        }

        //private bool ImsSetButton { get; set; }
        //private bool ImsM1Button { get; set; }
        //private bool ImsM2Button { get; set; }

        //private long ImsSetButtonFirst = 0;
        //private long ImsSetButtonLast = 0;

        private void IOInCheck()
        {
            bool Flag = false;


            if (IOPort.GetRHSelect == true)
            {
                if (comboBox3.SelectedItem == null) comboBox3.SelectedItem = "RH";
                if (comboBox3.SelectedItem.ToString() != "RH")
                {
                    comboBox3.SelectedItem = "RH";
                    CheckItem.LhRh = LH_RH.RH;
                    
                    CanReWrite.LhRhSelect = LH_RH.RH;
                    Flag = true;
                }
            }
            else
            {
                if (comboBox3.SelectedItem == null) comboBox3.SelectedItem = "LH";
                if (comboBox3.SelectedItem.ToString() != "LH")
                {
                    comboBox3.SelectedItem = "LH";
                    CheckItem.LhRh = LH_RH.LH;
                    CanReWrite.LhRhSelect = LH_RH.LH;                    
                    Flag = true;
                }
            }

            if (comboBox3.SelectedItem.ToString() == "LH")
            {
                if(CanReWrite.LhRhSelect != LH_RH.LH) CanReWrite.LhRhSelect = LH_RH.LH;
            }
            else
            {
                if(CanReWrite.LhRhSelect != LH_RH.RH) CanReWrite.LhRhSelect = LH_RH.RH;
            }

            if (IOPort.GetSeatRelax == true)
            {
                if (comboBox4.SelectedItem == null) comboBox4.SelectedItem = "RELAX";

                if (comboBox4.SelectedItem.ToString() != "RELAX")
                {
                    comboBox4.SelectedItem = "RELAX";
                    //CheckItem.Can = true;
                    Flag = true;
                }
            }
            else //if (IOPort.GetSeatPower == true) 
            {
                if (comboBox4.SelectedItem == null) comboBox4.SelectedItem = "RELAX";
                if (comboBox4.SelectedItem.ToString() != "POWER")
                {
                    comboBox4.SelectedItem = "POWER";
                    CheckItem.Can = false;
                    Flag = true;
                }
            }

            if (IOPort.GetTestSetMode == true)
            {
                if (IOPort.GetReclinerFwd == true)
                {
                    if (CheckItem.Recline == false) Flag = true;
                    CheckItem.Recline = true;
                }
                else if (IOPort.GetReclinerBwd == true)
                {
                    if (CheckItem.Recline == true) Flag = true;
                    CheckItem.Recline = false;
                }

                if (IOPort.GetRelax_Relax == true)
                {
                    if (CheckItem.Relax == false) Flag = true;
                    CheckItem.Relax = true;
                }
                else if (IOPort.GetRelaxReturn == true)
                {
                    if (CheckItem.Relax == true) Flag = true;
                    CheckItem.Relax = false;
                }
                if (IOPort.GetHeightUp == true)
                {
                    if (CheckItem.Height == false) Flag = true;
                    CheckItem.Height = true;
                }
                else if (IOPort.GetHeightDn == true)
                {
                    if (CheckItem.Height == true) Flag = true;
                    CheckItem.Height = false;
                }

                if (IOPort.GetLegrest_Rest == true)
                {
                    if (CheckItem.Legrest == false) Flag = true;
                    CheckItem.Legrest = true;
                }
                else if (IOPort.GetLegrest_Return == true)
                {
                    if (CheckItem.Legrest == true) Flag = true;
                    CheckItem.Legrest = false;
                }

                if (IOPort.GetLegrestExt == true)
                {
                    if (CheckItem.LegrestExt == false) Flag = true;
                    CheckItem.LegrestExt = true;
                }
                else if (IOPort.GetLegrestExtReturn == true)
                {
                    if (CheckItem.LegrestExt == true) Flag = true;
                    CheckItem.LegrestExt = false;
                }
            }
            else
            {
                if (ledArrow4.Value.AsBoolean != IOPort.GetReclinerFwd)
                {
                    ledArrow4.Value.AsBoolean = IOPort.GetReclinerFwd;
                    C_ReclineFwdOnOff = IOPort.GetReclinerFwd;
                }
                if (ledArrow3.Value.AsBoolean != IOPort.GetReclinerBwd)
                {
                    ledArrow3.Value.AsBoolean = IOPort.GetReclinerBwd;
                    C_ReclineBwdOnOff = IOPort.GetReclinerBwd;
                }
                if (ledArrow7.Value.AsBoolean != IOPort.GetRelax_Relax)
                {
                    ledArrow7.Value.AsBoolean = IOPort.GetRelax_Relax;
                    C_RelaxOnOff = IOPort.GetRelax_Relax;
                }
                if (ledArrow8.Value.AsBoolean != IOPort.GetRelaxReturn)
                {
                    ledArrow8.Value.AsBoolean = IOPort.GetRelaxReturn;
                    C_RelaxReturnOnOff = IOPort.GetRelaxReturn;
                }
                //if (ledArrow5.Value.AsBoolean != IOPort.GetHeightUp) ledArrow5.Value.AsBoolean = IOPort.GetHeightUp;
                //if (ledArrow6.Value.AsBoolean != IOPort.GetHeightDn) ledArrow6.Value.AsBoolean = IOPort.GetHeightDn;

                if (ledArrow11.Value.AsBoolean != IOPort.GetLegrest_Rest)
                {
                    ledArrow11.Value.AsBoolean = IOPort.GetLegrest_Rest;
                    C_LegrestOnOff = IOPort.GetLegrest_Rest;
                }
                if (ledArrow12.Value.AsBoolean != IOPort.GetLegrest_Return)
                {
                    ledArrow12.Value.AsBoolean = IOPort.GetLegrest_Return;
                    C_LegrestReturnOnOff = IOPort.GetLegrest_Return;
                }

                if (ledArrow9.Value.AsBoolean != IOPort.GetLegrestExt)
                {
                    ledArrow9.Value.AsBoolean = IOPort.GetLegrestExt;
                    C_LegrestExtOnOff = IOPort.GetLegrestExt;
                }
                if (ledArrow10.Value.AsBoolean != IOPort.GetLegrestExtReturn)
                {
                    ledArrow10.Value.AsBoolean = IOPort.GetLegrestExtReturn;
                    C_LegrestExtReturnOnOff = IOPort.GetLegrestExtReturn;
                }
            }
            //----------------------------------------------------------------------
            //사양에 따라 모델 선택을 진행한다.

            //if(Flag == true)
            //{
            //    string LHType = comboBox3.SelectedItem.ToString();
            //    string PSeat = comboBox4.SelectedItem.ToString();
            //    string s = comboBox1.SelectedItem.ToString();
            //    //string LHDType;
            //    string RELAXType;
            //    string ModelType;

            //    //if (CheckItem.LhdRhd == LHD_RHD.LHD)
            //    //    LHDType = "LHD";
            //    //else LHDType = "RHD";

            //    RELAXType = comboBox4.SelectedItem.ToString();

            //    if (comboBox3.SelectedItem != null)
            //        LHType = comboBox3.SelectedItem.ToString();
            //    else LHType = "LH";

            //    ModelType = RELAXType + "_" + LHType + "_" + PSeat;

            //    if (s.IndexOf(ModelType) < 0)
            //    {
            //        foreach (string Item in comboBox1.Items)
            //        {
            //            if (0 <= Item.IndexOf(ModelType))
            //            {
            //                comboBox1.SelectedItem = Item;
            //                Flag = false;
            //                break;
            //            }
            //        }
            //    }
            //}

            //----------------------------------------------------------------------
            if (Flag == true) DisplaySpec();
            return;
        }

        private BackgroundWorker backgroundWorker1 = null;
        private void ThreadSetting()
        {
            backgroundWorker1 = new BackgroundWorker();

            //ReportProgress메소드를 호출하기 위해서 반드시 true로 설정, false일 경우 ReportProgress메소드를 호출하면 exception 발생
            backgroundWorker1.WorkerReportsProgress = true;
            //스레드에서 취소 지원 여부
            backgroundWorker1.WorkerSupportsCancellation = true;
            //스레드가 run시에 호출되는 핸들러 등록
            backgroundWorker1.DoWork += new DoWorkEventHandler(BackgroundWorker1_DoWork);
            // ReportProgress메소드 호출시 호출되는 핸들러 등록
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(BackgroundWorker1_ProgressChanged);
            // 스레드 완료(종료)시 호출되는 핸들러 동록
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker1_RunWorkerCompleted);


            // 스레드가 Busy(즉, run)가 아니라면
            if (backgroundWorker1.IsBusy != true)
            {
                // 스레드 작동!! 아래 함수 호출 시 위에서 bw.DoWork += new DoWorkEventHandler(bw_DoWork); 에 등록한 핸들러가
                // 호출 됩니다.

                backgroundWorker1.RunWorkerAsync();
            }
            return;
        }

        private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //바로 위에서 worker.ReportProgress((i * 10));호출 시 
            // bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged); 등록한 핸들러가 호출 된다고
            // 하였는데요.. 이 부분에서는 기존 Thread에서 처럼 Dispatcher를 이용하지 않아도 됩니다. 
            // 즉 아래처럼!!사용이 가능합니다.
            //this.tbProgress.Text = (e.ProgressPercentage.ToString() + "%");

            // 기존의 Thread클래스에서 아래와 같이 UI 엘리먼트를 갱신하려면
            // Dispatcher.BeginInvoke(delegate() 
            // {
            //        this.tbProgress.Text = (e.ProgressPercentage.ToString() + "%");
            // )};
            //처럼 처리해야 할 것입니다. 그러나 바로 UI 엘리먼트를 업데이트 하고 있죠??
        }


        //스레드의 run함수가 종료될 경우 해당 핸들러가 호출됩니다.
        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            //스레드가 종료한 이유(사용자 취소, 완료, 에러)에 맞쳐 처리하면 됩니다.
            if ((e.Cancelled == true))
            {
            }
            else if (!(e.Error == null))
            {

            }
            else
            {

            }
        }


        private long TotalTimeToFirst;
        private long TotalTimeToLast;
        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            do
            {
                //CancellationPending 속성이 true로 set되었다면(위에서 CancelAsync 메소드 호출 시 true로 set된다고 하였죠?
                if ((worker.CancellationPending == true))
                {
                    //루프를 break한다.(즉 스레드 run 핸들러를 벗어나겠죠)
                    e.Cancel = true;
                    break;
                }
                else
                {
                    // 이곳에는 스레드에서 처리할 연산을 넣으시면 됩니다.

                    this.Invoke(new EventHandler(Processing));

                    Thread.Sleep(1);
                    // 스레드 진행상태 보고 - 이 메소드를 호출 시 위에서 
                    // bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged); 등록한 핸들러가 호출 됩니다.
                    worker.ReportProgress(10);
                }
                if ((ExitFlag == true) || (RunningFlag == false))
                {
                    worker.CancelAsync();
                }
            } while (true);//while ((ExitFlag == false) && (RunningFlag == true));
            //;
        }
        private bool SpecOutputFlag { get; set; }
        private short Step { get; set; }
        private short SubStep { get; set; }

        private double NormalSound { get; set; }
        private bool PowerControlFlag = false;
        private long StepTimeFirst = 0;
        private long StepTimeLast = 0;
        //private string CanData = "";
        private string OldCanData = "";
        private void Processing(object sender, EventArgs e)
        {
            TotalTimeToLast = ComF.timeGetTimems();

            __CanMsg Can = CanReWrite.GetCanData;

            if (0x700 <= Can.ID)
            {
                string CanData = string.Format("ID - {0:X4}  : {1:X2}, {2:X2}, {3:X2}, {4:X2}, {5:X2}, {6:X2}, {7:X2}, {8:X2}", Can.ID,
                    Can.DATA[0], Can.DATA[1], Can.DATA[2], Can.DATA[3], Can.DATA[4], Can.DATA[5], Can.DATA[6], Can.DATA[7]);

                if (CanData != OldCanData)
                {
                    richTextBox1.AppendText(CanData + "\r");
                    richTextBox1.ScrollToCaret();
                    OldCanData = CanData;
                }
            }
            if (sevenSegmentInteger4.Value.AsInteger != ((TotalTimeToLast - TotalTimeToFirst) / 1000)) sevenSegmentInteger4.Value.AsInteger = (int)((TotalTimeToLast - TotalTimeToFirst) / 1000);
            switch (Step)
            {
                case 0:
                    if (SpecOutputFlag == false)
                    {
                        //if (Config.AutoConnection == true) IOPort.SetPinConnection = true;
                        mData.Time = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();

                        SubStep = 0;

                        SpecOutputFlag = true;
                        CurrFilter.InitAll();

                        StepTimeFirst = ComF.timeGetTimems();
                        StepTimeLast = ComF.timeGetTimems();
                        NormalSound = 0;

                    }
                    else
                    {
                        SpecOutputFlag = false;
                        Step++;
                    }
                    //Jig On
                    break;
                case 1:
                    //Batt
                    if (SpecOutputFlag == false)
                    {
                        PowerControlFlag = false;                        
                        StepTimeFirst = ComF.timeGetTimems();
                        StepTimeLast = ComF.timeGetTimems();
                        SpecOutputFlag = true;
                    }
                    else
                    {
                        StepTimeLast = ComF.timeGetTimems();
                        if (500 <= (StepTimeLast - StepTimeFirst))
                        {
                            //자동 컨넥션이 안정화 되는 시간을 줌
                            if (PowerControlFlag == false)
                            {
                                BattOnOff = true;
                                PowerControlFlag = true;
                            }
                        }
                        if (1500 <= (StepTimeLast - StepTimeFirst))
                        {
                            CanReWrite.SetLimitClear = true;
                            Step++;
                            SpecOutputFlag = false;
                        }
                    }
                    break;
                case 2:
                    CheckVersion();
                    break;
                case 3:
                    bool vCheckFlag = false;
                    if (CheckItem.Relax == true)
                    {
                        if (CanReWrite.GetRelaxLeftRightLimitSensor == true)
                        {
                            vCheckFlag = true;
                        }
                        else
                        {
                            panel7.Parent = this;
                            panel7.Visible = true;
                            panel7.BringToFront();
                            panel7.Location = new Point(this.Left + ((this.Width / 2) - (panel7.Width / 2)), 300);

                            mData.Result = RESULT.REJECT;
                            
                            mData.Height.Test = true;
                            mData.Height.Result1 = RESULT.REJECT;
                            mData.Height.Result2 = RESULT.REJECT;

                            mData.Relax.Test = true;
                            mData.Relax.Result1 = RESULT.REJECT;
                            mData.Relax.Result2 = RESULT.REJECT;

                            mData.Legrest.Test = true;
                            mData.Legrest.Result1 = RESULT.REJECT;
                            mData.Legrest.Result2 = RESULT.REJECT;

                            mData.LegrestExt.Test = true;
                            mData.LegrestExt.Result1 = RESULT.REJECT;
                            mData.LegrestExt.Result2 = RESULT.REJECT;

                            mData.Recline.Test = true;
                            mData.Recline.Result1 = RESULT.REJECT;
                            mData.Recline.Result2 = RESULT.REJECT;

                            ResultCheck();
                            Step = 14;
                        }
                    }
                    else
                    {
                        vCheckFlag = true;
                    }
                    if(vCheckFlag == true)
                    { 
                        label5.Text = "가상 리미트 중 입니다.";
                        if (Config.AllLimit == false)
                        {
                            VirtualLimiteToRecline();
                            //VirtualLimiteToLegrest();
                            //VirtualLimiteToLegrestExt();
                        }
                        else
                        {
                            VirtualLimiteToAll();
                        }
                        StepTimeFirst = ComF.timeGetTimems();
                        StepTimeLast = ComF.timeGetTimems();
                        CurrFilter.InitAll();
                        VirtualLimitStartFlag = false;
                        Step++;
                        SpecOutputFlag = false;
                    }
                    break;
                case 4:
                    CheckVirtualLimitEnd();
                    break;
                case 5:
                    CheckVirtualLimite();
                    break;
                case 6:
                    //DeliveryPosMoveing();
                    //if (CheckItem.Relax == true)
                    //{
                    //    DeliveryPosMoveingToRecline();
                    //}
                    //else
                    //{
                    Step++;
                    SpecOutputFlag = false;
                    //}
                    break;
                case 7:
                case 13:
                    if(SpecOutputFlag == false)
                    {
                        ResultCheck();
                        SpecOutputFlag = true;
                        if (Config.AutoConnection == true)
                        {
                            CanOpenClose = false;
                            BattOnOff = false;
                            IOPort.SetPinConnection = false;
                        }
                        StepTimeFirst = ComF.timeGetTimems();
                        StepTimeLast = ComF.timeGetTimems();
                    }
                    else
                    {
                        StepTimeLast = ComF.timeGetTimems();

                        if ((Config.AutoConnection == true) && (IOPort.GetAuto == true))
                        {
                            if(IOPort.GetPinConnectionBwd == true)
                            {
                                SpecOutputFlag = false;
                                Step++;
                            }
                            else
                            {
                                if((long)(Config.ConnectionDelay * 1000) <= (StepTimeLast - StepTimeFirst))
                                {
                                    SpecOutputFlag = false;
                                    Step++;
                                }
                            }
                        }
                        else
                        {
                            SpecOutputFlag = false;
                            Step++;
                        }
                    }
                    break;
                default:
                    
                    if (mData.Result != RESULT.REJECT)
                    {
                        IOPort.TestINGOnOff = false;
                        IOPort.YellowLampOnOff = false;
                        IOPort.RedLampOnOff = false;
                        IOPort.GreenLampOnOff = true;
                        if ((Config.AutoConnection == true) && (IOPort.GetAuto == true)) IOPort.TestOKOnOff = true;                        
                        //IOPort.TestNGOnOff = false;
                        TInfor.Count.OK++;
                        SendTestData();
                    }
                    else
                    {
                        IOPort.YellowLampOnOff = false;
                        IOPort.RedLampOnOff = true;
                        IOPort.GreenLampOnOff = false;
                        //if (Config.AutoConnection == false) IOPort.TestNGOnOff = true;
                                                
                        IOPort.TestOKOnOff = false;
                        TInfor.Count.NG++;
                        TestEndResultToReject = true;
                        SendTestData();
                    }
                    ProductOutFlag = true;
                    RunningFlag = false;
                    SaveData();
                    break;
            }
            return;
        }

        private void DeliveryPosMoveingToRecline()
        {
            if (SpecOutputFlag == false)
            {
                PosMoveFlag = false;
                SpecOutputFlag = true;

                if (CheckItem.Recline == true)
                {
                    C_ReclineFwdOnOff = true;
                    ReclineMotorMoveEndFlag = false;
                }
                else
                {
                    ReclineMotorMoveEndFlag = true;
                }

                StepTimeFirst = ComF.timeGetTimems();
                StepTimeLast = ComF.timeGetTimems();

                label5.Text = "출하 위치로 이동 중 입니다.";
            }
            else
            {
                StepTimeLast = ComF.timeGetTimems();

                if (ReclineMotorMoveEndFlag == false)
                {
                    if (3000 <= (StepTimeLast - StepTimeFirst))
                    {
                        C_ReclineFwdOnOff = false;
                        ReclineMotorMoveEndFlag = true;
                    }
                }
            }

            if (ReclineMotorMoveEndFlag == true)
            {
                SpecOutputFlag = false;
                Step++;
            }
            return;
        }
        private bool TestEndResultToReject { get; set; }
        private long RelaxRunTime { get; set; }
        private bool RelaxRunTimeFlag { get; set; }
        private bool VirtualLimitStartFlag { get; set; }

        private void RelaxCheck()
        {
            if (SpecOutputFlag == false)
            {
                //label5.Text = "RELAX 가상 리미트 설정 입니다.";
                StepTimeFirst = ComF.timeGetTimems();
                StepTimeLast = ComF.timeGetTimems();
                SpecOutputFlag = true;

                //RelaxOnOff = true;
                //RelaxRunTimeFlag = false;
                //CurrFilter.InitAll();
            }
            else
            {
                float AdData;

                //if ((mSpec.RelaxSwOnTime * 1000) <= (StepTimeLast - StepTimeFirst))
                //{
                //    if (RelaxOnOff == true) RelaxOnOff = false;
                //}

                //if ((mSpec.RelaxLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))
                //{
                //    if (RelaxOnOff == true) RelaxOnOff = false;
                //    if (RelaxReturnOnOff == true) RelaxReturnOnOff = false;
                //    if (Pos == 0)
                //    {
                //        mData.Relax.Result1 = RESULT.REJECT;
                //        if (mData.Relax.Result1 == RESULT.REJECT) mData.Result = RESULT.REJECT;
                //    }
                //    else
                //    {
                //        mData.Relax.Result2 = RESULT.REJECT;
                //        if (mData.Relax.Result1 == RESULT.REJECT) mData.Result = RESULT.REJECT;
                //    }
                //    mData.Relax.Test = true;

                //    if (Pos == 1)
                //    {
                //        fpSpread1.ActiveSheet.Cells[3, 3].Text = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? "2" : "1";
                //        fpSpread1.ActiveSheet.Cells[3, 4].Text = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? "O.K" : "N.G";
                //        fpSpread1.ActiveSheet.Cells[3, 4].ForeColor = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? Color.Lime : Color.Red;
                //    }
                //    Step++;
                //    SpecOutputFlag = false;
                //    ComF.timedelay(500);
                //}

                StepTimeLast = ComF.timeGetTimems();
                AdData = pMeter.GetPSeat;

                //if (0.5 < AdData)
                //{
                //    if (RelaxRunTimeFlag == false)
                //    {
                //        RelaxRunTimeFlag = true;
                //        RelaxRunTime = ComF.timeGetTimems();
                //    }
                //    RelaxRunTime = ComF.timeGetTimems() - RelaxRunTime;
                //    VirtualLimitStartFlag = true;
                //}

                //plot1.Channels[0].AddXY(plot1.Channels[0].Count, AdData);
                //plot1.YAxes[0].Tracking.ZoomToFitAll();
                //plot1.XAxes[0].Tracking.ZoomToFitAll();


                if (AdData < 0.5)
                {
                    //if (VirtualLimitStartFlag == true)
                    //{
                        //float CheckData;

                        //if (float.TryParse(fpSpread1.ActiveSheet.Cells[3, 6].Text, out CheckData) == false) CheckData = 0;

                        if (RelaxOnOff == true) RelaxOnOff = false;
                        if (RelaxReturnOnOff == true) RelaxReturnOnOff = false;
                        //if (Pos == 0)
                        //{
                            //mData.Relax.Data1 = CheckData;
                        mData.Relax.Result1 = RESULT.PASS;
                        if (mData.Relax.Result1 == RESULT.REJECT) mData.Result = RESULT.REJECT;
                        //}
                        //else
                        //{
                            //mData.Relax.Data2 = CheckData;
                        mData.Relax.Result2 = RESULT.PASS;
                        if (mData.Relax.Result1 == RESULT.REJECT) mData.Result = RESULT.REJECT;
                        //}
                        mData.Relax.Test = true;

                        //if (Pos == 1)
                        //{
                            fpSpread1.ActiveSheet.Cells[3, 3].Text = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? "2" : "1";
                            fpSpread1.ActiveSheet.Cells[3, 4].Text = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? "O.K" : "N.G";
                            fpSpread1.ActiveSheet.Cells[3, 4].ForeColor = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? Color.Lime : Color.Red;
                        //}
                        Step++;
                        SpecOutputFlag = false;
                        ComF.timedelay(500);
                    //}
                }
                //else
                //{
                //    //fpSpread1.ActiveSheet.Cells[3, 3].Text = AdData.ToString("0.0");
                //}
            }
            return;
        }


        private bool TestVoltChangeFlag = false;
        private void CheckVirtualLimitEnd()
        {
            if (SpecOutputFlag == false)
            {
                StepTimeFirst = ComF.timeGetTimems();
                StepTimeLast = ComF.timeGetTimems();
                CurrFilter.InitAll();
                SpecOutputFlag = true;
                VirtualLimitStartFlag = false;
                TestVoltChangeFlag = true;
            }
            else
            {
                float AdData;


                if(mSpec.EndVoltUse == true)
                {
                    if(TestVoltChangeFlag == false)
                    {
                        if ((long)(mSpec.TestVoltOperTime * 1000F) <= (StepTimeLast - StepTimeFirst))
                        {
                            PowerChange = mSpec.EndVolt;
                            TestVoltChangeFlag = true;
                        }
                    }
                }


                StepTimeLast = ComF.timeGetTimems();
                AdData = pMeter.GetPSeat;

                plot1.Channels[0].AddXY(plot1.Channels[0].Count, AdData);
                plot1.YAxes[0].Tracking.ZoomToFitAll();
                plot1.XAxes[0].Tracking.ZoomToFitAll();

                if ((mSpec.ReclinerLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))//time over
                {
                    Step++;
                    SpecOutputFlag = false;
                    ComF.timedelay(500);
                    return;
                }
                if (1000 <= (StepTimeLast - StepTimeFirst))
                {
                    if (CanReWrite.GetCanFail == true)//통신 불량
                    {
                        Step++;
                        SpecOutputFlag = false;
                        ComF.timedelay(500);
                        return;
                    }

                    CanMap.VirtualLimitState Rl = CanReWrite.RLVirtualLimitDataRead();
                    CanMap.VirtualLimitState Rr = CanReWrite.RRVirtualLimitDataRead();

                    if (VirtualLimitStartFlag == false)
                    {
                        if (0.5 <= AdData) VirtualLimitStartFlag = true;

                        if (5000 <= (StepTimeLast - StepTimeFirst)) //가상 리미트 동작 안함
                        {
                            Step++;
                            SpecOutputFlag = false;
                            ComF.timedelay(500);
                        }
                    }
                    else
                    {
                        if (5000 <= (StepTimeLast - StepTimeFirst))
                        {
                            bool Flag = true;

                            if (comboBox3.SelectedItem.ToString() == "LH")
                            {
                                if (CheckItem.Recline == true)
                                {
                                    if ((Rl.Recline != CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) || (Rl.Recline != CanMap.VirtualLimitValue.NoSetOperationAndNotLimitSet)) Flag = false;
                                }
                                if (CheckItem.Relax == true)
                                {
                                    if ((Rl.Relax != CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) || (Rl.Relax != CanMap.VirtualLimitValue.NoSetOperationAndNotLimitSet)) Flag = false;
                                }
                                if (CheckItem.Height == true)
                                {
                                    if ((Rl.Height != CanMap.VirtualLimitValue.NoSetOperationAndLimitSet)  ||(Rl.Height != CanMap.VirtualLimitValue.NoSetOperationAndNotLimitSet)) Flag = false;
                                }
                                if (CheckItem.Legrest == true)
                                {
                                    if ((Rl.Legrest != CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) && (Rl.Legrest != CanMap.VirtualLimitValue.NoSetOperationAndNotLimitSet)) Flag = false;
                                }
                                if (CheckItem.LegrestExt == true)
                                {
                                    if ((Rl.LegrestExt != CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) && (Rl.LegrestExt != CanMap.VirtualLimitValue.NoSetOperationAndNotLimitSet)) Flag = false;
                                }
                            }
                            else
                            {
                                if (CheckItem.Recline == true)
                                {
                                    if ((Rr.Recline != CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) || (Rr.Recline != CanMap.VirtualLimitValue.NoSetOperationAndNotLimitSet)) Flag = false;
                                }
                                if (CheckItem.Relax == true)
                                {
                                    if ((Rr.Relax != CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) || (Rr.Relax != CanMap.VirtualLimitValue.NoSetOperationAndNotLimitSet)) Flag = false;
                                }
                                if (CheckItem.Height == true)
                                {
                                    if ((Rr.Height != CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) || (Rr.Height != CanMap.VirtualLimitValue.NoSetOperationAndNotLimitSet)) Flag = false;
                                }
                                if (CheckItem.Legrest == true)
                                {
                                    if ((Rr.Legrest != CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) && (Rr.Legrest != CanMap.VirtualLimitValue.NoSetOperationAndNotLimitSet)) Flag = false;
                                }
                                if (CheckItem.LegrestExt == true)
                                {
                                    if ((Rr.LegrestExt != CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) && (Rr.LegrestExt != CanMap.VirtualLimitValue.NoSetOperationAndNotLimitSet)) Flag = false;
                                }
                            }
                            

                            if (Flag == true)
                            {
                                Step++;
                                SpecOutputFlag = false;
                                //ComF.timedelay(500);
                            }
                        }
                    }
                }
            }
            return;
        }

        private void CheckVirtualLimite()
        {
            if (SpecOutputFlag == false)
            {
                SpecOutputFlag = true;
                StepTimeFirst = ComF.timeGetTimems();
                StepTimeLast = ComF.timeGetTimems();
                CurrFilter.InitAll();
            }
            else
            {
                StepTimeLast = ComF.timeGetTimems();
                if (500 <= (StepTimeLast - StepTimeFirst))
                {
                    if (CanReWrite.GetCanFail == true)//통신 불량
                    {
                        if (CheckItem.Recline == true)
                        {
                            mData.Recline.Result1 = RESULT.REJECT;
                            mData.Recline.Result2 = RESULT.REJECT;

                            if ((mData.Recline.Result1 == RESULT.REJECT) || (mData.Recline.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;

                            mData.Recline.Data1 = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? 2 : 1;
                            fpSpread1.ActiveSheet.Cells[2, 4].Text = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? "2" : "1";

                            fpSpread1.ActiveSheet.Cells[2, 5].Text = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                            fpSpread1.ActiveSheet.Cells[2, 5].ForeColor = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;

                            mData.Recline.Test = true;
                        }


                        if (CheckItem.Relax == true)
                        {
                            mData.Relax.Result1 = RESULT.REJECT;
                            mData.Relax.Result2 = RESULT.REJECT;

                            if ((mData.Relax.Result1 == RESULT.REJECT) || (mData.Relax.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;

                            mData.Relax.Test = true;

                            fpSpread1.ActiveSheet.Cells[3, 4].Text = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? "2" : "1";
                            fpSpread1.ActiveSheet.Cells[3, 5].Text = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? "O.K" : "N.G";
                            fpSpread1.ActiveSheet.Cells[3, 5].ForeColor = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? Color.Lime : Color.Red;
                        }
                        if (CheckItem.Height == true)
                        {
                            mData.Height.Result1 = RESULT.REJECT;
                            mData.Height.Result2 = RESULT.REJECT;
                            if ((mData.Height.Result1 == RESULT.REJECT) || (mData.Height.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;

                            mData.Height.Test = true;
                            fpSpread1.ActiveSheet.Cells[4, 4].Text = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? "2" : "1";
                            fpSpread1.ActiveSheet.Cells[4, 5].Text = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                            fpSpread1.ActiveSheet.Cells[4, 5].ForeColor = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                        }
                        if (CheckItem.Legrest == true)
                        {
                            mData.Legrest.Result1 = RESULT.REJECT;
                            mData.Legrest.Result2 = RESULT.REJECT;
                            if ((mData.Legrest.Result1 == RESULT.REJECT) || (mData.Legrest.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;

                            mData.Legrest.Test = true;

                            fpSpread1.ActiveSheet.Cells[5, 4].Text = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? "2" : "1";
                            fpSpread1.ActiveSheet.Cells[5, 5].Text = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                            fpSpread1.ActiveSheet.Cells[5, 5].ForeColor = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;

                        }
                        if (CheckItem.LegrestExt == true)
                        {
                            mData.LegrestExt.Result1 = RESULT.REJECT;
                            mData.LegrestExt.Result2 = RESULT.REJECT;
                            mData.LegrestExt.Test = true;

                            if ((mData.LegrestExt.Result1 == RESULT.REJECT) || (mData.LegrestExt.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;

                            fpSpread1.ActiveSheet.Cells[6, 4].Text = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? "2" : "1";
                            fpSpread1.ActiveSheet.Cells[6, 5].Text = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                            fpSpread1.ActiveSheet.Cells[6, 5].ForeColor = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                        }


                        Step = 13;
                        SpecOutputFlag = false;
                        ComF.timedelay(500);
                        return;
                    }

                    CanMap.VirtualLimitState Rl = CanReWrite.RLVirtualLimitDataRead();
                    CanMap.VirtualLimitState Rr = CanReWrite.RRVirtualLimitDataRead();


                    if (60000 <= (StepTimeLast - StepTimeFirst))
                    {                        
                        if (CheckItem.Recline == true)
                        {
                            if ((Rl.Recline == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) || (Rr.Recline == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet))
                            {
                                mData.Recline.Result1 = RESULT.PASS;
                                mData.Recline.Result2 = RESULT.PASS;
                                mData.Result = RESULT.PASS;
                                mData.Recline.Data1 = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? 2 : 1;
                                fpSpread1.ActiveSheet.Cells[2, 4].Text = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? "2" : "1";
                                fpSpread1.ActiveSheet.Cells[2, 5].Text = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                                fpSpread1.ActiveSheet.Cells[2, 5].ForeColor = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                                mData.Recline.Test = true;
                            }
                            else
                            {
                                mData.Recline.Result1 = RESULT.REJECT;
                                mData.Recline.Result2 = RESULT.REJECT;

                                if ((mData.Recline.Result1 == RESULT.REJECT) || (mData.Recline.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;

                                mData.Recline.Data1 = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? 2 : 1;
                                fpSpread1.ActiveSheet.Cells[2, 4].Text = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? "2" : "1";

                                fpSpread1.ActiveSheet.Cells[2, 5].Text = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                                fpSpread1.ActiveSheet.Cells[2, 5].ForeColor = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;

                                mData.Recline.Test = true;
                            }
                        }
                        if (CheckItem.Relax == true)
                        {
                            if ((Rl.Relax == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) || (Rr.Relax == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet))
                            {
                                mData.Relax.Result1 = RESULT.PASS;
                                mData.Relax.Result2 = RESULT.PASS;
                                mData.Result = RESULT.PASS;
                                mData.Relax.Data1 = ((mData.Relax.Result1 == RESULT.PASS) && (mData.Relax.Result2 == RESULT.PASS)) ? 2 : 1;
                                fpSpread1.ActiveSheet.Cells[3, 4].Text = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? "2" : "1";
                                fpSpread1.ActiveSheet.Cells[3, 5].Text = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? "O.K" : "N.G";
                                fpSpread1.ActiveSheet.Cells[3, 5].ForeColor = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? Color.Lime : Color.Red;
                                mData.Relax.Test = true;
                            }
                            else
                            {
                                mData.Relax.Result1 = RESULT.REJECT;
                                mData.Relax.Result2 = RESULT.REJECT;

                                if ((mData.Relax.Result1 == RESULT.REJECT) || (mData.Relax.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;

                                mData.Relax.Data1 = ((mData.Relax.Result1 == RESULT.PASS) && (mData.Relax.Result2 == RESULT.PASS)) ? 2 : 1;
                                fpSpread1.ActiveSheet.Cells[3, 4].Text = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? "2" : "1";
                                fpSpread1.ActiveSheet.Cells[3, 5].Text = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? "O.K" : "N.G";
                                fpSpread1.ActiveSheet.Cells[3, 5].ForeColor = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? Color.Lime : Color.Red;
                                mData.Relax.Test = true;
                            }
                        }

                        if(CheckItem.Height == true)
                        {
                            if ((Rl.Height == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) || (Rr.Height == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet))
                            {
                                mData.Height.Result1 = RESULT.PASS;
                                mData.Height.Result2 = RESULT.PASS;
                                mData.Result = RESULT.PASS;
                                mData.Height.Data1 = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? 2 : 1;
                                fpSpread1.ActiveSheet.Cells[4, 4].Text = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? "2" : "1";
                                fpSpread1.ActiveSheet.Cells[4, 5].Text = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                                fpSpread1.ActiveSheet.Cells[4, 5].ForeColor = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                                mData.Height.Test = true;
                            }
                            else
                            {
                                mData.Height.Result1 = RESULT.REJECT;
                                mData.Height.Result2 = RESULT.REJECT;

                                if ((mData.Height.Result1 == RESULT.REJECT) || (mData.Height.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;

                                mData.Height.Data1 = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? 2 : 1;
                                fpSpread1.ActiveSheet.Cells[4, 4].Text = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? "2" : "1";
                                fpSpread1.ActiveSheet.Cells[4, 5].Text = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                                fpSpread1.ActiveSheet.Cells[4, 5].ForeColor = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                                mData.Height.Test = true;
                            }
                        }

                        if(CheckItem.Legrest == true)
                        {
                            if ((Rl.Legrest == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) || (Rr.Legrest == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet))
                            {
                                mData.Legrest.Result1 = RESULT.PASS;
                                mData.Legrest.Result2 = RESULT.PASS;
                                mData.Result = RESULT.PASS;
                                mData.Legrest.Data1 = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? 2 : 1;
                                fpSpread1.ActiveSheet.Cells[5, 4].Text = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? "2" : "1";
                                fpSpread1.ActiveSheet.Cells[5, 5].Text = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                                fpSpread1.ActiveSheet.Cells[5, 5].ForeColor = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                                mData.Legrest.Test = true;
                            }
                            else
                            {
                                mData.Legrest.Result1 = RESULT.REJECT;
                                mData.Legrest.Result2 = RESULT.REJECT;

                                if ((mData.Legrest.Result1 == RESULT.REJECT) || (mData.Legrest.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;

                                mData.Legrest.Data1 = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? 2 : 1;
                                fpSpread1.ActiveSheet.Cells[5, 4].Text = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? "2" : "1";
                                fpSpread1.ActiveSheet.Cells[5, 5].Text = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                                fpSpread1.ActiveSheet.Cells[5, 5].ForeColor = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                                mData.Legrest.Test = true;
                            }
                        }

                        if (CheckItem.LegrestExt == true)
                        {
                            if ((Rl.LegrestExt == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) || (Rr.LegrestExt == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet))
                            {
                                mData.LegrestExt.Result1 = RESULT.PASS;
                                mData.LegrestExt.Result2 = RESULT.PASS;
                                mData.Result = RESULT.PASS;
                                mData.LegrestExt.Data1 = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? 2 : 1;
                                fpSpread1.ActiveSheet.Cells[6, 4].Text = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? "2" : "1";
                                fpSpread1.ActiveSheet.Cells[6, 5].Text = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                                fpSpread1.ActiveSheet.Cells[6, 5].ForeColor = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                                mData.LegrestExt.Test = true;
                            }
                            else
                            {
                                mData.LegrestExt.Result1 = RESULT.REJECT;
                                mData.LegrestExt.Result2 = RESULT.REJECT;

                                if ((mData.LegrestExt.Result1 == RESULT.REJECT) || (mData.LegrestExt.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;

                                mData.LegrestExt.Data1 = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? 2 : 1;
                                fpSpread1.ActiveSheet.Cells[6, 4].Text = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? "2" : "1";
                                fpSpread1.ActiveSheet.Cells[6, 5].Text = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                                fpSpread1.ActiveSheet.Cells[6, 5].ForeColor = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                                mData.LegrestExt.Test = true;
                            }
                        }
                        Step++;
                        SpecOutputFlag = false;
                        ComF.timedelay(500);
                    }
                    else
                    {
                        bool Flag = true;
                        if (CheckItem.Recline == true)
                        {
                            if ((Rl.Recline == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) || (Rr.Recline == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet))
                            {
                                mData.Recline.Result1 = RESULT.PASS;
                                mData.Recline.Result2 = RESULT.PASS;
                                mData.Result = RESULT.PASS;
                                mData.Recline.Data1 = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? 2 : 1;
                                fpSpread1.ActiveSheet.Cells[2, 4].Text = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? "2" : "1";
                                fpSpread1.ActiveSheet.Cells[2, 5].Text = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                                fpSpread1.ActiveSheet.Cells[2, 5].ForeColor = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                                mData.Recline.Test = true;
                            }
                            else
                            {
                                Flag = false;
                            }
                        }
                        if (CheckItem.Relax == true)
                        {
                            if ((Rl.Relax == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) || (Rr.Relax == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet))
                            {
                                mData.Relax.Result1 = RESULT.PASS;
                                mData.Relax.Result2 = RESULT.PASS;
                                mData.Result = RESULT.PASS;
                                mData.Relax.Data1 = ((mData.Relax.Result1 == RESULT.PASS) && (mData.Relax.Result2 == RESULT.PASS)) ? 2 : 1;
                                fpSpread1.ActiveSheet.Cells[3, 4].Text = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? "2" : "1";
                                fpSpread1.ActiveSheet.Cells[3, 5].Text = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? "O.K" : "N.G";
                                fpSpread1.ActiveSheet.Cells[3, 5].ForeColor = (mData.Relax.Result1 == RESULT.PASS && mData.Relax.Result2 == RESULT.PASS) ? Color.Lime : Color.Red;
                                mData.Relax.Test = true;
                            }
                            else
                            {
                                Flag = false;
                            }
                        }

                        if (CheckItem.Height == true)
                        {
                            if ((Rl.Height == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) || (Rr.Height == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet))
                            {
                                mData.Height.Result1 = RESULT.PASS;
                                mData.Height.Result2 = RESULT.PASS;
                                mData.Result = RESULT.PASS;
                                mData.Height.Data1 = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? 2 : 1;
                                fpSpread1.ActiveSheet.Cells[4, 4].Text = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? "2" : "1";
                                fpSpread1.ActiveSheet.Cells[4, 5].Text = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                                fpSpread1.ActiveSheet.Cells[4, 5].ForeColor = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                                mData.Height.Test = true;
                            }
                            else
                            {
                                Flag = false;
                            }
                        }

                        if (CheckItem.Legrest == true)
                        {
                            if ((Rl.Legrest == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) || (Rr.Legrest == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet))
                            {
                                mData.Legrest.Result1 = RESULT.PASS;
                                mData.Legrest.Result2 = RESULT.PASS;
                                mData.Result = RESULT.PASS;
                                mData.Legrest.Data1 = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? 2 : 1;
                                fpSpread1.ActiveSheet.Cells[5, 4].Text = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? "2" : "1";
                                fpSpread1.ActiveSheet.Cells[5, 5].Text = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                                fpSpread1.ActiveSheet.Cells[5, 5].ForeColor = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                                mData.Legrest.Test = true;
                            }
                            else
                            {
                                Flag = false;
                            }
                        }

                        if (CheckItem.LegrestExt == true)
                        {
                            if ((Rl.LegrestExt == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) || (Rr.LegrestExt == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet))
                            {
                                mData.LegrestExt.Result1 = RESULT.PASS;
                                mData.LegrestExt.Result2 = RESULT.PASS;
                                mData.Result = RESULT.PASS;
                                mData.LegrestExt.Data1 = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? 2 : 1;
                                fpSpread1.ActiveSheet.Cells[6, 4].Text = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? "2" : "1";
                                fpSpread1.ActiveSheet.Cells[6, 5].Text = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                                fpSpread1.ActiveSheet.Cells[6, 5].ForeColor = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                                mData.LegrestExt.Test = true;
                            }
                            else
                            {
                                Flag = false;
                            }
                        }

                        if (Flag == true)
                        {
                            Step++;
                            SpecOutputFlag = false;
                            ComF.timedelay(500);
                        }
                    }                        
                }
            }
            return;
        }

        private void ReclineCheck()
        {
            if (SpecOutputFlag == false)
            {
                //if (Pos == 0)
                //    label5.Text = "RECLINE BWD 가상리미트 중 입니다.";
                //else label5.Text = "RECLINE FWD 가상리미트 중 입니다.";
                label5.Text = "RECLINE 가상 리미트 중 입니다.";
                SpecOutputFlag = true;

                //Recline 
                //if (Pos == 0)
                //    ReclineBwdOnOff = true;
                //else ReclineFwdOnOff = true;
                VirtualLimiteToRecline();
                StepTimeFirst = ComF.timeGetTimems();
                StepTimeLast = ComF.timeGetTimems();
                CurrFilter.InitAll();
                VirtualLimitStartFlag = false;
            }
            else
            {
                float AdData;

                StepTimeLast = ComF.timeGetTimems();
                AdData = pMeter.GetPSeat;

                plot1.Channels[0].AddXY(plot1.Channels[0].Count, AdData);
                plot1.YAxes[0].Tracking.ZoomToFitAll();
                plot1.XAxes[0].Tracking.ZoomToFitAll();

                if ((mSpec.ReclinerLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))//time over
                {
                    if (CheckItem.Recline == true)
                    {
                        mData.Recline.Result1 = RESULT.REJECT;
                        mData.Recline.Result2 = RESULT.REJECT;

                        if ((mData.Recline.Result1 == RESULT.REJECT) || (mData.Recline.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;

                        mData.Recline.Data1 = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? 2 : 1;
                        fpSpread1.ActiveSheet.Cells[2, 3].Text = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? "02" : "01";

                        fpSpread1.ActiveSheet.Cells[2, 4].Text = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                        fpSpread1.ActiveSheet.Cells[2, 4].ForeColor = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;

                        mData.Recline.Test = true;
                    }
                    Step++;
                    SpecOutputFlag = false;
                    ComF.timedelay(500);
                    return;
                }
                if (1000 <= (StepTimeLast - StepTimeFirst))
                {
                    if (CanReWrite.GetCanFail == true)//통신 불량
                    {
                        if (CheckItem.Recline == true)
                        {
                            mData.Recline.Result1 = RESULT.REJECT;
                            mData.Recline.Result2 = RESULT.REJECT;

                            if ((mData.Recline.Result1 == RESULT.REJECT) || (mData.Recline.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;

                            mData.Recline.Data1 = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? 2 : 1;
                            fpSpread1.ActiveSheet.Cells[2, 3].Text = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? "2" : "1";

                            fpSpread1.ActiveSheet.Cells[2, 4].Text = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                            fpSpread1.ActiveSheet.Cells[2, 4].ForeColor = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;

                            mData.Recline.Test = true;
                        }
                        Step = 13;
                        SpecOutputFlag = false;
                        ComF.timedelay(500);
                        return;
                    }

                    CanMap.VirtualLimitState Rl = CanReWrite.RLVirtualLimitDataRead();
                    CanMap.VirtualLimitState Rr = CanReWrite.RRVirtualLimitDataRead();

                    if (VirtualLimitStartFlag == false)
                    {
                        if (0.5 <= AdData) VirtualLimitStartFlag = true;

                        if (5000 <= (StepTimeLast - StepTimeFirst)) //가상 리미트 동작 안함
                        {
                            if (CheckItem.Recline == true)
                            {
                                mData.Recline.Result1 = RESULT.REJECT;
                                mData.Recline.Result2 = RESULT.REJECT;

                                if ((mData.Recline.Result1 == RESULT.REJECT) || (mData.Recline.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;

                                mData.Recline.Data1 = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? 2 : 1;
                                fpSpread1.ActiveSheet.Cells[2, 3].Text = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? "2" : "1";

                                fpSpread1.ActiveSheet.Cells[2, 4].Text = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                                fpSpread1.ActiveSheet.Cells[2, 4].ForeColor = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;

                                mData.Recline.Test = true;
                            }
                            Step = 13;
                            SpecOutputFlag = false;
                            ComF.timedelay(500);
                        }
                    }
                    else
                    {
                        if (5000 <= (StepTimeLast - StepTimeFirst))
                        {
                            if ((Rl.Recline == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet) || (Rr.Recline == CanMap.VirtualLimitValue.NoSetOperationAndLimitSet))
                            {
                                if (CheckItem.Recline == true)
                                {
                                    mData.Recline.Result1 = RESULT.PASS;
                                    mData.Recline.Result2 = RESULT.PASS;
                                    mData.Result = RESULT.PASS;
                                    mData.Recline.Data1 = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? 2 : 1;
                                    fpSpread1.ActiveSheet.Cells[2, 3].Text = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? "2" : "1";
                                    fpSpread1.ActiveSheet.Cells[2, 4].Text = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                                    fpSpread1.ActiveSheet.Cells[2, 4].ForeColor = ((mData.Recline.Result1 == RESULT.PASS) && (mData.Recline.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                                    mData.Recline.Test = true;
                                }
                                Step++;
                                SpecOutputFlag = false;
                                ComF.timedelay(500);
                            }
                        }
                    }
                }
            }
            return;
        }

        private void LegrestCheck(short Pos)
        {
            if (SpecOutputFlag == false)
            {
                if (Pos == 0)
                    label5.Text = "LEGREST REST 가상 리미트 설정 중 입니다.";
                else label5.Text = "LEGREST RETURN 가상 리미트 중 입니다.";
                StepTimeFirst = ComF.timeGetTimems();
                StepTimeLast = ComF.timeGetTimems();
                SpecOutputFlag = true;

                if (Pos == 0)
                    LegrestOnOff = true;
                else LegrestReturnOnOff = true;

                VirtualLimitStartFlag = false;
                CurrFilter.InitAll();
            }
            else
            {
                float AdData;

                StepTimeLast = ComF.timeGetTimems();

                AdData = pMeter.GetPSeat;

                if ((mSpec.LegrestLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))
                {
                    if (Pos == 0)
                    {
                        //mData.Legrest.Data1 = CheckData;

                        //if ((mSpec.Current.Legrest.Min <= CheckData) && (CheckData <= mSpec.Current.Legrest.Max))
                        mData.Legrest.Result1 = RESULT.REJECT;
                        //else mData.Legrest.Result1 = RESULT.REJECT;

                        if (mData.Legrest.Result1 == RESULT.REJECT) mData.Result = RESULT.REJECT;
                        //fpSpread1.ActiveSheet.Cells[5, 4].Text = mData.Legrest.Result1 == RESULT.PASS ? "O.K" : "N.G";
                        //fpSpread1.ActiveSheet.Cells[5, 4].ForeColor = mData.Legrest.Result1 == RESULT.PASS ? Color.Black : Color.Red;
                    }
                    else
                    {
                        //mData.Legrest.Data2 = CheckData;

                        //if ((mSpec.Current.Legrest.Min <= CheckData) && (CheckData <= mSpec.Current.Legrest.Max))
                        mData.Legrest.Result2 = RESULT.REJECT;
                        //else mData.Legrest.Result2 = RESULT.REJECT;

                        if (mData.Legrest.Result2 == RESULT.REJECT) mData.Result = RESULT.REJECT;

                        fpSpread1.ActiveSheet.Cells[5, 3].Text = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? "2" : "1";
                        fpSpread1.ActiveSheet.Cells[5, 4].Text = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                        fpSpread1.ActiveSheet.Cells[5, 4].ForeColor = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                    }
                }


                plot1.Channels[0].AddXY(plot1.Channels[0].Count, AdData);
                plot1.YAxes[0].Tracking.ZoomToFitAll();
                plot1.XAxes[0].Tracking.ZoomToFitAll();

                if (AdData < 0.5)
                {
                    if (VirtualLimitStartFlag == true)
                    {
                        //Slide Fwd Sw Off
                        //fpSpread1.ActiveSheet.Cells[5, 6].Text = AdData.ToString("0.0");

                        //float CheckData;

                        //if (float.TryParse(fpSpread1.ActiveSheet.Cells[5, 6].Text, out CheckData) == false) CheckData = 0;

                        if (Pos == 0)
                            LegrestOnOff = false;
                        else LegrestReturnOnOff = false;

                        mData.Legrest.Test = true;

                        if (Pos == 0)
                        {
                            //mData.Legrest.Data1 = CheckData;

                            //if ((mSpec.Current.Legrest.Min <= CheckData) && (CheckData <= mSpec.Current.Legrest.Max))
                            mData.Legrest.Result1 = RESULT.PASS;
                            //else mData.Legrest.Result1 = RESULT.REJECT;

                            if (mData.Legrest.Result1 == RESULT.REJECT) mData.Result = RESULT.REJECT;
                            //fpSpread1.ActiveSheet.Cells[5, 4].Text = mData.Legrest.Result1 == RESULT.PASS ? "O.K" : "N.G";
                            //fpSpread1.ActiveSheet.Cells[5, 4].ForeColor = mData.Legrest.Result1 == RESULT.PASS ? Color.Black : Color.Red;
                        }
                        else
                        {
                            //mData.Legrest.Data2 = CheckData;

                            //if ((mSpec.Current.Legrest.Min <= CheckData) && (CheckData <= mSpec.Current.Legrest.Max))
                            mData.Legrest.Result2 = RESULT.PASS;
                            //else mData.Legrest.Result2 = RESULT.REJECT;

                            if (mData.Legrest.Result2 == RESULT.REJECT) mData.Result = RESULT.REJECT;

                            fpSpread1.ActiveSheet.Cells[5, 3].Text = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? "2" : "1";
                            fpSpread1.ActiveSheet.Cells[5, 4].Text = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                            fpSpread1.ActiveSheet.Cells[5, 4].ForeColor = ((mData.Legrest.Result1 == RESULT.PASS) && (mData.Legrest.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                        }


                        Step++;
                        SpecOutputFlag = false;
                        ComF.timedelay(500);
                    }
                }
                else
                {
                    //fpSpread1.ActiveSheet.Cells[5, 6].Text = AdData.ToString("0.0");
                    VirtualLimitStartFlag = true;
                }
            }
            return;
        }

        private void HeightCheck(short Pos)
        {
            if (SpecOutputFlag == false)
            {
                if (Pos == 0)
                    label5.Text = "HEIGHT UP 가상 리미트 설정 중 입니다.";
                else label5.Text = "HEIGHT DN 가상 리미트 설정 중 입니다.";
                StepTimeFirst = ComF.timeGetTimems();
                StepTimeLast = ComF.timeGetTimems();
                SpecOutputFlag = true;

                if (Pos == 0)
                    HeightDnOnOff = true;
                else HeightUpOnOff = true;

                CurrFilter.InitAll();
                VirtualLimitStartFlag = false;
            }
            else
            {
                float AdData;

                StepTimeLast = ComF.timeGetTimems();

                AdData = pMeter.GetPSeat;


                plot1.Channels[0].AddXY(plot1.Channels[0].Count, AdData);
                plot1.YAxes[0].Tracking.ZoomToFitAll();
                plot1.XAxes[0].Tracking.ZoomToFitAll();


                if ((mSpec.HeightLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))
                {
                    if (Pos == 0)
                    {
                        //mData.Legrest.Data1 = CheckData;

                        //if ((mSpec.Current.Legrest.Min <= CheckData) && (CheckData <= mSpec.Current.Legrest.Max))
                        mData.Height.Result1 = RESULT.REJECT;
                        //else mData.Legrest.Result1 = RESULT.REJECT;

                        if (mData.Height.Result1 == RESULT.REJECT) mData.Result = RESULT.REJECT;
                        //fpSpread1.ActiveSheet.Cells[5, 4].Text = mData.Legrest.Result1 == RESULT.PASS ? "O.K" : "N.G";
                        //fpSpread1.ActiveSheet.Cells[5, 4].ForeColor = mData.Legrest.Result1 == RESULT.PASS ? Color.Black : Color.Red;
                    }
                    else
                    {
                        //mData.Legrest.Data2 = CheckData;

                        //if ((mSpec.Current.Legrest.Min <= CheckData) && (CheckData <= mSpec.Current.Legrest.Max))
                        mData.Height.Result2 = RESULT.REJECT;
                        //else mData.Legrest.Result2 = RESULT.REJECT;

                        if (mData.Height.Result2 == RESULT.REJECT) mData.Result = RESULT.REJECT;

                        fpSpread1.ActiveSheet.Cells[4, 3].Text = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? "2" : "1";
                        fpSpread1.ActiveSheet.Cells[4, 4].Text = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                        fpSpread1.ActiveSheet.Cells[4, 4].ForeColor = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                    }
                }
                if (AdData < 0.5)
                {
                    //Height
                    if (VirtualLimitStartFlag == true)
                    {
                        //fpSpread1.ActiveSheet.Cells[4, 6].Text = AdData.ToString("0.0");

                        //oat.TryParse(fpSpread1.ActiveSheet.Cells[4, 6].Text, out CheckData) == false) CheckData = 0;

                        if (Pos == 0)
                            HeightDnOnOff = false;
                        else HeightUpOnOff = false;

                        mData.Height.Test = true;

                        if (Pos == 0)
                        {
                            //mData.Height.Data1 = CheckData;

                            //if ((mSpec.Current.Height.Min <= CheckData) && (CheckData <= mSpec.Current.Height.Max))
                            mData.Height.Result1 = RESULT.PASS;
                            //else mData.Height.Result1 = RESULT.REJECT;

                            if (mData.Height.Result1 == RESULT.REJECT) mData.Result = RESULT.REJECT;

                            //fpSpread1.ActiveSheet.Cells[4, 7].Text = mData.Height.Result1 == RESULT.PASS ? "O.K" : "N.G";
                            //fpSpread1.ActiveSheet.Cells[4, 7].ForeColor = mData.Height.Result1 == RESULT.PASS ? Color.Black : Color.Red;
                        }
                        else
                        {
                            //mData.Height.Data2 = CheckData;
                            //if ((mSpec.Current.Height.Min <= CheckData) && (CheckData <= mSpec.Current.Height.Max))
                            mData.Height.Result2 = RESULT.PASS;
                            //else mData.Height.Result2 = RESULT.REJECT;

                            if (mData.Height.Result2 == RESULT.REJECT) mData.Result = RESULT.REJECT;

                            fpSpread1.ActiveSheet.Cells[4, 3].Text = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? "2" : "1";
                            fpSpread1.ActiveSheet.Cells[4, 4].Text = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                            fpSpread1.ActiveSheet.Cells[4, 4].ForeColor = ((mData.Height.Result1 == RESULT.PASS) && (mData.Height.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                        }
                        Step++;
                        SpecOutputFlag = false;
                        ComF.timedelay(500);
                    }
                }
                else
                {
                    //fpSpread1.ActiveSheet.Cells[4, 6].Text = AdData.ToString("0.0");
                    VirtualLimitStartFlag = true;
                }
            }
            return;
        }

        private void LegrestExtCheck(short Pos)
        {
            if (SpecOutputFlag == false)
            {
                if (Pos == 0)
                    label5.Text = "LEGREST EXT REST 가상 리미트 설정 중 입니다.";
                else label5.Text = "LEGREST EXT RETURN 가상 리미트 중 입니다.";
                StepTimeFirst = ComF.timeGetTimems();
                StepTimeLast = ComF.timeGetTimems();
                SpecOutputFlag = true;

                if (Pos == 0)
                    LegrestExtOnOff = true;
                else LegrestExtReturnOnOff = true;

                VirtualLimitStartFlag = false;
                CurrFilter.InitAll();
            }
            else
            {
                float AdData;

                StepTimeLast = ComF.timeGetTimems();

                AdData = pMeter.GetPSeat;

                if ((mSpec.LegrestExtLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))
                {
                    if (Pos == 0)
                    {
                        //mData.Legrest.Data1 = CheckData;

                        //if ((mSpec.Current.Legrest.Min <= CheckData) && (CheckData <= mSpec.Current.Legrest.Max))
                        mData.LegrestExt.Result1 = RESULT.REJECT;
                        //else mData.Legrest.Result1 = RESULT.REJECT;

                        if (mData.LegrestExt.Result1 == RESULT.REJECT) mData.Result = RESULT.REJECT;
                        //fpSpread1.ActiveSheet.Cells[5, 4].Text = mData.Legrest.Result1 == RESULT.PASS ? "O.K" : "N.G";
                        //fpSpread1.ActiveSheet.Cells[5, 4].ForeColor = mData.Legrest.Result1 == RESULT.PASS ? Color.Black : Color.Red;
                    }
                    else
                    {
                        //mData.Legrest.Data2 = CheckData;

                        //if ((mSpec.Current.Legrest.Min <= CheckData) && (CheckData <= mSpec.Current.Legrest.Max))
                        mData.LegrestExt.Result2 = RESULT.REJECT;
                        //else mData.Legrest.Result2 = RESULT.REJECT;

                        if (mData.LegrestExt.Result2 == RESULT.REJECT) mData.Result = RESULT.REJECT;

                        fpSpread1.ActiveSheet.Cells[6, 3].Text = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? "2" : "1";
                        fpSpread1.ActiveSheet.Cells[6, 4].Text = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                        fpSpread1.ActiveSheet.Cells[6, 4].ForeColor = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                    }
                }


                plot1.Channels[0].AddXY(plot1.Channels[0].Count, AdData);
                plot1.YAxes[0].Tracking.ZoomToFitAll();
                plot1.XAxes[0].Tracking.ZoomToFitAll();

                if (AdData < 0.5)
                {
                    if (VirtualLimitStartFlag == true)
                    {
                        //Slide Fwd Sw Off
                        //fpSpread1.ActiveSheet.Cells[5, 6].Text = AdData.ToString("0.0");

                        //float CheckData;

                        //if (float.TryParse(fpSpread1.ActiveSheet.Cells[5, 6].Text, out CheckData) == false) CheckData = 0;

                        if (Pos == 0)
                            LegrestOnOff = false;
                        else LegrestReturnOnOff = false;

                        mData.LegrestExt.Test = true;

                        if (Pos == 0)
                        {
                            //mData.LegrestExt.Data1 = CheckData;

                            //if ((mSpec.Current.LegrestExt.Min <= CheckData) && (CheckData <= mSpec.Current.LegrestExt.Max))
                            mData.LegrestExt.Result1 = RESULT.PASS;
                            //else mData.LegrestExt.Result1 = RESULT.REJECT;

                            if (mData.LegrestExt.Result1 == RESULT.REJECT) mData.Result = RESULT.REJECT;
                            //fpSpread1.ActiveSheet.Cells[5, 4].Text = mData.LegrestExt.Result1 == RESULT.PASS ? "O.K" : "N.G";
                            //fpSpread1.ActiveSheet.Cells[5, 4].ForeColor = mData.LegrestExt.Result1 == RESULT.PASS ? Color.Black : Color.Red;
                        }
                        else
                        {
                            //mData.LegrestExt.Data2 = CheckData;

                            //if ((mSpec.Current.LegrestExt.Min <= CheckData) && (CheckData <= mSpec.Current.LegrestExt.Max))
                            mData.LegrestExt.Result2 = RESULT.PASS;
                            //else mData.LegrestExt.Result2 = RESULT.REJECT;

                            if (mData.LegrestExt.Result2 == RESULT.REJECT) mData.Result = RESULT.REJECT;

                            fpSpread1.ActiveSheet.Cells[6, 3].Text = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? "2" : "1";
                            fpSpread1.ActiveSheet.Cells[6, 4].Text = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? "O.K" : "N.G";
                            fpSpread1.ActiveSheet.Cells[6, 4].ForeColor = ((mData.LegrestExt.Result1 == RESULT.PASS) && (mData.LegrestExt.Result2 == RESULT.PASS)) ? Color.Lime : Color.Red;
                        }


                        Step++;
                        SpecOutputFlag = false;
                        ComF.timedelay(500);
                    }
                }
                else
                {
                    //fpSpread1.ActiveSheet.Cells[5, 6].Text = AdData.ToString("0.0");
                    VirtualLimitStartFlag = true;
                }
            }
            return;
        }




        //private bool SlideMotorMoveEndFlag { get; set; }
        //private bool ReclineMotorMoveEndFlag { get; set; }
        //private bool TiltMotorMoveEndFlag { get; set; }
        //private bool HeightMotorMoveEndFlag { get; set; }
        //void SoundCheckInitPositionMove()
        //{
        //    if (SpecOutputFlag == false)
        //    {
        //        SlideFwdOnOff = true;
        //        ReclineFwdOnOff = true;
        //        TiltUpOnOff = true;
        //        HeightUpOnOff = true;
        //        StepTimeFirst = ComF.timeGetTimems();
        //        StepTimeLast = ComF.timeGetTimems();
        //        SpecOutputFlag = true;
        //        SlideMotorMoveEndFlag = false;
        //        ReclineMotorMoveEndFlag = false;
        //        TiltMotorMoveEndFlag = false;
        //        HeightMotorMoveEndFlag = false;
        //        label5.Text = "속도 측정을 하기 위해 측정 위치로 이동 중 입니다.";
        //    }
        //    else
        //    {
        //        if(30 < pMeter.GetPSeat)
        //        {
        //            SlideFwdOnOff = false;
        //            ReclineFwdOnOff = false;
        //            TiltUpOnOff = false;
        //            HeightUpOnOff = false;
        //            SpecOutputFlag = false;
        //            SlideMotorMoveEndFlag = true;
        //            ReclineMotorMoveEndFlag = true;
        //            TiltMotorMoveEndFlag = true;
        //            HeightMotorMoveEndFlag = true;
        //        }

        //        StepTimeLast = ComF.timeGetTimems();

        //        if (2000 <= (StepTimeLast - StepTimeFirst))
        //        {
        //            if (SlideMotorMoveEndFlag == false)
        //            {
        //                float AdData;

        //                bool Flag = false;
        //                if (comboBox4.SelectedItem.ToString() == "IMS")
        //                    AdData = pMeter.GetPSeat;
        //                else AdData = IOPort.ADRead[mSpec.PinMap.SlideFWD.PinNo];
        //                if (comboBox4.SelectedItem.ToString() == "IMS")
        //                {
        //                    if (AdData < mSpec.SlideLimitCurr) Flag = true;
        //                }
        //                else
        //                {
        //                    if (mSpec.SlideLimitCurr < AdData) Flag = true;
        //                }
        //                if (Flag == true)
        //                {
        //                    SlideFwdOnOff = false;
        //                    SlideMotorMoveEndFlag = true;
        //                }
        //                else if ((mSpec.SlideLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))
        //                {
        //                    SlideFwdOnOff = false;
        //                    SlideMotorMoveEndFlag = true;
        //                }
        //            }
        //            if (ReclineMotorMoveEndFlag == false)
        //            {
        //                float AdData;

        //                bool Flag = false;
        //                if (comboBox4.SelectedItem.ToString() == "IMS")
        //                    AdData = pMeter.GetPSeat;
        //                else AdData = IOPort.ADRead[mSpec.PinMap.SlideFWD.PinNo];
        //                if (comboBox4.SelectedItem.ToString() == "IMS")
        //                {
        //                    if (AdData < mSpec.ReclinerLimitCurr) Flag = true;
        //                }
        //                else
        //                {
        //                    if (mSpec.ReclinerLimitCurr < AdData) Flag = true;
        //                }
        //                if (Flag == true)
        //                {
        //                    ReclineFwdOnOff = false;
        //                    ReclineMotorMoveEndFlag = true;
        //                }
        //                else if ((mSpec.ReclinerLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))
        //                {
        //                    ReclineFwdOnOff = false;
        //                    ReclineMotorMoveEndFlag = true;
        //                }
        //            }
        //            if (TiltMotorMoveEndFlag == false)
        //            {
        //                float AdData;
        //                bool Flag = false;

        //                if (comboBox4.SelectedItem.ToString() == "IMS")
        //                    AdData = pMeter.GetPSeat;
        //                else AdData = IOPort.ADRead[mSpec.PinMap.TiltUp.PinNo];

        //                if (comboBox4.SelectedItem.ToString() == "IMS")
        //                {
        //                    if (AdData < mSpec.TiltLimitCurr) Flag = true;
        //                }
        //                else
        //                {
        //                    if (mSpec.TiltLimitCurr < AdData) Flag = true;
        //                }
        //                if (Flag == true)
        //                {
        //                    TiltUpOnOff = false;
        //                    TiltMotorMoveEndFlag = true;
        //                }
        //                else if ((mSpec.TiltLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))
        //                {
        //                    TiltUpOnOff = false;
        //                    TiltMotorMoveEndFlag = true;
        //                }
        //            }
        //            if (HeightMotorMoveEndFlag == false)
        //            {
        //                float AdData;
        //                bool Flag = false;

        //                if (comboBox4.SelectedItem.ToString() == "IMS")
        //                    AdData = pMeter.GetPSeat;
        //                else AdData = IOPort.ADRead[mSpec.PinMap.HeightUp.PinNo];
        //                if (comboBox4.SelectedItem.ToString() == "IMS")
        //                {
        //                    if (AdData < mSpec.HeightLimitCurr) Flag = true;
        //                }
        //                else
        //                {
        //                    if (mSpec.HeightLimitCurr < AdData) Flag = true;
        //                }
        //                if (Flag == true)
        //                {
        //                    HeightUpOnOff = false;
        //                    HeightMotorMoveEndFlag = true;
        //                }
        //                else if ((mSpec.HeightLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))
        //                {
        //                    HeightUpOnOff = false;
        //                    HeightMotorMoveEndFlag = true;
        //                }
        //            }
        //        }

        //        if ((SlideMotorMoveEndFlag == true) && (TiltMotorMoveEndFlag == true) && (HeightMotorMoveEndFlag == true))
        //        {
        //            SpecOutputFlag = false;
        //            Step++;
        //            ComF.timedelay(500);
        //        }
        //    }
        //    return;
        //}
        //private bool[] SpeedCheckEnd = { false, false, false, false, false, false, false, false };
        //private void CheckSpeedToIMS(short SpeedCheckStep)
        //{
        //    if (SpecOutputFlag == false)
        //    {
        //        StepTimeFirst = ComF.timeGetTimems();
        //        StepTimeLast = ComF.timeGetTimems();
        //        SpecOutputFlag = true;

        //        //Slide Fwd Sw On

        //        switch (SpeedCheckStep)
        //        {
        //            case 0:
        //                //FirstPosCheckEnd[SpeedCheckStep] = false;
        //                plot2.Channels[0].Clear();
        //                SoundCheckTimeToStart[SpeedCheckStep] = plot2.Channels[0].Count;
        //                SlideBwdOnOff = true;
        //                label5.Text = "SLIDE BACKWORD 속도 측정 중 입니다.";
        //                break;
        //            case 1:
        //                //FirstPosCheckEnd[SpeedCheckStep] = false;
        //                plot2.Channels[0].Clear();
        //                SoundCheckTimeToStart[SpeedCheckStep] = plot2.Channels[0].Count;
        //                SlideFwdOnOff = true;
        //                label5.Text = "SLIDE FORWORD 속도 측정 중 입니다.";
        //                break;
        //            case 2:
        //                //FirstPosCheckEnd[SpeedCheckStep] = false;
        //                plot2.Channels[0].Clear();
        //                SoundCheckTimeToStart[SpeedCheckStep] = plot2.Channels[0].Count;
        //                ReclineBwdOnOff = true;
        //                label5.Text = "RECLINE BACKWORD 속도 측정 중 입니다.";
        //                break;
        //            case 3:
        //                //FirstPosCheckEnd[SpeedCheckStep] = false;
        //                plot2.Channels[0].Clear();
        //                SoundCheckTimeToStart[SpeedCheckStep] = plot2.Channels[0].Count;
        //                ReclineFwdOnOff = true;
        //                label5.Text = "RECLINE FORWORD 속도 측정 중 입니다.";
        //                break;
        //            case 4:
        //                //FirstPosCheckEnd[SpeedCheckStep] = false;
        //                plot2.Channels[0].Clear();
        //                SoundCheckTimeToStart[SpeedCheckStep] = plot2.Channels[0].Count;
        //                TiltDnOnOff = true;
        //                label5.Text = "TULT UP 속도 측정 중 입니다.";
        //                break;
        //            case 5:
        //                //FirstPosCheckEnd[SpeedCheckStep] = false;
        //                plot2.Channels[0].Clear();
        //                SoundCheckTimeToStart[SpeedCheckStep] = plot2.Channels[0].Count;
        //                TiltUpOnOff = true;
        //                label5.Text = "TULT DOWN 속도 측정 중 입니다.";
        //                break;
        //            case 6:
        //                //FirstPosCheckEnd[SpeedCheckStep] = false;
        //                plot2.Channels[0].Clear();
        //                SoundCheckTimeToStart[SpeedCheckStep] = plot2.Channels[0].Count;
        //                HeightDnOnOff = true;
        //                label5.Text = "HEIGHT UP 속도 측정 중 입니다.";
        //                break;
        //            case 7:
        //                //FirstPosCheckEnd[SpeedCheckStep] = false;
        //                plot2.Channels[0].Clear();
        //                SoundCheckTimeToStart[SpeedCheckStep] = plot2.Channels[0].Count;
        //                HeightUpOnOff = true;
        //                label5.Text = "HEIGHT DOWN 속도 측정 중 입니다.";
        //                break;
        //        }
        //        SpeedCheckStartTime[SpeedCheckStep] = ComF.timeGetTimems();
        //    }
        //    else
        //    {
        //        StepTimeLast = ComF.timeGetTimems();

        //        float AdData = pMeter.GetPSeat;

        //        if (500 <= (StepTimeLast - StepTimeFirst))
        //        {
        //            plot2.Channels[0].AddXY(plot2.Channels[0].Count, Math.Abs(Sound.GetSound - NormalSound));
        //            plot2.XAxes[0].Tracking.ZoomToFitAll();
        //            plot2.YAxes[0].Tracking.ZoomToFitAll();
        //            if (SpeedCheckStep == 0)
        //            {
        //                if (/*(FirstPosCheckEnd[SpeedCheckStep] == false) && (*/AdData < mSpec.SlideLimitCurr)//)
        //                {
        //                    SpeedCheckEnd[SpeedCheckStep] = true;
        //                    SlideBwdOnOff = false;
        //                    SoundCheckTimeToEnd[SpeedCheckStep] = plot2.Channels[0].Count;

        //                    float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[SpeedCheckStep]) / 1000F;
        //                    mData.SlideBwdTime.Data = (mSpec.MovingStroke.Slide / EndTime) * 1F;
        //                    mData.SlideBwdTime.Test = true;
        //                    if ((mSpec.MovingSpeed.SlideBwd.Min <= mData.SlideBwdTime.Data) && (mData.SlideBwdTime.Data <= mSpec.MovingSpeed.SlideBwd.Max))
        //                        mData.SlideBwdTime.Result = RESULT.PASS;
        //                    else mData.SlideBwdTime.Result = RESULT.REJECT;

        //                    fpSpread1.ActiveSheet.Cells[4, 6].Text = mData.SlideBwdTime.Data.ToString("0.00");
        //                    fpSpread1.ActiveSheet.Cells[4, 7].Text = mData.SlideBwdTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                    fpSpread1.ActiveSheet.Cells[4, 7].ForeColor = mData.SlideBwdTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                }
        //                else
        //                {
        //                    //if (FirstPosCheckEnd[SpeedCheckStep] == false)
        //                    //{
        //                    if ((mSpec.SlideLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))
        //                    {
        //                        SlideBwdOnOff = false;
        //                        float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[SpeedCheckStep]) / 1000F;
        //                        mData.SlideBwdTime.Data = (mSpec.MovingStroke.Slide / EndTime) * 1F;
        //                        mData.SlideBwdTime.Test = true;
        //                        if ((mSpec.MovingSpeed.SlideBwd.Min <= mData.SlideBwdTime.Data) && (mData.SlideBwdTime.Data <= mSpec.MovingSpeed.SlideBwd.Max))
        //                            mData.SlideBwdTime.Result = RESULT.PASS;
        //                        else mData.SlideBwdTime.Result = RESULT.REJECT;

        //                        fpSpread1.ActiveSheet.Cells[4, 6].Text = mData.SlideBwdTime.Data.ToString("0.00");
        //                        fpSpread1.ActiveSheet.Cells[4, 7].Text = mData.SlideBwdTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                        fpSpread1.ActiveSheet.Cells[4, 7].ForeColor = mData.SlideBwdTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                    }
        //                    //}
        //                }
        //            }
        //            else if (SpeedCheckStep == 1)
        //            {
        //                if (/*(FirstPosCheckEnd[SpeedCheckStep] == false) && (*/AdData < mSpec.SlideLimitCurr)//)
        //                {
        //                    SpeedCheckEnd[SpeedCheckStep] = true;
        //                    SlideFwdOnOff = false;
        //                    SoundCheckTimeToEnd[SpeedCheckStep] = plot2.Channels[0].Count;

        //                    float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[SpeedCheckStep]) / 1000F;
        //                    mData.SlideFwdTime.Data = (mSpec.MovingStroke.Slide / EndTime) * 1F;
        //                    mData.SlideFwdTime.Test = true;
        //                    if ((mSpec.MovingSpeed.SlideFwd.Min <= mData.SlideFwdTime.Data) && (mData.SlideFwdTime.Data <= mSpec.MovingSpeed.SlideFwd.Max))
        //                        mData.SlideFwdTime.Result = RESULT.PASS;
        //                    else mData.SlideFwdTime.Result = RESULT.REJECT;

        //                    fpSpread1.ActiveSheet.Cells[3, 6].Text = mData.SlideFwdTime.Data.ToString("0.00");
        //                    fpSpread1.ActiveSheet.Cells[3, 7].Text = mData.SlideFwdTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                    fpSpread1.ActiveSheet.Cells[3, 7].ForeColor = mData.SlideFwdTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                }
        //                else
        //                {
        //                    //if (FirstPosCheckEnd[SpeedCheckStep] == false)
        //                    //{
        //                    if ((mSpec.SlideLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))
        //                    {
        //                        SpeedCheckEnd[SpeedCheckStep] = true;
        //                        SlideFwdOnOff = false;
        //                        SoundCheckTimeToEnd[SpeedCheckStep] = plot2.Channels[0].Count;

        //                        float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[SpeedCheckStep]) / 1000F;
        //                        mData.SlideFwdTime.Data = (mSpec.MovingStroke.Slide / EndTime) * 1F;
        //                        mData.SlideFwdTime.Test = true;
        //                        if ((mSpec.MovingSpeed.SlideFwd.Min <= mData.SlideFwdTime.Data) && (mData.SlideFwdTime.Data <= mSpec.MovingSpeed.SlideFwd.Max))
        //                            mData.SlideFwdTime.Result = RESULT.PASS;
        //                        else mData.SlideFwdTime.Result = RESULT.REJECT;

        //                        fpSpread1.ActiveSheet.Cells[3, 6].Text = mData.SlideFwdTime.Data.ToString("0.00");
        //                        fpSpread1.ActiveSheet.Cells[3, 7].Text = mData.SlideFwdTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                        fpSpread1.ActiveSheet.Cells[3, 7].ForeColor = mData.SlideFwdTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                    }
        //                    //}
        //                }
        //            }
        //            else if (SpeedCheckStep == 2)
        //            {
        //                if (/*(FirstPosCheckEnd[SpeedCheckStep] == false) && (*/AdData < mSpec.SlideLimitCurr)//)
        //                {
        //                    SpeedCheckEnd[SpeedCheckStep] = true;
        //                    ReclineBwdOnOff = false;
        //                    SoundCheckTimeToEnd[SpeedCheckStep] = plot2.Channels[0].Count;

        //                    float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[SpeedCheckStep]) / 1000F;
        //                    mData.ReclineBwdTime.Data = (mSpec.MovingStroke.Recliner / EndTime) * 1F;
        //                    mData.ReclineBwdTime.Test = true;
        //                    if ((mSpec.MovingSpeed.ReclinerBwd.Min <= mData.ReclineBwdTime.Data) && (mData.ReclineBwdTime.Data <= mSpec.MovingSpeed.ReclinerBwd.Max))
        //                        mData.ReclineBwdTime.Result = RESULT.PASS;
        //                    else mData.ReclineBwdTime.Result = RESULT.REJECT;

        //                    fpSpread1.ActiveSheet.Cells[6, 6].Text = mData.ReclineBwdTime.Data.ToString("0.00");
        //                    fpSpread1.ActiveSheet.Cells[6, 7].Text = mData.ReclineBwdTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                    fpSpread1.ActiveSheet.Cells[6, 7].ForeColor = mData.ReclineBwdTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                }
        //                else
        //                {
        //                    //if (FirstPosCheckEnd[SpeedCheckStep] == false)
        //                    //{
        //                    if ((mSpec.ReclinerLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))
        //                    {
        //                        ReclineBwdOnOff = false;
        //                        float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[SpeedCheckStep]) / 1000F;
        //                        mData.ReclineBwdTime.Data = (mSpec.MovingStroke.Recliner / EndTime) * 1F;
        //                        mData.ReclineBwdTime.Test = true;
        //                        if ((mSpec.MovingSpeed.ReclinerBwd.Min <= mData.ReclineBwdTime.Data) && (mData.ReclineBwdTime.Data <= mSpec.MovingSpeed.ReclinerBwd.Max))
        //                            mData.ReclineBwdTime.Result = RESULT.PASS;
        //                        else mData.ReclineBwdTime.Result = RESULT.REJECT;

        //                        fpSpread1.ActiveSheet.Cells[6, 6].Text = mData.ReclineBwdTime.Data.ToString("0.00");
        //                        fpSpread1.ActiveSheet.Cells[6, 7].Text = mData.ReclineBwdTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                        fpSpread1.ActiveSheet.Cells[6, 7].ForeColor = mData.ReclineBwdTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                    }
        //                    //}
        //                }
        //            }
        //            else if (SpeedCheckStep == 3)
        //            {
        //                if (/*(FirstPosCheckEnd[SpeedCheckStep] == false) && (*/AdData < mSpec.ReclinerLimitCurr)//)
        //                {
        //                    SpeedCheckEnd[SpeedCheckStep] = true;
        //                    ReclineFwdOnOff = false;
        //                    SoundCheckTimeToEnd[SpeedCheckStep] = plot2.Channels[0].Count;

        //                    float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[SpeedCheckStep]) / 1000F;
        //                    mData.ReclineFwdTime.Data = (mSpec.MovingStroke.Recliner / EndTime) * 1F;
        //                    mData.ReclineFwdTime.Test = true;
        //                    if ((mSpec.MovingSpeed.ReclinerFwd.Min <= mData.ReclineFwdTime.Data) && (mData.ReclineFwdTime.Data <= mSpec.MovingSpeed.ReclinerFwd.Max))
        //                        mData.ReclineFwdTime.Result = RESULT.PASS;
        //                    else mData.ReclineFwdTime.Result = RESULT.REJECT;

        //                    fpSpread1.ActiveSheet.Cells[5, 6].Text = mData.ReclineFwdTime.Data.ToString("0.00");
        //                    fpSpread1.ActiveSheet.Cells[5, 7].Text = mData.ReclineFwdTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                    fpSpread1.ActiveSheet.Cells[5, 7].ForeColor = mData.ReclineFwdTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                }
        //                else
        //                {
        //                    //if (FirstPosCheckEnd[SpeedCheckStep] == false)
        //                    //{
        //                    if ((mSpec.ReclinerLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))
        //                    {
        //                        SpeedCheckEnd[SpeedCheckStep] = true;
        //                        ReclineFwdOnOff = false;
        //                        SoundCheckTimeToEnd[SpeedCheckStep] = plot2.Channels[0].Count;

        //                        float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[SpeedCheckStep]) / 1000F;
        //                        mData.ReclineFwdTime.Data = (mSpec.MovingStroke.Recliner / EndTime) * 1F;
        //                        mData.ReclineFwdTime.Test = true;
        //                        if ((mSpec.MovingSpeed.ReclinerFwd.Min <= mData.ReclineFwdTime.Data) && (mData.ReclineFwdTime.Data <= mSpec.MovingSpeed.ReclinerFwd.Max))
        //                            mData.ReclineFwdTime.Result = RESULT.PASS;
        //                        else mData.ReclineFwdTime.Result = RESULT.REJECT;

        //                        fpSpread1.ActiveSheet.Cells[5, 6].Text = mData.ReclineFwdTime.Data.ToString("0.00");
        //                        fpSpread1.ActiveSheet.Cells[5, 7].Text = mData.ReclineFwdTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                        fpSpread1.ActiveSheet.Cells[5, 7].ForeColor = mData.ReclineFwdTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                    }
        //                    //}
        //                }
        //            }
        //            else if (SpeedCheckStep == 4)
        //            {
        //                if (/*(FirstPosCheckEnd[SpeedCheckStep] == false) && (*/AdData < mSpec.TiltLimitCurr)//)
        //                {
        //                    SpeedCheckEnd[SpeedCheckStep] = true;
        //                    TiltDnOnOff = false;
        //                    SoundCheckTimeToEnd[SpeedCheckStep] = plot2.Channels[0].Count;
        //                    float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[SpeedCheckStep]) / 1000F;
        //                    mData.TiltDnTime.Data = (mSpec.MovingStroke.Tilt / EndTime) * 1F;
        //                    mData.TiltDnTime.Test = true;
        //                    if ((mSpec.MovingSpeed.TiltDn.Min <= mData.TiltDnTime.Data) && (mData.TiltDnTime.Data <= mSpec.MovingSpeed.TiltDn.Max))
        //                        mData.TiltDnTime.Result = RESULT.PASS;
        //                    else mData.TiltDnTime.Result = RESULT.REJECT;

        //                    fpSpread1.ActiveSheet.Cells[8, 6].Text = mData.TiltDnTime.Data.ToString("0.00");
        //                    fpSpread1.ActiveSheet.Cells[8, 7].Text = mData.TiltDnTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                    fpSpread1.ActiveSheet.Cells[8, 7].ForeColor = mData.TiltDnTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                }
        //                else
        //                {
        //                    //if (FirstPosCheckEnd[SpeedCheckStep] == false)
        //                    //{
        //                    if ((mSpec.TiltLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))
        //                    {
        //                        SpeedCheckEnd[SpeedCheckStep] = true;
        //                        TiltDnOnOff = false;
        //                        SoundCheckTimeToEnd[SpeedCheckStep] = plot2.Channels[0].Count;
        //                        float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[SpeedCheckStep]) / 1000F;
        //                        mData.TiltDnTime.Data = (mSpec.MovingStroke.Tilt / EndTime) * 1F;
        //                        mData.TiltDnTime.Test = true;
        //                        if ((mSpec.MovingSpeed.TiltDn.Min <= mData.TiltDnTime.Data) && (mData.TiltDnTime.Data <= mSpec.MovingSpeed.TiltDn.Max))
        //                            mData.TiltDnTime.Result = RESULT.PASS;
        //                        else mData.TiltDnTime.Result = RESULT.REJECT;

        //                        fpSpread1.ActiveSheet.Cells[8, 6].Text = mData.TiltDnTime.Data.ToString("0.00");
        //                        fpSpread1.ActiveSheet.Cells[8, 7].Text = mData.TiltDnTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                        fpSpread1.ActiveSheet.Cells[8, 7].ForeColor = mData.TiltDnTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                    }
        //                    //}
        //                }
        //            }
        //            else if (SpeedCheckStep == 5)
        //            {
        //                if (/*(FirstPosCheckEnd[SpeedCheckStep] == false) && (*/AdData < mSpec.TiltLimitCurr)//)
        //                {
        //                    SpeedCheckEnd[SpeedCheckStep] = true;
        //                    TiltUpOnOff = false;
        //                    SoundCheckTimeToEnd[SpeedCheckStep] = plot2.Channels[0].Count;
        //                    float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[SpeedCheckStep]) / 1000F;
        //                    mData.TiltUpTime.Data = (mSpec.MovingStroke.Tilt / EndTime) * 1F;
        //                    mData.TiltUpTime.Test = true;
        //                    if ((mSpec.MovingSpeed.TiltUp.Min <= mData.TiltUpTime.Data) && (mData.TiltUpTime.Data <= mSpec.MovingSpeed.TiltUp.Max))
        //                        mData.TiltUpTime.Result = RESULT.PASS;
        //                    else mData.TiltUpTime.Result = RESULT.REJECT;

        //                    fpSpread1.ActiveSheet.Cells[7, 6].Text = mData.TiltUpTime.Data.ToString("0.00");
        //                    fpSpread1.ActiveSheet.Cells[7, 7].Text = mData.TiltUpTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                    fpSpread1.ActiveSheet.Cells[7, 7].ForeColor = mData.TiltUpTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                }
        //                else
        //                {
        //                    //if (FirstPosCheckEnd[SpeedCheckStep] == false)
        //                    //{
        //                    if ((mSpec.TiltLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))
        //                    {
        //                        SpeedCheckEnd[SpeedCheckStep] = true;
        //                        TiltUpOnOff = false;
        //                        SoundCheckTimeToEnd[SpeedCheckStep] = plot2.Channels[0].Count;
        //                        float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[SpeedCheckStep]) / 1000F;
        //                        mData.TiltUpTime.Data = (mSpec.MovingStroke.Tilt / EndTime) * 1F;
        //                        mData.TiltUpTime.Test = true;
        //                        if ((mSpec.MovingSpeed.TiltUp.Min <= mData.TiltUpTime.Data) && (mData.TiltUpTime.Data <= mSpec.MovingSpeed.TiltUp.Max))
        //                            mData.TiltUpTime.Result = RESULT.PASS;
        //                        else mData.TiltUpTime.Result = RESULT.REJECT;

        //                        fpSpread1.ActiveSheet.Cells[7, 6].Text = mData.TiltUpTime.Data.ToString("0.00");
        //                        fpSpread1.ActiveSheet.Cells[7, 7].Text = mData.TiltUpTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                        fpSpread1.ActiveSheet.Cells[7, 7].ForeColor = mData.TiltUpTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                    }
        //                    //}
        //                }
        //            }
        //            else if (SpeedCheckStep == 6)
        //            {
        //                if (/*(FirstPosCheckEnd[SpeedCheckStep] == false) && (*/AdData < mSpec.HeightLimitCurr)//)
        //                {
        //                    SpeedCheckEnd[SpeedCheckStep] = true;
        //                    HeightDnOnOff = false;
        //                    SoundCheckTimeToEnd[SpeedCheckStep] = plot2.Channels[0].Count;
        //                    float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[SpeedCheckStep]) / 1000F;
        //                    mData.HeightDnTime.Data = (mSpec.MovingStroke.Height / EndTime) * 1F;
        //                    mData.HeightDnTime.Test = true;
        //                    if ((mSpec.MovingSpeed.HeightDn.Min <= mData.HeightDnTime.Data) && (mData.HeightDnTime.Data <= mSpec.MovingSpeed.HeightDn.Max))
        //                        mData.HeightDnTime.Result = RESULT.PASS;
        //                    else mData.HeightDnTime.Result = RESULT.REJECT;

        //                    fpSpread1.ActiveSheet.Cells[10, 6].Text = mData.HeightDnTime.Data.ToString("0.00");
        //                    fpSpread1.ActiveSheet.Cells[10, 7].Text = mData.HeightDnTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                    fpSpread1.ActiveSheet.Cells[10, 7].ForeColor = mData.HeightDnTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                }
        //                else
        //                {
        //                    //if (FirstPosCheckEnd[SpeedCheckStep] == false)
        //                    //{
        //                    if ((mSpec.HeightLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))
        //                    {
        //                        SpeedCheckEnd[SpeedCheckStep] = true;
        //                        HeightDnOnOff = false;
        //                        SoundCheckTimeToEnd[SpeedCheckStep] = plot2.Channels[0].Count;
        //                        float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[SpeedCheckStep]) / 1000F;
        //                        mData.HeightDnTime.Data = (mSpec.MovingStroke.Height / EndTime) * 1F;
        //                        mData.HeightDnTime.Test = true;
        //                        if ((mSpec.MovingSpeed.HeightDn.Min <= mData.HeightDnTime.Data) && (mData.HeightDnTime.Data <= mSpec.MovingSpeed.HeightDn.Max))
        //                            mData.HeightDnTime.Result = RESULT.PASS;
        //                        else mData.HeightDnTime.Result = RESULT.REJECT;

        //                        fpSpread1.ActiveSheet.Cells[10, 6].Text = mData.HeightDnTime.Data.ToString("0.00");
        //                        fpSpread1.ActiveSheet.Cells[10, 7].Text = mData.HeightDnTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                        fpSpread1.ActiveSheet.Cells[10, 7].ForeColor = mData.HeightDnTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                    }
        //                    //}
        //                }
        //            }
        //            else
        //            {
        //                if (/*(FirstPosCheckEnd[SpeedCheckStep] == false) && (*/AdData < mSpec.HeightLimitCurr)//)
        //                {
        //                    SpeedCheckEnd[SpeedCheckStep] = true;
        //                    HeightUpOnOff = false;
        //                    SoundCheckTimeToEnd[SpeedCheckStep] = plot2.Channels[0].Count;
        //                    float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[SpeedCheckStep]) / 1000F;
        //                    mData.HeightUpTime.Data = (mSpec.MovingStroke.Height / EndTime) * 1F;
        //                    mData.HeightUpTime.Test = true;
        //                    if ((mSpec.MovingSpeed.HeightUp.Min <= mData.HeightUpTime.Data) && (mData.HeightUpTime.Data <= mSpec.MovingSpeed.HeightUp.Max))
        //                        mData.HeightUpTime.Result = RESULT.PASS;
        //                    else mData.HeightUpTime.Result = RESULT.REJECT;

        //                    fpSpread1.ActiveSheet.Cells[9, 6].Text = mData.HeightUpTime.Data.ToString("0.00");
        //                    fpSpread1.ActiveSheet.Cells[9, 7].Text = mData.HeightUpTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                    fpSpread1.ActiveSheet.Cells[9, 7].ForeColor = mData.HeightUpTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                }
        //                else
        //                {
        //                    //if (FirstPosCheckEnd[SpeedCheckStep] == false)
        //                    //{
        //                    if ((mSpec.HeightLimitTime * 1000) <= (StepTimeLast - StepTimeFirst))
        //                    {
        //                        SpeedCheckEnd[SpeedCheckStep] = true;
        //                        HeightDnOnOff = false;
        //                        SoundCheckTimeToEnd[SpeedCheckStep] = plot2.Channels[0].Count;
        //                        float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[SpeedCheckStep]) / 1000F;
        //                        mData.HeightUpTime.Data = (mSpec.MovingStroke.Height / EndTime) * 1F;
        //                        mData.HeightUpTime.Test = true;
        //                        if ((mSpec.MovingSpeed.HeightUp.Min <= mData.HeightUpTime.Data) && (mData.HeightUpTime.Data <= mSpec.MovingSpeed.HeightUp.Max))
        //                            mData.HeightUpTime.Result = RESULT.PASS;
        //                        else mData.HeightUpTime.Result = RESULT.REJECT;

        //                        fpSpread1.ActiveSheet.Cells[9, 6].Text = mData.HeightUpTime.Data.ToString("0.00");
        //                        fpSpread1.ActiveSheet.Cells[9, 7].Text = mData.HeightUpTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                        fpSpread1.ActiveSheet.Cells[9, 7].ForeColor = mData.HeightUpTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                    }
        //                    //}
        //                }
        //            }
        //        }
        //        if (SpeedCheckEnd[SpeedCheckStep] == true)
        //        {
        //            SoundLavelCheck((short)(SpeedCheckStep / 2));
        //            SubStep++;
        //            SpecOutputFlag = false;
        //        }
        //    }
        //    return;
        //}

        //private bool NotImsSlideSpeedCheckFlag { get; set; }
        //private bool NotImsReclineSpeedCheckFlag { get; set; }
        //private bool NotImsTiltSpeedCheckFlag { get; set; }
        //private bool NotImsHeightSpeedCheckFlag { get; set; }

        //private bool NotImsSlideSpeedCheckEnd { get; set; }
        //private bool NotImsReclineSpeedCheckEnd { get; set; }
        //private bool NotImsTiltSpeedCheckEnd { get; set; }
        //private bool NotImsHeightSpeedCheckEnd { get; set; }
        //private long[] SpeedCheckStartTime = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //private void CheckSpeedNotIMS()
        //{
        //    if (SpecOutputFlag == false)
        //    {
        //        label5.Text = "P/SEAT 속도 측정 중 입니다.";
        //        //label5.Text = "TULT UP 속도 측정 중 입니다.";
        //        //label5.Text = "HEIGHT UP 속도 측정 중 입니다.";
        //        //label5.Text = "RECLINE UP 속도 측정 중 입니다.";
        //        SlideBwdOnOff = true;
        //        ReclineBwdOnOff = true;
        //        TiltDnOnOff = true;
        //        HeightDnOnOff = true;

        //        SpeedCheckStartTime[0] = ComF.timeGetTimems();
        //        SpeedCheckStartTime[1] = ComF.timeGetTimems();
        //        SpeedCheckStartTime[2] = ComF.timeGetTimems();
        //        SpeedCheckStartTime[3] = ComF.timeGetTimems();
        //        NotImsSlideSpeedCheckFlag = false;
        //        NotImsReclineSpeedCheckFlag = false;
        //        NotImsTiltSpeedCheckFlag = false;
        //        NotImsHeightSpeedCheckFlag = false;

        //        NotImsSlideSpeedCheckEnd = false;
        //        NotImsReclineSpeedCheckEnd = false;
        //        NotImsTiltSpeedCheckEnd = false;
        //        NotImsHeightSpeedCheckEnd = false;
        //        SpecOutputFlag = true;
        //    }
        //    else
        //    {
        //        float AdData1;
        //        float AdData2;
        //        float AdData3;
        //        float AdData4;

        //        if (NotImsSlideSpeedCheckEnd == false)
        //        {
        //            if (NotImsSlideSpeedCheckFlag == false)
        //                AdData1 = IOPort.ADRead[mSpec.PinMap.SlideBWD.PinNo];
        //            else AdData1 = IOPort.ADRead[mSpec.PinMap.SlideFWD.PinNo];

        //            if (1000 <= (ComF.timeGetTimems() - SpeedCheckStartTime[0]))
        //            {
        //                bool Flag = false;

        //                if (comboBox4.SelectedItem.ToString() == "IMS")
        //                {
        //                    if ((AdData1 <= mSpec.SlideLimitCurr) || ((mSpec.SlideLimitTime * 1000) <= (ComF.timeGetTimems() - SpeedCheckStartTime[0]))) Flag = true;
        //                }
        //                else
        //                {
        //                    if ((mSpec.SlideLimitCurr <= AdData1) || ((mSpec.SlideLimitTime * 1000) <= (ComF.timeGetTimems() - SpeedCheckStartTime[0]))) Flag = true;
        //                }
        //                if (Flag == true)
        //                {
        //                    if (NotImsSlideSpeedCheckFlag == false)
        //                        SlideBwdOnOff = false;
        //                    else SlideFwdOnOff = false;

        //                    float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[0]) / 1000F;

        //                    if (NotImsSlideSpeedCheckFlag == false)
        //                    {
        //                        mData.SlideBwdTime.Test = true;
        //                        mData.SlideBwdTime.Data = (mSpec.MovingStroke.Slide / EndTime) * 1F;
        //                        if ((mSpec.MovingSpeed.SlideBwd.Min <= mData.SlideBwdTime.Data) && (mData.SlideBwdTime.Data <= mSpec.MovingSpeed.SlideBwd.Max))
        //                            mData.SlideBwdTime.Result = RESULT.PASS;
        //                        else mData.SlideBwdTime.Result = RESULT.REJECT;

        //                        fpSpread1.Sheets[0].Cells[4, 6].Text = mData.SlideBwdTime.Data.ToString("0.00");
        //                        fpSpread1.Sheets[0].Cells[4, 7].Text = mData.SlideBwdTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                        fpSpread1.Sheets[0].Cells[4, 7].ForeColor = mData.SlideBwdTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                    }
        //                    else
        //                    {
        //                        mData.SlideFwdTime.Test = true;
        //                        mData.SlideFwdTime.Data = (mSpec.MovingStroke.Slide / EndTime) * 1F;
        //                        if ((mSpec.MovingSpeed.SlideFwd.Min <= mData.SlideFwdTime.Data) && (mData.SlideFwdTime.Data <= mSpec.MovingSpeed.SlideFwd.Max))
        //                            mData.SlideFwdTime.Result = RESULT.PASS;
        //                        else mData.SlideFwdTime.Result = RESULT.REJECT;

        //                        fpSpread1.Sheets[0].Cells[3, 6].Text = mData.SlideFwdTime.Data.ToString("0.00");
        //                        fpSpread1.Sheets[0].Cells[3, 7].Text = mData.SlideFwdTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                        fpSpread1.Sheets[0].Cells[3, 7].ForeColor = mData.SlideFwdTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                    }

        //                    if (NotImsSlideSpeedCheckFlag == false)
        //                    {
        //                        ComF.timedelay(200);
        //                        NotImsSlideSpeedCheckFlag = true;
        //                        SlideFwdOnOff = true;
        //                        SpeedCheckStartTime[0] = ComF.timeGetTimems();
        //                    }
        //                    else
        //                    {
        //                        NotImsSlideSpeedCheckEnd = true;
        //                    }
        //                }
        //            }
        //        }


        //        if (NotImsReclineSpeedCheckEnd == false)
        //        {
        //            if (NotImsReclineSpeedCheckFlag == false)
        //                AdData2 = IOPort.ADRead[mSpec.PinMap.ReclineBWD.PinNo];
        //            else AdData2 = IOPort.ADRead[mSpec.PinMap.ReclineFWD.PinNo];

        //            if (1000 <= (ComF.timeGetTimems() - SpeedCheckStartTime[0]))
        //            {
        //                bool Flag = false;

        //                if (comboBox4.SelectedItem.ToString() == "IMS")
        //                {
        //                    if ((AdData2 <= mSpec.ReclinerLimitCurr) || ((mSpec.ReclinerLimitTime * 1000) <= (ComF.timeGetTimems() - SpeedCheckStartTime[0]))) Flag = true;
        //                }
        //                else
        //                {
        //                    if ((mSpec.ReclinerLimitCurr <= AdData2) || ((mSpec.ReclinerLimitTime * 1000) <= (ComF.timeGetTimems() - SpeedCheckStartTime[0]))) Flag = true;
        //                }
        //                if (Flag == true)
        //                {
        //                    if (NotImsReclineSpeedCheckFlag == false)
        //                        ReclineBwdOnOff = false;
        //                    else ReclineFwdOnOff = false;

        //                    float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[0]) / 1000F;

        //                    if (NotImsReclineSpeedCheckFlag == false)
        //                    {
        //                        mData.ReclineBwdTime.Test = true;
        //                        mData.ReclineBwdTime.Data = (mSpec.MovingStroke.Recliner / EndTime) * 1F;
        //                        if ((mSpec.MovingSpeed.ReclinerBwd.Min <= mData.ReclineBwdTime.Data) && (mData.ReclineBwdTime.Data <= mSpec.MovingSpeed.ReclinerBwd.Max))
        //                            mData.ReclineBwdTime.Result = RESULT.PASS;
        //                        else mData.ReclineBwdTime.Result = RESULT.REJECT;

        //                        fpSpread1.Sheets[0].Cells[6, 6].Text = mData.ReclineBwdTime.Data.ToString("0.00");
        //                        fpSpread1.Sheets[0].Cells[6, 7].Text = mData.ReclineBwdTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                        fpSpread1.Sheets[0].Cells[6, 7].ForeColor = mData.ReclineBwdTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                    }
        //                    else
        //                    {
        //                        mData.ReclineFwdTime.Test = true;
        //                        mData.ReclineFwdTime.Data = (mSpec.MovingStroke.Recliner / EndTime) * 1F;
        //                        if ((mSpec.MovingSpeed.ReclinerFwd.Min <= mData.ReclineFwdTime.Data) && (mData.ReclineFwdTime.Data <= mSpec.MovingSpeed.ReclinerBwd.Max))
        //                            mData.ReclineFwdTime.Result = RESULT.PASS;
        //                        else mData.ReclineFwdTime.Result = RESULT.REJECT;

        //                        fpSpread1.Sheets[0].Cells[5, 6].Text = mData.ReclineFwdTime.Data.ToString("0.00");
        //                        fpSpread1.Sheets[0].Cells[5, 7].Text = mData.ReclineFwdTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                        fpSpread1.Sheets[0].Cells[5, 7].ForeColor = mData.ReclineFwdTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                    }

        //                    if (NotImsReclineSpeedCheckFlag == false)
        //                    {
        //                        ComF.timedelay(200);
        //                        NotImsReclineSpeedCheckFlag = true;
        //                        ReclineFwdOnOff = true;
        //                        SpeedCheckStartTime[1] = ComF.timeGetTimems();
        //                    }
        //                    else
        //                    {
        //                        NotImsReclineSpeedCheckEnd = true;
        //                    }
        //                }
        //            }
        //        }

        //        if (NotImsTiltSpeedCheckEnd == false)
        //        {
        //            if (NotImsTiltSpeedCheckFlag == false)
        //                AdData3 = IOPort.ADRead[mSpec.PinMap.TiltDn.PinNo];
        //            else AdData3 = IOPort.ADRead[mSpec.PinMap.TiltUp.PinNo];

        //            if (1000 <= (ComF.timeGetTimems() - SpeedCheckStartTime[1]))
        //            {
        //                bool Flag = false;

        //                if (comboBox4.SelectedItem.ToString() == "IMS")
        //                {
        //                    if ((AdData3 <= mSpec.TiltLimitCurr) || ((mSpec.TiltLimitTime * 1000) <= (ComF.timeGetTimems() - SpeedCheckStartTime[1]))) Flag = true;
        //                }
        //                else
        //                {
        //                    if ((mSpec.TiltLimitCurr <= AdData3) || ((mSpec.TiltLimitTime * 1000) <= (ComF.timeGetTimems() - SpeedCheckStartTime[1]))) Flag = true;
        //                }
        //                if (Flag == true)
        //                {
        //                    if (NotImsTiltSpeedCheckFlag == false)
        //                        TiltDnOnOff = false;
        //                    else TiltUpOnOff = false;

        //                    float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[1]) / 1000F;

        //                    if (NotImsTiltSpeedCheckFlag == false)
        //                    {
        //                        mData.TiltDnTime.Test = true;
        //                        mData.TiltDnTime.Data = (mSpec.MovingStroke.Tilt / EndTime) * 1F;
        //                        if ((mSpec.MovingSpeed.TiltDn.Min <= mData.TiltDnTime.Data) && (mData.TiltDnTime.Data <= mSpec.MovingSpeed.TiltDn.Max))
        //                            mData.TiltDnTime.Result = RESULT.PASS;
        //                        else mData.TiltDnTime.Result = RESULT.REJECT;

        //                        fpSpread1.Sheets[0].Cells[8, 6].Text = mData.TiltDnTime.Data.ToString("0.00");
        //                        fpSpread1.Sheets[0].Cells[8, 7].Text = mData.TiltDnTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                        fpSpread1.Sheets[0].Cells[8, 7].ForeColor = mData.TiltDnTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                    }
        //                    else
        //                    {
        //                        mData.TiltUpTime.Test = true;
        //                        mData.TiltUpTime.Data = (mSpec.MovingStroke.Tilt / EndTime) * 1F;
        //                        if ((mSpec.MovingSpeed.TiltUp.Min <= mData.TiltUpTime.Data) && (mData.TiltUpTime.Data <= mSpec.MovingSpeed.TiltUp.Max))
        //                            mData.TiltUpTime.Result = RESULT.PASS;
        //                        else mData.TiltUpTime.Result = RESULT.REJECT;

        //                        fpSpread1.Sheets[0].Cells[7, 6].Text = mData.TiltUpTime.Data.ToString("0.00");
        //                        fpSpread1.Sheets[0].Cells[7, 7].Text = mData.TiltUpTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                        fpSpread1.Sheets[0].Cells[7, 7].ForeColor = mData.TiltUpTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                    }

        //                    if (NotImsTiltSpeedCheckFlag == false)
        //                    {
        //                        ComF.timedelay(200);
        //                        NotImsTiltSpeedCheckFlag = true;
        //                        TiltUpOnOff = true;
        //                        SpeedCheckStartTime[2] = ComF.timeGetTimems();
        //                    }
        //                    else
        //                    {
        //                        NotImsTiltSpeedCheckEnd = true;
        //                    }
        //                }
        //            }
        //        }

        //        if (NotImsHeightSpeedCheckEnd == false)
        //        {
        //            if (NotImsHeightSpeedCheckFlag == false)
        //                AdData4 = IOPort.ADRead[mSpec.PinMap.HeightDn.PinNo];
        //            else AdData4 = IOPort.ADRead[mSpec.PinMap.HeightUp.PinNo];

        //            if (1000 <= (ComF.timeGetTimems() - SpeedCheckStartTime[2]))
        //            {
        //                bool Flag = false;

        //                if (comboBox4.SelectedItem.ToString() == "IMS")
        //                {
        //                    if ((AdData4 <= mSpec.HeightLimitCurr) || ((mSpec.HeightLimitTime * 1000) <= (ComF.timeGetTimems() - SpeedCheckStartTime[2]))) Flag = true;
        //                }
        //                else
        //                {
        //                    if (NotImsHeightSpeedCheckFlag == true)
        //                    {
        //                        if ((mSpec.HeightLimitCurr <= AdData4) || ((mSpec.HeightLimitTime * 1000) <= (ComF.timeGetTimems() - SpeedCheckStartTime[2]))) Flag = true;
        //                    }
        //                    else
        //                    {
        //                        if ((5 <= AdData4) || ((mSpec.HeightLimitTime * 1000) <= (ComF.timeGetTimems() - SpeedCheckStartTime[2]))) Flag = true;
        //                    }
        //                }
        //                if (Flag == true)
        //                {
        //                    if (NotImsHeightSpeedCheckFlag == false)
        //                        HeightDnOnOff = false;
        //                    else HeightUpOnOff = false;

        //                    float EndTime = (ComF.timeGetTimems() - SpeedCheckStartTime[2]) / 1000F;

        //                    if (NotImsHeightSpeedCheckFlag == false)
        //                    {
        //                        mData.HeightDnTime.Test = true;
        //                        mData.HeightDnTime.Data = (mSpec.MovingStroke.Height / EndTime) * 1F;
        //                        if ((mSpec.MovingSpeed.HeightDn.Min <= mData.HeightDnTime.Data) && (mData.HeightDnTime.Data <= mSpec.MovingSpeed.HeightDn.Max))
        //                            mData.HeightDnTime.Result = RESULT.PASS;
        //                        else mData.HeightDnTime.Result = RESULT.REJECT;

        //                        fpSpread1.Sheets[0].Cells[10, 6].Text = mData.HeightDnTime.Data.ToString("0.00");
        //                        fpSpread1.Sheets[0].Cells[10, 7].Text = mData.HeightDnTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                        fpSpread1.Sheets[0].Cells[10, 7].ForeColor = mData.HeightDnTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                    }
        //                    else
        //                    {
        //                        mData.HeightUpTime.Test = true;
        //                        mData.HeightUpTime.Data = (mSpec.MovingStroke.Height / EndTime) * 1F;
        //                        if ((mSpec.MovingSpeed.HeightUp.Min <= mData.HeightUpTime.Data) && (mData.HeightUpTime.Data <= mSpec.MovingSpeed.HeightUp.Max))
        //                            mData.HeightUpTime.Result = RESULT.PASS;
        //                        else mData.HeightUpTime.Result = RESULT.REJECT;

        //                        fpSpread1.Sheets[0].Cells[9, 6].Text = mData.HeightUpTime.Data.ToString("0.00");
        //                        fpSpread1.Sheets[0].Cells[9, 7].Text = mData.HeightUpTime.Result == RESULT.PASS ? "O.K" : "N.G";
        //                        fpSpread1.Sheets[0].Cells[9, 7].ForeColor = mData.HeightUpTime.Result == RESULT.PASS ? Color.Black : Color.Red;
        //                    }

        //                    if (NotImsHeightSpeedCheckFlag == false)
        //                    {
        //                        ComF.timedelay(200);
        //                        NotImsHeightSpeedCheckFlag = true;
        //                        HeightUpOnOff = true;
        //                        SpeedCheckStartTime[3] = ComF.timeGetTimems();
        //                    }
        //                    else
        //                    {
        //                        NotImsHeightSpeedCheckEnd = true;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    if ((NotImsSlideSpeedCheckEnd == true) && (NotImsTiltSpeedCheckEnd == true) && (NotImsHeightSpeedCheckEnd == true) && (NotImsReclineSpeedCheckEnd == true))
        //    {
        //        SpecOutputFlag = false;
        //        Step++;
        //    }

        //    return;
        //}
        //private void SoundCheck()
        //{
        //    if (mData.ReclineFwdSound.Test == true)
        //    {
        //        if (mSpec.Sound.StartMax < mData.ReclineFwdSound.StartData)
        //            mData.ReclineFwdSound.ResultStart = RESULT.REJECT;
        //        else mData.ReclineFwdSound.ResultStart = RESULT.PASS;

        //        if (mSpec.Sound.RunMax < mData.ReclineFwdSound.RunData)
        //            mData.ReclineFwdSound.RunData = RESULT.REJECT;
        //        else mData.ReclineFwdSound.RunData = RESULT.PASS;

        //    }

        //    if (mData.ReclineBwdSound.Test == true)
        //    {
        //        if (mSpec.Sound.StartMax < mData.ReclineBwdSound.StartData)
        //            mData.ReclineBwdSound.ResultStart = RESULT.REJECT;
        //        else mData.ReclineBwdSound.ResultStart = RESULT.PASS;

        //        if (mSpec.Sound.RunMax < mData.ReclineBwdSound.RunData)
        //            mData.ReclineBwdSound.ResultRun = RESULT.REJECT;
        //        else mData.ReclineBwdSound.ResultRun = RESULT.PASS;
        //    }

        //    if (mData.RelaxSound.Test == true)
        //    {
        //        if (mSpec.Sound.StartMax < mData.RelaxSound.StartData)
        //            mData.RelaxSound.ResultStart = RESULT.REJECT;
        //        else mData.RelaxSound.ResultStart = RESULT.PASS;

        //        if (mSpec.Sound.RunMax < mData.RelaxSound.RunData)
        //            mData.RelaxSound.ResultRun = RESULT.REJECT;
        //        else mData.RelaxSound.ResultRun = RESULT.PASS;
        //    }

        //    if (mData.RelaxReturnSound.Test == true)
        //    {
        //        if (mSpec.Sound.StartMax < mData.RelaxReturnSound.StartData)
        //            mData.RelaxReturnSound.ResultStart = RESULT.REJECT;
        //        else mData.RelaxReturnSound.ResultStart = RESULT.PASS;

        //        if (mSpec.Sound.RunMax < mData.RelaxReturnSound.RunData)
        //            mData.RelaxReturnSound.ResultRun = RESULT.REJECT;
        //        else mData.RelaxReturnSound.ResultRun = RESULT.PASS;
        //    }
        //    if (mData.HeightUpSound.Test == true)
        //    {
        //        if (mSpec.Sound.StartMax < mData.HeightUpSound.StartData)
        //            mData.HeightUpSound.ResultStart = RESULT.REJECT;
        //        else mData.HeightUpSound.ResultStart = RESULT.PASS;

        //        if (mSpec.Sound.RunMax < mData.HeightUpSound.RunData)
        //            mData.HeightUpSound.ResultRun = RESULT.REJECT;
        //        else mData.HeightUpSound.ResultRun = RESULT.PASS;
        //    }

        //    if (mData.HeightDnSound.Test == true)
        //    {
        //        if (mSpec.Sound.StartMax < mData.HeightDnSound.StartData)
        //            mData.HeightDnSound.ResultStart = RESULT.REJECT;
        //        else mData.HeightDnSound.ResultStart = RESULT.PASS;

        //        if (mSpec.Sound.RunMax < mData.HeightDnSound.RunData)
        //            mData.HeightDnSound.ResultRun = RESULT.REJECT;
        //        else mData.HeightDnSound.ResultRun = RESULT.PASS;
        //    }

        //    if (mData.LegrestSound.Test == true)
        //    {
        //        if (mSpec.Sound.StartMax < mData.LegrestSound.StartData)
        //            mData.LegrestSound.ResultStart = RESULT.REJECT;
        //        else mData.LegrestSound.ResultStart = RESULT.PASS;

        //        if (mSpec.Sound.RunMax < mData.LegrestSound.RunData)
        //            mData.LegrestSound.ResultRun = RESULT.REJECT;
        //        else mData.LegrestSound.ResultRun = RESULT.PASS;
        //    }

        //    if (mData.LegrestReturnSound.Test == true)
        //    {
        //        if (mSpec.Sound.StartMax < mData.LegrestReturnSound.StartData)
        //            mData.LegrestReturnSound.ResultStart = RESULT.REJECT;
        //        else mData.LegrestReturnSound.ResultStart = RESULT.PASS;

        //        if (mSpec.Sound.RunMax < mData.LegrestReturnSound.RunData)
        //            mData.LegrestReturnSound.ResultRun = RESULT.REJECT;
        //        else mData.LegrestReturnSound.ResultRun = RESULT.PASS;
        //    }

        //    if (mData.LegrestExtSound.Test == true)
        //    {
        //        if (mSpec.Sound.StartMax < mData.LegrestExtSound.StartData)
        //            mData.LegrestExtSound.ResultStart = RESULT.REJECT;
        //        else mData.LegrestExtSound.ResultStart = RESULT.PASS;

        //        if (mSpec.Sound.RunMax < mData.LegrestExtSound.RunData)
        //            mData.LegrestExtSound.ResultRun = RESULT.REJECT;
        //        else mData.LegrestExtSound.ResultRun = RESULT.PASS;
        //    }

        //    if (mData.LegrestExtReturnSound.Test == true)
        //    {
        //        if (mSpec.Sound.StartMax < mData.LegrestExtReturnSound.StartData)
        //            mData.LegrestExtReturnSound.ResultStart = RESULT.REJECT;
        //        else mData.LegrestExtReturnSound.ResultStart = RESULT.PASS;

        //        if (mSpec.Sound.RunMax < mData.LegrestExtReturnSound.RunData)
        //            mData.LegrestExtReturnSound.ResultRun = RESULT.REJECT;
        //        else mData.LegrestExtReturnSound.ResultRun = RESULT.PASS;
        //    }


        //    if (mData.ReclineFwdSound.Test == true)
        //    {
        //        fpSpread1.ActiveSheet.Cells[17, 6].Text = mData.ReclineFwdSound.StartData.ToString("0.00");
        //        fpSpread1.ActiveSheet.Cells[18, 6].Text = mData.ReclineFwdSound.RunData.ToString("0.00");

        //        fpSpread1.ActiveSheet.Cells[17, 7].Text = (mData.ReclineFwdSound.ResultStart == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[17, 7].ForeColor = (mData.ReclineFwdSound.ResultStart == RESULT.PASS) ? Color.Black : Color.Red;

        //        fpSpread1.ActiveSheet.Cells[18, 7].Text = (mData.ReclineFwdSound.ResultRun == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[18, 7].ForeColor = (mData.ReclineFwdSound.ResultRun == RESULT.PASS) ? Color.Black : Color.Red;
        //    }


        //    if (mData.ReclineBwdSound.Test == true)
        //    {
        //        fpSpread1.ActiveSheet.Cells[17, 6].Text = mData.ReclineBwdSound.StartData.ToString("0.00");
        //        fpSpread1.ActiveSheet.Cells[18, 6].Text = mData.ReclineBwdSound.RunData.ToString("0.00");

        //        fpSpread1.ActiveSheet.Cells[17, 7].Text = (mData.ReclineBwdSound.ResultStart == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[17, 7].ForeColor = (mData.ReclineBwdSound.ResultStart == RESULT.PASS) ? Color.Black : Color.Red;

        //        fpSpread1.ActiveSheet.Cells[18, 7].Text = (mData.ReclineBwdSound.ResultRun == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[18, 7].ForeColor = (mData.ReclineBwdSound.ResultRun == RESULT.PASS) ? Color.Black : Color.Red;
        //    }

        //    if (mData.RelaxSound.Test == true)
        //    {
        //        fpSpread1.ActiveSheet.Cells[19, 6].Text = mData.RelaxSound.StartData.ToString("0.00");
        //        fpSpread1.ActiveSheet.Cells[20, 6].Text = mData.RelaxSound.RunData.ToString("0.00");

        //        fpSpread1.ActiveSheet.Cells[19, 7].Text = (mData.RelaxSound.ResultStart == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[19, 7].ForeColor = (mData.RelaxSound.ResultStart == RESULT.PASS) ? Color.Black : Color.Red;

        //        fpSpread1.ActiveSheet.Cells[20, 7].Text = (mData.RelaxSound.ResultRun == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[20, 7].ForeColor = (mData.RelaxSound.ResultRun == RESULT.PASS) ? Color.Black : Color.Red;
        //    }


        //    if (mData.RelaxReturnSound.Test == true)
        //    {
        //        fpSpread1.ActiveSheet.Cells[19, 6].Text = mData.RelaxReturnSound.StartData.ToString("0.00");
        //        fpSpread1.ActiveSheet.Cells[20, 6].Text = mData.RelaxReturnSound.RunData.ToString("0.00");

        //        fpSpread1.ActiveSheet.Cells[19, 7].Text = (mData.RelaxReturnSound.ResultStart == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[19, 7].ForeColor = (mData.RelaxReturnSound.ResultStart == RESULT.PASS) ? Color.Black : Color.Red;

        //        fpSpread1.ActiveSheet.Cells[20, 7].Text = (mData.RelaxReturnSound.ResultRun == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[20, 7].ForeColor = (mData.RelaxReturnSound.ResultRun == RESULT.PASS) ? Color.Black : Color.Red;
        //    }

        //    if(mData.HeightUpSound.Test == true)
        //    {
        //        fpSpread1.ActiveSheet.Cells[21, 6].Text = mData.HeightUpSound.StartData.ToString("0.00");
        //        fpSpread1.ActiveSheet.Cells[22, 6].Text = mData.HeightUpSound.RunData.ToString("0.00");

        //        fpSpread1.ActiveSheet.Cells[21, 7].Text = (mData.HeightUpSound.ResultStart == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[21, 7].ForeColor = (mData.HeightUpSound.ResultStart == RESULT.PASS) ? Color.Black : Color.Red;

        //        fpSpread1.ActiveSheet.Cells[22, 7].Text = (mData.HeightUpSound.ResultRun == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[22, 7].ForeColor = (mData.HeightUpSound.ResultRun == RESULT.PASS) ? Color.Black : Color.Red;
        //    }


        //    if (mData.HeightDnSound.Test == true)
        //    {
        //        fpSpread1.ActiveSheet.Cells[21, 6].Text = mData.HeightDnSound.StartData.ToString("0.00");
        //        fpSpread1.ActiveSheet.Cells[22, 6].Text = mData.HeightDnSound.RunData.ToString("0.00");

        //        fpSpread1.ActiveSheet.Cells[21, 7].Text = (mData.HeightDnSound.ResultStart == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[21, 7].ForeColor = (mData.HeightDnSound.ResultStart == RESULT.PASS) ? Color.Black : Color.Red;

        //        fpSpread1.ActiveSheet.Cells[22, 7].Text = (mData.HeightDnSound.ResultRun == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[22, 7].ForeColor = (mData.HeightDnSound.ResultRun == RESULT.PASS) ? Color.Black : Color.Red;
        //    }

        //    if (mData.LegrestSound.Test == true)
        //    {
        //        fpSpread1.ActiveSheet.Cells[23, 6].Text = mData.LegrestSound.StartData.ToString("0.00");
        //        fpSpread1.ActiveSheet.Cells[24, 6].Text = mData.LegrestSound.RunData.ToString("0.00");

        //        fpSpread1.ActiveSheet.Cells[23, 7].Text = (mData.LegrestSound.ResultStart == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[23, 7].ForeColor = (mData.LegrestSound.ResultStart == RESULT.PASS) ? Color.Black : Color.Red;

        //        fpSpread1.ActiveSheet.Cells[24, 7].Text = (mData.LegrestSound.ResultRun == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[24, 7].ForeColor = (mData.LegrestSound.ResultRun == RESULT.PASS) ? Color.Black : Color.Red;
        //    }


        //    if (mData.LegrestReturnSound.Test == true)
        //    {
        //        fpSpread1.ActiveSheet.Cells[23, 6].Text = mData.LegrestReturnSound.StartData.ToString("0.00");
        //        fpSpread1.ActiveSheet.Cells[24, 6].Text = mData.LegrestReturnSound.RunData.ToString("0.00");

        //        fpSpread1.ActiveSheet.Cells[23, 7].Text = (mData.LegrestReturnSound.ResultStart == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[23, 7].ForeColor = (mData.LegrestReturnSound.ResultStart == RESULT.PASS) ? Color.Black : Color.Red;

        //        fpSpread1.ActiveSheet.Cells[24, 7].Text = (mData.LegrestReturnSound.ResultRun == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[24, 7].ForeColor = (mData.LegrestReturnSound.ResultRun == RESULT.PASS) ? Color.Black : Color.Red;
        //    }

        //    if (mData.LegrestExtSound.Test == true)
        //    {
        //        fpSpread1.ActiveSheet.Cells[25, 6].Text = mData.LegrestExtSound.StartData.ToString("0.00");
        //        fpSpread1.ActiveSheet.Cells[26, 6].Text = mData.LegrestExtSound.RunData.ToString("0.00");

        //        fpSpread1.ActiveSheet.Cells[25, 7].Text = (mData.LegrestExtSound.ResultStart == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[25, 7].ForeColor = (mData.LegrestExtSound.ResultStart == RESULT.PASS) ? Color.Black : Color.Red;

        //        fpSpread1.ActiveSheet.Cells[26, 7].Text = (mData.LegrestExtSound.ResultRun == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[26, 7].ForeColor = (mData.LegrestExtSound.ResultRun == RESULT.PASS) ? Color.Black : Color.Red;
        //    }


        //    if (mData.LegrestExtReturnSound.Test == true)
        //    {
        //        fpSpread1.ActiveSheet.Cells[25, 6].Text = mData.LegrestExtReturnSound.StartData.ToString("0.00");
        //        fpSpread1.ActiveSheet.Cells[26, 6].Text = mData.LegrestExtReturnSound.RunData.ToString("0.00");

        //        fpSpread1.ActiveSheet.Cells[25, 7].Text = (mData.LegrestExtReturnSound.ResultStart == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[25, 7].ForeColor = (mData.LegrestExtReturnSound.ResultStart == RESULT.PASS) ? Color.Black : Color.Red;

        //        fpSpread1.ActiveSheet.Cells[26, 7].Text = (mData.LegrestExtReturnSound.ResultRun == RESULT.PASS) ? "O.K" : "N.G";
        //        fpSpread1.ActiveSheet.Cells[26, 7].ForeColor = (mData.LegrestExtReturnSound.ResultRun == RESULT.PASS) ? Color.Black : Color.Red;
        //    }
        //    SpecOutputFlag = false;
        //    Step++;
        //    return;
        //}

        private bool ReclineMotorMoveEndFlag { get; set; }
        private bool HeightMotorMoveEndFlag { get; set; }
        private bool RelaxMotorMoveEndFlag { get; set; }
        private bool LegrestMotorMoveEndFlag { get; set; }
        private bool LegrestExtMotorMoveEndFlag { get; set; }
        private bool PosMoveFlag { get; set; }
        private void DeliveryPosMoveing()
        {
            if (SpecOutputFlag == false)
            {
                PosMoveFlag = false;
                SpecOutputFlag = true;

                if (CheckItem.Recline == true)
                {
                    if (mSpec.DeliveryPos.Recliner == 0)//Fwd
                    {
                        ReclineFwdOnOff = true;
                        ReclineMotorMoveEndFlag = false;
                    }
                    else if (mSpec.DeliveryPos.Recliner == 1)//Mid
                    {
                        ReclineBwdOnOff = true;
                        ReclineMotorMoveEndFlag = false;
                    }
                    else
                    {
                        ReclineBwdOnOff = true;
                        ReclineMotorMoveEndFlag = false;
                    }
                }
                else
                {
                    ReclineMotorMoveEndFlag = true;
                }

                if (CheckItem.Relax == true)
                {
                    if (mSpec.DeliveryPos.Relax == 0)
                    {
                        RelaxOnOff = true;
                        RelaxMotorMoveEndFlag = false;

                    }
                    else
                    {
                        RelaxReturnOnOff = true;
                        RelaxMotorMoveEndFlag = false;
                    }
                }
                else
                {
                    RelaxMotorMoveEndFlag = true;
                }

                if (CheckItem.Legrest == true)
                {
                    if (mSpec.DeliveryPos.Legrest == 0)//Up
                    {
                        LegrestOnOff = true;
                        LegrestMotorMoveEndFlag = false;
                    }
                    else
                    {
                        LegrestReturnOnOff = true;
                        LegrestMotorMoveEndFlag = false;
                    }
                }
                else
                {
                    LegrestMotorMoveEndFlag = true;
                }

                if (CheckItem.LegrestExt == true)
                {
                    if (mSpec.DeliveryPos.LegrestExt == 0)//Up
                    {
                        LegrestExtOnOff = true;
                        LegrestExtMotorMoveEndFlag = false;
                    }
                    else
                    {
                        LegrestExtReturnOnOff = true;
                        LegrestExtMotorMoveEndFlag = false;
                    }
                }
                else
                {
                    LegrestExtMotorMoveEndFlag = true;
                }

                if (CheckItem.Height == true)
                {
                    if (mSpec.DeliveryPos.Height == 0)//Up
                    {
                        HeightUpOnOff = true;
                        HeightMotorMoveEndFlag = false;
                    }
                    else
                    {
                        HeightDnOnOff = true;
                        HeightMotorMoveEndFlag = false;
                    }
                }
                else
                {
                    HeightMotorMoveEndFlag = true;
                }
                StepTimeFirst = ComF.timeGetTimems();
                StepTimeLast = ComF.timeGetTimems();

                label5.Text = "출하 위치로 이동 중 입니다.";
            }
            else
            {
                StepTimeLast = ComF.timeGetTimems();

                if (CheckItem.Relax == true)
                {
                    if ((mSpec.RelaxSwOnTime * 1000) <= (StepTimeLast - StepTimeFirst))
                    {
                        if (RelaxOnOff == true) RelaxOnOff = false;
                        if (RelaxReturnOnOff == true) RelaxReturnOnOff = false;
                    }
                }

                if (1000 <= (StepTimeLast - StepTimeFirst))
                {
                    float AdData;

                    AdData = pMeter.GetPSeat;

                    if (AdData < 0.5)
                    {
                        if (PosMoveFlag == true)
                        {
                            if (ReclineMotorMoveEndFlag == false)
                            {
                                if (ReclineFwdOnOff == true) ReclineFwdOnOff = false;
                                if (ReclineBwdOnOff == true) ReclineBwdOnOff = false;
                                ReclineMotorMoveEndFlag = true;
                            }

                            if (HeightMotorMoveEndFlag == false)
                            {
                                if (HeightUpOnOff == true) HeightUpOnOff = false;
                                if (HeightDnOnOff == true) HeightDnOnOff = false;
                                HeightMotorMoveEndFlag = true;
                            }

                            if (LegrestMotorMoveEndFlag == false)
                            {
                                if (LegrestOnOff == true) LegrestOnOff = false;
                                if (LegrestReturnOnOff == true) LegrestReturnOnOff = false;
                                LegrestMotorMoveEndFlag = true;
                            }

                            if (LegrestExtMotorMoveEndFlag == false)
                            {
                                if (LegrestExtOnOff == true) LegrestExtOnOff = false;
                                if (LegrestExtReturnOnOff == true) LegrestExtReturnOnOff = false;
                                LegrestExtMotorMoveEndFlag = true;
                            }

                            if (RelaxMotorMoveEndFlag == false)
                            {
                                RelaxMotorMoveEndFlag = true;
                            }
                        }
                        else
                        {
                            if (3000 <= (StepTimeLast - StepTimeFirst))
                            {
                                if (ReclineMotorMoveEndFlag == false)
                                {
                                    if (ReclineFwdOnOff == true) ReclineFwdOnOff = false;
                                    if (ReclineBwdOnOff == true) ReclineBwdOnOff = false;
                                    ReclineMotorMoveEndFlag = true;
                                }

                                if (HeightMotorMoveEndFlag == false)
                                {
                                    if (HeightUpOnOff == true) HeightUpOnOff = false;
                                    if (HeightDnOnOff == true) HeightDnOnOff = false;
                                    HeightMotorMoveEndFlag = true;
                                }

                                if (LegrestMotorMoveEndFlag == false)
                                {
                                    if (LegrestOnOff == true) LegrestOnOff = false;
                                    if (LegrestReturnOnOff == true) LegrestReturnOnOff = false;
                                    LegrestMotorMoveEndFlag = true;
                                }

                                if (LegrestExtMotorMoveEndFlag == false)
                                {
                                    if (LegrestExtOnOff == true) LegrestExtOnOff = false;
                                    if (LegrestExtReturnOnOff == true) LegrestExtReturnOnOff = false;
                                    LegrestExtMotorMoveEndFlag = true;
                                }

                                if (RelaxMotorMoveEndFlag == false)
                                {
                                    RelaxMotorMoveEndFlag = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (PosMoveFlag == false) PosMoveFlag = true;
                        
                        if (ReclineMotorMoveEndFlag == false)
                        {
                            if ((mSpec.ReclinerLimitTime * 1000F) <= (StepTimeLast - StepTimeFirst))
                            {
                                if (ReclineFwdOnOff == true) ReclineFwdOnOff = false;
                                if (ReclineBwdOnOff == true) ReclineBwdOnOff = false;
                                ReclineMotorMoveEndFlag = true;
                            }
                        }

                        if (HeightMotorMoveEndFlag == false)
                        {
                            if ((mSpec.HeightLimitTime * 1000F) <= (StepTimeLast - StepTimeFirst))
                            {
                                if (HeightUpOnOff == true) HeightUpOnOff = false;
                                if (HeightDnOnOff == true) HeightDnOnOff = false;
                                HeightMotorMoveEndFlag = true;
                            }
                        }
                        if (RelaxMotorMoveEndFlag == false)
                        {
                            if ((mSpec.RelaxLimitTime * 1000F) <= (StepTimeLast - StepTimeFirst))
                            {
                                RelaxMotorMoveEndFlag = true;
                            }
                        }

                        if (LegrestMotorMoveEndFlag == false)
                        {
                            if ((mSpec.LegrestLimitTime * 1000F) <= (StepTimeLast - StepTimeFirst))
                            {
                                if (LegrestOnOff == true) LegrestOnOff = false;
                                if (LegrestReturnOnOff == true) LegrestReturnOnOff = false;
                                LegrestMotorMoveEndFlag = true;
                            }
                        }

                        if (LegrestExtMotorMoveEndFlag == false)
                        {
                            if ((mSpec.LegrestExtLimitTime * 1000F) <= (StepTimeLast - StepTimeFirst))
                            {
                                if (LegrestExtOnOff == true) LegrestExtOnOff = false;
                                if (LegrestExtReturnOnOff == true) LegrestExtReturnOnOff = false;
                                LegrestExtMotorMoveEndFlag = true;
                            }
                        }
                    }
                }
            }

            if ((ReclineMotorMoveEndFlag == true) && (HeightMotorMoveEndFlag == true) && (RelaxMotorMoveEndFlag == true) && (LegrestMotorMoveEndFlag == true) && (LegrestExtMotorMoveEndFlag == true))
            {
                SpecOutputFlag = false;
                Step++;
            }
            return;
        }

        private void ResultCheck()
        {
            mData.Result = RESULT.PASS;
            if ((mData.Height.Test == true) && (mData.Height.Result1 == RESULT.REJECT)) mData.Result = RESULT.REJECT;
            if ((mData.Height.Test == true) && (mData.Height.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;
            if ((mData.Relax.Test == true) && (mData.Relax.Result1 == RESULT.REJECT)) mData.Result = RESULT.REJECT;
            if ((mData.Relax.Test == true) && (mData.Relax.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;
            if ((mData.Legrest.Test == true) && (mData.Legrest.Result1 == RESULT.REJECT)) mData.Result = RESULT.REJECT;
            if ((mData.Legrest.Test == true) && (mData.Legrest.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;
            if ((mData.LegrestExt.Test == true) && (mData.LegrestExt.Result1 == RESULT.REJECT)) mData.Result = RESULT.REJECT;
            if ((mData.LegrestExt.Test == true) && (mData.LegrestExt.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;
            if ((mData.Recline.Test == true) && (mData.Recline.Result1 == RESULT.REJECT)) mData.Result = RESULT.REJECT;
            if ((mData.Recline.Test == true) && (mData.Recline.Result2 == RESULT.REJECT)) mData.Result = RESULT.REJECT;
            if ((mData.SWVer.Test == true) && (mData.SWVer.Result == RESULT.REJECT)) mData.Result = RESULT.REJECT;

            
            if (mData.Result == RESULT.REJECT)
            {
                label16.Text = "NG";
                label16.ForeColor = Color.Red;
                BuzzerOnCount = 0;
                BuzzerOnOff = true;
                IOPort.BuzzerOnOff = BuzzerOnOff;
                BuzzerRunFlag = true;
                BuzzerFirst = ComF.timeGetTimems();
                BuzzerLast = ComF.timeGetTimems();
            }
            else
            {
                label16.Text = "OK";
                label16.ForeColor = Color.Lime;
                IOPort.BuzzerOnOff = true;
                ComF.timedelay(500);
                IOPort.BuzzerOnOff = false;
            }
            if (mData.Result == RESULT.REJECT)
                label5.Text = "불량 제품 입니다.";
            else label5.Text = "양품 제품 입니다.";

            return;
        }

        private void SendTestData()
        {
            float Relax;
            float Recline;
            float Legrest;
            float LegrestExt;

            if (mData.Relax.Test == true)
                Relax = ((mData.Relax.Result1 == RESULT.REJECT) || (mData.Relax.Result2 == RESULT.REJECT)) ? 1F : 2F;
            else Relax = -9999;

            if(mData.Recline.Test == true)
                Recline = ((mData.Recline.Result1 == RESULT.REJECT) || (mData.Recline.Result2 == RESULT.REJECT)) ? 1F : 2F;
            else Recline = -9999;

            if (mData.Legrest.Test == true)
                Legrest = ((mData.Legrest.Result1 == RESULT.REJECT) || (mData.Legrest.Result2 == RESULT.REJECT)) ? 1F : 2F;
            else Legrest = 2;

            if (mData.LegrestExt.Test == true)
                LegrestExt = ((mData.LegrestExt.Result1 == RESULT.REJECT) || (mData.LegrestExt.Result2 == RESULT.REJECT)) ? 1F : 2F;
            else LegrestExt = -9999;

            string s = PopCtrl.CrateData(
                Serial: TInfor.Count.OK,
                TestCount: TInfor.Count.Total,
                Min_1: -9999, //ims
                Max_1: -9999, //ims
                Min_2: -9999, //ims
                Max_2: -9999, //ims
                Min_3: (mData.Relax.Test == true) ? 2 : -9999, //relax
                Max_3: (mData.Relax.Test == true) ? 2 : -9999, //relax
                Min_4: (mData.Recline.Test == true) ? 2 : -9999, //recline
                Max_4: (mData.Recline.Test == true) ? 2 : -9999, //recline
                Min_5: (mData.Legrest.Test == true) ? 2 : -9999, //Legrest
                Max_5: (mData.Legrest.Test == true) ? 2 : -9999, //Legrest
                Min_6: (mData.LegrestExt.Test == true) ? 2 : -9999, //LegrestExt
                Max_6: (mData.LegrestExt.Test == true) ? 2 : -9999, //LegrestExt
                Min_7: -9999,
                Max_7: -9999,
                Min_8: -9999,
                Max_8: -9999,
                Min_9: -9999,
                Max_9: -9999,
                Min_10: -9999,
                Max_10: -9999,
                Min_11: -9999,
                Max_11: -9999,
                Min_12: -9999,
                Max_12: -9999,
                Min_13: -9999,
                Max_13: -9999,
                Min_14: -9999,
                Max_14: -9999,
                Min_15: -9999,
                Max_15: -9999,
                Value1: -9999,
                Value2: -9999, 
                Value3: Relax, 
                Value4: Recline, 
                Value5: Legrest, 
                Value6: LegrestExt,
                Value7: -9999,
                Value8: -9999,
                Value9: -9999,
                Value10: -9999,
                Value11: -9999,
                Value12: -9999,
                Value13: -9999,
                Value14: -9999,
                Value15: -9999,
                Result: mData.Result
                );

            if (IOPort.GetAuto == true)
            {
                //if(PopCtrl.isServerConnection == true) PopCtrl.ServerSend = s;
                if (PopCtrl.isClientConnection == true) PopCtrl.Write = s + ",1";
            }
            else
            {
                if (PopCtrl.isClientConnection == true) PopCtrl.Write = s + ",2";
            }

            SaveLogData = "Sending - " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
            SaveLogData = s;
            return; 
        }


        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (IOPort.GetAuto == false) panel4.Visible = !panel4.Visible;
            if (panel4.Visible == true)
            {
                //panel4.Parent = this;
                panel4.BringToFront();
                if (CanReWrite.ModelTypeToRelax == CanMap.MODELTYPE.RELAX)
                {
                    ledBulb1.On = false;
                    ledBulb2.On = true;
                }
                else
                {
                    ledBulb1.On = true;
                    ledBulb2.On = false;
                }
                //    imageButton3.ButtonColor = Color.Red;
                //else imageButton3.ButtonColor = Color.Black;
            }
            else
            {
                CanOpenClose = false;
                IOPort.SetPinConnection = false;
            }
            IOPort.FunctionIOInit();
            return;
        }

        private void imageButton1_Click(object sender, EventArgs e)
        {
            UserImageButton.ImageButton But = sender as UserImageButton.ImageButton;
            if (But.ButtonColor == Color.Black)
            {
                PowerOnOff = true;
                ComF.timedelay(1000);
                BattOnOff = true;

                But.ButtonColor = Color.Red;
            }
            else
            {
                //PowerOnOff = false;
                BattOnOff = false;
                But.ButtonColor = Color.Black;
            }
            return;
        }


        private void ledArrow4_ValueChanged(object sender, Iocomp.Classes.ValueBooleanEventArgs e)
        {
            //Recline Fwd
            C_ReclineFwdOnOff = e.ValueNew;
            return;
        }

        private void ledArrow3_ValueChanged(object sender, Iocomp.Classes.ValueBooleanEventArgs e)
        {
            //Recline Bwd
            C_ReclineBwdOnOff = e.ValueNew;
            return;
        }

        private void ledArrow7_ValueChanged(object sender, Iocomp.Classes.ValueBooleanEventArgs e)
        {
            //relax
            C_RelaxOnOff = e.ValueNew;
            return;
        }

        private void ledArrow8_ValueChanged(object sender, Iocomp.Classes.ValueBooleanEventArgs e)
        {
            //relax return
            C_RelaxReturnOnOff = e.ValueNew;
            return;
        }



        private void ledArrow11_ValueChanged(object sender, Iocomp.Classes.ValueBooleanEventArgs e)
        {
            //Legrest 
            C_LegrestOnOff = e.ValueNew;
            return;
        }

        private void ledArrow12_ValueChanged(object sender, Iocomp.Classes.ValueBooleanEventArgs e)
        {
            //regrest return
            C_LegrestReturnOnOff = e.ValueNew;
            return;
        }

        private void ledArrow9_ValueChanged(object sender, Iocomp.Classes.ValueBooleanEventArgs e)
        {
            //regrest ext
            C_LegrestExtOnOff = e.ValueNew;
            return;
        }

        private void ledArrow10_ValueChanged(object sender, Iocomp.Classes.ValueBooleanEventArgs e)
        {
            //legrest ext return
            C_LegrestExtReturnOnOff = e.ValueNew;
            return;
        }

        private void ledArrow4_MouseDown(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = true;
            return;
        }

        private void ledArrow4_MouseUp(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = false;
            return;
        }

        private void ledArrow3_MouseDown(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = true;
            return;
        }

        private void ledArrow3_MouseUp(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = false;
            return;
        }

        private void ledArrow7_MouseDown(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = true;
            return;
        }

        private void ledArrow7_MouseUp(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = false;
            return;
        }

        private void ledArrow8_MouseDown(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = true;
            return;
        }

        private void ledArrow8_MouseUp(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = false;
            return;
        }

        private void ledArrow5_MouseDown(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = true;
            return;
        }

        private void ledArrow5_MouseUp(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = false;
            return;
        }

        private void ledArrow6_MouseDown(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = true;
            return;
        }

        private void ledArrow6_MouseUp(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = false;
            return;
        }

        private void ledArrow11_MouseDown(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = true;
            return;
        }

        private void ledArrow11_MouseUp(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = false;
            return;
        }

        private void ledArrow12_MouseDown(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = true;
            return;
        }

        private void ledArrow12_MouseUp(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = false;
            return;
        }

        private void ledArrow9_MouseDown(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = true;
            return;
        }

        private void ledArrow9_MouseUp(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = false;
            return;
        }

        private void ledArrow10_MouseDown(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = true;
            return;
        }

        private void ledArrow10_MouseUp(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = false;
            return;
        }

        private void imageButton2_Click(object sender, EventArgs e)
        {
            if (RunningFlag == false)
            {
                if (IOPort.GetAuto == true)
                {
                    if (PopReadOk == true) StartSetting();
                }
                else
                {
                    StartSetting();
                }
            }
            return;
        }

        private int RowCount = 0;

        private void CreateFileName()
        {
            string Path = Program.DATA_PATH.ToString() + "\\" + DateTime.Now.ToString("yyyyMM") + ".xls";

            //if ((TInfor.DataName != "") && (TInfor.DataName != null))
            //{
            //    if (File.Exists(TInfor.DataName))
            //    {
            //        if (TInfor.DataName != Path)
            //        {
            //            TInfor.DataName = Path;
            //            TInfor.Count.Total = 0;
            //            TInfor.Count.OK = 0;
            //            TInfor.Count.NG = 0;
            //            TInfor.Model = comboBox1.SelectedItem.ToString();
            //            SaveInfor();
            //        }
            //    }
            //}
            //else
            //{
            //    TInfor.DataName = Path;
            //    TInfor.DataName = Path;
            //    TInfor.Count.Total = 0;
            //    TInfor.Count.OK = 0;
            //    TInfor.Count.NG = 0;

            //    if (comboBox1.SelectedItem != null)
            //        TInfor.Model = comboBox1.SelectedItem.ToString();
            //    else TInfor.Model = "";
            //    SaveInfor();
            //}

            if (File.Exists(Path) == false)
            {
                CreateDataFile(Path);
            }
            else
            {
                fpSpread2.OpenExcel(Path);
                fpSpread2.Visible = false;
                fpSpread2.ActiveSheet.Protect = false;
                RowCount = 6;
                for (int i = RowCount; i < fpSpread2.ActiveSheet.RowCount; i++)
                {
                    if (fpSpread2.ActiveSheet.Cells[RowCount, 0].Text == "") break;
                    if (fpSpread2.ActiveSheet.Cells[RowCount, 0].Text == null) break;
                    RowCount++;
                }

                int Col = 0;
                for (int i = 0; i < fpSpread2.ActiveSheet.ColumnCount; i++)
                {
                    if (fpSpread2.ActiveSheet.Cells[3, Col].Text == "판정")
                        break;
                    else Col++;
                }
                fpSpread2.ActiveSheet.ColumnCount = Col + 1;
            }
            return;
        }

        private void SaveData()
        {
            string Path = Program.DATA_PATH.ToString() + "\\" + DateTime.Now.ToString("yyyyMM") + ".xls";

            //if ((TInfor.DataName != "") && (TInfor.DataName != null))
            //{
            //    if (File.Exists(TInfor.DataName))
            //    {
            //        if (TInfor.DataName != Path)
            //        {
            //            TInfor.DataName = Path;
            //            TInfor.Count.Total = 0;
            //            TInfor.Count.OK = 0;
            //            TInfor.Count.NG = 0;
            //            TInfor.Model = comboBox1.SelectedItem.ToString();
            //            SaveInfor();
            //        }
            //    }
            //}
            //else
            //{
            //    TInfor.DataName = Path;
            //    TInfor.Count.Total = 0;
            //    TInfor.Count.OK = 0;
            //    TInfor.Count.NG = 0;
            //    TInfor.Model = comboBox1.SelectedItem.ToString();
            //    SaveInfor();
            //}

            //if (TInfor.DataName != Path)
            //{
            //    TInfor.DataName = Path;
            //    TInfor.Count.Total = 0;
            //    TInfor.Count.OK = 0;
            //    TInfor.Count.NG = 0;
            //    TInfor.Model = comboBox1.SelectedItem.ToString();
            //    SaveInfor();
            //}

            if (File.Exists(Path) == false) CreateDataFile(Path);

            int Col = 0;

            fpSpread2.SuspendLayout();
            fpSpread2.ActiveSheet.RowCount = RowCount + 1;

            fpSpread2.ActiveSheet.SetRowHeight(RowCount, 21);
            //fpSpread2.ActiveSheet.ColumnCount = 39;
            for (int i = 0; i < fpSpread2.ActiveSheet.ColumnCount; i++)
            {
                fpSpread2.ActiveSheet.Cells[RowCount, i].CellType = new FarPoint.Win.Spread.CellType.TextCellType();
                fpSpread2.ActiveSheet.Cells[RowCount, i].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
                fpSpread2.ActiveSheet.Cells[RowCount, i].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
                fpSpread2.ActiveSheet.Cells[RowCount, i].Border = LineBorderToData;
                fpSpread2.ActiveSheet.Cells[RowCount, i].Text = "";
            }
            //No.
            fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
            fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = ((RowCount - 6) + 1).ToString();
            Col++;

            //Time
            fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
            fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = DateTime.Now.ToString("yyyy-MM-dd") + " " + DateTime.Now.ToLongTimeString();
            Col++;

            //Barcode
            fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
            fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;

            if (IOPort.GetAuto == true)
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = label17.Text;
            else fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = textBox1.Text;
            Col++;

            //자동,소동
            fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
            fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;

            if (IOPort.GetAuto == true)
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = "자동";
            else fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = "수동";
            Col++;

            if (mData.Recline.Test == true)
            {
                if ((mData.Recline.Result1 != RESULT.REJECT) && (mData.Recline.Result2 != RESULT.REJECT))
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                }
                else
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
                }
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = ((mData.Recline.Result1 != RESULT.REJECT) && (mData.Recline.Result2 != RESULT.REJECT)) ? "OK" : "NG";
            }
            Col++;

            if (mData.Relax.Test == true)
            {
                if ((mData.Relax.Result1 != RESULT.REJECT) && (mData.Relax.Result2 != RESULT.REJECT))
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                }
                else
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
                }
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = ((mData.Relax.Result1 != RESULT.REJECT) && (mData.Relax.Result2 != RESULT.REJECT)) ? "OK" : "NG";
            }
            Col++;

            //if (mData.Height.Test == true)
            //{
            //    if ((mData.Height.Result1 != RESULT.REJECT) && (mData.Height.Result2 != RESULT.REJECT))
            //    {
            //        fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
            //        fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
            //    }
            //    else
            //    {
            //        fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
            //        fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
            //    }
            //    fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = ((mData.Height.Result1 != RESULT.REJECT) && (mData.Height.Result2 != RESULT.REJECT)) ? "OK" : "NG";
            //}
            //Col++;

            if (mData.Legrest.Test == true)
            {
                if ((mData.Legrest.Result1 != RESULT.REJECT) && (mData.Legrest.Result2 != RESULT.REJECT))
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                }
                else
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
                }
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = ((mData.Legrest.Result1 != RESULT.REJECT) && (mData.Legrest.Result2 != RESULT.REJECT)) ? "OK" : "NG";
            }
            Col++;

            if (mData.LegrestExt.Test == true)
            {
                if ((mData.LegrestExt.Result1 != RESULT.REJECT) && (mData.LegrestExt.Result2 != RESULT.REJECT))
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                }
                else
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
                }
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = ((mData.LegrestExt.Result1 != RESULT.REJECT) && (mData.LegrestExt.Result2 != RESULT.REJECT)) ? "OK" : "NG";
            }

            Col++;

            if(mData.SWVer.Test == true)
            {
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = fpSpread1.ActiveSheet.Cells[7, 3].Text;
                Col++;
                if (mData.SWVer.Result != RESULT.REJECT)
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
                }
                else
                {
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                    fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
                }
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = mData.SWVer.Data;
            }
            else
            {
                Col++;
            }
            Col++;


            if (mData.Result == RESULT.PASS)
            {
                fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.White;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.Black;
            }
            else if (mData.Result == RESULT.REJECT)
            {
                fpSpread2.ActiveSheet.Cells[RowCount, Col].BackColor = Color.Red;
                fpSpread2.ActiveSheet.Cells[RowCount, Col].ForeColor = Color.White;
            }
            if (mData.Result == RESULT.PASS)
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = "O.K";
            else if (mData.Result == RESULT.REJECT)
                fpSpread2.ActiveSheet.Cells[RowCount, Col].Text = "N.G";
            Col++;
            
            RowCount++;

            //if (fpSpread2.ActiveSheet.ColumnCount != Col) fpSpread2.ActiveSheet.ColumnCount = Col;

            fpSpread2.ResumeLayout();
            fpSpread2.SaveExcel(Path);
            return;
        }


        private FarPoint.Win.LineBorder LineBorderToHeader = new FarPoint.Win.LineBorder(Color.Black, 1/*RowHeight*/, true, true, true, true);//line color,line style,left,top,right,buttom                       
        private FarPoint.Win.LineBorder LineBorderToData = new FarPoint.Win.LineBorder(Color.Black, 1/*RowHeight*/, true, false, true, true);//line color,line style,left,top,right,buttom                       
        private void CreateDataFile(string dName)
        {
            fpSpread2.ActiveSheet.Reset();

            fpSpread2.SuspendLayout();
            fpSpread2.ActiveSheet.Protect = false;
            //fpSpread2.Visible = true;
            fpSpread2.ActiveSheet.RowCount = 6;

            //용지 방향
            fpSpread2.ActiveSheet.PrintInfo.Orientation = FarPoint.Win.Spread.PrintOrientation.Landscape;
            //프린트 할 때 가로,세로 중앙에 프린트 할 수 있도록 설정
            fpSpread2.ActiveSheet.PrintInfo.Centering = FarPoint.Win.Spread.Centering.Horizontal; //좌/우 중앙                        
            //fpSpread2.ActiveSheet.PrintInfo.PrintCenterOnPageV = false; //Top 쪽으로간다. 만약 true로 설정할 경우 상,하 중간에 프린트가 된다.

            //여백
            fpSpread2.ActiveSheet.PrintInfo.Margin.Bottom = 1;
            fpSpread2.ActiveSheet.PrintInfo.Margin.Left = 1;
            fpSpread2.ActiveSheet.PrintInfo.Margin.Right = 1;
            fpSpread2.ActiveSheet.PrintInfo.Margin.Top = 2;

            //프린트에서 컬러 표시
            fpSpread2.ActiveSheet.PrintInfo.ShowColor = true;
            //프린트에서 셀 라인 표시여부 (true일경우 내가 그린 라인 말고 셀에 사각 표시 라인도 같이 프린트가 된다.
            fpSpread2.ActiveSheet.PrintInfo.ShowGrid = false;

            fpSpread2.ActiveSheet.VerticalGridLine = new FarPoint.Win.Spread.GridLine(FarPoint.Win.Spread.GridLineType.None);
            fpSpread2.ActiveSheet.HorizontalGridLine = new FarPoint.Win.Spread.GridLine(FarPoint.Win.Spread.GridLineType.None);
            //용지 넓이에 페이지 맞춤
            fpSpread2.ActiveSheet.PrintInfo.UseSmartPrint = true;

            //그리드를 표시할 경우 저장할 때나 프린트 할 때 화면에 같이 표시,그리드가 프린트 되기 때문에 지저분해 보인다.
            fpSpread2.ActiveSheet.PrintInfo.ShowColumnFooter = FarPoint.Win.Spread.PrintHeader.Hide;
            fpSpread2.ActiveSheet.PrintInfo.ShowColumnFooterEachPage = false;


            //헤더와 밖같 라인이 같이 프린트 되지 않도록 한다.
            fpSpread2.ActiveSheet.PrintInfo.ShowBorder = false;
            fpSpread2.ActiveSheet.PrintInfo.ShowColumnHeader = FarPoint.Win.Spread.PrintHeader.Hide;
            fpSpread2.ActiveSheet.PrintInfo.ShowRowHeader = FarPoint.Win.Spread.PrintHeader.Hide;
            fpSpread2.ActiveSheet.PrintInfo.ShowShadows = false;
            fpSpread2.ActiveSheet.PrintInfo.ShowTitle = FarPoint.Win.Spread.PrintTitle.Hide;
            fpSpread2.ActiveSheet.PrintInfo.ShowSubtitle = FarPoint.Win.Spread.PrintTitle.Hide;

            //시트 보호를 해지 한다.
            //fpSpread2.ActiveSheet.PrintInfo.PrintType = FarPoint.Win.Spread.PrintType.All;
            //fpSpread2.ActiveSheet.PrintInfo.SmartPrintRules.Add(new ReadOnlyAttribute(false));
            //axfpSpread1.Protect = false;


            //for (int i = 0; i < 24; i++) fpSpread2.ActiveSheet.SetColumnWidth(i, 80);

            //틀 고정
            int Col;

            Col = 0;
            fpSpread2.ActiveSheet.FrozenColumnCount = 3;
            fpSpread2.ActiveSheet.FrozenRowCount = 6;
            fpSpread2.ActiveSheet.Cells[1, 0].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[1, 0].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Right;
            fpSpread2.ActiveSheet.Cells[1, 0].Text = "날짜 :";
            fpSpread2.ActiveSheet.AddSpanCell(1, 1, 1, 22);

            fpSpread2.ActiveSheet.Cells[1, 1].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[1, 1].Text = DateTime.Now.ToLongDateString();
            fpSpread2.ActiveSheet.Cells[1, 1].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[1, 1].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Left;
            //Col++;
            
            //No
            fpSpread2.ActiveSheet.SetRowHeight(3, 31);
            fpSpread2.ActiveSheet.SetRowHeight(4, 31);
            fpSpread2.ActiveSheet.SetRowHeight(5, 31);
            fpSpread2.ActiveSheet.AddSpanCell(3, Col, 3, 1);
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].Text = "NO.";
            fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;
            Col++;

            //Time
            fpSpread2.ActiveSheet.AddSpanCell(3, Col, 3, 1);
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 300);
            fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].Text = "생산 시간";
            fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;
            Col++;

            //바코드
            fpSpread2.ActiveSheet.AddSpanCell(3, Col, 3, 1);
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 300);
            fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].Text = "바코드";
            fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;
            Col++;

            //자동/수동
            fpSpread2.ActiveSheet.AddSpanCell(3, Col, 3, 1);
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].Text = "자동/수동";
            fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;
            Col++;

            fpSpread2.ActiveSheet.AddSpanCell(3, Col, 1, 4);
            fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].Text = "데이타";
            fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;

            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.AddSpanCell(4, Col, 2, 1);
            fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].Text = "RECLINE";
            fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;
            Col++;
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.AddSpanCell(4, Col, 2, 1);
            fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].Text = "RELAX";
            fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;
            Col++;
            //fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            //fpSpread2.ActiveSheet.AddSpanCell(4, Col, 2, 1);
            //fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            //fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            //fpSpread2.ActiveSheet.Cells[4, Col].Text = "HEIGHT";
            //fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            //fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            //fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;
            //Col++;

            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.AddSpanCell(4, Col, 2, 1);
            fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].Text = "LEGREST";
            fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;
            Col++;
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.AddSpanCell(4, Col, 2, 1);
            fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].Text = "REGREST EXT";
            fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;
            Col++;

            fpSpread2.ActiveSheet.AddSpanCell(3, Col, 1, 2);
            fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].Text = "S/W VERSION";
            fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;

            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.AddSpanCell(4, Col, 2, 1);
            fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].Text = "SPEC";
            fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;
            Col++;
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.AddSpanCell(4, Col, 2, 1);
            fpSpread2.ActiveSheet.Cells[4, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[4, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[4, Col].Text = "DATA";
            fpSpread2.ActiveSheet.Cells[4, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[4, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[4, Col].Border = LineBorderToData;
            Col++;

            //판정
            fpSpread2.ActiveSheet.AddSpanCell(3, Col, 3, 1);
            fpSpread2.ActiveSheet.SetColumnWidth(Col, 100);
            fpSpread2.ActiveSheet.Cells[3, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread2.ActiveSheet.Cells[3, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[3, Col].Text = "판정";
            fpSpread2.ActiveSheet.Cells[3, Col].BackColor = Color.WhiteSmoke;
            fpSpread2.ActiveSheet.Cells[3, Col].ForeColor = Color.Black;
            fpSpread2.ActiveSheet.Cells[3, Col].Border = LineBorderToHeader;
            Col++;
            fpSpread2.ActiveSheet.ColumnCount = Col;

            //Header
            fpSpread2.ActiveSheet.AddSpanCell(0, 0, 1, Col);
            fpSpread2.ActiveSheet.SetRowHeight(0, 100);
            fpSpread2.ActiveSheet.Cells[0, 0].Font = new Font("맑은 고딕", 26);
            fpSpread2.ActiveSheet.SetText(0, 0, "레포트");
            fpSpread2.ActiveSheet.Cells[0, 0].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread2.ActiveSheet.Cells[0, 0].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;

            fpSpread2.ResumeLayout();
            RowCount = 6;
            fpSpread2.SaveExcel(dName);
            return;
        }

        private void SaveInfor()
        {
            string Path = Program.INFOR_PATH.ToString() + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".inf";


            TIniFile Ini = new TIniFile(Path);

            Ini.WriteInteger("COUNT", "TOTAL", TInfor.Count.Total);
            Ini.WriteInteger("COUNT", "OK", TInfor.Count.OK);
            Ini.WriteInteger("COUNT", "NG", TInfor.Count.NG);

            Ini.WriteString("NAME", "DATA", TInfor.DataName);
            Ini.WriteString("NAME", "MODEL", TInfor.Model);
            Ini.WriteString("DATE", "VALUE", TInfor.Date);
            Ini.WriteBool("OPTION", "VALUE", TInfor.ReBootingFlag);
            return;
        }

        private void OpenInfor()
        {
            string Path = Program.INFOR_PATH.ToString() + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".inf";
            string dPath = Program.DATA_PATH.ToString() + "\\" + DateTime.Now.ToString("yyyyMM") + ".xls";

            if (File.Exists(dPath) == false)
            {
                TInfor.DataName = Path;
                TInfor.Count.Total = 0;
                TInfor.Count.OK = 0;
                TInfor.Count.NG = 0;
                TInfor.Date = DateTime.Now.ToString("yyyyMMdd");
                TInfor.Model = comboBox1.SelectedItem.ToString();
                SaveInfor();
            }
            else
            {
                TIniFile Ini = new TIniFile(Path);

                if (Ini.ReadInteger("COUNT", "TOTAL", ref TInfor.Count.Total) == false) TInfor.Count.Total = 0;
                if (Ini.ReadInteger("COUNT", "OK", ref TInfor.Count.OK) == false) TInfor.Count.OK = 0;
                if (Ini.ReadInteger("COUNT", "NG", ref TInfor.Count.NG) == false) TInfor.Count.NG = 0;

                if (Ini.ReadString("NAME", "DATA", ref TInfor.DataName) == false) TInfor.DataName = dPath;
                if (Ini.ReadString("NAME", "MODEL", ref TInfor.Model) == false) TInfor.Model = "";
                if (Ini.ReadString("DATE", "VALUE", ref TInfor.Date) == false) TInfor.Date = DateTime.Now.ToString("yyyyMMdd");
                if (Ini.ReadBool("OPTION", "VALUE", ref TInfor.ReBootingFlag) == false) TInfor.ReBootingFlag = false;
            }
            if (File.Exists(dPath) == false)
            {
                CreateFileName();
                return;
            }
            else
            {
                fpSpread2.OpenExcel(dPath);
                fpSpread2.Visible = false;
                fpSpread2.ActiveSheet.Protect = false;
                RowCount = 6;
                for (int i = RowCount; i < fpSpread2.ActiveSheet.RowCount; i++)
                {
                    if (fpSpread2.ActiveSheet.Cells[RowCount, 0].Text == "") break;
                    if (fpSpread2.ActiveSheet.Cells[RowCount, 0].Text == null) break;
                    RowCount++;
                }

                int Col = 0;
                for (int i = 0; i < fpSpread2.ActiveSheet.ColumnCount; i++)
                {
                    if (fpSpread2.ActiveSheet.Cells[3, Col].Text == "판정")
                        break;
                    else Col++;
                }
                fpSpread2.ActiveSheet.ColumnCount = Col + 1;
            }
            return;
        }


        private long VersionCheckTimeFirst;
        private long VersionCheckTimeLast;
        public void CheckVersion()
        {
            if(SpecOutputFlag == false)
            {
                //CanReWrite.SetVersionCheckMode = true;
                SpecOutputFlag = true;
                CanReWrite.SendVersionReadMessage();
                StepTimeFirst = ComF.timeGetTimems();
                StepTimeLast = ComF.timeGetTimems();
                VersionCheckTimeFirst = ComF.timeGetTimems();
                VersionCheckTimeLast = ComF.timeGetTimems();
            }
            else
            {
                StepTimeLast = ComF.timeGetTimems();
                if(5000 <= (StepTimeLast - StepTimeFirst))
                {
                    SpecOutputFlag = false;
                    Step++;
                    mData.SWVer.Result = RESULT.REJECT;

                    if (mData.SWVer.Test == false) mData.SWVer.Test = true;
                    fpSpread1.ActiveSheet.Cells[7, 4].Text = mData.SWVer.Data;
                    fpSpread1.ActiveSheet.Cells[7, 5].Text = mData.SWVer.Result == RESULT.PASS ? "OK" : "NG";
                    fpSpread1.ActiveSheet.Cells[7, 5].ForeColor = mData.SWVer.Result == RESULT.PASS ? Color.Lime : Color.Red;
                }
                else
                {
                    VersionCheckTimeLast = ComF.timeGetTimems();

                    if (500 <= (VersionCheckTimeLast - VersionCheckTimeFirst))
                    {
                        VersionCheckTimeFirst = ComF.timeGetTimems();
                        VersionCheckTimeLast = ComF.timeGetTimems();
                        CanReWrite.SendVersionReadMessage();
                    }

                    if(mData.SWVer.Test == false) mData.SWVer.Test = true;

                    if (1500 <= (StepTimeLast - StepTimeFirst))
                    {
                        if (2 < CanReWrite.GetSWVersion.Length)
                        {
                            mData.SWVer.Data = CanReWrite.GetSWVersion;

                            if (CheckItem.Relax == true)
                            {
                                if (mData.SWVer.Data == mSpec.RelaxSWVer)
                                {
                                    SpecOutputFlag = false;
                                    Step++;
                                    mData.SWVer.Result = RESULT.PASS;
                                    //CanReWrite.SetVersionCheckMode = false;
                                }
                                else
                                {
                                    SpecOutputFlag = false;
                                    Step++;
                                    mData.SWVer.Result = RESULT.REJECT;
                                    //CanReWrite.SetVersionCheckMode = false;
                                }
                            }
                            else
                            {
                                if (mData.SWVer.Data == mSpec.PowerReclinerSWVer)
                                {
                                    SpecOutputFlag = false;
                                    Step++;
                                    mData.SWVer.Result = RESULT.PASS;
                                    //CanReWrite.SetVersionCheckMode = false;
                                }
                                else
                                {
                                    SpecOutputFlag = false;
                                    Step++;
                                    mData.SWVer.Result = RESULT.REJECT;
                                    //CanReWrite.SetVersionCheckMode = false;
                                }
                            }

                            fpSpread1.ActiveSheet.Cells[7, 4].Text = mData.SWVer.Data;
                            fpSpread1.ActiveSheet.Cells[7, 5].Text = mData.SWVer.Result == RESULT.PASS ? "OK" : "NG";
                            fpSpread1.ActiveSheet.Cells[7, 5].ForeColor = mData.SWVer.Result == RESULT.PASS ? Color.Lime : Color.Red;
                        }
                    }
                }
            }
            return;
        }


        public bool C_RelaxOnOff
        {
            set
            {
                if (comboBox3.SelectedItem.ToString() == "LH")
                    CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RL_PSeat_Relax_Up);
                else CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RR_PSeat_Relax_Up);
            }
        }

        public bool C_RelaxReturnOnOff
        {
            set
            {
                if (comboBox3.SelectedItem.ToString() == "LH")
                    CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RL_PSeat_Relax_Down);
                else CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RR_PSeat_Relax_Down);
            }
        }

        public bool C_ReclineFwdOnOff
        {
            set
            {
                if (comboBox3.SelectedItem.ToString() == "LH")
                    CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RL_PSeat_Recline_Fwd);
                else CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RR_PSeat_Recline_Fwd);
            }
        }

        public bool C_ReclineBwdOnOff
        {
            set
            {
                if (comboBox3.SelectedItem.ToString() == "LH")
                    CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RL_PSeat_Recline_Bwd);
                else CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RR_PSeat_Recline_Bwd);
            }
        }

        public bool C_LegrestOnOff
        {
            set
            {
                if (comboBox3.SelectedItem.ToString() == "LH")
                    CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RL_PSeat_Legrest_Up);
                else CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RR_PSeat_Legrest_Up);
            }
        }

        public bool C_LegrestReturnOnOff
        {
            set
            {
                if (comboBox3.SelectedItem.ToString() == "LH")
                    CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RL_PSeat_Legrest_Down);
                else CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RR_PSeat_Legrest_Down);
            }
        }

        public bool C_HeightUpOnOff
        {
            set
            {

                if (comboBox3.SelectedItem.ToString() == "LH")
                    CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RL_PSeat_Height_Up);
                else CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RR_PSeat_Height_Up);
            }
        }

        public bool C_HeightDnOnOff
        {
            set
            {
                if (comboBox3.SelectedItem.ToString() == "LH")
                    CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RL_PSeat_Height_Down);
                else CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RR_PSeat_Height_Down);
            }
        }

        public bool C_LegrestExtOnOff
        {
            set
            {
                if (comboBox3.SelectedItem.ToString() == "LH")
                    CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RL_PSeat_LegrestExt_Up);
                else CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RR_PSeat_LegrestExt_Up);
            }
        }

        public bool C_LegrestExtReturnOnOff
        {
            set
            {
                if (comboBox3.SelectedItem.ToString() == "LH")
                    CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RL_PSeat_LegrestExt_Down);
                else CanReWrite.CanDataOutput(Data: value == true ? (byte)0x01 : (byte)0x00, Msg: OUT_CAN_LIST.RR_PSeat_LegrestExt_Down);
            }
        }

        public void VirtualLimiteToAll()
        {
            if (comboBox3.SelectedItem.ToString() == "LH")
                CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RL_VirtualLimit_All);
            else CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RR_VirtualLimit_All);
            //ComF.timedelay(1000);
            //CanReWrite.CanDataOutput(Data: 0x00, Msg: OUT_CAN_LIST.RL_VirtualLimit_All);
            //CanReWrite.CanDataOutput(Data: 0x00, Msg: OUT_CAN_LIST.RR_VirtualLimit_All);
            CanReWrite.VirtualLimitStatusReadStart = true;
            return;
        }

        public void VirtualLimiteToRecline()
        {
            if (comboBox3.SelectedItem.ToString() == "LH")
                CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RL_VirtualLimit_Recline);
            else CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RR_VirtualLimit_Recline);
            //ComF.timedelay(1000);
            //CanReWrite.CanDataOutput(Data: 0x00, Msg: OUT_CAN_LIST.RL_VirtualLimit_Recline);
            //CanReWrite.CanDataOutput(Data: 0x00, Msg: OUT_CAN_LIST.RR_VirtualLimit_Recline);
            CanReWrite.VirtualLimitStatusReadStart = true;
            return;
        }

        public void VirtualLimiteToRelax()
        {
            if (comboBox3.SelectedItem.ToString() == "LH")
                CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RL_VirtualLimit_Relax);
            else CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RR_VirtualLimit_Relax);
            //ComF.timedelay(1000);
            //CanReWrite.CanDataOutput(Data: 0x00, Msg: OUT_CAN_LIST.RL_VirtualLimit_Relax);
            //CanReWrite.CanDataOutput(Data: 0x00, Msg: OUT_CAN_LIST.RR_VirtualLimit_Relax);
            CanReWrite.VirtualLimitStatusReadStart = true;
            return;
        }

        public void VirtualLimiteToLegrest()
        {
            if (comboBox3.SelectedItem.ToString() == "LH")
                CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RL_VirtualLimit_Legrest);
            else CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RR_VirtualLimit_Legrest);
            //ComF.timedelay(1000);
            //CanReWrite.CanDataOutput(Data: 0x00, Msg: OUT_CAN_LIST.RL_VirtualLimit_Legrest);
            //CanReWrite.CanDataOutput(Data: 0x00, Msg: OUT_CAN_LIST.RR_VirtualLimit_Legrest);
            CanReWrite.VirtualLimitStatusReadStart = true;
            return;
        }

        public void VirtualLimiteToLegrestExt()
        {
            if (comboBox3.SelectedItem.ToString() == "LH")
                CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RL_VirtualLimit_LegrestExt);
            else CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RR_VirtualLimit_LegrestExt);
            //ComF.timedelay(1000);
            //CanReWrite.CanDataOutput(Data: 0x00, Msg: OUT_CAN_LIST.RL_VirtualLimit_LegrestExt);
            //CanReWrite.CanDataOutput(Data: 0x00, Msg: OUT_CAN_LIST.RR_VirtualLimit_LegrestExt);
            CanReWrite.VirtualLimitStatusReadStart = true;
            return;
        }

        public void VirtualLimiteToHeight()
        {
            if (comboBox3.SelectedItem.ToString() == "LH")
                CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RL_VirtualLimit_Height);
            else CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RR_VirtualLimit_Height);
            //ComF.timedelay(1000);
            //CanReWrite.CanDataOutput(Data: 0x00, Msg: OUT_CAN_LIST.RL_VirtualLimit_Height);
            //CanReWrite.CanDataOutput(Data: 0x00, Msg: OUT_CAN_LIST.RR_VirtualLimit_Height);
            CanReWrite.VirtualLimitStatusReadStart = true;
            return;
        }

        public void VirtualLimiteToTilt()
        {
            if (comboBox3.SelectedItem.ToString() == "LH")
                CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RL_VirtualLimit_Tilt);
            else CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RR_VirtualLimit_Tilt);
            //ComF.timedelay(1000);
            //CanReWrite.CanDataOutput(Data: 0x00, Msg: OUT_CAN_LIST.RL_VirtualLimit_Tilt);
            //CanReWrite.CanDataOutput(Data: 0x00, Msg: OUT_CAN_LIST.RR_VirtualLimit_Tilt);
            CanReWrite.VirtualLimitStatusReadStart = true;
            return;
        }
        public void InComingToAll()
        {
            if (comboBox3.SelectedItem.ToString() == "LH")
                CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RL_ALL_IncomingPostion);
            else CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RR_ALL_IncomingPostion);
            CanReWrite.VirtualLimitStatusReadStart = true;
            return;
        }

        public void InComingToRecline()
        {
            if (comboBox3.SelectedItem.ToString() == "LH")
                CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RL_RECLINE_IncomingPostion);
            else CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RR_RECLINE_IncomingPostion);
            CanReWrite.VirtualLimitStatusReadStart = true;
            return;
        }

        public void InComingToRelax()
        {
            if (comboBox3.SelectedItem.ToString() == "LH")
                CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RL_RELAX_IncomingPostion);
            else CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RR_RELAX_IncomingPostion);
            CanReWrite.VirtualLimitStatusReadStart = true;
            return;
        }

        public void InComingToLegrest()
        {
            if (comboBox3.SelectedItem.ToString() == "LH")
                CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RL_LEGREST_IncomingPostion);
            else CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RR_LEGREST_IncomingPostion);
            CanReWrite.VirtualLimitStatusReadStart = true;
            return;
        }

        public void InComingToLegrestExt()
        {
            if (comboBox3.SelectedItem.ToString() == "LH")
                CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RL_LEGRESTEXT_IncomingPostion);
            else CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RR_LEGRESTEXT_IncomingPostion);
            CanReWrite.VirtualLimitStatusReadStart = true;
            return;
        }

        public void InComingToHeight()
        {
            if (comboBox3.SelectedItem.ToString() == "LH")
                CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RL_HEIGHT_IncomingPostion);
            else CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RR_HEIGHT_IncomingPostion);
            CanReWrite.VirtualLimitStatusReadStart = true;
            return;
        }

        public void InComingToTilt()
        {
            if (comboBox3.SelectedItem.ToString() == "LH")
                CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RL_TILT_IncomingPostion);
            else CanReWrite.CanDataOutput(Data: 0x01, Msg: OUT_CAN_LIST.RR_TILT_IncomingPostion);
            CanReWrite.VirtualLimitStatusReadStart = true;
            return;
        }

        private void DisplayIO()
        {
            //셀프 모드이거나 모델 설정등의 모드이면 처리하지 않는다.
            if (panel2.Visible == false)
            {
                if (IOPort.GetProductIn != IOPort.ProductInOut) IOPort.ProductInOut = IOPort.GetProductIn;
                
                if((IOPort.GetPinConnectionFwd == true) || (IOPort.SetPinConnection == true))
                {
                    if (IOPort.SetOrg == false) IOPort.SetOrg = true;
                }
                else
                {
                    if (IOPort.SetOrg == true) IOPort.SetOrg = false;
                }
                if ((IOPort.GetProductIn == false) && (RunningFlag == false))
                {
                    if (panel4.Visible == false)
                    {
                        if (JigUpCount < 10)
                        {
                            JigUpCount++;
                        }
                        else
                        {
                            if (JigUpFlag == true) JigUpFlag = false;
                            //if (NAutoBarcode != "") NAutoBarcode = "";
                        }

                        if (JigUpFlag == false)
                        {
                            if (IOPort.TestINGOnOff == true) IOPort.TestINGOnOff = false;
                        }
                        ScreenInit();

                        if (RetestFlag == true) RetestFlag = false;

                        if (BattOnOff == true) BattOnOff = false;

                        if (IOPort.TestOKOnOff == true) IOPort.TestOKOnOff = false;
                        //if (IOPort.TestNGOnOff == true) IOPort.TestNGOnOff = false;
                        ProductOutFlag = false;
                        //PopReadOk = false;
                        PopReadOkOld = false;
                        if (IOPort.YellowLampOnOff == true) IOPort.YellowLampOnOff = false;
                        if (IOPort.RedLampOnOff == true) IOPort.RedLampOnOff = false;
                        if (IOPort.GreenLampOnOff == true) IOPort.GreenLampOnOff = false;
                    }
                }
                else
                {
                    if(JigUpCount != 0) JigUpCount = 0;
                }
                if (led4.Value.AsBoolean != IOPort.GetProductIn) led4.Value.AsBoolean = IOPort.GetProductIn;
                if (led9.Value.AsBoolean != JigUpFlag) led9.Value.AsBoolean = JigUpFlag;
                if (led6.Value.AsBoolean != IOPort.TestOKOnOff) led6.Value.AsBoolean = IOPort.TestOKOnOff;
                if (led8.Value.AsBoolean != IOPort.SetOrg) led8.Value.AsBoolean = IOPort.SetOrg;
                if (led5.Value.AsBoolean != IOPort.TestINGOnOff) led5.Value.AsBoolean = IOPort.TestINGOnOff;

                
                if (IOPort.GetAuto == false)
                {
                    if (IOPort.GetPinConnectionSW == true)
                    {
                        if (IOPort.SetPinConnection == false)
                        {
                            IOPort.SetPinConnection = true;
                            CanOpenClose = true;
                            BattOnOff = true;
                            ComF.timedelay(1000);
                            PowerOnOff = true;
                        }
                    }
                    else
                    {
                        if (IOPort.SetPinConnection == true)
                        {
                            CanOpenClose = false;
                            IOPort.SetPinConnection = false;
                            BattOnOff = false;
                        }
                    }
                }
            }

            if (BuzzerRunFlag == true)
            {
                BuzzerLast = ComF.timeGetTimems();
                if (BuzzerOnOff == true)
                {
                    if (700 <= (BuzzerLast - BuzzerFirst))
                    {
                        BuzzerOnOff = false;
                        IOPort.BuzzerOnOff = BuzzerOnOff;
                        BuzzerFirst = ComF.timeGetTimems();
                        BuzzerOnCount++;
                    }
                }
                else
                {
                    if (500 <= (BuzzerLast - BuzzerFirst))
                    {
                        if (BuzzerOnCount < 3)
                        {
                            BuzzerOnOff = true;
                            IOPort.BuzzerOnOff = BuzzerOnOff;
                            BuzzerFirst = ComF.timeGetTimems();
                        }
                        else
                        {
                            BuzzerOnOff = false;
                            IOPort.BuzzerOnOff = BuzzerOnOff;
                            BuzzerOnCount = 0;
                            BuzzerRunFlag = false;
                        }
                    }
                }
            }

            if (IOPort.GetAuto == false)
            {
                if (led2.Indicator.Text != "수동")
                {
                    led2.Indicator.Text = "수동";
                    if (panel1.Visible == true)
                        toolStripButton4.Visible = true;
                    else toolStripButton4.Visible = false;

                    textBox1.Visible = true;
                    label17.Visible = false;
                    PowerOnOff = true;
                    ComF.timedelay(1000);
                    IOPort.SetPSeatBatt = true;
                    IOPort.SetIgn1 = true;
                    IOPort.SetIgn2 = true;
                }
                if (toolStrip1.Enabled == false) toolStrip1.Enabled = true;
            }
            else
            {
                if (led2.Indicator.Text != "자동")
                {
                    textBox1.Visible = false;
                    label17.Visible = true;
                    PowerOnOff = false;
                    IOPort.SetPSeatBatt = false;
                    IOPort.SetIgn1 = false;
                    IOPort.SetIgn2 = false;
                    led2.Indicator.Text = "자동";
                    toolStripSeparator4.Visible = false;
                    CanOpenClose = false;
                    IOPort.SetPinConnection = false;
                    CheckPopData();
                    if (panel4.Visible == true) panel4.Visible = false;
                }
                if (panel1.Visible == true)
                {
                    if (toolStrip1.Enabled == true) toolStrip1.Enabled = false;
                }
            }
            return;
        }

        private bool isCheckItem
        {
            get
            {
                if((CheckItem.Height ==  true) || (CheckItem.Legrest == true) || (CheckItem.LegrestExt == true) || (CheckItem.Recline == true) || (CheckItem.Relax == true))
                    return true;
                else return false;
            }
        }

        private string SaveLogData
        {
            set
            {
                string xPath = Program.LOG_PATH + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                using (FileStream fp = File.Open(xPath, FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(fp);
                    writer.Write(value + "\n");
                    writer.Close();
                    fp.Close();
                }

                //using (File.Open(xPath, FileMode.Open))
                //{
                //    File.AppendAllText(value + "\n", xPath);
                //}
            }
        }

        private void ledArrow1_ValueChanged(object sender, Iocomp.Classes.ValueBooleanEventArgs e)
        {
            //Height up
            C_HeightUpOnOff = e.ValueNew;
            return;
            
        }

        private void ledArrow1_MouseDown(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = true;
            return;             
        }

        private void ledArrow1_MouseUp(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = false;
            return;
        }

        private void ledArrow2_MouseDown(object sender, MouseEventArgs e)
        {
            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = true;
            return;
        }

        private void ledArrow2_MouseUp(object sender, MouseEventArgs e)
        {

            Iocomp.Instrumentation.Professional.LedArrow LBut = sender as Iocomp.Instrumentation.Professional.LedArrow;
            LBut.Value.AsBoolean = false;
            return;
        }

        private void ledArrow2_ValueChanged(object sender, Iocomp.Classes.ValueBooleanEventArgs e)
        {
            //Height Down
            C_HeightDnOnOff = e.ValueNew;
            return;
        }

        private void imageButton3_Click(object sender, EventArgs e)
        {
            //UserImageButton.ImageButton But = sender as UserImageButton.ImageButton;
            //if (But.ButtonColor == Color.Black)
            //{
            //    CanReWrite.ModelTypeToRelax = CanMap.MODELTYPE.RELAX;
            //    But.ButtonColor = Color.Red;
            //}
            //else
            //{
            //    CanReWrite.ModelTypeToRelax = CanMap.MODELTYPE.NOT_RELAX;
            //    But.ButtonColor = Color.Black;
            //}

            if (ledBulb1.On == true)
            {
                CanReWrite.ModelTypeToRelax = CanMap.MODELTYPE.RELAX;
                ledBulb1.On = false;
                ledBulb2.On = true;
            }
            else
            {
                CanReWrite.ModelTypeToRelax = CanMap.MODELTYPE.NOT_RELAX;
                ledBulb1.On = true;
                ledBulb2.On = false;
            }
            return;
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", Program.DATA_PATH.ToString());
            return;
        }

        private void imageButton4_Click(object sender, EventArgs e)
        {
            CanReWrite.SendVersionReadMessage();
            return;
        }

        private void imageButton5_Click(object sender, EventArgs e)
        {
            InComingToAll();
            return;
        }

        private void imageButton6_Click(object sender, EventArgs e)
        {
            InComingToRecline();
            return;
        }

        private void imageButton7_Click(object sender, EventArgs e)
        {
            InComingToRelax();
            return;
        }

        private void imageButton8_Click(object sender, EventArgs e)
        {
            InComingToLegrest();
            return;
        }

        private void imageButton9_Click(object sender, EventArgs e)
        {
            InComingToLegrestExt();
            return;
        }

        private void 윈도우재시작ToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if PROGRAM_RUNNING
            if (CanCtrl.isOpen(0) == true) CanCtrl.CanClose(0);
            
            if (PwCtrl.IsOpen == true) PwCtrl.Close();
            if (pMeter.isOpen == true) pMeter.Close();
#endif
            SaveInfor();
            ComF.WindowRestartToDelay(0);

            System.Diagnostics.Process[] mProcess = System.Diagnostics.Process.GetProcessesByName(Application.ProductName);
            foreach (System.Diagnostics.Process p in mProcess) p.Kill();
            return;
        }

        private void 윈도우종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if PROGRAM_RUNNING
            if (CanCtrl.isOpen(0) == true) CanCtrl.CanClose(0);

            if (PwCtrl.IsOpen == true) PwCtrl.Close();
            if (pMeter.isOpen == true) pMeter.Close();
#endif
            SaveInfor();
            ComF.WindowExit();

            System.Diagnostics.Process[] mProcess = System.Diagnostics.Process.GetProcessesByName(Application.ProductName);
            foreach (System.Diagnostics.Process p in mProcess) p.Kill();
            return;
        }

        private void 시작프로그램등록ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ComF.StartProgramRegistrySet();
            return;
        }

        private void 시작프로그램삭제ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ComF.StartProgramRegistryDel();
            return;
        }

        public bool CanOpenClose
        {
            set
            {
                if (value == true)
                {
                    if (0 <= Config.Can.Device)
                    {
                        if (CanCtrl != null)
                        {
                            if (CanCtrl.isOpen(0) == false)
                            {
                                CanCtrl.OpenCan(0, CanPosition(), (short)Config.Can.Speed, false);
                                if (CanCtrl.isOpen(0) == false) { }
                            }
                        }
                    }
                }
                else
                {
                    if (CanCtrl != null)
                    {
                        if (CanCtrl.isOpen(0) == true) CanCtrl.CanClose(0);
                    }
                }
            }
        }
    }
    public class KALMAN_FILETER
    {
        private struct myKalmanFilterType
        {
            public float z_Din;
            public float Q;
            public float R;
            public float A;
            public float B_uk;
            public float H;
            public float x_Predict;
            public float Xk;
            public float p_Predict;
            public float Pk;
            public float K_gain;
        }

        private myKalmanFilterType[] KalmanStruct = new myKalmanFilterType[20];

        public KALMAN_FILETER()
        {

        }

        //public myKalmanFilterType[] myKalmanBuff = new myKalmanFilterType[5];
        //public myKalmanFilterType myKalmanBuff = new myKalmanFilterType();

        ~KALMAN_FILETER()
        {

        }

        //public const double Speed = 0.01;
        public const double Speed = 0.15;
        private void init_kalmanFiltering(short Ch, ref myKalmanFilterType[] kf)
        {
            //kf->Q = pow(0.01,2);


            kf[Ch].Q = (float)Math.Pow(Speed, 2);
            kf[Ch].R = (float)Math.Pow(0.5, 2);
            kf[Ch].A = 1;
            kf[Ch].B_uk = 0;
            kf[Ch].H = (float)1.0;
            kf[Ch].Xk = 0;     //25
            kf[Ch].Pk = (float)1.0;
            kf[Ch].K_gain = (float)1.0;

            return;
        }

        private void init_kalmanFiltering(ref myKalmanFilterType[] kf)
        {
            //kf->Q = pow(0.01,2);

            for (int i = 0; i < kf.Length; i++)
            {
                kf[i].Q = (float)Math.Pow(Speed, 2);
                kf[i].R = (float)Math.Pow(0.5, 2);
                kf[i].A = 1;
                kf[i].B_uk = 0;
                kf[i].H = (float)1.0;
                kf[i].Xk = 0;     //25
                kf[i].Pk = (float)1.0;
                kf[i].K_gain = (float)1.0;
            }
            return;
        }

        private float kalmanFilter(ref myKalmanFilterType kf)
        {

            kf.x_Predict = (kf.A * kf.Xk) + kf.B_uk;

            kf.p_Predict = (kf.A * kf.Pk) + kf.Q;

            kf.K_gain = kf.p_Predict / (kf.H * kf.p_Predict + kf.R);

            kf.Xk = kf.x_Predict + kf.K_gain * (kf.z_Din - kf.x_Predict);

            kf.Pk = (1 - kf.K_gain) * kf.p_Predict;

            return kf.Xk;

        }

        private float kalmanFilter_(short Ch, float getdata, ref myKalmanFilterType myKalmanBuff)
        {
            myKalmanBuff.z_Din = getdata;

            return kalmanFilter(ref myKalmanBuff);
        }

        //초기화 함수
        private void init_kalmanFilter(ref myKalmanFilterType[] myKalmanBuff)
        {
            //init_kalmanFiltering(ref myKalmanBuff[0]);
            //init_kalmanFiltering(ref myKalmanBuff[1]);
            //init_kalmanFiltering(ref myKalmanBuff[2]);
            //init_kalmanFiltering(ref myKalmanBuff[3]);
            //init_kalmanFiltering(ref myKalmanBuff[4]);
            init_kalmanFiltering(ref myKalmanBuff);
            return;
        }
        private void init_kalmanFilter(short Ch, ref myKalmanFilterType[] myKalmanBuff)
        {
            //init_kalmanFiltering(ref myKalmanBuff[0]);
            //init_kalmanFiltering(ref myKalmanBuff[1]);
            //init_kalmanFiltering(ref myKalmanBuff[2]);
            //init_kalmanFiltering(ref myKalmanBuff[3]);
            //init_kalmanFiltering(ref myKalmanBuff[4]);
            init_kalmanFiltering(Ch, ref myKalmanBuff);
            return;
        }
        public void InitAll()
        {
            init_kalmanFilter(ref KalmanStruct);
            return;
        }

        public void Init(short Ch)
        {
            init_kalmanFilter(Ch, ref KalmanStruct);
            return;
        }

        public float CheckData(short Ch, float Data)
        {
            float rData = kalmanFilter_(Ch, Data, ref KalmanStruct[Ch]);
            rData = KalmanStruct[Ch].Xk;
            return rData;
            //return Data;
        }
    }
}
