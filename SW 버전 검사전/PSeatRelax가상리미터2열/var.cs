using System;
using System.Drawing;
using System.Runtime.InteropServices;
using MES;


namespace PSeatRelax가상리미터2열
{
    [StructLayout(LayoutKind.Explicit)]
    public struct union_r
    {
        [FieldOffset(0)]
        public int i;
        [FieldOffset(0)]
        public byte c1;
        [FieldOffset(1)]
        public byte c2;
        [FieldOffset(2)]
        public byte c3;
        [FieldOffset(3)]
        public byte c4;
    };


    public enum MENU
    {
        NONE,
        AGING_TESTING,
        PERFORMANCE_TESTING,
        AGING_SETTING,
        PERFORMANCE_SETTING,
        OPTION,
        LIN1,
        LIN2,
        CAN,
        PASSWORD,
        AGING_DATAVIEW,
        PERFORMANCE_DATAVIEW,
        SELF
    }


    public class RESULT
    {
        public const short READY = 0;
        public const short PASS = 1;
        public const short NG = 2;
        public const short REJECT = 2;
        public const short END = 3;
        public const short STOP = 4;
        public const short CLEAR = 5;
        public const short TEST = 6;
        public const short NOT_TEST = 7;
        public const short TEST_STANDBY = 8;
    };


    public class IO_IN
    {
        public const short PASS = 0;
        public const short RESET = 1;
        public const short RH_SELECT = 2;
        public const short SEAT_RELAX = 19;
        public const short AUTO = 3;

        public const short TEST_SELECT = 4;

        public const short RELAX_RELAX = 5;
        public const short RELAX_RETURN = 6;

        public const short RECLINE_FWD = 7;
        public const short RECLINE_BWD = 8;

        public const short HEIGHT_UP = 9;
        public const short HEIGHT_DN = 10;

        public const short LEGREST_EXT = 11;
        public const short LEGREST_EXT_RETURN = 12;

        public const short LEGREST = 13;
        public const short LEGREST_RETURN = 14;
        public const short PRODUCT = 16;
        public const short JIG_UP = 17;
        //public const short SlideDeliveryPosSensor = 17;

        public const short PIN_CONNECTION_FWD = 24;
        public const short PIN_CONNECTION_BWD = 25;
        public const short PIN_CONNECTION_SW = 20;
    }


    public class IO_OUT
    {
        public const short RED = 0;
        public const short YELLOW = 1;
        public const short GREEN = 2;
        public const short BUZZER = 3;
        public const short PRODUCT = 4;
        public const short TEST_ING = 7;
        public const short TEST_OK = 5;
        public const short TEST_ORG = 6;
        public const short RH_SELECT = 7;
        public const short RECLINER_FWD = 9;
        public const short RECLINER_BWD = 8;

        public const short PIN_CONNECTION = 24;
        //public const short PSEAT_IGN = 8;        
        //public const short PRODUCT_HORIZENTAL = 9;
        //public const short AIRBAG_RESI_SELECT = 10;        
        public const short MAX = 11;
    }

    public class IO_FUNCTION_OUT
    {
        public const short PSEAT_BATT = 0;
        public const short IGN1 = 10;
        public const short IGN2 = 12;


        public const short RELAX = 10;
        public const short RELAX_RETURN = 11;
        public const short HEIGHT_UP = 12;
        public const short HEIGHT_DN = 13;
        public const short LEGREST = 14;
        public const short LEGREST_RETURN = 15;
        public const short LEGRESTEXT = 16;
        public const short LEGRESTEXT_RETURN = 17;
    }

    public enum LH_RH
    {
        LH,
        RH
    }
    //public enum PSEAT_SELECT
    //{
    //    PSEAT_8WAY_4W,
    //    PSEAT_8WAY,
    //    PSEAT_8WAY_2W,
    //    PSEAT_8WAY_WALKIN
    //}

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]//CharSet = CharSet.Unicode를 선언해 주지 않으면 한글 처리할 때 파일에 저장하거나 할 경우 에러가 발생한다.
    public struct __MinMax__
    {
        public double Min;
        public double Max;
    }

    public struct __MinMaxToInt__
    {
        public int Min;
        public int Max;
    }

    public struct __Port__
    {
        public string Port;
        public int Speed;
    }


    public struct __LinDevice__
    {
        public short Device;
        public int Speed;
    }
    public struct __CanDevice__
    {
        public short Device;
        public short Channel;
        public short ID;
        public int Speed;
    }

    public struct __TcpIP__
    {
        public string IP;
        public int Port;
    }

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]//CharSet = CharSet.Unicode를 선언해 주지 않으면 한글 처리할 때 파일에 저장하거나 할 경우 에러가 발생한다.
    public struct __Config__
    {
        /// <summary>
        /// Master
        /// </summary>
        //public __LinDevice__ Lin;
        public __CanDevice__ Can;
        //public __Port__ NoiseMeter;
        public __TCPIP__ Client;
        public __TCPIP__ Server;
        public __TcpIP__ Board;
        public __TcpIP__ PC;
        public __Port__ Power;
        public __Port__ Panel;
        public short BattID;
        public short CurrID;
        public float ConnectionDelay;
        public bool AutoConnection;
    }

    public struct __Time__
    {
        public int Hour;
        public short Min;
        public short Sec;
        public short mSec;
    }


    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]//CharSet = CharSet.Unicode를 선언해 주지 않으면 한글 처리할 때 파일에 저장하거나 할 경우 에러가 발생한다.
    //[StructLayout(LayoutKind.Sequential, Pack = 1)]    
    public struct __CanData__
    {
        public short Data;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] Title; //[50];
    };

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]//CharSet = CharSet.Unicode를 선언해 주지 않으면 한글 처리할 때 파일에 저장하거나 할 경우 에러가 발생한다.
    public struct __ItemCan__
    {
        public short StartBit;
        public short Size;
        public short Mode; // 0이면 일반데이타가 1 이면 숫치 데이타가 들어가는 항목임 2 이면 아스키 데이타가 들어간다.
        public short DataCounter;
        public short Length;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] Title; //[50];
        public int CanID;
        public short S_ID;
        public short ReceiveTime; // 전송 간격을 갖는다.
        public bool CanLin; // true can, false Lin
        public bool InOut; // true 이면 output mode

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 40)]
        public __CanData__[] Data;//[40];
    }

    public struct __Can__
    {
        public short ItemCounter;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 700)]
        public __ItemCan__[] Item; // [700]
    };
    public struct __sCan__
    {
        public bool Run;
        public short sID;
        public short dBit;
    }

    public struct __CanMsg
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] DATA;// [8];
        public int Length;
        public int ID;
    }


    public struct __SendCan__
    {
        public int ID;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Data; //[8]
        public int Length;
        public long first;
        public long last;
        public long sendtime;
        //public byte AliveCnt;
    }

    public struct __InOutCanMsg__
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public __SendCan__[] Send; // [20]
        public int Max;
    }

    public struct CanInOutStruct
    {
        public __InOutCanMsg__ Can;
    }
    public struct LinInOutStruct
    {
        public __InOutCanMsg__ Lin;
    }

    public struct __InOutCan__
    {
        public CanInOutStruct In;
        public CanInOutStruct Out;
    }
    public struct __InOutLin__
    {
        public LinInOutStruct In;
        public LinInOutStruct Out;
    }


    public struct __IOData__
    {
        public short Card;
        public short Pos;
        public byte Data;
    }

    public struct __LinOutPos
    {
        /// <summary>
        /// 데이타
        /// </summary>
        public byte Data;
        /// <summary>
        /// 초기화 데이타 
        /// </summary>
        public byte Mask;
        //시작 위치
        public short Byte;
        /// <summary>
        /// 비트 위치
        /// </summary>
        public short Pos;
        /// <summary>
        /// Lin FID/PID
        /// </summary>
        public byte ID;
    }
    public struct __LinInPos
    {
        /// <summary>
        /// 데이타
        /// </summary>
        public byte Length;
        /// <summary>
        /// 초기화 데이타 
        /// </summary>
        public byte Mask;
        //시작 위치
        public short Byte;
        /// <summary>
        /// 비트 위치
        /// </summary>
        public short Pos;
        /// <summary>
        /// Lin FID/PID
        /// </summary>
        public byte ID;
    }

    public struct __TestDataItem__
    {
        public short Result;
        public float Data;
        public bool Test;
    }
    
    public struct __TestDataItemToStringData__
    {
        public short Result;
        public string Data;
        public bool Test;
    }
    
    public struct __TestDataItem2__
    {
        public short Result1;
        public short Result2;
        public float Data1;
        public float Data2;
        public bool Test;
    }
    public struct __SoundDataItem__
    {
        public short ResultStart;
        public short ResultRun;
        public float StartData;
        public float RunData;
        public bool Test;
    }

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]//CharSet = CharSet.Unicode를 선언해 주지 않으면 한글 처리할 때 파일에 저장하거나 할 경우 에러가 발생한다.
    public struct __TestCanLinDataItem__
    {
        public short Result;
        public byte Data;
        public short Message;
        public bool Test;
    }

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]//CharSet = CharSet.Unicode를 선언해 주지 않으면 한글 처리할 때 파일에 저장하거나 할 경우 에러가 발생한다.
    public struct __TestData__
    {
        public short Result;
        public string Time;

        public __TestDataItem2__ Recline;
        public __TestDataItem2__ Relax;
        public __TestDataItem2__ Height;
        public __TestDataItem2__ Legrest;
        public __TestDataItem2__ LegrestExt;
        public __TestDataItemToStringData__ SWVer;
    }

    public struct __Counter__
    {
        public int Total;
        public int OK;
        public int NG;
    }

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct __Infor__
    {
        public string Date;
        public string DataName;
        public string Model;
        public __Counter__ Count;
        public bool ReBootingFlag;
    }
    public struct __CanLin__
    {
        public __InOutCan__ Can;
        public __InOutLin__ Lin;
    }
    public struct __CanInPos
    {
        /// <summary>
        /// 데이타
        /// </summary>
        public byte Length;
        /// <summary>
        /// 초기화 데이타 
        /// </summary>
        public byte Mask;
        //시작 위치
        public short Byte;
        /// <summary>
        /// 비트 위치
        /// </summary>
        public short Pos;
        /// <summary>
        /// Lin FID/PID
        /// </summary>
        public int ID;
    }
    public struct __CanOutMessage
    {
        /// <summary>
        /// 데이타
        /// </summary>
        public byte[] Data;
        /// <summary>
        /// Lin FID/PID
        /// </summary>
        public int ID;
    }
    public struct __CanOutPos
    {
        /// <summary>
        /// 데이타
        /// </summary>
        public byte Data;
        /// <summary>
        /// 초기화 데이타 
        /// </summary>
        public byte Mask;
        //시작 위치
        public short Byte;
        /// <summary>
        /// 비트 위치
        /// </summary>
        public short Pos;
        /// <summary>
        /// Lin FID/PID
        /// </summary>
        public int ID;
    }

    public struct PSeatPowrItem
    {
        public int Batt;
        public int Gnd;
    }

    public struct PSeatPower
    {
        public PSeatPowrItem Batt1;
        public PSeatPowrItem Batt2;
    }
    public struct PinMapItem
    {
        public int PinNo;
        public int Mode; // 0 - B+ , 1 - Gnd
    }

    public class PSeatRuNMode
    {
        public const short Batt = 0;
        public const short Gnd = 1;
    }


    public enum LHD_RHD
    {
        LHD,
        RHD
    }
    public enum PSEAT_TYPE
    {
        IMS,
        POWER,
        MANUAL
    }
    public struct __CheckItem__
    {
        public bool Recline;
        public bool Height;
        public bool Relax;
        public bool Legrest;
        public bool LegrestExt;

        public bool Can;
        public bool ProductTestRunFlag;
        public LH_RH LhRh;
        public LHD_RHD LhdRhd;
    }
   

    public struct __DeliveryPos__
    {
        public short Legrest;
        public short Relax;
        public short Height;
        public short Recliner;
        public short LegrestExt;
    }

   

    public struct __Spec__
    {
        public string CarName;

        public __DeliveryPos__ DeliveryPos;
        //-----------------------------
        public float RelaxLimitTime;
        public float LegrestLimitTime;
        public float LegrestExtLimitTime;
        public float HeightLimitTime;
        public float ReclinerLimitTime;
        public float RelaxSwOnTime;
        public float TestVolt;
        public string SWVersion;
    }
}