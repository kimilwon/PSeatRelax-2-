using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PSeatRelax가상리미터2열
{
    public partial class SpecSetting : Form
    {
        private MyInterface mControl = null;
        private __Spec__ mSpec = new __Spec__();
        private string mName = string.Empty;
        private bool ModelBoxChangeFlag = false;
        
        public SpecSetting()
        {
            InitializeComponent();
        }
        public SpecSetting(MyInterface mControl, string fName)
        {
            InitializeComponent();
            this.mControl = mControl;
            mName = fName;
        }

        private void SpecSetting_Load(object sender, EventArgs e)
        {
            mControl.공용함수.ReadFileListNotExt(Program.SPEC_PATH.ToString(), "*.Spc", COMMON_FUCTION.FileSortMode.FILENAME_ODERBY);
            List<string> FList = mControl.공용함수.GetFileList;

            if (0 < FList.Count)
            {
                ModelBoxChangeFlag = true;
                comboBox1.Items.Clear();
                foreach (string s in FList) comboBox1.Items.Add(s);

                if (0 < comboBox1.Items.Count)
                {
                    if (0 < comboBox1.Items.Count)
                    {
                        if ((mName != null) && (mName != "") && (mName != string.Empty))
                        {
                            if (comboBox1.Items.Contains(mName) == true) comboBox1.SelectedItem = mName;
                        }
                    }

                }
                if (mName != null)
                {
                    string sName = Program.SPEC_PATH.ToString() + "\\" + mName + ".Spc";
                    mControl.공용함수.OpenSpec(sName, ref mSpec);
                }
            }
            DisplaySpec();
            ModelBoxChangeFlag = false;
            return;
        }

        private void SpecSetting_FormClosing(object sender, FormClosingEventArgs e)
        {
            return;
        }

        private void imageButton1_Click(object sender, EventArgs e)
        {
            //저장
            ClearSpec();
            MoveSpec();

            string sName;
            string ModName = null;
            if (comboBox1.SelectedItem == null)
            {
                if (MessageBox.Show("선택된 모델이 없습니다.\n모델을 생성하시겠습니까?", "경고", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    if (InputBox.Show("모델명 입력", "모델명", ref ModName) == DialogResult.OK)
                    {
                        if ((ModName != null) && (ModName != "") && (ModName != string.Empty))
                        {
                            mName = ModName;
                            sName = Program.SPEC_PATH.ToString() + "\\" + ModName + ".Spc";

                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                ModName = comboBox1.SelectedItem.ToString();
                sName = Program.SPEC_PATH.ToString() + "\\" + comboBox1.SelectedItem.ToString() + ".Spc";
            }
            mSpec.CarName = ModName;
            mControl.공용함수.SaveSpec(TSpec: mSpec, Name: sName);
        }

        private void MoveSpec()
        {
            if (float.TryParse(fpSpread1.ActiveSheet.Cells[2, 2].Text, out mSpec.ReclinerLimitTime) == false) mSpec.ReclinerLimitTime = 0;
            if (float.TryParse(fpSpread1.ActiveSheet.Cells[3, 2].Text, out mSpec.RelaxLimitTime) == false) mSpec.RelaxLimitTime = 0;
            if (float.TryParse(fpSpread1.ActiveSheet.Cells[4, 2].Text, out mSpec.HeightLimitTime) == false) mSpec.HeightLimitTime = 0;
            if (float.TryParse(fpSpread1.ActiveSheet.Cells[5, 2].Text, out mSpec.LegrestLimitTime) == false) mSpec.LegrestLimitTime = 0;
            if (float.TryParse(fpSpread1.ActiveSheet.Cells[6, 2].Text, out mSpec.LegrestExtLimitTime) == false) mSpec.LegrestExtLimitTime = 0;

            //Pos;

            var Pos1 = fpSpread1.ActiveSheet.Cells[7, 2].Value;
            mSpec.DeliveryPos.Recliner = (short)Pos1;
            var Pos2 = fpSpread1.ActiveSheet.Cells[8, 2].Value;
            mSpec.DeliveryPos.Relax = (short)Pos2;
            var Pos3 = fpSpread1.ActiveSheet.Cells[9, 2].Value;
            mSpec.DeliveryPos.Height = (short)Pos3;
            var Pos4 = fpSpread1.ActiveSheet.Cells[10, 2].Value;
            mSpec.DeliveryPos.Legrest = (short)Pos4;
            var Pos5 = fpSpread1.ActiveSheet.Cells[11, 2].Value;
            mSpec.DeliveryPos.LegrestExt = (short)Pos5;

            if (float.TryParse(fpSpread1.ActiveSheet.Cells[12, 2].Text, out mSpec.RelaxSwOnTime) == false) mSpec.RelaxSwOnTime = 0;
            if (float.TryParse(fpSpread1.ActiveSheet.Cells[13, 2].Text, out mSpec.TestVolt) == false) mSpec.TestVolt = 0;

            var Chk = fpSpread1.ActiveSheet.Cells[14, 1].Value;

            mSpec.EndVoltUse = (bool)Chk;//(string)Chk == "True" ? true : false;

            if (float.TryParse(fpSpread1.ActiveSheet.Cells[14, 2].Text, out mSpec.EndVolt) == false) mSpec.EndVolt = 0;
            if (float.TryParse(fpSpread1.ActiveSheet.Cells[15, 2].Text, out mSpec.TestVoltOperTime) == false) mSpec.TestVoltOperTime = 0;


            mSpec.RelaxSWVer = fpSpread1.ActiveSheet.Cells[16, 2].Text;
            mSpec.PowerReclinerSWVer = fpSpread1.ActiveSheet.Cells[17, 2].Text;
            return;
        }

        private void ClearSpec()
        {
            mSpec.CarName  = "";
            mSpec.DeliveryPos.Height = 0;
            mSpec.DeliveryPos.Legrest = 0;
            mSpec.DeliveryPos.LegrestExt = 0;
            mSpec.DeliveryPos.Recliner = 0;
            mSpec.DeliveryPos.Relax = 0;
            mSpec.TestVolt = 0;
            mSpec.HeightLimitTime = 0;
            mSpec.LegrestExtLimitTime = 0;
            mSpec.LegrestLimitTime = 0;
            mSpec.ReclinerLimitTime = 0;
            mSpec.RelaxLimitTime = 0;
            mSpec.RelaxSwOnTime = 0;
            mSpec.RelaxSWVer = "";
            mSpec.PowerReclinerSWVer = "";
            mSpec.EndVoltUse = false;
            mSpec.EndVolt = 0;
            mSpec.TestVoltOperTime = 0;
            return;
        }

        private void DisplaySpec()
        {
            fpSpread1.ActiveSheet.Cells[2, 2].Text = mSpec.ReclinerLimitTime.ToString("0.0");
            fpSpread1.ActiveSheet.Cells[3, 2].Text = mSpec.RelaxLimitTime.ToString("0.0");
            fpSpread1.ActiveSheet.Cells[4, 2].Text = mSpec.HeightLimitTime.ToString("0.0");
            fpSpread1.ActiveSheet.Cells[5, 2].Text = mSpec.LegrestLimitTime.ToString("0.0");
            fpSpread1.ActiveSheet.Cells[6, 2].Text = mSpec.LegrestExtLimitTime.ToString("0.0");
            
            fpSpread1.ActiveSheet.Cells[7, 2].Value = mSpec.DeliveryPos.Recliner;
            fpSpread1.ActiveSheet.Cells[8, 2].Value = mSpec.DeliveryPos.Relax; 
            fpSpread1.ActiveSheet.Cells[9, 2].Value = mSpec.DeliveryPos.Height;
            fpSpread1.ActiveSheet.Cells[10, 2].Value = mSpec.DeliveryPos.Legrest;
            fpSpread1.ActiveSheet.Cells[11, 2].Value = mSpec.DeliveryPos.LegrestExt;

            fpSpread1.ActiveSheet.Cells[12, 2].Text = mSpec.RelaxSwOnTime.ToString("0.0");
            fpSpread1.ActiveSheet.Cells[13, 2].Text = mSpec.TestVolt.ToString("0.0");


            fpSpread1.ActiveSheet.Cells[14, 1].Value = mSpec.EndVoltUse;
            fpSpread1.ActiveSheet.Cells[14, 2].Text = mSpec.EndVolt.ToString("0.0");
            fpSpread1.ActiveSheet.Cells[15, 2].Text =  mSpec.TestVoltOperTime.ToString("0.0");

            fpSpread1.ActiveSheet.Cells[16, 2].Text = mSpec.RelaxSWVer;
            fpSpread1.ActiveSheet.Cells[17, 2].Text = mSpec.PowerReclinerSWVer;
            return;
        }

        private void imageButton2_Click(object sender, EventArgs e)
        {
            //생성
            string ModName = null;
            if (InputBox.Show("모델명 입력", "모델명", ref ModName) == DialogResult.OK)
            {
                if ((ModName != null) && (ModName != "") && (ModName != string.Empty))
                {
                    string sName;
                    mName = ModName;
                    sName = Program.SPEC_PATH.ToString() + "\\" + ModName + ".Spc";

                    ModelBoxChangeFlag = true;
                    comboBox1.Items.Add(ModName);

                    ClearSpec();
                    DisplaySpec();
                    mSpec.CarName = mName;

                    mControl.공용함수.SaveSpec(TSpec: mSpec, Name: sName);
                    ModelBoxChangeFlag = false;
                }
            }
            return;
        }

        private void imageButton4_Click(object sender, EventArgs e)
        {
            //다른 이름으로 저장
            ClearSpec();
            MoveSpec();

            string ModName = null;
            if (InputBox.Show("모델명 입력", "모델명", ref ModName) == DialogResult.OK)
            {
                if ((ModName != null) && (ModName != "") && (ModName != string.Empty))
                {
                    string sName;
                    mName = ModName;
                    sName = Program.SPEC_PATH.ToString() + "\\" + ModName + ".Spc";

                    MoveSpec();
                    mSpec.CarName = ModName;
                    mControl.공용함수.SaveSpec(TSpec: mSpec, Name: sName);
                    ModelBoxChangeFlag = true;
                    comboBox1.Items.Add(ModName);
                    comboBox1.SelectedItem = ModName;
                    ModelBoxChangeFlag = false;
                }
            }
            return;
        }

        private void imageButton3_Click(object sender, EventArgs e)
        {
            //삭제
            if (comboBox1.SelectedItem == null) return;
            string sName = Program.SPEC_PATH.ToString() + "\\" + comboBox1.SelectedItem.ToString() + ".Spc";

            if (File.Exists(sName) == true) File.Delete(sName);

            ClearSpec();
            DisplaySpec();
            comboBox1.Items.Remove(comboBox1.SelectedItem);
            return;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ModelBoxChangeFlag == true) return;
            string s = comboBox1.SelectedItem.ToString();

            string sName = Program.SPEC_PATH.ToString() + "\\" + s + ".Spc";

            ClearSpec();
            if (File.Exists(sName) == true) mControl.공용함수.OpenSpec(sName, ref mSpec);
            mName = mSpec.CarName;
            DisplaySpec();
            return;
        }
    }
}
